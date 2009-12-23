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
using System.IO;
using Fudge.Taxon;
using Fudge.Util;

namespace Fudge.Types
{
    /// <summary>
    /// The type definition for an array of strings.
    /// </summary>
    public class StringArrayFieldType : FudgeFieldType<string[]>
    {
        public static readonly StringArrayFieldType Instance = new StringArrayFieldType();

        public StringArrayFieldType()
            : base(FudgeTypeDictionary.STRING_ARRAY_TYPE_ID, true, 0)
        {
        }

        public override int GetVariableSize(string[] value, IFudgeTaxonomy taxonomy)
        {
            int size = 0;
            foreach (string s in value)
            {
                size += 2;      // String length (Fudge has a max string size of 65535)
                size += ModifiedUTF8Util.ModifiedUTF8Length(s);
            }
            return size;
        }

        public override string[] ReadTypedValue(BinaryReader input, int dataSize)
        {
            var strings = new List<string>();
            int size = 0;
            while (size < dataSize)
            {
                int stringSize = input.ReadUInt16();
                size += 2;
                string s = ModifiedUTF8Util.ReadString(input, stringSize);
                size += stringSize;

                strings.Add(s);
            }
            return strings.ToArray();
        }

        public override void WriteValue(BinaryWriter output, string[] value)
        {
            foreach (string s in value)
            {
                byte[] bytes = ModifiedUTF8Util.Encoding.GetBytes(s);
                output.Write((ushort)bytes.Length);
                output.Write(bytes);
            }
        }
    }
}
