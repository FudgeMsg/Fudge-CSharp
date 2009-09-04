/**
 * Copyright (C) 2009 - 2009 by OpenGamma Inc.
 *
 * Please see distribution for license.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using OpenGamma.Fudge.Taxon;

namespace OpenGamma.Fudge.Types
{
    public class IntArrayFieldType : FudgeFieldType<int[]>
    {
        public static readonly IntArrayFieldType Instance = new IntArrayFieldType();

        public IntArrayFieldType()
            : base(FudgeTypeDictionary.INT_ARRAY_TYPE_ID, true, 0)
        {
        }

        public override int GetVariableSize(int[] value, IFudgeTaxonomy taxonomy)
        {
            return value.Length * 4;
        }

        public override int[] ReadTypedValue(BinaryReader input, int dataSize) //throws IOException
        {
            int nInts = dataSize / 4;
            int[] result = new int[nInts];
            for (int i = 0; i < nInts; i++)
            {
                result[i] = input.ReadInt32();
            }
            return result;
        }

        public override void WriteValue(BinaryWriter output, int[] value, IFudgeTaxonomy taxonomy, short taxonomyId)  //throws IOException
        {
            foreach (int i in value)
            {
                output.Write(i);
            }
        }
    }
}
