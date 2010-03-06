﻿/*
 * <!--
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
 * -->
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fudge.Util
{
    /// <summary>
    /// <c>FudgeStreamPipe</c> is used to automatically push all data read from an input data stream via an <see cref="IFudgeStreamReader"/>
    /// to an output data stream via an <see cref="IFudgeStreamWriter"/>.
    /// </summary>
    public class FudgeStreamPipe
    {
        private readonly IFudgeStreamReader reader;
        private readonly IFudgeStreamWriter writer;
        private bool aborted;

        /// <summary>
        /// Constructs a new pipe from an <see cref="IFudgeStreamReader"/> to an <see cref="IFudgeStreamWriter"/>.
        /// </summary>
        /// <param name="reader"><see cref="IFudgeStreamReader"/> from which to fetch the data.</param>
        /// <param name="writer"><see cref="IFudgeStreamWriter"/> to output the data.</param>
        public FudgeStreamPipe(IFudgeStreamReader reader, IFudgeStreamWriter writer)
        {
            this.reader = reader;
            this.writer = writer;
        }

        /// <summary>
        /// MessageProcessed is fired whenever a complete message has been processed by the writer.
        /// </summary>
        public event Action MessageProcessed;

        /// <summary>
        /// Passes all elements from the <see cref="IFudgeStreamReader"/> to the <see cref="IFudgeStreamWriter"/> until the
        /// reader indicates it has no more data.
        /// </summary>
        /// <remarks>
        /// If the reader is processing an asynchronous source (e.g. a socket) then <c>Process()</c> may block whilst the
        /// reader is waiting for data.
        /// </remarks>
        public void Process()
        {
            while (!aborted && reader.HasNext)
            {
                ProcessOne();
            }
        }

        /// <summary>
        /// Stops the pipe processing any further
        /// </summary>
        public void Abort()
        {
            aborted = true;
        }

        /// <summary>
        /// Reads the next complete message from the <see cref="IFudgeStreamReader"/> and sends it to the <see cref="IFudgeStreamWriter"/>.
        /// </summary>
        public void ProcessOne()
        {
            while (!aborted && reader.HasNext)
            {
                switch (reader.MoveNext())
                {
                    case FudgeStreamElement.MessageStart:
                        writer.StartMessage();
                        break;
                    case FudgeStreamElement.MessageEnd:
                        writer.EndMessage();
                        FireMessageProcessed();
                        return;                 // We're done now
                    case FudgeStreamElement.SimpleField:
                        writer.WriteField(reader.FieldName, reader.FieldOrdinal, reader.FieldType, reader.FieldValue);
                        break;
                    case FudgeStreamElement.SubmessageFieldStart:
                        writer.StartSubMessage(reader.FieldName, reader.FieldOrdinal);
                        break;
                    case FudgeStreamElement.SubmessageFieldEnd:
                        writer.EndSubMessage();
                        break;
                    default:
                        break;      // Unknown
                }
            }
        }

        private void FireMessageProcessed()
        {
            // Tell everyone that we've processed a full message
            if (MessageProcessed != null)
            {
                MessageProcessed();
            }
        }
    }
}
