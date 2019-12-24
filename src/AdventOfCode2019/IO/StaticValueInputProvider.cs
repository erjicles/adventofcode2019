﻿using System;
using System.Collections.Generic;
using System.Text;

namespace AdventOfCode2019.IO
{
    public class StaticValueInputProvider : IInputProvider
    {
        private readonly int[] _values = new int[] { };
        private int _valueIndex = 0;
        public StaticValueInputProvider(int value)
        {
            _values = new int[] { value };
        }
        public StaticValueInputProvider(int[] values)
        {
            _values = new int[values.Length];
            Array.Copy(values, _values, values.Length);
        }

        public int GetInput()
        {
            if (_values == null || _values.Length == 0)
                throw new Exception("No values defined");
            if (_valueIndex >= _values.Length)
                throw new Exception("Not enough values in list");
            var currentValueIndex = _valueIndex;
            _valueIndex++;
            return _values[currentValueIndex];
        }
        public bool HasInput()
        {
            if (_values == null || _values.Length == 0)
                return false;
            if (_valueIndex < _values.Length)
                return true;
            return false;
        }
    }
}
