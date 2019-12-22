﻿using System;
using System.Collections.Generic;
using System.Text;

namespace AdventOfCode2019.Challenges.Day3
{
    public class GridPoint
    {
        public int X { get; set; }
        public int Y { get; set; }

        public GridPoint()
        {
            X = 0;
            Y = 0;
        }

        public GridPoint(int x, int y)
        {
            X = x;
            Y = y;
        }

        public GridPoint MoveRight(int d)
        {
            return new GridPoint
            {
                X = this.X + d,
                Y = this.Y
            };
        }
        public GridPoint MoveLeft(int d)
        {
            return new GridPoint
            {
                X = this.X - d,
                Y = this.Y
            };
        }
        public GridPoint MoveUp(int d)
        {
            return new GridPoint
            {
                X = this.X,
                Y = this.Y + d
            };
        }
        public GridPoint MoveDown(int d)
        {
            return new GridPoint
            {
                X = this.X,
                Y = this.Y - d
            };
        }
        /// <summary>
        /// Computes the manhattan distance between two points on the grid.
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public static int GetManhattanDistance(GridPoint p1, GridPoint p2)
        {
            return Math.Abs(p1.X - p2.X)
                + Math.Abs(p1.Y - p2.Y);
        }

        // Equals, GetHashCode, and ToString() adapted from Microsoft example here:
        // https://docs.microsoft.com/en-us/dotnet/api/system.object.equals?view=netcore-3.1
        public override bool Equals(Object obj)
        {
            //Check for null and compare run-time types.
            if ((obj == null) || !this.GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                GridPoint p = (GridPoint)obj;
                return (X == p.X) && (Y == p.Y);
            }
        }

        public override int GetHashCode()
        {
            var tuple = new Tuple<int, int>(X, Y);
            int hash = tuple.GetHashCode();
            return hash;
        }

        public override string ToString()
        {
            return String.Format("GridPoint({0}, {1})", X, Y);
        }
    }
}