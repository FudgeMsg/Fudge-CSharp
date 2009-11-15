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
using Fudge.Util;
using System.Diagnostics;

namespace Fudge.Encodings
{
    public class FudgeEncodedStreamWriter : IFudgeStreamWriter
    {
        // TODO t0rx 2009-11-12 -- Currently using FudgeMsg to hold fields because of size, but need to do better.  See FRJ-23
        private readonly FudgeContext context;
        private BinaryWriter writer;
        private readonly Stack<FudgeMsg> messageStack = new Stack<FudgeMsg>();
        private FudgeMsg currentMessage;
        private const int EnvelopeVersion = 0;      // TODO t0rx 2009-11-12 -- Is this the Fudge encoding version, or what?

        public FudgeEncodedStreamWriter(FudgeContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            this.context = context;
        }

        public short? TaxonomyId
        {
            get;
            set;
        }

        public void Reset(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            Reset(new FudgeBinaryWriter(stream));
        }

        public void Reset(BinaryWriter writer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }
            this.writer = writer;
        }

        #region IFudgeStreamWriter Members

        public void StartSubMessage(string name, int? ordinal)
        {
            var newMsg = new FudgeMsg();
            if (currentMessage != null)
            {
                currentMessage.Add(name, ordinal, newMsg);
            }
            messageStack.Push(currentMessage);
            currentMessage = newMsg;
        }

        public void WriteField(string name, int? ordinal, FudgeFieldType type, object value)
        {
            currentMessage.Add(name, ordinal, type, value);
        }

        public void WriteFields(IEnumerable<IFudgeField> fields)
        {
            foreach (var field in fields)
            {
                currentMessage.Add(field);
            }
        }

        public void EndSubMessage()
        {
            if (messageStack.Count == 0)
            {
                throw new InvalidOperationException("Ended more sub-messages than started!");
            }

            var newCurrentMessage = messageStack.Pop();

            if (newCurrentMessage == null)
            {
                // We're back to the top so now we can write
                var env = new FudgeMsgEnvelope(currentMessage, EnvelopeVersion);
                IFudgeTaxonomy taxonomy = null;
                if ((context.TaxonomyResolver != null) && (TaxonomyId != null))
                {
                    taxonomy = context.TaxonomyResolver.ResolveTaxonomy(TaxonomyId.Value);
                }

                WriteMsg(writer, env, context.TypeDictionary, taxonomy, TaxonomyId ?? 0);
            }
            currentMessage = newCurrentMessage;
        }

        public void End()
        {
            // Noop
        }

        #endregion

        #region Main encoding stuff

        private static void WriteMsg(BinaryWriter bw, FudgeMsgEnvelope envelope)// throws IOException
        {
            WriteMsg(bw, envelope, FudgeTypeDictionary.Instance, null, 0);
        }

        private static void WriteMsg(BinaryWriter bw, FudgeMsgEnvelope envelope, FudgeTypeDictionary typeDictionary, IFudgeTaxonomy taxonomy, short taxonomyId)// throws IOException
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

        private static int WriteMsgFields(BinaryWriter bw, IFudgeFieldContainer container, IFudgeTaxonomy taxonomy) //throws IOException
        {
            int nWritten = 0;
            foreach (IFudgeField field in container.GetAllFields())
            {
                nWritten += WriteField(bw, field.Type, field.Value, field.Ordinal, field.Name, taxonomy);
            }
            return nWritten;
        }

        private static int WriteMsgEnvelopeHeader(BinaryWriter bw, int taxonomy, int messageSize, int version)// throws IOException
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

        private static int WriteField(BinaryWriter bw, FudgeFieldType type, object value, short? ordinal, string name)// throws IOException
        {
            return WriteField(bw, type, value, ordinal, name, null);
        }

        private static int WriteField(BinaryWriter bw, FudgeFieldType type,
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
            int nWritten = WriteFieldContents(bw, value, type, valueSize, type.IsVariableSize, type.TypeId, ordinal, name, taxonomy);
            return nWritten;
        }

        private static int WriteFieldContents(BinaryWriter bw, object value, FudgeFieldType type, int valueSize, bool variableSize, int typeId, short? ordinal, string name, IFudgeTaxonomy taxonomy)
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

        private static int WriteFieldValue(BinaryWriter bw, FudgeFieldType type, object value, int valueSize, IFudgeTaxonomy taxonomy) //throws IOException
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

                if (value is IFudgeFieldContainer)
                {
                    IFudgeFieldContainer subMsg = (IFudgeFieldContainer)value;
                    WriteMsgFields(bw, subMsg, taxonomy);
                }
                else
                {
                    type.WriteValue(bw, value);
                }
            }
            return nWritten;
        }

        private static void CheckOutputStream(BinaryWriter bw)
        {
            if (bw == null)
            {
                throw new ArgumentNullException("Must specify a BinaryWriter for processing.");
            }
        }

        #endregion
    }
}
