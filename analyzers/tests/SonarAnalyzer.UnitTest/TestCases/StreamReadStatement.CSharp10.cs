﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Tests.Diagnostics
{
    public class StreamReadStatement
    {
        public StreamReadStatement(string fileName)
        {
            using (var stream = File.Open(fileName, FileMode.Open))
            {
                var result = new byte[stream.Length];
                (int a, int b) = (stream.Read(result, 0, (int)stream.Length), 42); // FN
            }
        }
    }
}
