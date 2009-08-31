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
    /// <summary>
    /// The type definition for an array of single-precision floating point numbers.
    /// </summary>
    public class FloatArrayFieldType : FudgeFieldType<float[]>
    {
        public static readonly FloatArrayFieldType Instance = new FloatArrayFieldType();

        public FloatArrayFieldType()
            : base(FudgeTypeDictionary.FLOAT_ARRAY_TYPE_ID, true, 0)
        {
        }

        public override int GetVariableSize(float[] value, IFudgeTaxonomy taxonomy)
        {
            return value.Length * 4;
        }

        public override float[] ReadTypedValue(BinaryReader input, int dataSize, IFudgeTaxonomy taxonomy) //throws IOException
        {
            int nFloats = dataSize / 4;
            float[] result = new float[nFloats];
            for (int i = 0; i < nFloats; i++)
            {
                result[i] = input.ReadSingle();
            }
            return result;
        }

        public override void WriteValue(BinaryWriter output, float[] value, IFudgeTaxonomy taxonomy, short taxonomyId)  //throws IOException
        {
            foreach (float f in value)
            {
                output.Write(f);
            }
        }
    }
}
