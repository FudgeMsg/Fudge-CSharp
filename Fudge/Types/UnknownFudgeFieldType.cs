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
    /// A type class for handling variable sized field values where the type
    /// isn't available in the current <see cref="FudgeTypeDictionary"/>.
    /// </summary>
    public class UnknownFudgeFieldType : FudgeFieldType<UnknownFudgeFieldValue>
    {

        /// <summary>
        /// Creates a new placeholder for a type ID not available in the current dictionary.
        /// </summary>
        /// <param name="typeId"></param>
        public UnknownFudgeFieldType(int typeId)
            : base(typeId, true, 0)
        {
        }

        /// <inheritdoc cref="Fudge.FudgeFieldType.GetVariableSize(System.Object,Fudge.Taxon.IFudgeTaxonomy)" />
        public override int GetVariableSize(UnknownFudgeFieldValue value, IFudgeTaxonomy taxonomy)
        {
            return value.Contents.Length;
        }

        /// <inheritdoc/>
        public override UnknownFudgeFieldValue ReadTypedValue(BinaryReader input, int dataSize)
        {
            byte[] contents = new byte[dataSize];
            input.Read(contents, 0, dataSize);
            return new UnknownFudgeFieldValue(contents, this);
        }

        /// <inheritdoc/>
        public override void WriteValue(BinaryWriter output, UnknownFudgeFieldValue value)
        {
            output.Write(value.Contents);
        }
    }
}
