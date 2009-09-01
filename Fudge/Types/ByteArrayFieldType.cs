/**
 * Copyright (C) 2009 - 2009 by OpenGamma Inc.
 *
 * Please see distribution for license.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenGamma.Fudge.Taxon;
using System.IO;

namespace OpenGamma.Fudge.Types
{
    /// <summary>
    /// The type definition for a byte array.
    /// </summary>
    public class ByteArrayFieldType : FudgeFieldType<byte[]>
    {
        public static readonly ByteArrayFieldType Instance = new ByteArrayFieldType();

        public ByteArrayFieldType()
            : base(FudgeTypeDictionary.BYTE_ARRAY_TYPE_ID, true, 0)
        {
        }

        public override int GetVariableSize(byte[] value, IFudgeTaxonomy taxonomy)
        {
            return value.Length;
        }

        public override byte[] ReadTypedValue(BinaryReader input, int dataSize) //throws IOException
        {
            byte[] result = new byte[dataSize];
            input.Read(result, 0, dataSize);
            return result;
        }

        public override void WriteValue(BinaryWriter output, byte[] value, IFudgeTaxonomy taxonomy, short taxonomyId) //throws IOException
        {
            output.Write(value);
        }
    }
}
