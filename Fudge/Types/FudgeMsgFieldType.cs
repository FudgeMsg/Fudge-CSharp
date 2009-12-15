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
using System.Diagnostics;

namespace Fudge.Types
{
    /// <summary>
    /// The type definition for a sub-message in a hierarchical message format.
    /// </summary>
    public class FudgeMsgFieldType : FudgeFieldType<FudgeMsg>
    {
        public static readonly FudgeMsgFieldType Instance = new FudgeMsgFieldType();

        FudgeMsgFieldType()
            : base(FudgeTypeDictionary.FUDGE_MSG_TYPE_ID, true, 0)
        {
        }

        /// <inheritdoc cref="Fudge.FudgeFieldType.GetVariableSize(System.Object,Fudge.Taxon.IFudgeTaxonomy)" />
        public override int GetVariableSize(FudgeMsg value, IFudgeTaxonomy taxonomy)
        {
            return value.GetSize(taxonomy);
        }

        /// <inheritdoc cref="Fudge.FudgeFieldType{TValue}.ReadTypedValue(BinaryReader,int,Fudge.FudgeTypeDictionary)" />
        public override FudgeMsg ReadTypedValue(BinaryReader input, int dataSize, FudgeTypeDictionary typeDictionary) //throws IOException
        {
            FudgeMsg msg = new FudgeMsg();
            // REVIEW kirk 2009-09-01 -- This is right. We have to use the same taxonomy,
            // so the parent taxonomy resolver will be fixed up later on.
            int nRead = FudgeStreamDecoder.ReadMsgFields(input, dataSize, typeDictionary, null, msg);
            Debug.Assert(dataSize == nRead, "Sub-message reading failed in size; envelope unpacking will throw exception in prod.");
            return msg;
        }

        /// <inheritdoc cref="Fudge.FudgeFieldType.WriteValue(System.IO.BinaryWriter,System.Object,Fudge.Taxon.IFudgeTaxonomy)" />
        public override void WriteValue(BinaryWriter output, FudgeMsg value, IFudgeTaxonomy taxonomy) //throws IOException
        {
            FudgeStreamEncoder.WriteMsgFields(output, value, taxonomy);
        }
    }
}
