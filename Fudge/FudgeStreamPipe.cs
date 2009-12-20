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

namespace Fudge
{
    public class FudgeStreamPipe
    {
        private readonly IFudgeStreamReader reader;
        private readonly IFudgeStreamWriter writer;

        public FudgeStreamPipe(IFudgeStreamReader reader, IFudgeStreamWriter writer)
        {
            this.reader = reader;
            this.writer = writer;
        }

        public void Process()
        {
            while (reader.HasNext)
            {
                ProcessOne();
            }
        }

        public void ProcessOne()
        {
            while (reader.HasNext)
            {
                switch (reader.MoveNext())
                {
                    case FudgeStreamElement.MessageStart:
                        writer.StartMessage();
                        break;
                    case FudgeStreamElement.MessageEnd:
                        writer.EndMessage();
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
    }
}
