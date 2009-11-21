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
using Fudge.Types;

namespace Fudge.Encodings
{
    public class FudgeXmlStreamWriter : IFudgeStreamWriter
    {
        // TODO t0rx 2009-11-14 -- Handle writing XML for ordinals

        private readonly XmlWriter writer;
        private readonly string outerElementName;

        public FudgeXmlStreamWriter(XmlWriter writer, string outerElementName)
        {
            if (writer == null)
                throw new ArgumentNullException("writer");
            if (outerElementName == null)
                throw new ArgumentNullException("outerElementName");

            this.writer = writer;
            this.outerElementName = outerElementName;

            writer.WriteStartElement(outerElementName);
        }

        #region IFudgeStreamWriter Members

        public void StartSubMessage(string name, int? ordinal)
        {
            writer.WriteStartElement(name);
        }

        public void WriteField(string name, int? ordinal, FudgeFieldType type, object value)
        {
            writer.WriteStartElement(name);
            writer.WriteValue(value);
            writer.WriteEndElement();
        }

        public void WriteFields(IEnumerable<IFudgeField> fields)
        {
            foreach (var field in fields)
            {
                if (field.Type == FudgeMsgFieldType.Instance)
                {
                    StartSubMessage(field.Name, field.Ordinal);
                    WriteFields((FudgeMsg)field.Value);
                    EndSubMessage();
                }
                else
                {
                    WriteField(field.Name, field.Ordinal, field.Type, field.Value);
                }
            }
        }

        public void EndSubMessage()
        {
            writer.WriteEndElement();
        }

        public void End()
        {
            writer.WriteEndElement();
            writer.Flush();
        }

        #endregion
    }
}
