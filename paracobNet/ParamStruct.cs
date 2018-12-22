﻿using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace paracobNET
{
    public class ParamStruct : IParam
    {
        public ParamType TypeKey { get; } = ParamType.@struct;
        public uint ID { get; set; }
        public Dictionary<Hash40, IParam> Nodes { get; set; }

        internal void Read()
        {
            var reader = ParamFile.Reader;
            uint startPos = (uint)reader.BaseStream.Position - 1;
            uint size = reader.ReadUInt32();
            Nodes = new Dictionary<Hash40, IParam>();

            uint StructRefOffset = reader.ReadUInt32();
            if (ParamFile.StructOffsets.Contains(StructRefOffset))
            {
                ID = (uint)ParamFile.StructOffsets.IndexOf(StructRefOffset);
            }
            else
            {
                ID = (uint)ParamFile.StructOffsets.Count;
                ParamFile.StructOffsets.Add(StructRefOffset);
            }
            reader.BaseStream.Seek(StructRefOffset + ParamFile.RefStart, SeekOrigin.Begin);
            Dictionary<uint, uint> pairs = new Dictionary<uint, uint>();
            for (int i = 0; i < size; i++)
                pairs.Add(reader.ReadUInt32(), reader.ReadUInt32());
            var hashIndeces = pairs.Keys.ToList();
            hashIndeces.Sort();
            for (int i = 0; i < size; i++)
            {
                var key = hashIndeces[i];
                reader.BaseStream.Seek(startPos + pairs[key], SeekOrigin.Begin);
                Nodes.Add(ParamFile.DisasmHashTable[key], Util.ReadParam());
            }
        }
        internal void Write()
        {
            var paramWriter = ParamFile.WriterParam;
            var refWriter = ParamFile.WriterRef;
            uint[] offsets = new uint[Nodes.Count];
            long paramStartPos = paramWriter.BaseStream.Position - 1;
            long refStartPos = refWriter.BaseStream.Position;

            paramWriter.Write(Nodes.Count);
            paramWriter.Write((uint)refWriter.BaseStream.Position);

            List<Hash40> sortedHashes = Nodes.Keys.ToList();
            sortedHashes.Sort();

            //allocate space for the entire node's contents first
            //so we can generate offsets when each one is assembled
            //THIS LEAVES NO ROOM FOR STRINGS
            refWriter.BaseStream.Seek(Nodes.Count * 8, SeekOrigin.Current);
            for (int i = 0; i < Nodes.Count; i++)
            {
                offsets[i] = (uint)(paramWriter.BaseStream.Position - paramStartPos);
                Util.WriteParam(Nodes[sortedHashes[i]]);
            }
            refWriter.BaseStream.Seek(refStartPos, SeekOrigin.Begin);
            for (int i = 0; i < Nodes.Count; i++)
            {
                refWriter.Write(ParamFile.AsmHashTable.IndexOf(sortedHashes[i]));
                refWriter.Write(offsets[i]);
            }
        }

        public IParam GetNode(uint hash)
        {
            foreach (var node in Nodes)
            {
                if (node.Key.Hash == hash)
                    return node.Value;
            }
            return null;
        }
    }
}
