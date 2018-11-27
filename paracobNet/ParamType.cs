﻿using System;
using System.Collections.Generic;
using System.Text;

namespace paracobNet
{
    public enum ParamType : byte
    {
        boolean = 1,
        int16 = 4,
        int32 = 6,
        uint32 = 7,
        float32 = 8,
        uint32_2 = 9,
        str = 10,
        array = 11,
        structure = 12
    }
}