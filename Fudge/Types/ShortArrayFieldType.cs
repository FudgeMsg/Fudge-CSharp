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
    public class ShortArrayFieldType : FudgeFieldType<short[]>
    {
        public static readonly ShortArrayFieldType Instance = new ShortArrayFieldType();

        public ShortArrayFieldType()
            : base(FudgeTypeDictionary.SHORT_ARRAY_TYPE_ID, true, 0)
        {
        }

        public override int GetVariableSize(short[] value, IFudgeTaxonomy taxonomy)
        {
            return value.Length * 2;
        }

        public override short[] ReadTypedValue(BinaryReader input, int dataSize) //throws IOException
        {
            int nShorts = dataSize / 2;
            short[] result = new short[nShorts];
            for (int i = 0; i < nShorts; i++)
            {
                result[i] = input.ReadInt16();
            }
            return result;
        }

        public override void WriteValue(BinaryWriter output, short[] value, IFudgeTaxonomy taxonomy, short taxonomyId)  //throws IOException
        {
            foreach (short s in value)
            {
                output.Write(s);
            }
        }
    }
}
