/*
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
using Fudge.Util;
using System.Diagnostics;
using Fudge.Taxon;

namespace Fudge.Encodings
{
    public class FudgeEncodedStreamReader : IFudgeStreamReader
    {
        // Injected Inputs:
        private BinaryReader reader;
        private readonly FudgeContext fudgeContext;
        // Runtime State:
        private readonly Stack<MessageProcessingState> processingStack = new Stack<MessageProcessingState>();
        private FudgeStreamElement currentElement;
        private IFudgeTaxonomy taxonomy;
        // Set for the envelope
        private int processingDirectives;
        private int schemaVersion;
        private short taxonomyId;
        private int envelopeSize;

        // Set for each non-sub-msg field
        private FudgeFieldType fieldType;
        private int? fieldOrdinal;
        private string fieldName;
        private object fieldValue;

        public FudgeEncodedStreamReader(FudgeContext fudgeContext)
        {
            if (fudgeContext == null)
            {
                throw new ArgumentNullException("fudgeContext");
            }
            this.fudgeContext = fudgeContext;
        }

        public FudgeEncodedStreamReader(FudgeContext fudgeContext, BinaryReader reader)
            : this(fudgeContext)
        {
            Reset(reader);
        }

        protected BinaryReader Reader
        {
            get
            {
                return reader;
            }
        }

        /**
         * Reset the state of this parser for a new message.
         * This method is primarily designed so that instances can be pooled to minimize
         * object creation in performance sensitive code.
         * 
         * @param dataInput
         */
        public void Reset(BinaryReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader", "Must provide a DataInput to consume data from.");
            }
            this.reader = reader;
            currentElement = FudgeStreamElement.NoElement;
            processingStack.Clear();

            processingDirectives = 0;
            schemaVersion = 0;
            taxonomyId = 0;
            envelopeSize = 0;

            fieldType = null;
            fieldOrdinal = null;
            fieldName = null;
            fieldValue = null;
        }

        public void Reset(Stream inputStream)
        {
            if (inputStream == null)
            {
                throw new ArgumentNullException("inputStream", "Must provide a Stream to consume data from.");
            }
            Reset(new FudgeBinaryReader(inputStream));
        }

        /**
         * @return the fudgeContext
         */
        public FudgeContext FudgeContext
        {
            get
            {
                return fudgeContext;
            }
        }

        public FudgeStreamElement MoveNext()
        {
            try
            {
                if (processingStack.Count == 0)
                {
                    // Must be an envelope.
                    ConsumeMessageEnvelope();
                }
                else if (IsEndOfSubMessage)
                {
                    currentElement = FudgeStreamElement.SubmessageFieldEnd;
                    fieldName = null;
                    fieldOrdinal = null;
                    fieldType = null;
                }
                else
                {
                    ConsumeFieldData();
                }
            }
            catch (IOException ioe)
            {
                throw new FudgeRuntimeException("Unable to consume data", ioe);
            }
            Debug.Assert(currentElement != FudgeStreamElement.NoElement);
            return currentElement;
        }

        /**
         * @return
         */
        protected bool IsEndOfSubMessage
        {
            get
            {
                if (processingStack.Count == 1)
                {
                    return false;
                }
                MessageProcessingState processingState = processingStack.Peek();
                if (processingState.Consumed >= processingState.MessageSize)
                {
                    processingStack.Pop();
                    processingStack.Peek().Consumed += processingState.Consumed;
                    return true;
                }
                return false;
            }
        }

        /**
         * 
         */
        protected void ConsumeFieldData() //throws IOException
        {
            sbyte fieldPrefix = Reader.ReadSByte();
            int typeId = Reader.ReadByte();
            int nRead = 2;
            bool fixedWidth = FudgeFieldPrefixCodec.IsFixedWidth(fieldPrefix);
            bool hasOrdinal = FudgeFieldPrefixCodec.HasOrdinal(fieldPrefix);
            bool hasName = FudgeFieldPrefixCodec.HasName(fieldPrefix);

            int? ordinal = null;
            if (hasOrdinal)
            {
                ordinal = Reader.ReadInt16();
                nRead += 2;
            }

            string name = null;
            if (hasName)
            {
                int nameSize = Reader.ReadByte();
                nRead++;
                name = ModifiedUTF8Util.ReadString(Reader, nameSize);
                nRead += nameSize;
            }
            else if (ordinal != null)
            {
                if (Taxonomy != null)
                {
                    name = Taxonomy.GetFieldName((short)ordinal.Value);
                }
            }

            FudgeFieldType type = FudgeContext.TypeDictionary.GetByTypeId(typeId);
            if (type == null)
            {
                if (fixedWidth)
                {
                    throw new FudgeRuntimeException("Unknown fixed width type " + typeId + " for field " + ordinal + ":" + name + " cannot be handled.");
                }
                type = FudgeContext.TypeDictionary.GetUnknownType(typeId);
            }

            int varSize = 0;
            if (!fixedWidth)
            {
                int varSizeBytes = FudgeFieldPrefixCodec.GetFieldWidthByteCount(fieldPrefix);
                switch (varSizeBytes)
                {
                    case 0: varSize = 0; break;
                    case 1: varSize = Reader.ReadByte(); nRead += 1; break;
                    case 2: varSize = Reader.ReadInt16(); nRead += 2; break;
                    case 4: varSize = Reader.ReadInt32(); nRead += 4; break;
                    default:
                        throw new FudgeRuntimeException("Illegal number of bytes indicated for variable width encoding: " + varSizeBytes);
                }
            }

            fieldName = name;
            fieldOrdinal = ordinal;
            fieldType = type;
            MessageProcessingState currMsgProcessingState = processingStack.Peek();
            currMsgProcessingState.Consumed += nRead;
            if (typeId == FudgeTypeDictionary.FUDGE_MSG_TYPE_ID)
            {
                currentElement = FudgeStreamElement.SubmessageFieldStart;
                fieldValue = null;
                MessageProcessingState subState = new MessageProcessingState();
                subState.MessageSize = varSize;
                subState.Consumed = 0;
                processingStack.Push(subState);
            }
            else
            {
                currentElement = FudgeStreamElement.SimpleField;
                fieldValue = ReadFieldValue(Reader, fieldType, varSize);
                if (fixedWidth)
                {
                    currMsgProcessingState.Consumed += type.FixedSize;
                }
                else
                {
                    currMsgProcessingState.Consumed += varSize;
                }
            }
        }

        public static object ReadFieldValue(
            BinaryReader br,
            FudgeFieldType type,
            int varSize) //throws IOException
        {
            Debug.Assert(type != null);
            Debug.Assert(br != null);

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

            return type.ReadValue(br, varSize);
        }

        /**
         * 
         */
        protected void ConsumeMessageEnvelope() //throws IOException
        {
            currentElement = FudgeStreamElement.MessageEnvelope;
            processingDirectives = Reader.ReadByte();
            schemaVersion = Reader.ReadByte();
            taxonomyId = Reader.ReadInt16();
            envelopeSize = Reader.ReadInt32();
            if (FudgeContext.TaxonomyResolver != null)
            {
                IFudgeTaxonomy taxonomy = FudgeContext.TaxonomyResolver.ResolveTaxonomy(taxonomyId);
                this.taxonomy = taxonomy;
            }
            MessageProcessingState processingState = new MessageProcessingState();
            processingState.Consumed = 8;
            processingState.MessageSize = envelopeSize;
            processingStack.Push(processingState);
        }

        public bool HasNext
        {
            get
            {
                if (processingStack.Count > 1)
                {
                    // Always have at least one more.
                    return true;
                }
                else if (processingStack.Count == 1)
                {
                    MessageProcessingState messageProcessingState = processingStack.Peek();
                    return messageProcessingState.Consumed < messageProcessingState.MessageSize;
                }
                else
                {
                    // Always have the envelope to read.
                    return true;
                }
            }
        }

        /**
         * @return the currentElement
         */
        public FudgeStreamElement CurrentElement
        {
            get
            {
                return currentElement;
            }
        }

        /**
         * @return the processingDirectives
         */
        public int ProcessingDirectives
        {
            get
            {
                return processingDirectives;
            }
        }

        /**
         * @return the schemaVersion
         */
        public int SchemaVersion
        {
            get
            {
                return schemaVersion;
            }
        }

        /**
         * @return the taxonomy
         */
        public int TaxonomyId
        {
            get
            {
                return taxonomyId;
            }
        }

        /**
         * @return the envelopeSize
         */
        public int EnvelopeSize
        {
            get
            {
                return envelopeSize;
            }
        }

        /**
         * @return the fieldType
         */
        public FudgeFieldType FieldType
        {
            get
            {
                return fieldType;
            }
        }

        /**
         * @return the fieldOrdinal
         */
        public int? FieldOrdinal
        {
            get
            {
                return fieldOrdinal;
            }
        }

        /**
         * @return the fieldName
         */
        public string FieldName
        {
            get
            {
                return fieldName;
            }
        }

        /**
         * @return the fieldValue
         */
        public object FieldValue
        {
            get
            {
                return fieldValue;
            }
        }

        /**
         * @return the taxonomy
         */
        public IFudgeTaxonomy Taxonomy
        {
            get
            {
                return taxonomy;
            }
        }

        private class MessageProcessingState
        {
            public int MessageSize;
            public int Consumed;
        }
    }
}
