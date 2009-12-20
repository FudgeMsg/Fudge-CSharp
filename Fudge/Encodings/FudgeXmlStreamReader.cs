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
using System.Xml;
using System.Diagnostics;
using Fudge.Taxon;
using Fudge.Types;
using System.IO;

namespace Fudge.Encodings
{
    public class FudgeXmlStreamReader : FudgeStreamReaderBase
    {
        private readonly XmlReader reader;
        private int depth = 0;
        private bool atEnd;
        private readonly Queue<KeyValuePair<string, string>> pendingAttributes = new Queue<KeyValuePair<string, string>>();
        private bool oneTokenAhead = false;

        public FudgeXmlStreamReader(XmlReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");
            this.reader = reader;
        }

        public FudgeXmlStreamReader(string xml)
            : this(XmlReader.Create(new StringReader(xml), new XmlReaderSettings { ConformanceLevel = ConformanceLevel.Fragment }))
        {
        }

        #region IFudgeStreamReader Members

        public override bool HasNext
        {
            get
            {
                if (oneTokenAhead)
                    return true;
                oneTokenAhead = ReadNext();
                return oneTokenAhead;
            }
        }

        public override FudgeStreamElement MoveNext()
        {
            CurrentElement = FudgeStreamElement.NoElement;

            if (pendingAttributes.Count > 0)
            {
                ConsumePendingAttribute();
            }
            else
            {
                while (HasNext)
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            ConsumeElement();
                            break;
                        case XmlNodeType.EndElement:
                            ConsumeEndElement();
                            break;
                        default:
                            // Keep going
                            ReadNext();
                            break;
                    }
                    if (CurrentElement != FudgeStreamElement.NoElement)
                        break;
                }
            }
            return CurrentElement;
        }

        private bool ReadNext()
        {
            atEnd = !reader.Read();
            return !atEnd;
        }

        private void ReadNextOrThrow(string position)
        {
            if (!ReadNext())
                throw new FudgeRuntimeException("XML ends prematurely at " + position);
        }

        private bool ReadToNextOfInterest()
        {
            while (!atEnd && reader.NodeType != XmlNodeType.Element && reader.NodeType != XmlNodeType.Attribute && reader.NodeType != XmlNodeType.EndElement)
            {
                ReadNext();
            }
            return !atEnd;
        }

        private void ConsumeEndElement()
        {
            HandleEndDepth();
            oneTokenAhead = false;
        }

        private void HandleEndDepth()
        {
            depth--;
            if (depth > 0)
                CurrentElement = FudgeStreamElement.SubmessageFieldEnd;
            else
                CurrentElement = FudgeStreamElement.MessageEnd;
        }

        private void ConsumePendingAttribute()
        {
            var pair = pendingAttributes.Dequeue();
            if (pair.Key == null)
            {
                HandleEndDepth();
            }
            else
            {
                CurrentElement = FudgeStreamElement.SimpleField;
                FieldName = pair.Key;
                FieldValue = GetValue(pair.Value);
            }
        }

        private void ReadAttributes()
        {
            Debug.Assert(pendingAttributes.Count == 0);

            while (reader.MoveToNextAttribute())
            {
                pendingAttributes.Enqueue(new KeyValuePair<string, string>(reader.Name, reader.Value));
            }
            reader.MoveToElement();

            if (reader.IsEmptyElement)
            {
                pendingAttributes.Enqueue(new KeyValuePair<string, string>(null, null));    // Marker for endsubmessage
                ReadNext();
            }
            else
            {
                // Check to see if we have some text in there
                ReadNextOrThrow(reader.Name);
                if (reader.NodeType == XmlNodeType.Text)
                {
                    // Have to put value in a sub-field
                    pendingAttributes.Enqueue(new KeyValuePair<string, string>("", reader.Value));
                    ReadNext();
                }
            }
        }

        private void ConsumeElement()
        {
            FieldName = reader.Name;
            if (reader.IsEmptyElement && !reader.HasAttributes && depth > 0)
            {
                FieldValue = IndicatorType.Instance;
                CurrentElement = FudgeStreamElement.SimpleField;
                FieldType = IndicatorFieldType.Instance;
                ReadNext();
                return;
            }

            if (reader.HasAttributes)
            {
                // Treat as message rather than simple field
                ReadAttributes();
                CurrentElement = FudgeStreamElement.SubmessageFieldStart;
                depth++;
                return;
            }

            while (CurrentElement == FudgeStreamElement.NoElement)
            {
                ReadNextOrThrow(FieldName);
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        // Must be in a message
                        if (depth == 0)
                            CurrentElement = FudgeStreamElement.MessageStart;
                        else
                            CurrentElement = FudgeStreamElement.SubmessageFieldStart;
                        depth++;
                        break;
                    case XmlNodeType.Text:
                        // Value
                        ConsumeElementValue();
                        break;
                    default:
                        // Ignore
                        break;
                }
            }
        }

        private void ConsumeElementValue()
        {
            CurrentElement = FudgeStreamElement.SimpleField;
            FieldValue = GetValue(reader.Value);
            while (true)
            {
                if (!reader.Read())
                {
                    // Bad XML?
                    throw new FudgeRuntimeException("XML ends prematurely at element " + FieldName);
                }

                if (reader.NodeType == XmlNodeType.EndElement)
                {
                    // Read the next node and stop
                    oneTokenAhead = false;
                    break;
                }
                else
                {
                    // TODO t0rx 2009-11-15 -- Handle odd stuff in XML value
                }
            }
        }

        private object GetValue(string data)
        {
            // TODO t0rx 2009-11-15 -- Attempt to convert to best type
            FieldType = StringFieldType.Instance;
            return data;
        }

        #endregion
    }
}
