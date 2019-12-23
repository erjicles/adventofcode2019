﻿using System;
using System.Collections.Generic;
using System.Text;

namespace AdventOfCode2019.Intcode
{
    public class ConsoleOutputListener : IOutputListener
    {
        public void SendOutput(int value)
        {
            Console.WriteLine($"---->Output value: {value}");
        }
    }
}