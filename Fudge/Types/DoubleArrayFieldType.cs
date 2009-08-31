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
    /// The type definition for an array of double-precision floating point numbers.
    /// </summary>
    public class DoubleArrayFieldType : FudgeFieldType<double[]>
    {
        public static readonly DoubleArrayFieldType Instance = new DoubleArrayFieldType();

        public DoubleArrayFieldType()
            : base(FudgeTypeDictionary.DOUBLE_ARRAY_TYPE_ID, true, 0)
        {
        }

        public override int GetVariableSize(double[] value, IFudgeTaxonomy taxonomy)
        {
            return value.Length * 4;
        }

        public override double[] ReadTypedValue(BinaryReader input, int dataSize, IFudgeTaxonomy taxonomy) //throws IOException
        {
            int nFloats = dataSize / 4;
            double[] result = new double[nFloats];
            for (int i = 0; i < nFloats; i++)
            {
                result[i] = input.ReadSingle();
            }
            return result;
        }

        public override void WriteValue(BinaryWriter output, double[] value, IFudgeTaxonomy taxonomy, short taxonomyId)  //throws IOException
        {
            foreach (double f in value)
            {
                output.Write(f);
            }
        }
    }
}
