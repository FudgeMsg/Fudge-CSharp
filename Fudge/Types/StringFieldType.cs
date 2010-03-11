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
using System.Text;
using Fudge.Taxon;
using System.IO;

namespace Fudge.Types
{
    /// <summary>
    /// The type definition for a UTF-8 encoded string.
    /// </summary>
    public class StringFieldType : FudgeFieldType<string>
    {
        // Note that we ignore the encoder in the BinaryReader or BinaryWriter and just go for UTF-8 anyway.
        // This is because we don't have the writer when we are in GetVariableSize.

        /// <summary>
        /// A type defintion for string data.
        /// </summary>
        public static readonly StringFieldType Instance = new StringFieldType();

        internal static readonly Encoding Encoding = Encoding.UTF8;                 // Keep here so we know we're consistent everywhere
        private const int maxStringSize = 65535;

        /// <summary>
        /// Creates a type definition for string data.
        /// </summary>
        public StringFieldType()
            : base(FudgeTypeDictionary.STRING_TYPE_ID, true, 0)
        {
        }

        /// <inheritdoc cref="Fudge.FudgeFieldType.GetVariableSize(System.Object,Fudge.Taxon.IFudgeTaxonomy)" />
        public override int GetVariableSize(string value, IFudgeTaxonomy taxonomy)
        {
            return Encoding.GetByteCount(value);
        }

        /// <inheritdoc/>
        public override string ReadTypedValue(BinaryReader input, int dataSize)
        {
            return ReadString(input, dataSize);
        }

        /// <inheritdoc/>
        public override void WriteValue(BinaryWriter output, string value)
        {
            WriteString(output, value);
        }

        internal static string ReadString(BinaryReader input, int dataSize)
        {
            var bytes = input.ReadBytes(dataSize);
            return Encoding.GetString(bytes);
        }

        internal static int WriteString(BinaryWriter output, string value)
        {
            var bytes = Encoding.GetBytes(value);
            if (bytes.Length > maxStringSize)      // Fudge has a maximum string size
                throw new FudgeRuntimeException("Encoded string too long: " + bytes.Length + " bytes");
            output.Write(bytes);
            return bytes.Length;
        }
    }
}
