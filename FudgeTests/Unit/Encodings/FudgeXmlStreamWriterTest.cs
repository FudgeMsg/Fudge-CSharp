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
using Xunit;
using System.Xml;
using Fudge.Encodings;
using Fudge.Types;

namespace Fudge.Tests.Unit.Encodings
{
    public class FudgeXmlStreamWriterTest
    {
        [Fact]
        public void NestedMessages()
        {
            var msg = new FudgeMsg(new Field("name", "Fred"),
                                   new Field("address",
                                       new Field("number", 17),
                                       new Field("line1", "Our House"),
                                       new Field("line2", "In the middle of our street")));

            var sb = new StringBuilder();
            var xmlWriter = XmlWriter.Create(sb);
            var writer = new FudgeXmlStreamWriter(xmlWriter, "msg");
            var reader = new FudgeMsgStreamReader(msg);
            new FudgeStreamPipe(reader, writer).Process();
            xmlWriter.Flush();

            string s = sb.ToString();
            Assert.Equal("<?xml version=\"1.0\" encoding=\"utf-16\"?><msg><name>Fred</name><address><number>17</number><line1>Our House</line1><line2>In the middle of our street</line2></address></msg>", s);
        }

        [Fact]
        public void WriteIndicatorType()
        {
            var msg = new FudgeMsg(new Field("blank", IndicatorType.Instance));
            var sb = new StringBuilder();
            var xmlWriter = XmlWriter.Create(sb);
            var writer = new FudgeXmlStreamWriter(xmlWriter, "msg");
            var reader = new FudgeMsgStreamReader(msg);
            new FudgeStreamPipe(reader, writer).Process();
            xmlWriter.Flush();

            string s = sb.ToString();
            Assert.Equal("<?xml version=\"1.0\" encoding=\"utf-16\"?><msg><blank /></msg>", s);
        }
    }
}
