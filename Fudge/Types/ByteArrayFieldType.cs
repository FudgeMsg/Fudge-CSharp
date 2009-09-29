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

        public override object Minimize(object value, ref FudgeFieldType type)
        {
            if (value == null) return value;

            byte[] array = value as byte[];
            if (array == null)
                throw new ArgumentException("value must be a byte array");

            // Any size can be minimized to Indicator
            if (array.Length == 0)
            {
                type = IndicatorFieldType.Instance;
                return IndicatorType.Instance;
            }

            if (!IsVariableSize)
            {
                // We're already fixed, so no further minimization possible
                return value;
            }

            switch (array.Length)
            {
                case 4:
                    type = Length4Instance;
                    break;
                case 8:
                    type = Length8Instance;
                    break;
                case 16:
                    type = Length16Instance;
                    break;
                case 20:
                    type = Length20Instance;
                    break;
                case 32: 
                    type = Length32Instance;
                    break;
                case 64:
                    type = Length64Instance;
                    break;
                case 128: 
                    type = Length128Instance;
                    break;
                case 256: 
                    type = Length256Instance;
                    break;
                case 512: 
                    type = Length512Instance;
                    break;
                default:
                    // Have to use variable-sized
                    break;
            }
            return value;
        }

        public override object ConvertValueFrom(object value)
        {
            if (value == IndicatorType.Instance)
                return new byte[0];

            return base.ConvertValueFrom(value);
        }
    }
}
