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
    public class FudgeXmlStreamReader : IFudgeStreamReader
    {
        private readonly XmlReader reader;
        private FudgeStreamElement currentElement = FudgeStreamElement.NoElement;
        private string fieldName;
        private object fieldValue;
        private FudgeFieldType fieldType;
        private int depth = 0;
        private bool atEnd;
        private readonly Queue<KeyValuePair<string, string>> pendingAttributes = new Queue<KeyValuePair<string, string>>();

        public FudgeXmlStreamReader(XmlReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");
            this.reader = reader;
            Begin();
        }

        public FudgeXmlStreamReader(string xml) : this(XmlReader.Create(new StringReader(xml)))
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

        public bool HasNext
        {
            get { return !atEnd; }
        }

        public FudgeStreamElement MoveNext()
        {
            currentElement = FudgeStreamElement.NoElement;

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
                    if (currentElement != FudgeStreamElement.NoElement)
                        break;
                }
            }
            return currentElement;
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
            currentElement = FudgeStreamElement.SubmessageFieldEnd;
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
                currentElement = FudgeStreamElement.SubmessageFieldEnd;
                depth--;
                CheckIfReachedEnd();                                    // Case of last field in outermost message (e.g. <msg><a b="c" d="e" /></msg>)
            }
            else
            {
                currentElement = FudgeStreamElement.SimpleField;
                fieldName = pair.Key;
                fieldValue = GetValue(pair.Value);
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
            fieldName = reader.Name;
            if (reader.IsEmptyElement && !reader.HasAttributes)
            {
                fieldValue = IndicatorType.Instance;
                currentElement = FudgeStreamElement.SimpleField;
                fieldType = IndicatorFieldType.Instance;
                ReadNext();
                return;
            }

            if (reader.HasAttributes)
            {
                // Treat as message rather than simple field
                ReadAttributes();
                currentElement = FudgeStreamElement.SubmessageFieldStart;
                depth++;
                return;
            }

            while (currentElement == FudgeStreamElement.NoElement)
            {
                ReadNextOrThrow(fieldName);
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        // Must be in a message
                        currentElement = FudgeStreamElement.SubmessageFieldStart;
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
            currentElement = FudgeStreamElement.SimpleField;
            fieldValue = GetValue(reader.Value);
            bool stop = false;
            while (!atEnd)
            {
                if (!reader.Read())
                {
                    // Bad XML?
                    throw new FudgeRuntimeException("XML ends prematurely at element " + fieldName);
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
            fieldType = StringFieldType.Instance;
            return data;
        }

        public FudgeStreamElement CurrentElement
        {
            get { return currentElement ; }
        }

        public int ProcessingDirectives
        {
            get { throw new NotImplementedException(); }
        }

        public int SchemaVersion
        {
            get { throw new NotImplementedException(); }
        }

        public int TaxonomyId
        {
            get { throw new NotImplementedException(); }
        }

        public int EnvelopeSize
        {
            get { throw new NotImplementedException(); }
        }

        public FudgeFieldType FieldType
        {
            get { return fieldType; }
        }

        public int? FieldOrdinal
        {
            get { return null; }
        }

        public string FieldName
        {
            get { return fieldName; }
        }

        public object FieldValue
        {
            get { return fieldValue; }
        }

        public IFudgeTaxonomy Taxonomy
        {
            get { throw new NotImplementedException(); }
        }

        #endregion
    }
}
