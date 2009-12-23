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
using Fudge.Encodings;

namespace Fudge.Tests.Unit.Encodings
{
    public class FudgeMsgStreamTest
    {
        [Fact]
        public void GeneralTest()
        {
            var context = new FudgeContext();

            var msg = StandardFudgeMessages.CreateMessageWithSubMsgs(context);
            var reader = new FudgeMsgStreamReader(msg);
            var writer = new FudgeMsgStreamWriter();

            var pipe = new FudgeStreamPipe(reader, writer);
            pipe.Process();

            var newMsg = writer.Messages[0];

            FudgeUtils.AssertAllFieldsMatch(msg, newMsg);
        }

        [Fact]
        public void MultipleMessages()
        {
            var context = new FudgeContext();

            var msg1 = StandardFudgeMessages.CreateMessageWithSubMsgs(context);
            var msg2 = StandardFudgeMessages.CreateMessageAllNames(context);
            var reader = new FudgeMsgStreamReader(new FudgeMsg[] {msg1, msg2});
            var writer = new FudgeMsgStreamWriter();

            var pipe = new FudgeStreamPipe(reader, writer);
            pipe.Process();

            FudgeUtils.AssertAllFieldsMatch(msg1, writer.Messages[0]);
            FudgeUtils.AssertAllFieldsMatch(msg2, writer.Messages[1]);
        }
    }
}
