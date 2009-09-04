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
    public class LongArrayFieldType : FudgeFieldType<long[]>
    {
        public static readonly LongArrayFieldType Instance = new LongArrayFieldType();

        public LongArrayFieldType()
            : base(FudgeTypeDictionary.LONG_ARRAY_TYPE_ID, true, 0)
        {
        }

        public override int GetVariableSize(long[] value, IFudgeTaxonomy taxonomy)
        {
            return value.Length * 8;
        }

        public override long[] ReadTypedValue(BinaryReader input, int dataSize) //throws IOException
        {
            int nLongs = dataSize / 8;
            long[] result = new long[nLongs];
            for (int i = 0; i < nLongs; i++)
            {
                result[i] = input.ReadInt64();
            }
            return result;
        }

        public override void WriteValue(BinaryWriter output, long[] value, IFudgeTaxonomy taxonomy, short taxonomyId)  //throws IOException
        {
            foreach (long l in value)
            {
                output.Write(l);
            }
        }
    }
}
