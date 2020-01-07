﻿using AdventOfCode2019.Grid;
using AdventOfCode2019.Intcode;
using AdventOfCode2019.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace AdventOfCode2019.Challenges.Day17
{
    /// <summary>
    /// Solution to the Day 17 challenge:
    /// https://adventofcode.com/2019/day/17
    /// </summary>
    public class Day17
    {
        public const string FILE_NAME = "Day17Input.txt";

        public static int GetDay17Part1Answer()
        {
            // To calibrate the cameras, you need the sum of the alignment 
            // parameters.
            // Run your ASCII program. What is the sum of the alignment 
            // parameters for the scaffold intersections?
            // Answer: 8928
            var scaffoldMap = PerformScaffoldScan();
            DrawScaffold(scaffoldMap);
            var scaffoldCells = GetScaffoldCells(scaffoldMap);
            var scaffoldIntersections = GetScaffoldIntersections(scaffoldCells);
            int alignmentParameterSum = GetCameraCalibrationNumber(scaffoldIntersections);
            return alignmentParameterSum;
        }

        public static int GetCameraCalibrationNumber(ICollection<GridPoint> scaffoldIntersections)
        {
            int alignmentParameterSum = 0;
            foreach (var point in scaffoldIntersections)
            {
                int alignmentParameter = GetAlignmentParameter(point);
                alignmentParameterSum += alignmentParameter;
            }
            return alignmentParameterSum;
        }

        public static int GetAlignmentParameter(GridPoint point)
        {
            // The first step is to calibrate the cameras by getting the 
            // alignment parameters of some well-defined points. Locate all 
            // scaffold intersections; for each, its alignment parameter is 
            // the distance between its left edge and the left edge of the 
            // view multiplied by the distance between its top edge and the 
            // top edge of the view.
            return point.X * point.Y;
        }

        public static void DrawScaffold(Dictionary<GridPoint, string> scaffoldMap)
        {
            string GetScaffoldCellString(GridPoint point)
            {
                if (scaffoldMap.ContainsKey(point))
                    return scaffoldMap[point];
                return " ";
            }
            GridHelper.DrawGrid2D(
                gridPoints: scaffoldMap.Select(kvp => kvp.Key).ToList(),
                GetPointString: GetScaffoldCellString);
        }

        public static HashSet<GridPoint> GetScaffoldIntersections(HashSet<GridPoint> scaffoldCells)
        {
            var result = new HashSet<GridPoint>();
            foreach (var scaffoldCell in scaffoldCells)
            {
                // It is only an intersection if the cells to the left, right,
                // top, and bottom are all also scaffold cells
                bool isScaffoldLeft = scaffoldCells.Contains(scaffoldCell.MoveLeft(1));
                bool isScaffoldRight = scaffoldCells.Contains(scaffoldCell.MoveRight(1));
                bool isScaffoldTop = scaffoldCells.Contains(scaffoldCell.MoveDown(1));
                bool isScaffoldBottom = scaffoldCells.Contains(scaffoldCell.MoveUp(1));
                if (isScaffoldLeft 
                    && isScaffoldRight 
                    && isScaffoldTop 
                    && isScaffoldBottom)
                {
                    result.Add(scaffoldCell);
                }
            }
            return result;
        }

        public static HashSet<GridPoint> GetScaffoldCells(Dictionary<GridPoint, string> scaffoldMap)
        {
            var result = new HashSet<GridPoint>();
            foreach (var kvp in scaffoldMap)
            {
                if (".".Equals(kvp.Value))
                    continue;
                if ("X".Equals(kvp.Value))
                    continue;
                result.Add(kvp.Key);
            }
            return result;
        }

        public static Dictionary<GridPoint, string> PerformScaffoldScan()
        {
            BigInteger[] program = GetDay17Input();
            var inputProvider = new BufferedInputProvider();
            var outputListener = new ListOutputListener();
            IntcodeComputer computer = new IntcodeComputer(inputProvider, outputListener);
            computer.LoadProgram(program);
            computer.RunProgram();
            //var programStatus = IntcodeProgramStatus.Running;
            //while (IntcodeProgramStatus.Running.Equals(programStatus)
            //    || IntcodeProgramStatus.AwaitingInput.Equals(programStatus))
            //{
            //    programStatus = computer.RunProgram();
            //}

            var scaffoldMap = ProcessScan(outputListener.Values);
            return scaffoldMap;
        }

        public static Dictionary<GridPoint, string> ProcessScan(IList<BigInteger> cameraScanOutput)
        {
            var rowStrings = new List<string>();
            var rowStringBuilder = new StringBuilder();
            foreach (int cameraOutputCode in cameraScanOutput)
            {
                // If it's a newline, then set x and y for the next line
                if (cameraOutputCode == 10)
                {
                    rowStrings.Add(rowStringBuilder.ToString());
                    rowStringBuilder.Clear();
                    continue;
                }
                string cameraOutputString = char.ConvertFromUtf32(cameraOutputCode);
                rowStringBuilder.Append(cameraOutputString);
            }
            if (rowStringBuilder.Length > 0)
                rowStrings.Add(rowStringBuilder.ToString());

            var result = ProcessScan(rowStrings);
            return result;
        }

        public static Dictionary<GridPoint, string> ProcessScan(IList<string> rowStrings)
        {
            var result = new Dictionary<GridPoint, string>();
            int y = 0;
            foreach (var rowString in rowStrings)
            {
                for (int x = 0; x < rowString.Length; x++)
                {
                    result.Add(new GridPoint(x, y), rowString.Substring(x, 1));
                }
                y++;
            }
            return result;
        }

        public static BigInteger[] GetDay17Input()
        {
            var filePath = FileHelper.GetInputFilePath(FILE_NAME);
            return IntcodeComputer.ReadProgramFromFile(filePath);
        }
    }
}
