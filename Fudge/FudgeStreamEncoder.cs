/**
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
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Fudge.Taxon;
using System.Diagnostics;

namespace Fudge
{
    public class FudgeStreamEncoder
    {
        public static void WriteMsg(BinaryWriter bw, FudgeMsg msg) //throws IOException
        {
            WriteMsg(bw, new FudgeMsgEnvelope(msg));
        }

        public static void WriteMsg(BinaryWriter bw, FudgeMsgEnvelope envelope)// throws IOException
        {
            WriteMsg(bw, envelope, FudgeTypeDictionary.Instance, null, 0);
        }

        public static void WriteMsg(BinaryWriter bw, FudgeMsgEnvelope envelope, FudgeTypeDictionary typeDictionary, IFudgeTaxonomy taxonomy, short taxonomyId)// throws IOException
        {
            CheckOutputStream(bw);
            if (envelope == null)
            {
                throw new ArgumentNullException("envelope", "Must provide a message envelope to output.");
            }
            if (typeDictionary == null)
            {
                throw new ArgumentNullException("typeDictionary", "Type dictionary must be provided.");
            }
            int nWritten = 0;
            int msgSize = envelope.GetSize(taxonomy);
            FudgeMsg msg = envelope.Message;
            nWritten += WriteMsgEnvelopeHeader(bw, taxonomyId, msgSize, envelope.Version);
            nWritten += WriteMsgFields(bw, msg, taxonomy);
            Debug.Assert(nWritten == msgSize, "Expected to write " + msgSize + " but actually wrote " + nWritten);
        }

        public static int WriteMsgFields(BinaryWriter bw, FudgeMsg msg, IFudgeTaxonomy taxonomy) //throws IOException
        {
            int nWritten = 0;
            foreach (IFudgeField field in msg.GetAllFields())
            {
                nWritten += WriteField(bw, field.Type, field.Value, field.Ordinal, field.Name, taxonomy);
            }
            return nWritten;
        }

        public static int WriteMsgEnvelopeHeader(BinaryWriter bw, int taxonomy, int messageSize, int version)// throws IOException
        {
            CheckOutputStream(bw);
            int nWritten = 0;

            bw.Write((byte)0); // Processing Directives
            nWritten += 1;
            bw.Write((byte)version);
            nWritten += 1;
            bw.Write((short)taxonomy);      // TODO t0rx 2009-10-04 -- This should probably be ushort, but we'll need to change throughout
            nWritten += 2;
            bw.Write(messageSize);
            nWritten += 4;
            return nWritten;
        }

        public static int WriteField(BinaryWriter bw, FudgeFieldType type, object value, short? ordinal, string name)// throws IOException
        {
            return WriteField(bw, type, value, ordinal, name, null);
        }

        public static int WriteField(BinaryWriter bw, FudgeFieldType type,
              object value, short? ordinal, string name,
              IFudgeTaxonomy taxonomy)// throws IOException
        {
            CheckOutputStream(bw);
            if (type == null)
            {
                throw new ArgumentNullException("Must provide the type of data encoded.");
            }
            if (value == null)
            {
                throw new ArgumentNullException("Must provide the value to encode.");
            }

            // First, normalize the name/ordinal bit
            if ((taxonomy != null) && (name != null))
            {
                short? ordinalFromTaxonomy = taxonomy.GetFieldOrdinal(name);
                if (ordinalFromTaxonomy != null)
                {
                    if ((ordinal != null) && !object.Equals(ordinalFromTaxonomy, ordinal))
                    {
                        // In this case, we've been provided an ordinal, but it doesn't match the
                        // one from the taxonomy. We have to assume the user meant what they were doing,
                        // and not do anything.
                    }
                    else
                    {
                        ordinal = ordinalFromTaxonomy;
                        name = null;
                    }
                }
            }

            int valueSize = type.IsVariableSize ? type.GetVariableSize(value, taxonomy) : type.FixedSize;
            int nWritten = WriteFieldContents(bw, value, type, taxonomy, valueSize, type.IsVariableSize, type.TypeId, ordinal, name);
            return nWritten;
        }

        private static int WriteFieldContents(BinaryWriter bw, object value, FudgeFieldType type, IFudgeTaxonomy taxonomy, int valueSize, bool variableSize, int typeId, short? ordinal, string name)
        {
            int nWritten = 0;

            int fieldPrefix = FudgeFieldPrefixCodec.ComposeFieldPrefix(!variableSize, valueSize, (ordinal != null), (name != null));
            bw.Write((byte)fieldPrefix);
            nWritten++;
            bw.Write((byte)typeId);
            nWritten++;
            if (ordinal != null)
            {
                bw.Write(ordinal.Value);
                nWritten += 2;
            }
            if (name != null)
            {
                int utf8size = ModifiedUTF8Util.ModifiedUTF8Length(name);
                if (utf8size > 0xFF)
                {
                    throw new ArgumentOutOfRangeException("UTF-8 encoded field name cannot exceed 255 characters. Name \"" + name + "\" is " + utf8size + " bytes encoded.");
                }
                bw.Write((byte)utf8size);
                nWritten++;
                nWritten += ModifiedUTF8Util.WriteModifiedUTF8(name, bw);
            }
            if (value != null)
            {
                Debug.Assert(type != null);
                nWritten += WriteFieldValue(bw, type, value, valueSize, taxonomy);
            }
            return nWritten;
        }

        protected static int WriteFieldValue(BinaryWriter bw, FudgeFieldType type, object value, int valueSize, IFudgeTaxonomy taxonomy) //throws IOException
        {
            // Note that we fast-path types for which at compile time we know how to handle
            // in an optimized way. This is because this particular method is known to
            // be a massive hot-spot for performance.
            int nWritten = 0;
            switch (type.TypeId)
            {
                case FudgeTypeDictionary.BOOLEAN_TYPE_ID:
                    bw.Write((bool)value);
                    nWritten = 1;
                    break;
                case FudgeTypeDictionary.SBYTE_TYPE_ID:
                    bw.Write((sbyte)value);
                    nWritten = 1;
                    break;
                case FudgeTypeDictionary.SHORT_TYPE_ID:
                    bw.Write((short)value);
                    nWritten = 2;
                    break;
                case FudgeTypeDictionary.INT_TYPE_ID:
                    bw.Write((int)value);
                    nWritten = 4;
                    break;
                case FudgeTypeDictionary.LONG_TYPE_ID:
                    bw.Write((long)value);
                    nWritten = 8;
                    break;
                case FudgeTypeDictionary.FLOAT_TYPE_ID:
                    bw.Write((float)value);
                    nWritten = 4;
                    break;
                case FudgeTypeDictionary.DOUBLE_TYPE_ID:
                    bw.Write((double)value);
                    nWritten = 8;
                    break;
            }
            if (nWritten == 0)
            {
                if (type.IsVariableSize)
                {
                    // This is correct. We read this using a .readUnsignedByte(), so we can go to
                    // 255 here.
                    if (valueSize <= 255)
                    {
                        bw.Write((byte)valueSize);
                        nWritten = valueSize + 1;
                    }
                    else if (valueSize <= short.MaxValue)
                    {
                        bw.Write((short)valueSize);
                        nWritten = valueSize + 2;
                    }
                    else
                    {
                        bw.Write((int)valueSize);
                        nWritten = valueSize + 4;
                    }
                }
                else
                {
                    nWritten = type.FixedSize;
                }
                type.WriteValue(bw, value, taxonomy);
            }
            return nWritten;
        }

        protected static void CheckOutputStream(BinaryWriter bw)
        {
            if (bw == null)
            {
                throw new ArgumentNullException("Must specify a BinaryWriter for processing.");
            }
        }
    }
}
