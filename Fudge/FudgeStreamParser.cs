/*
 * Copyright (C) 2009 - 2010 by OpenGamma Inc. and other contributors.
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
using Fudge.Encodings;
using Fudge.Util;

namespace Fudge
{
    /// <summary>
    /// A parser for <see cref="FudgeMsg"/> instances which uses a <see cref="FudgeEncodedStreamReader"/>.
    /// </summary>
    public class FudgeStreamParser
    {
        private readonly FudgeContext fudgeContext;

        /// <summary>
        /// Constructs a new <see cref="FudgeStreamParser"/>.
        /// </summary>
        /// <param name="fudgeContext"></param>
        public FudgeStreamParser(FudgeContext fudgeContext)
        {
            if (fudgeContext == null)
            {
                throw new ArgumentNullException("fudgeContext", "Must provide a fudge context.");
            }
            this.fudgeContext = fudgeContext;
        }

        /// <summary>
        /// Gets the <see cref="FudgeContext"/> used by the parser.
        /// </summary>
        public FudgeContext FudgeContext
        {
            get { return fudgeContext; }
        }

        /// <summary>
        /// Parses a given stream.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public FudgeMsgEnvelope Parse(Stream stream)
        {
            return Parse(new FudgeBinaryReader(stream));
        }

        /// <summary>
        /// Parses data from a given <see cref="BinaryReader"/>.
        /// </summary>
        /// <param name="binaryReader"></param>
        /// <returns></returns>
        public FudgeMsgEnvelope Parse(BinaryReader binaryReader)
        {
            FudgeEncodedStreamReader reader = new FudgeEncodedStreamReader(FudgeContext);       // TODO t0rx 2009-11-12 -- In Fudge-Java this is retrieved from the context
            reader.Reset(binaryReader);
            FudgeStreamElement element = reader.MoveNext();
            if (element == FudgeStreamElement.NoElement)
            {
                return null;
            }
            if (element != FudgeStreamElement.MessageStart)
            {
                throw new ArgumentException("First element in encoding stream wasn't a message element.");
            }
            int version = reader.SchemaVersion;
            FudgeMsg msg = FudgeContext.NewMessage();
            FudgeMsgEnvelope envelope = new FudgeMsgEnvelope(msg, version);
            ProcessFields(reader, msg);
            return envelope;                                                                    // TODO t0rx 2009-11-12 -- In Fudge-Java the reader is released to the context
        }

        /**
         * @param reader
         * @param msg
         */
        protected void ProcessFields(IFudgeStreamReader reader, FudgeMsg msg)
        {
            while (reader.HasNext)
            {
                FudgeStreamElement element = reader.MoveNext();
                switch (element)
                {
                    case FudgeStreamElement.SimpleField:
                        msg.Add(reader.FieldName, reader.FieldOrdinal, reader.FieldType, reader.FieldValue);
                        break;
                    case FudgeStreamElement.SubmessageFieldStart:
                        FudgeMsg subMsg = FudgeContext.NewMessage();
                        msg.Add(reader.FieldName, reader.FieldOrdinal, subMsg);
                        ProcessFields(reader, subMsg);
                        break;
                    case FudgeStreamElement.SubmessageFieldEnd:
                    case FudgeStreamElement.MessageEnd:
                        return;
                }
            }
        }
    }
}
