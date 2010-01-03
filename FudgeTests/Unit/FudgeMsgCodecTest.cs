/**
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
using System.IO;
using Fudge.Util;

namespace Fudge.Tests.Unit
{
    /// <summary>
    /// A test class that will encode and decode a number of different Fudge messages
    /// to test that encoding and decoding works properly.
    /// </summary>
    public class FudgeMsgCodecTest
    {
        private readonly Random random = new Random();
        private static readonly FudgeContext fudgeContext = new FudgeContext();

        [Fact]
        public void AllNames()
        {
            FudgeMsg inputMsg = StandardFudgeMessages.CreateMessageAllNames(fudgeContext);
            FudgeMsg outputMsg = CycleMessage(inputMsg);

            Assert.NotNull(outputMsg);

            FudgeUtils.AssertAllFieldsMatch(inputMsg, outputMsg);
        }

        [Fact]
        public void VariableWidthColumnSizes()
        {
            FudgeMsg inputMsg = new FudgeMsg();
            inputMsg.Add("100", new byte[100]);
            inputMsg.Add("1000", new byte[1000]);
            inputMsg.Add("10000", new byte[100000]);

            FudgeMsg outputMsg = CycleMessage(inputMsg);

            Assert.NotNull(outputMsg);

            FudgeUtils.AssertAllFieldsMatch(inputMsg, outputMsg);
        }

        [Fact]
        public void SubMsg() //throws IOException
        {
            var inputMsg = StandardFudgeMessages.CreateMessageWithSubMsgs(fudgeContext);

            FudgeMsg outputMsg = CycleMessage(inputMsg);

            Assert.NotNull(outputMsg);

            FudgeUtils.AssertAllFieldsMatch(inputMsg, outputMsg);
        }

        [Fact]
        public void Unknown()
        {
            FudgeMsg inputMsg = new FudgeMsg();
            inputMsg.Add("unknown", new UnknownFudgeFieldValue(new byte[10], new FudgeTypeDictionary().GetUnknownType(200)));
            FudgeMsg outputMsg = CycleMessage(inputMsg);
            FudgeUtils.AssertAllFieldsMatch(inputMsg, outputMsg);
        }


        protected byte[] CreateRandomArray(int length)
        {
            byte[] bytes = new byte[length];
            random.NextBytes(bytes);
            return bytes;
        }

        [Fact]
        public void FixedWidthByteArrays()
        {
            FudgeMsg inputMsg = new FudgeMsg(
                        new Field("byte[4]", CreateRandomArray(4)),
                        new Field("byte[8]", CreateRandomArray(8)),
                        new Field("byte[16]", CreateRandomArray(16)),
                        new Field("byte[20]", CreateRandomArray(20)),
                        new Field("byte[32]", CreateRandomArray(32)),
                        new Field("byte[64]", CreateRandomArray(64)),
                        new Field("byte[128]", CreateRandomArray(128)),
                        new Field("byte[256]", CreateRandomArray(256)),
                        new Field("byte[512]", CreateRandomArray(512)),
                        new Field("byte[28]", CreateRandomArray(28)));

            FudgeMsg outputMsg = CycleMessage(inputMsg);
            FudgeUtils.AssertAllFieldsMatch(inputMsg, outputMsg);
        }

        protected static FudgeMsg CycleMessage(FudgeMsg msg) //throws IOException
        {

            byte[] content = fudgeContext.ToByteArray(msg);
            // Double-check the size calc was right
            Assert.Equal(content.Length, new FudgeMsgEnvelope(msg).ComputeSize(null));

            MemoryStream stream2 = new MemoryStream(content);
            FudgeMsgEnvelope outputMsgEnvelope = fudgeContext.Deserialize(stream2);
            Assert.NotNull(outputMsgEnvelope);
            Assert.NotNull(outputMsgEnvelope.Message);
            return outputMsgEnvelope.Message;
        }
    }
}
