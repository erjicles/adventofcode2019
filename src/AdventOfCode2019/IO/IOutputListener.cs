﻿using System;
using System.Collections.Generic;
using System.Text;

namespace AdventOfCode2019.IO
{
    public interface IOutputListener
    {
        void SendOutput(int value);
    }
}
