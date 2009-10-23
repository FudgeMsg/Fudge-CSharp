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

        [Fact]
        public void AllNames()
        {
            FudgeMsg inputMsg = FudgeMsgTest.CreateMessageAllNames();
            FudgeMsg outputMsg = CycleMessage(inputMsg);

            Assert.NotNull(outputMsg);

            AssertAllFieldsMatch(inputMsg, outputMsg);
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

            AssertAllFieldsMatch(inputMsg, outputMsg);
        }

        [Fact]
        public void SubMsg() //throws IOException
        {
            FudgeMsg inputMsg = new FudgeMsg(
                        new Field("sub1",
                            new Field("bibble", "fibble"),
                            new Field(827, "Blibble")),
                        new Field("sub2",
                            new Field("bibble9", 9837438),
                            new Field(828, 82.77f)));

            FudgeMsg outputMsg = CycleMessage(inputMsg);

            Assert.NotNull(outputMsg);

            AssertAllFieldsMatch(inputMsg, outputMsg);
        }

        [Fact]
        public void Unknown()
        {
            FudgeMsg inputMsg = new FudgeMsg();
            inputMsg.Add("unknown", new UnknownFudgeFieldValue(new byte[10], new FudgeTypeDictionary().GetUnknownType(200)));
            FudgeMsg outputMsg = CycleMessage(inputMsg);
            AssertAllFieldsMatch(inputMsg, outputMsg);
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
            AssertAllFieldsMatch(inputMsg, outputMsg);
        }

        // REVIEW kirk 2009-08-21 -- This should be moved to a utility class.
        protected internal static void AssertAllFieldsMatch(FudgeMsg expectedMsg, FudgeMsg actualMsg)
        {
            var expectedIter = expectedMsg.GetAllFields().GetEnumerator();
            var actualIter = actualMsg.GetAllFields().GetEnumerator();
            while (expectedIter.MoveNext())
            {
                Assert.True(actualIter.MoveNext());
                IFudgeField expectedField = expectedIter.Current;
                IFudgeField actualField = actualIter.Current;

                Assert.Equal(expectedField.Name, actualField.Name);
                Assert.Equal(expectedField.Type, actualField.Type);
                Assert.Equal(expectedField.Ordinal, actualField.Ordinal);
                if (expectedField.Value.GetType().IsArray)
                {
                    Assert.Equal(expectedField.Value.GetType(), actualField.Value.GetType());
                    Assert.Equal(expectedField.Value, actualField.Value);       // XUnit will check all values in the arrays
                }
                else if (expectedField.Value is FudgeMsg)
                {
                    Assert.True(actualField.Value is FudgeMsg);
                    AssertAllFieldsMatch((FudgeMsg)expectedField.Value,
                        (FudgeMsg)actualField.Value);
                }
                else if (expectedField.Value is UnknownFudgeFieldValue)
                {
                    Assert.IsType<UnknownFudgeFieldValue>(actualField.Value);
                    UnknownFudgeFieldValue expectedValue = (UnknownFudgeFieldValue)expectedField.Value;
                    UnknownFudgeFieldValue actualValue = (UnknownFudgeFieldValue)actualField.Value;
                    Assert.Equal(expectedField.Type.TypeId, actualField.Type.TypeId);
                    Assert.Equal(expectedValue.Type.TypeId, actualField.Type.TypeId);
                    Assert.Equal(expectedValue.Contents, actualValue.Contents);
                }
                else
                {
                    Assert.Equal(expectedField.Value, actualField.Value);
                }
            }
            Assert.False(actualIter.MoveNext());
        }

        protected static FudgeMsg CycleMessage(FudgeMsg msg) //throws IOException
        {
            MemoryStream stream = new MemoryStream();
            BinaryWriter bw = new FudgeBinaryWriter(stream);
            FudgeStreamEncoder.WriteMsg(bw, msg);

            byte[] content = stream.ToArray();
            // Double-check the size calc was right
            Assert.Equal(content.Length, new FudgeMsgEnvelope(msg).ComputeSize(null));

            MemoryStream stream2 = new MemoryStream(content);
            BinaryReader br = new FudgeBinaryReader(stream2);
            FudgeMsgEnvelope outputMsgEnvelope = FudgeStreamDecoder.ReadMsg(br);
            Assert.NotNull(outputMsgEnvelope);
            Assert.NotNull(outputMsgEnvelope.Message);
            return outputMsgEnvelope.Message;
        }
    }
}
