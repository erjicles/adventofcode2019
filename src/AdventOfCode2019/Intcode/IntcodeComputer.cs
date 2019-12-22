﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AdventOfCode2019.Intcode
{
    public static class IntcodeComputer
    {
        /// <summary>
        /// Parses a command (containing the opcode and parameter modes), 
        /// and returns these separately as an array.
        /// The first element of the array is the opcode
        /// The subsequent elements are the parameter modes for the parameters.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public static int[] ParseCommand(int command)
        {
            var commandString = command.ToString();
            if (commandString.Length == 1
                || commandString.Length == 2)
            {
                return new int[] { command };
            }
            else if (commandString.Length > 2)
            {
                // Parameter modes are stored in the same value as the 
                // instruction's opcode. The opcode is a two-digit number 
                // based only on the ones and tens digit of the value, that is, 
                // the opcode is the rightmost two digits of the first value in 
                // an instruction. Parameter modes are single digits, one per 
                // parameter, read right-to-left from the opcode: the first 
                // parameter's mode is in the hundreds digit, the second 
                // parameter's mode is in the thousands digit, the third 
                // parameter's mode is in the ten-thousands digit, and so on. 
                // Any missing modes are 0.
                var result = new List<int>();
                result.Add(int.Parse(commandString.Substring(commandString.Length - 2, 2)));
                for (int i = commandString.Length - 3; i >= 0; i--)
                {
                    result.Add(int.Parse(commandString[i].ToString()));
                }
                return result.ToArray();
            }
            throw new Exception($"Invalid command {command}");
        }

        public static int GetParameterMode(int parameterNumber, int[] parsedCommand)
        {
            if (parameterNumber >= parsedCommand.Length)
                return 0;
            return parsedCommand[parameterNumber];
        }

        public static int GetParameterValue(
            int parameterIndex, 
            int parameterNumber,
            int[] parsedCommand,
            int[] program)
        {
            // Each parameter of an instruction is handled based on its 
            // parameter mode. Right now, your ship computer already 
            // understands parameter mode 0, position mode, which causes the 
            // parameter to be interpreted as a position - if the parameter is 
            // 50, its value is the value stored at address 50 in memory. 
            // Until now, all parameters have been in position mode.

            // Now, your ship computer will also need to handle parameters in 
            // mode 1, immediate mode. In immediate mode, a parameter is 
            // interpreted as a value - if the parameter is 50, its value is 
            // simply 50.
            var parameterMode = GetParameterMode(parameterNumber, parsedCommand);
            var parameterValue = program[parameterIndex];
            if (parameterMode == 0)
            {
                // Interpret as position
                return program[parameterValue];
            }
            else if (parameterMode == 1)
            {
                // Interpret as literal
                return parameterValue;
            }
            else
            {
                throw new Exception($"Invalid parameter mode {parameterMode}");
            }
        }

        public static int GetUserInput()
        {
            bool isValidInput = false;
            while (!isValidInput)
            {
                Console.WriteLine("---->Please input an integer:");
                var userInput = Console.ReadLine();
                if (int.TryParse(userInput, out int result))
                {
                    isValidInput = true;
                    return result;
                }
                else
                {
                    Console.WriteLine($"Invalid user input: {userInput}");
                }
            }
            throw new Exception("This code should never be reached");
        }

        public static int[] RunProgram(int[] inputProgram)
        {
            var result = new int[inputProgram.Length];
            Array.Copy(inputProgram, result, inputProgram.Length);
            int position = 0;
            while (result[position] != 99)
            {
                var parsedCommand = ParseCommand(result[position]);
                var opcode = parsedCommand[0];
                if (opcode == 1)
                {
                    // Add param1 + param2, store in param3
                    var val1 = GetParameterValue(position + 1, 1, parsedCommand, result);
                    var val2 = GetParameterValue(position + 2, 2, parsedCommand, result);
                    result[result[position + 3]] = val1 + val2;
                    position += 4;
                }
                else if (opcode == 2)
                {
                    // Multiply param1 * param2, store in param3
                    var val1 = GetParameterValue(position + 1, 1, parsedCommand, result);
                    var val2 = GetParameterValue(position + 2, 2, parsedCommand, result);
                    result[result[position + 3]] = val1 * val2;
                    position += 4;
                }
                else if (opcode == 3)
                {
                    int input = GetUserInput();
                    int storePosition = result[position + 1];
                    result[storePosition] = input;
                    position += 2;
                }
                else if (opcode == 4)
                {
                    var val1 = GetParameterValue(position + 1, 1, parsedCommand, result);
                    Console.WriteLine($"---->Output value: {val1}");
                    position += 2;
                }
                else if (opcode != 99)
                {
                    throw new Exception($"Invalid opcode {result[position]} at position {position}");
                }
            }
            return result;
        }

        public static int[] ReadProgramFromFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new Exception($"Cannot locate file {filePath}");
            }
            var inputText = File.ReadAllText(filePath);
            return inputText.Split(",").Select(v => int.Parse(v)).ToArray();
        }
    }
}