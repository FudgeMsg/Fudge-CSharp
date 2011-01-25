﻿/**
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
using Xunit;
using Fudge.Encodings;
using Fudge.Util;

namespace Fudge.Tests.Unit
{
    public class FudgeStreamPipeTest
    {
        [Fact]
        public void RaisesEventAfterEachMessage()
        {
            var context = new FudgeContext();
            var messages = new FudgeMsg[]
            {
                new FudgeMsg(context, new Field("test", "data")),
                new FudgeMsg(context, new Field("life", 42))
            };
            var reader = new FudgeMsgStreamReader(context, messages);
            var writer = new FudgeMsgStreamWriter(context);

            var pipe = new FudgeStreamPipe(reader, writer);

            int count = 0;
            pipe.MessageProcessed += () =>
            {
                var message = writer.DequeueMessage();
                FudgeUtils.AssertAllFieldsMatch(messages[count], message);
                count++;
            };

            pipe.Process();

            Assert.Equal(2, count);
        }
    }
}
