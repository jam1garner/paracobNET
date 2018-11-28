﻿using System;
using paracobNet;
using System.Collections.Generic;
using System.IO;

namespace Param2Txt
{
    class Program
    {
        static void Main(string[] args)
        {
            string input = "";
            string output = "output.txt";
            for (int i = 0; i < args.Length; i++)
            {
                if (File.Exists(args[i]))
                    input = args[i];
                else if (args[i] == "-o")
                    output = args[++i];
                else
                {
                    Console.WriteLine("usage: Param2Txt.exe [input]");
                    Console.WriteLine("  optional: -o [output]");
                    return;
                }
            }
            Console.WriteLine("Initializing...");
            ParamFile file = new ParamFile(input);
            Console.WriteLine("Writing...");
            File.WriteAllLines(output, RepresentParam(file.Root as ParamStruct).ToArray());
        }

        static List<string> RepresentStruct(ParamStruct param)
        {
            List<string> list = new List<string>();
            list.Add("(" + param.TypeKey.ToString() + ")[" + param.Nodes.Length + "]");
            foreach (var node in param.Nodes)
            {
                List<string> nodeRep = RepresentParam(node.Node);
                nodeRep[0] = "<0x" + node.Hash.ToString("x8") + ">" + nodeRep[0];
                nodeRep[nodeRep.Count - 1] += ",";
                foreach (var line in nodeRep)
                    list.Add(line);
            }
            return list;
        }

        static List<string> RepresentArray(ParamArray param)
        {
            List<string> list = new List<string>();
            list.Add("(" + param.TypeKey.ToString() + ")[" + param.Nodes.Length + "]");
            foreach (var node in param.Nodes)
            {
                List<string> nodeRep = RepresentParam(node);
                nodeRep[nodeRep.Count - 1] += ",";
                foreach (var line in nodeRep)
                    list.Add(line);
            }
            return list;
        }

        static string RepresentValue(ParamValue param)
        {
            string str = "(" + param.TypeKey.ToString() + ")";
            switch (param.TypeKey)
            {
                case ParamType.boolean:
                    str += (bool)param.Value;
                    break;
                case ParamType.int16:
                    str += (short)param.Value;
                    break;
                case ParamType.int32:
                    str += (int)param.Value;
                    break;
                case ParamType.uint32:
                    str += (uint)param.Value;
                    break;
                case ParamType.float32:
                    str += (float)param.Value;
                    break;
                case ParamType.uint32_2:
                    str += (uint)param.Value;
                    break;
                case ParamType.str:
                    str += (string)param.Value;
                    break;
            }
            return str;
        }

        static List<string> RepresentParam(IParam param)
        {
            if (param is ParamValue)
                return new List<string> { RepresentValue(param as ParamValue) };

            List<string> list;
            if (param is ParamArray)
                list = RepresentArray(param as ParamArray);
            else
                list = RepresentStruct(param as ParamStruct);

            if (list.Count > 0)
                list[0] = list[0] + " {";
            for (int i = 1; i < list.Count; i++)
                list[i] = "\t" + list[i];
            list.Add("}");
            return list;
        }
    }
}