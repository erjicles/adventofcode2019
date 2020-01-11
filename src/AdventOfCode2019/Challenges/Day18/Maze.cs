﻿using AdventOfCode2019.Grid;
using AdventOfCode2019.Grid.PathFinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace AdventOfCode2019.Challenges.Day18
{
    public class Maze
    {
        public Dictionary<GridPoint, MazeCell> MazeCells { get; private set; }
        public Dictionary<string, GridPoint> DoorCells { get; private set; }
        public Dictionary<string, GridPoint> KeyCells { get; private set; }
        public HashSet<string> Keys { get; private set; }
        public Dictionary<GridPoint, string> CellsWithDoors { get; private set; }
        public Dictionary<GridPoint, string> CellsWithKeys { get; private set; }
        public GridPoint StartingPosition { get; private set; }
        public Graph<GridPoint> MazeGraph { get; private set; }
        /// <summary>
        /// Contains the initial shortest accessible path between each pair
        /// of maze features (doors, keys, and initial position)
        /// </summary>
        public Dictionary<Tuple<GridPoint, GridPoint>, IList<GridPoint>> EdgeShortestPaths { get; private set; }
        /// <summary>
        /// Contains the keys located along the paths contained in the
        /// <seealso cref="EdgeShortestPaths"/> property.
        /// </summary>
        public Dictionary<Tuple<GridPoint, GridPoint>, HashSet<string>> EdgeKeys { get; private set; }
        /// <summary>
        /// Contains the shortest path between each pair of keys, ignoring
        /// doors encountered along the way.
        /// </summary>
        public Dictionary<Tuple<GridPoint, GridPoint>, IList<GridPoint>> ShortestPathsBetweenKeysIgnoringDoors { get; private set; }
        public Dictionary<Tuple<GridPoint, GridPoint>, HashSet<string>> DoorsAlongShortestPathBetweenKeys { get; private set; }
        public Dictionary<Tuple<GridPoint, GridPoint>, HashSet<string>> KeysAlongShortestPathBetweenKeys { get; private set; }
        public Maze(IList<string> mazeDefinition)
        {
            InitializeMaze(mazeDefinition);
        }

        private void InitializeMaze(IList<string> mazeDefinition)
        {
            MazeCells = new Dictionary<GridPoint, MazeCell>();
            DoorCells = new Dictionary<string, GridPoint>();
            KeyCells = new Dictionary<string, GridPoint>();
            Keys = new HashSet<string>();
            CellsWithDoors = new Dictionary<GridPoint, string>();
            CellsWithKeys = new Dictionary<GridPoint, string>();
            for (int y = 0; y < mazeDefinition.Count; y++)
            {
                var rowString = mazeDefinition[y];
                for (int x = 0; x < rowString.Length; x++)
                {
                    var point = new GridPoint(x, y);
                    var cellDefinition = rowString[x].ToString();
                    var mazeCellType = MazeCellType.Empty;
                    var cellItem = string.Empty;
                    if ("#".Equals(cellDefinition))
                    {
                        mazeCellType = MazeCellType.Wall;
                    }
                    else if ("@".Equals(cellDefinition))
                    {
                        StartingPosition = point;
                    }
                    else if (Regex.IsMatch(cellDefinition, @"^[a-z]$"))
                    {
                        KeyCells.Add(cellDefinition, point);
                        CellsWithKeys.Add(point, cellDefinition);
                        Keys.Add(cellDefinition);
                    }
                    else if (Regex.IsMatch(cellDefinition, @"^[A-Z]$"))
                    {
                        DoorCells.Add(cellDefinition, point);
                        CellsWithDoors.Add(point, cellDefinition);
                    }

                    var mazeCell = new MazeCell(point, mazeCellType);
                    MazeCells.Add(point, mazeCell);
                }
            }

            ConstructMazeGraph();
            ConstructShortestKeyToKeyPaths();
        }

        private void ConstructMazeGraph()
        {
            var graphNodes = CellsWithDoors.Select(kvp => kvp.Key)
                .ToList();
            graphNodes.AddRange(CellsWithKeys.Select(kvp => kvp.Key)
                .ToList());
            graphNodes.Add(StartingPosition);
            EdgeShortestPaths = new Dictionary<Tuple<GridPoint, GridPoint>, IList<GridPoint>>();
            EdgeKeys = new Dictionary<Tuple<GridPoint, GridPoint>, HashSet<string>>();
            IList<GridPoint> GetShortestPathBetweenPoints(GridPoint p1, GridPoint p2)
            {
                var pathResult = GetPathToPoint(
                    startPoint: p1, 
                    targetPoint: p2, 
                    keysCollected: new SortedDictionary<string, string>(),
                    forceAllowToEnterTarget: true);
                if (pathResult.Path.Count > 0)
                {
                    var edgeKey12 = new Tuple<GridPoint, GridPoint>(p1, p2);
                    var edgeKey21 = new Tuple<GridPoint, GridPoint>(p2, p1);
                    HashSet<string> keysAlongEdge = null;
                    if (!EdgeShortestPaths.ContainsKey(edgeKey12))
                    {
                        keysAlongEdge = GetKeysAlongPath(pathResult.Path);
                        EdgeShortestPaths.Add(edgeKey12, pathResult.Path);
                        EdgeKeys.Add(edgeKey12, keysAlongEdge);
                    }
                    if (!EdgeShortestPaths.ContainsKey(edgeKey21))
                    {
                        var reversedPath = pathResult.Path.ToList();
                        reversedPath.Reverse();
                        keysAlongEdge = GetKeysAlongPath(reversedPath);
                        EdgeShortestPaths.Add(edgeKey21, reversedPath);
                        EdgeKeys.Add(edgeKey21, keysAlongEdge);
                    }
                }
                return pathResult.Path;
            }
            MazeGraph = new Graph<GridPoint>(graphNodes, GetShortestPathBetweenPoints);
        }

        private void ConstructShortestKeyToKeyPaths()
        {
            ShortestPathsBetweenKeysIgnoringDoors = new Dictionary<Tuple<GridPoint, GridPoint>, IList<GridPoint>>();
            DoorsAlongShortestPathBetweenKeys = new Dictionary<Tuple<GridPoint, GridPoint>, HashSet<string>>();
            KeysAlongShortestPathBetweenKeys = new Dictionary<Tuple<GridPoint, GridPoint>, HashSet<string>>();
            var keyCells = Keys.Select(k => KeyCells[k]).ToList();
            keyCells.Add(StartingPosition);
            for (int i = 0; i < keyCells.Count; i++)
            {
                var startKeyCell = keyCells[i];
                for (int j = i+1; j < keyCells.Count; j++)
                {
                    var targetKeyCell = keyCells[j];

                    int Heuristic(GridPoint point)
                    {
                        return GridPoint.GetManhattanDistance(point, targetKeyCell);
                    }
                    IList<GridPoint> GetNeighbors(GridPoint point)
                    {
                        return GetPointNeighbors(
                            point: point,
                            keysCollected: new SortedDictionary<string, string>(),
                            specificNeighborToAllow: null,
                            ignoreDoors: true);
                    }
                    int GetEdgeCost(GridPoint start, GridPoint end)
                    {
                        return 1;
                    }
                    var pathResult = AStar.GetPath<GridPoint>(
                        startPoint: startKeyCell,
                        endPoint: targetKeyCell,
                        Heuristic: Heuristic,
                        GetNeighbors: GetNeighbors,
                        GetEdgeCost: GetEdgeCost);
                    if (pathResult.Path.Count > 0)
                    {
                        var edgeKey12 = new Tuple<GridPoint, GridPoint>(startKeyCell, targetKeyCell);
                        var edgeKey21 = new Tuple<GridPoint, GridPoint>(targetKeyCell, startKeyCell);
                        HashSet<string> keysAlongPath = null;
                        HashSet<string> doorsAlongPath = null;
                        if (!ShortestPathsBetweenKeysIgnoringDoors.ContainsKey(edgeKey12))
                        {
                            ShortestPathsBetweenKeysIgnoringDoors.Add(edgeKey12, pathResult.Path);
                            keysAlongPath = GetKeysAlongPath(pathResult.Path);
                            doorsAlongPath = GetDoorsAlongPath(pathResult.Path);
                            DoorsAlongShortestPathBetweenKeys.Add(edgeKey12, doorsAlongPath);
                            KeysAlongShortestPathBetweenKeys.Add(edgeKey12, keysAlongPath);
                        }
                        if (!ShortestPathsBetweenKeysIgnoringDoors.ContainsKey(edgeKey21))
                        {
                            var reversedPath = pathResult.Path.ToList();
                            reversedPath.Reverse();
                            ShortestPathsBetweenKeysIgnoringDoors.Add(edgeKey21, reversedPath);
                            keysAlongPath = GetKeysAlongPath(reversedPath);
                            doorsAlongPath = GetDoorsAlongPath(reversedPath);
                            DoorsAlongShortestPathBetweenKeys.Add(edgeKey21, doorsAlongPath);
                            KeysAlongShortestPathBetweenKeys.Add(edgeKey21, keysAlongPath);
                        }
                    }
                }
            }
        }

        public bool GetCanEnterCell(
            GridPoint point, 
            SortedDictionary<string, string> keysCollected,
            bool ignoreDoors = false)
        {
            if (!MazeCells.ContainsKey(point))
                return false;
            var cell = MazeCells[point];
            if (MazeCellType.Wall.Equals(cell.Type))
                return false;
            if (!ignoreDoors 
                && CellsWithDoors.ContainsKey(point)
                && !keysCollected.ContainsKey(CellsWithDoors[point].ToLower()))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Gets all keys that lie along the given node path, where each edge
        /// is in the EdgeKeys proeprty.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public HashSet<string> GetKeysCollectedAlongNodePath(IList<GraphNode<GridPoint>> path)
        {
            var result = new List<string>();
            for (int i = 0; i < path.Count - 1; i++)
            {
                var edgeKey = new Tuple<GridPoint, GridPoint>(path[i].Node, path[i + 1].Node);
                var keysAlongPath = EdgeKeys[edgeKey];
                result.AddRange(keysAlongPath.ToList());
            }
            return result.ToHashSet();
        }

        /// <summary>
        /// Gets all keys that lie along the given path. Does not include the
        /// starting point.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public HashSet<string> GetKeysAlongPath(IList<GridPoint> path)
        {
            var result = new HashSet<string>();
            for (int i = 0; i < path.Count; i++)
            {
                var point = path[i];
                if (CellsWithKeys.ContainsKey(point))
                    result.Add(CellsWithKeys[point]);
            }
            return result;
        }

        /// <summary>
        /// Gets all doors that lie along the given path. Does not include the
        /// starting point.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public HashSet<string> GetDoorsAlongPath(IList<GridPoint> path)
        {
            var result = new HashSet<string>();
            for (int i = 0; i < path.Count; i++)
            {
                var point = path[i];
                if (CellsWithDoors.ContainsKey(point))
                    result.Add(CellsWithDoors[point]);
            }
            return result;
        }

        public IList<GridPoint> GetPointNeighbors(
            GridPoint point,
            SortedDictionary<string, string> keysCollected,
            GridPoint specificNeighborToAllow = null,
            bool ignoreDoors = false)
        {
            var neighbors = new List<GridPoint>();
            var candidates = new List<GridPoint>()
                {
                    point.MoveLeft(1),
                    point.MoveRight(1),
                    point.MoveUp(1),
                    point.MoveDown(1)
                };
            foreach (var candidate in candidates)
            {
                if (!MazeCells.ContainsKey(candidate))
                    continue;
                var candidateCell = MazeCells[candidate];
                bool checkIfCanCenterCell = !(
                    specificNeighborToAllow != null
                    && specificNeighborToAllow.Equals(candidate));
                if (checkIfCanCenterCell)
                {
                    if (!GetCanEnterCell(candidate, keysCollected, ignoreDoors))
                        continue;
                }
                neighbors.Add(candidate);
            }
            return neighbors;
        }

        public PathResult<GridPoint> GetPathToPoint(
            GridPoint startPoint, 
            GridPoint targetPoint, 
            SortedDictionary<string, string> keysCollected,
            bool forceAllowToEnterTarget = false,
            bool ignoreDoors = false)
        {
            IList<GridPoint> GetNeighbors(GridPoint point)
            {
                GridPoint specificNeighbor = forceAllowToEnterTarget ? targetPoint : null;
                return GetPointNeighbors(
                    point: point,
                    keysCollected: keysCollected,
                    specificNeighborToAllow: specificNeighbor,
                    ignoreDoors: ignoreDoors);
            }

            int Heuristic(GridPoint currentPoint)
            {
                return GridPoint.GetManhattanDistance(currentPoint, targetPoint);
            }

            var pathResult = AStar.GetPath(
                startPoint: startPoint,
                endPoint: targetPoint,
                Heuristic: Heuristic,
                GetNeighbors: GetNeighbors,
                GetEdgeCost: (GridPoint p1, GridPoint p2) => { return 1; });

            return pathResult;
        }

        public int GetAverageManhattanDistanceBetweenKeys()
        {
            int totalManhattanDistance = 0;
            int totalPairs = 0;
            var keyCellList = KeyCells.Select(kvp => kvp.Value).ToList();
            for (int i = 0; i < keyCellList.Count; i++)
            {
                for (int j = i+1; j < keyCellList.Count; j++)
                {
                    totalManhattanDistance += GridPoint.GetManhattanDistance(keyCellList[i], keyCellList[j]);
                    totalPairs++;
                }
            }
            if (totalPairs == 0)
                return 0;
            return totalManhattanDistance / totalPairs;
        }

    }
}
