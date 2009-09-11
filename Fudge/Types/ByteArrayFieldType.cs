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
        public static readonly ByteArrayFieldType VariableSizedInstance = new ByteArrayFieldType();
        public static readonly ByteArrayFieldType Length4Instance = new ByteArrayFieldType(FudgeTypeDictionary.BYTE_ARR_4_TYPE_ID, 4);
        public static readonly ByteArrayFieldType Length8Instance = new ByteArrayFieldType(FudgeTypeDictionary.BYTE_ARR_8_TYPE_ID, 8);
        public static readonly ByteArrayFieldType Length16Instance = new ByteArrayFieldType(FudgeTypeDictionary.BYTE_ARR_16_TYPE_ID, 16);
        public static readonly ByteArrayFieldType Length20Instance = new ByteArrayFieldType(FudgeTypeDictionary.BYTE_ARR_20_TYPE_ID, 20);
        public static readonly ByteArrayFieldType Length32Instance = new ByteArrayFieldType(FudgeTypeDictionary.BYTE_ARR_32_TYPE_ID, 32);
        public static readonly ByteArrayFieldType Length64Instance = new ByteArrayFieldType(FudgeTypeDictionary.BYTE_ARR_64_TYPE_ID, 64);
        public static readonly ByteArrayFieldType Length128Instance = new ByteArrayFieldType(FudgeTypeDictionary.BYTE_ARR_128_TYPE_ID, 128);
        public static readonly ByteArrayFieldType Length256Instance = new ByteArrayFieldType(FudgeTypeDictionary.BYTE_ARR_256_TYPE_ID, 256);
        public static readonly ByteArrayFieldType Length512Instance = new ByteArrayFieldType(FudgeTypeDictionary.BYTE_ARR_512_TYPE_ID, 512);

        public ByteArrayFieldType()
            : base(FudgeTypeDictionary.BYTE_ARRAY_TYPE_ID, true, 0)
        {
        }

        public ByteArrayFieldType(byte typeId, int length)
            : base(typeId, false, length)
        {
        }

        public static ByteArrayFieldType GetBestMatch(byte[] array)
        {
            if (array == null)
            {
                return VariableSizedInstance;
            }
            switch (array.Length)
            {
                case 4: return Length4Instance;
                case 8: return Length8Instance;
                case 16: return Length16Instance;
                case 20: return Length20Instance;
                case 32: return Length32Instance;
                case 64: return Length64Instance;
                case 128: return Length128Instance;
                case 256: return Length256Instance;
                case 512: return Length512Instance;
                default: return VariableSizedInstance;
            }
        }

        public override int GetVariableSize(byte[] value, IFudgeTaxonomy taxonomy)
        {
            return value.Length;
        }

        public override byte[] ReadTypedValue(BinaryReader input, int dataSize) //throws IOException
        {
            if (!IsVariableSize)
            {
                dataSize = FixedSize;
            }
            byte[] result = new byte[dataSize];
            input.Read(result, 0, dataSize);
            return result;
        }

        public override void WriteValue(BinaryWriter output, byte[] value, IFudgeTaxonomy taxonomy, short taxonomyId) //throws IOException
        {
            if (!IsVariableSize)
            {
                if (value.Length != FixedSize)
                {
                    throw new ArgumentException("Used fixed size type of size " + FixedSize + " but passed array of size " + value.Length);
                }
            }
            output.Write(value);
        }
    }
}
