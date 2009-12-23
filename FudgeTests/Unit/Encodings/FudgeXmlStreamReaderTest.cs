/* <!--
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
 * -->
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Fudge.Encodings;
using System.Xml;
using System.IO;
using Fudge.Types;

namespace Fudge.Tests.Unit.Encodings
{
    public class FudgeXmlStreamReaderTest
    {
        [Fact]
        public void Attributes()
        {
            string xml = "<msg><name type=\"surname\" value=\"Smith\"/></msg>";

            var reader = new FudgeXmlStreamReader(xml);
            var msg = reader.ReadToMsg();

            Assert.Equal(FudgeMsgFieldType.Instance, msg.GetByName("name").Type);
            var name = msg.GetMessage("name");
            Assert.Equal("surname", name.GetString("type"));
            Assert.Equal("Smith", name.GetString("value"));
        }

        [Fact]
        public void AttributesAndText()
        {
            // Value should go into a field with empty name
            // TODO 2009-12-17 t0rx -- Is this a good thing to do, or should it go in a field called "value", or just be ignored?
            string xml = "<msg><name type=\"surname\">Smith</name></msg>";

            var reader = new FudgeXmlStreamReader(xml);
            var msg = reader.ReadToMsg();

            Assert.Equal(FudgeMsgFieldType.Instance, msg.GetByName("name").Type);
            var name = msg.GetMessage("name");
            Assert.Equal("surname", name.GetString("type"));
            Assert.Equal("Smith", name.GetString(""));
        }

        [Fact]
        public void AttributesAndSubElements()
        {
            // Value should go into a field with empty name
            // TODO 2009-12-17 t0rx -- Is this a good thing to do, or should it go in a field called "value", or just be ignored?
            string xml = "<msg><name type=\"surname\"><value>Smith</value></name></msg>";

            var reader = new FudgeXmlStreamReader(xml);
            var msg = reader.ReadToMsg();

            Assert.Equal(FudgeMsgFieldType.Instance, msg.GetByName("name").Type);
            var name = msg.GetMessage("name");
            Assert.Equal("surname", name.GetString("type"));
            Assert.Equal("Smith", name.GetString("value"));
        }

        [Fact]
        public void NestedMessages()
        {
            string xml = "<?xml version=\"1.0\" encoding=\"utf-16\"?><msg><name>Fred</name><address><number>17</number><line1>Our House</line1><line2>In the middle of our street</line2><phone>1234</phone><local /></address></msg>";

            var reader = new FudgeXmlStreamReader(xml);
            var writer = new FudgeMsgStreamWriter();
            new FudgeStreamPipe(reader, writer).Process();

            var msg = writer.Messages[0];

            Assert.Equal("Our House", msg.GetMessage("address").GetString("line1"));

            // Convert back to XML and see if it matches
            var sb = new StringBuilder();
            var xmlWriter = XmlWriter.Create(sb);
            var reader2 = new FudgeMsgStreamReader(msg);
            var writer2 = new FudgeXmlStreamWriter(xmlWriter, "msg") { AutoFlushOnMessageEnd = true };
            new FudgeStreamPipe(reader2, writer2).Process();

            var xml2 = sb.ToString();
            Assert.Equal(xml, xml2);
        }

        [Fact]
        public void MultipleMessages()
        {
            string inputXml = "<msg><name>Fred</name></msg><msg><name>Bob</name></msg>";
            var reader = new FudgeXmlStreamReader(inputXml);

            var sb = new StringBuilder();
            var xmlWriter = XmlWriter.Create(sb, new XmlWriterSettings {OmitXmlDeclaration = true, ConformanceLevel = ConformanceLevel.Fragment});
            var writer = new FudgeXmlStreamWriter(xmlWriter, "msg") { AutoFlushOnMessageEnd = true };
            var multiwriter = new FudgeStreamMultiwriter(new DebuggingWriter(), writer);
            new FudgeStreamPipe(reader, multiwriter).Process();
            string outputXml = sb.ToString();

            Assert.Equal(inputXml, outputXml);
        }
    }
}
