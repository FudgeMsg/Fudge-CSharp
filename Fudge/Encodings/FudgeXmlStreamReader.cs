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
    /// <summary>
    /// <c>FudgeXmlStreamReader</c> provides a way of reading XML data as a Fudge stream.
    /// </summary>
    /// <remarks>
    /// There is not a 1-1 mapping between XML structure and Fudge message structure.  In particular, attributes are treated in the same way as
    /// child elements (i.e. as fields of the Fudge message), so a message that is read from a <see cref="FudgeXmlStreamReader"/> and written
    /// to a <see cref="FudgeXmlStreamWriter"/> may not come out identical.
    /// </remarks>
    public class FudgeXmlStreamReader : FudgeStreamReaderBase
    {
        private readonly XmlReader reader;
        private int depth = 0;
        private bool atEnd;
        private readonly Queue<KeyValuePair<string, string>> pendingAttributes = new Queue<KeyValuePair<string, string>>();

        /// <summary>
        /// Constructs a new <c>FudgeXmlStreamReader</c> using a given <see cref="XmlReader"/> as the source of the XML data.
        /// </summary>
        /// <param name="reader"><see cref="XmlReader"/> providing the XML data</param>
        public FudgeXmlStreamReader(XmlReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");
            this.reader = reader;
            Begin();
        }

        /// <summary>
        /// Constructs a new <c>FudgeXmlStreamReader</c> with the XML data coming from a string.
        /// </summary>
        /// <param name="xml"></param>
        public FudgeXmlStreamReader(string xml) : this(XmlReader.Create(new StringReader(xml), new XmlReaderSettings{ConformanceLevel = ConformanceLevel.Fragment}))
        {
        }

        private void Begin()
        {
            // Move to first element
            ReadNext();

            // Now get inside the outermost message
            while (true)
            {
                var element = MoveNext();
                if (element == FudgeStreamElement.SubmessageFieldStart)
                    break;
            }
        }

        #region IFudgeStreamReader Members

        /// <inheritdoc/>
        public override bool HasNext
        {
            get { return !atEnd; }
        }

        /// <inheritdoc/>
        public override FudgeStreamElement MoveNext()
        {
            CurrentElement = FudgeStreamElement.NoElement;

            if (pendingAttributes.Count > 0)
            {
                ConsumePendingAttribute();
            }
            else
            {
                while (!atEnd)
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
            CurrentElement = FudgeStreamElement.SubmessageFieldEnd;
            depth--;
            ReadNext();

            // Check if we've reached the end of the message
            CheckIfReachedEnd();
        }

        private void CheckIfReachedEnd()
        {
            if (depth == 1)
            {
                ReadToNextOfInterest();
                if (reader.NodeType == XmlNodeType.EndElement)
                {
                    atEnd = true;
                }
            }
        }

        private void ConsumePendingAttribute()
        {
            var pair = pendingAttributes.Dequeue();
            if (pair.Key == null)
            {
                CurrentElement = FudgeStreamElement.SubmessageFieldEnd;
                depth--;
                CheckIfReachedEnd();                                    // Case of last field in outermost message (e.g. <msg><a b="c" d="e" /></msg>)
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
            if (reader.IsEmptyElement && !reader.HasAttributes)
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
            bool stop = false;
            while (!atEnd)
            {
                if (!reader.Read())
                {
                    // Bad XML?
                    throw new FudgeRuntimeException("XML ends prematurely at element " + FieldName);
                }
                if (stop)
                    break;

                if (reader.NodeType == XmlNodeType.EndElement)
                {
                    // Read the next node and stop
                    stop = true;
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
