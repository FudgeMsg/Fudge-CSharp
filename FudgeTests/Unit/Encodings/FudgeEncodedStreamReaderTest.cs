/* <!--
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
using System.IO;
using Fudge.Encodings;

namespace Fudge.Tests.Unit.Encodings
{
    public class FudgeEncodedStreamReaderTest
    {
        [Fact]
        public void CheckElementsCorrectForSimpleMessage()
        {
            var context = new FudgeContext();
            var msg = context.NewMessage();
            msg.Add("Test", "Bob");
            var bytes = context.ToByteArray(msg);

            var stream = new MemoryStream(bytes);
            var reader = new FudgeEncodedStreamReader(context, stream);

            Assert.True(reader.HasNext);
            Assert.Equal(FudgeStreamElement.MessageStart, reader.MoveNext());
            Assert.True(reader.HasNext);
            Assert.Equal(FudgeStreamElement.SimpleField, reader.MoveNext());
            Assert.True(reader.HasNext);
            Assert.Equal(FudgeStreamElement.MessageEnd, reader.MoveNext());
            Assert.False(reader.HasNext);
            Assert.Equal(FudgeStreamElement.NoElement, reader.MoveNext());
            Assert.False(reader.HasNext);
            Assert.Equal(FudgeStreamElement.NoElement, reader.MoveNext());
        }

        [Fact]
        public void CheckEndOfStreamWithoutHasNext()
        {
            // Same as CheckElementsCorrectForSimpleMessage but without using HasNext
            var context = new FudgeContext();
            var msg = context.NewMessage();
            msg.Add("Test", "Bob");
            var bytes = context.ToByteArray(msg);

            var stream = new MemoryStream(bytes);
            var reader = new FudgeEncodedStreamReader(context, stream);

            Assert.Equal(FudgeStreamElement.MessageStart, reader.MoveNext());
            Assert.Equal(FudgeStreamElement.SimpleField, reader.MoveNext());
            Assert.Equal(FudgeStreamElement.MessageEnd, reader.MoveNext());
            Assert.Equal(FudgeStreamElement.NoElement, reader.MoveNext());
            Assert.Equal(FudgeStreamElement.NoElement, reader.MoveNext());
        }

        [Fact]
        public void CheckElementsCorrectForSubMessage()
        {
            var context = new FudgeContext();
            var msg = context.NewMessage();
            var subMsg = context.NewMessage();
            msg.Add("sub", subMsg);
            subMsg.Add("Test", "Bob");
            var bytes = context.ToByteArray(msg);

            var stream = new MemoryStream(bytes);
            var reader = new FudgeEncodedStreamReader(context, stream);

            Assert.True(reader.HasNext);
            Assert.Equal(FudgeStreamElement.MessageStart, reader.MoveNext());
            Assert.True(reader.HasNext);
            Assert.Equal(FudgeStreamElement.SubmessageFieldStart, reader.MoveNext());
            Assert.True(reader.HasNext);
            Assert.Equal(FudgeStreamElement.SimpleField, reader.MoveNext());
            Assert.True(reader.HasNext);
            Assert.Equal(FudgeStreamElement.SubmessageFieldEnd, reader.MoveNext());
            Assert.True(reader.HasNext);
            Assert.Equal(FudgeStreamElement.MessageEnd, reader.MoveNext());
            Assert.False(reader.HasNext);
        }

        [Fact]
        public void MultipleMessages()
        {
            // Same as CheckElementsCorrectForSimpleMessage but without using HasNext
            var context = new FudgeContext();
            var msg1 = context.NewMessage();
            msg1.Add("Test", "Bob");
            var msg2 = context.NewMessage();
            msg2.Add("Test2", "Shirley");
            var msgs = new FudgeMsg[] {msg1, msg2};
            var stream = new MemoryStream();
            var writer = new FudgeEncodedStreamWriter(context, stream);
            new FudgeStreamPipe(new FudgeMsgStreamReader(context, msgs), writer).Process();

            stream.Position = 0;
            var reader = new FudgeEncodedStreamReader(context, stream);

            Assert.Equal(FudgeStreamElement.MessageStart, reader.MoveNext());
            Assert.Equal(FudgeStreamElement.SimpleField, reader.MoveNext());
            Assert.Equal(FudgeStreamElement.MessageEnd, reader.MoveNext());
            Assert.Equal(FudgeStreamElement.MessageStart, reader.MoveNext());
            Assert.Equal(FudgeStreamElement.SimpleField, reader.MoveNext());
            Assert.Equal(FudgeStreamElement.MessageEnd, reader.MoveNext());
            Assert.Equal(FudgeStreamElement.NoElement, reader.MoveNext());
            Assert.Equal(FudgeStreamElement.NoElement, reader.MoveNext());
        }
    }
}
