/* <!--
 * Copyright (C) 2009 - 2009 by OpenGamma Inc. and other contributors.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *     http://www.apache.org/licenses/LICENSE-2.0
 *     
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 * -->
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Fudge.Taxon;
using System.IO;

namespace Fudge.Types
{
    /// <summary>
    /// The type definition for a byte array. The Fudge encoding specification allows arrays of bytes to be handled as arbitrary
    /// lengths, or as one of a set of predefined fixed-width types for common sizes.
    /// </summary>
    public class ByteArrayFieldType : FudgeFieldType<byte[]>
    {
        /// <summary>
        /// A type defintion for an arbitrary length byte array.
        /// </summary>
        public static readonly ByteArrayFieldType VariableSizedInstance = new ByteArrayFieldType();

        /// <summary>
        /// A type definition for a 4 byte array.
        /// </summary>
        public static readonly ByteArrayFieldType Length4Instance = new ByteArrayFieldType(FudgeTypeDictionary.BYTE_ARR_4_TYPE_ID, 4);

        /// <summary>
        /// A type definition for an 8 byte array.
        /// </summary>
        public static readonly ByteArrayFieldType Length8Instance = new ByteArrayFieldType(FudgeTypeDictionary.BYTE_ARR_8_TYPE_ID, 8);

        /// <summary>
        /// A type definition for a 16 byte array.
        /// </summary>
        public static readonly ByteArrayFieldType Length16Instance = new ByteArrayFieldType(FudgeTypeDictionary.BYTE_ARR_16_TYPE_ID, 16);

        /// <summary>
        /// A type definition for a 20 byte array.
        /// </summary>
        public static readonly ByteArrayFieldType Length20Instance = new ByteArrayFieldType(FudgeTypeDictionary.BYTE_ARR_20_TYPE_ID, 20);

        /// <summary>
        /// A type definition for a 32 byte array.
        /// </summary>
        public static readonly ByteArrayFieldType Length32Instance = new ByteArrayFieldType(FudgeTypeDictionary.BYTE_ARR_32_TYPE_ID, 32);

        /// <summary>
        /// A type definition for a 64 byte array.
        /// </summary>
        public static readonly ByteArrayFieldType Length64Instance = new ByteArrayFieldType(FudgeTypeDictionary.BYTE_ARR_64_TYPE_ID, 64);

        /// <summary>
        /// A type definition for a 128 byte array.
        /// </summary>
        public static readonly ByteArrayFieldType Length128Instance = new ByteArrayFieldType(FudgeTypeDictionary.BYTE_ARR_128_TYPE_ID, 128);

        /// <summary>
        /// A type definition for a 256 byte array.
        /// </summary>
        public static readonly ByteArrayFieldType Length256Instance = new ByteArrayFieldType(FudgeTypeDictionary.BYTE_ARR_256_TYPE_ID, 256);

        /// <summary>
        /// A type definition for a 512 byte array.
        /// </summary>
        public static readonly ByteArrayFieldType Length512Instance = new ByteArrayFieldType(FudgeTypeDictionary.BYTE_ARR_512_TYPE_ID, 512);

        /// <summary>
        /// Creates a new type definition for arbitrary length byte arrays.
        /// </summary>
        public ByteArrayFieldType()
            : base(FudgeTypeDictionary.BYTE_ARRAY_TYPE_ID, true, 0)
        {
        }

        /// <summary>
        /// Creates a new type definition for a fixed length byte array. The default Fudge types have optimised typese for common lengths
        /// that produces a more compact encoding.
        /// </summary>
        /// <param name="typeId">type ID</param>
        /// <param name="length">fixed length of array</param>
        public ByteArrayFieldType(byte typeId, int length)
            : base(typeId, false, length)
        {
        }

        /// <inheritdoc />
        public override int GetVariableSize(byte[] value, IFudgeTaxonomy taxonomy)
        {
            return value.Length;
        }

        /// <inheritdoc cref="Fudge.FudgeFieldType{TValue}.ReadTypedValue(System.IO.BinaryReader,System.Int32,Fudge.FudgeTypeDictionary)" />
        public override byte[] ReadTypedValue(BinaryReader input, int dataSize, FudgeTypeDictionary typeDictionary) //throws IOException
        {
            if (!IsVariableSize)
            {
                dataSize = FixedSize;
            }
            byte[] result = new byte[dataSize];
            input.Read(result, 0, dataSize);
            return result;
        }

        /// <inheritdoc />
        public override void WriteValue(BinaryWriter output, byte[] value, IFudgeTaxonomy taxonomy) //throws IOException
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

        /// <summary>
        /// Attempts to reduce a variable length byte array to one of the standard Fudge types. The value
        /// is never modified, just the type changed.
        /// </summary>
        /// <param name="value">value to process</param>
        /// <param name="type">type definition</param>
        /// <returns>the value</returns>
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

        /// <inheritdoc />
        public override object ConvertValueFrom(object value)
        {
            if (value == IndicatorType.Instance)
                return new byte[0];

            return base.ConvertValueFrom(value);
        }
    }
}
