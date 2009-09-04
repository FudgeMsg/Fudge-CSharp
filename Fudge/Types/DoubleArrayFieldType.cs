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
            return value.Length * 8;
        }

        public override double[] ReadTypedValue(BinaryReader input, int dataSize) //throws IOException
        {
            int nDoubles = dataSize / 8;
            double[] result = new double[nDoubles];
            for (int i = 0; i < nDoubles; i++)
            {
                result[i] = input.ReadDouble();
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
