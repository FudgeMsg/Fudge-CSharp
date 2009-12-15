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
using System.IO;
using Fudge.Taxon;
using System.Diagnostics;

namespace Fudge
{

    // TODO 2009-12-11 Andrew -- I'm not too sure about having the read message functions here mixed with the lower level fudge stream components - it clutters the API space and might confuse library users. I think break into a Decoder/Encoder which does the low level stuff and a separate Reader/Writer built on top that can read whole messages

    // TODO 2009-12-14 Andrew -- e.g. Create a StreamReader on top of a BinaryReader. Create a MessageReader on top of a StreamReader for whole messages
    
    // TODO 2009-12-14 Andrew -- would this approach extend to higher layers which can process more complex C# data / RPC etc ... ?

    public class FudgeStreamDecoder
    {
        public static FudgeMsgEnvelope ReadMsg(BinaryReader br) //throws IOException
        {
            return ReadMsg(br, FudgeTypeDictionary.Instance, (TaxonomyResolver)null);
        }

        public static FudgeMsgEnvelope ReadMsg(BinaryReader br, FudgeTypeDictionary typeDictionary, ITaxonomyResolver taxonomyResolver) //throws IOException
        {
            if (taxonomyResolver == null)
                return ReadMsg(br, typeDictionary, (TaxonomyResolver)null);
            else
                return ReadMsg(br, typeDictionary, id => taxonomyResolver.ResolveTaxonomy(id));
        }

        public static FudgeMsgEnvelope ReadMsg(BinaryReader br, FudgeTypeDictionary typeDictionary, TaxonomyResolver taxonomyResolver) //throws IOException
        {
            CheckInputStream(br);
            int nRead = 0;
            /*int processingDirectives = */
            br.ReadByte();
            nRead += 1;
            int version = br.ReadByte();
            nRead += 1;
            short taxonomyId = br.ReadInt16();
            nRead += 2;
            int size = br.ReadInt32();
            nRead += 4;

            IFudgeTaxonomy taxonomy = null;
            if (taxonomyResolver != null)
            {
                taxonomy = taxonomyResolver(taxonomyId);
            }

            FudgeMsg msg = new FudgeMsg();
            // note that this is size-nRead because the size is for the whole envelope, including the header which we've already read in.
            nRead += ReadMsgFields(br, size - nRead, typeDictionary, taxonomy, msg);

            if ((size > 0) && (nRead != size))
            {
                throw new FudgeRuntimeException("Expected to read " + size + " but only had " + nRead + " in message.");      // TODO 2009-08-31 t0rx -- This is just RuntimeException in Fudge-Java
            }

            FudgeMsgEnvelope envelope = new FudgeMsgEnvelope(msg, version);
            return envelope;
        }

        public static int ReadMsgFields(BinaryReader br, int size, FudgeTypeDictionary typeDictionary, IFudgeTaxonomy taxonomy, FudgeMsg msg)   // throws IOException
        {
            if (msg == null)
            {
                throw new ArgumentNullException("msg", "Must specify a message to populate with fields.");
            }
            int nRead = 0;
            while (nRead < size)
            {
                byte fieldPrefix = br.ReadByte();
                nRead++;
                int typeId = br.ReadByte();
                nRead++;
                nRead += ReadField(br, msg, typeDictionary, fieldPrefix, typeId);
            }
            if (taxonomy != null)
            {
                msg.SetNamesFromTaxonomy(taxonomy);
            }
            return nRead;
        }


        /// <summary>
        /// Reads data about a field, and adds it to the message as a new field.
        /// </summary>
        /// <returns>The number of bytes read.</returns>
        public static int ReadField(BinaryReader br, FudgeMsg msg, FudgeTypeDictionary typeDictionary, byte fieldPrefix, int typeId) //throws IOException
        {
            CheckInputStream(br);
            int nRead = 0;

            bool fixedWidth = FudgeFieldPrefixCodec.IsFixedWidth(fieldPrefix);
            bool hasOrdinal = FudgeFieldPrefixCodec.HasOrdinal(fieldPrefix);
            bool hasName = FudgeFieldPrefixCodec.HasName(fieldPrefix);
            int varSizeBytes = 0;
            if (!fixedWidth)
            {
                varSizeBytes = (fieldPrefix << 1) >> 6;
            }

            short? ordinal = null;
            if (hasOrdinal)
            {
                ordinal = br.ReadInt16();
                nRead += 2;
            }

            String name = null;
            if (hasName)
            {
                int nameSize = br.ReadByte();
                nRead++;
                name = ModifiedUTF8Util.ReadString(br, nameSize);
                nRead += nameSize;
            }

            FudgeFieldType type = typeDictionary.GetByTypeId(typeId);
            if (type == null)
            {
                if (fixedWidth)
                {
                    throw new FudgeRuntimeException("Unknown fixed width type " + typeId + " for field " + ordinal + ":" + name + " cannot be handled.");       // TODO 2009-09-09 t0rx -- In Fudge-Java this is just RuntimeException
                }
                type = typeDictionary.GetUnknownType(typeId);
            }
            int varSize = 0;
            if (!fixedWidth)
            {
                switch (varSizeBytes)
                {
                    case 0: varSize = 0; break;
                    case 1: varSize = br.ReadByte(); nRead += 1; break;
                    case 2: varSize = br.ReadInt16(); nRead += 2; break;      // TODO 2009-08-31 t0rx -- Review whether this should be signed or not
                    // Yes, this is right. We only have 2 bits here.
                    case 3: varSize = br.ReadInt32(); nRead += 4; break;
                    default:
                        throw new FudgeRuntimeException("Illegal number of bytes indicated for variable width encoding: " + varSizeBytes);        // TODO 2009-08-31 t0rx -- In Fudge-Java this is just a RuntimeException
                }

            }
            object fieldValue = ReadFieldValue(br, type, varSize, typeDictionary);
            if (fixedWidth)
            {
                nRead += type.FixedSize;
            }
            else
            {
                nRead += varSize;
            }

            msg.Add(name, ordinal, type, fieldValue);

            return nRead;
        }

        public static object ReadFieldValue(BinaryReader br, FudgeFieldType type, int varSize, FudgeTypeDictionary typeDictionary) //throws IOException
        {
            Debug.Assert(type != null);
            Debug.Assert(br != null);
            Debug.Assert(typeDictionary != null);

            // Special fast-pass for known field types
            switch (type.TypeId)
            {
                case FudgeTypeDictionary.BOOLEAN_TYPE_ID:
                    return br.ReadBoolean();
                case FudgeTypeDictionary.SBYTE_TYPE_ID:
                    return br.ReadSByte();
                case FudgeTypeDictionary.SHORT_TYPE_ID:
                    return br.ReadInt16();
                case FudgeTypeDictionary.INT_TYPE_ID:
                    return br.ReadInt32();
                case FudgeTypeDictionary.LONG_TYPE_ID:
                    return br.ReadInt64();
                case FudgeTypeDictionary.FLOAT_TYPE_ID:
                    return br.ReadSingle();
                case FudgeTypeDictionary.DOUBLE_TYPE_ID:
                    return br.ReadDouble();
            }

            return type.ReadValue(br, varSize, typeDictionary);
        }

        protected static void CheckInputStream(BinaryReader br)
        {
            if (br == null)
            {
                throw new ArgumentNullException("Must specify a BinaryReader for processing.");
            }
        }
    }
}
