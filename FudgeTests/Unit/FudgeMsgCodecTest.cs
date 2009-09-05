/**
 * Copyright (C) 2009 - 2009 by OpenGamma Inc.
 *
 * Please see distribution for license.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using System.IO;

namespace OpenGamma.Fudge.Tests.Unit
{
    /// <summary>
    /// A test class that will encode and decode a number of different Fudge messages
    /// to test that encoding and decoding works properly.
    /// </summary>
    public class FudgeMsgCodecTest
    {
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
            inputMsg.Add(new byte[100], "100");
            inputMsg.Add(new byte[1000], "1000");
            inputMsg.Add(new byte[100000], "10000");

            FudgeMsg outputMsg = CycleMessage(inputMsg);

            Assert.NotNull(outputMsg);

            AssertAllFieldsMatch(inputMsg, outputMsg);
        }

        [Fact]
        public void SubMsg() //throws IOException
        {
            FudgeMsg inputMsg = new FudgeMsg();
            FudgeMsg sub1 = new FudgeMsg();
            sub1.Add("fibble", "bibble");
            sub1.Add("Blibble", (short)827);
            FudgeMsg sub2 = new FudgeMsg();
            sub2.Add(9837438, "bibble9");
            sub2.Add(82.77f, (short)828);
            inputMsg.Add(sub1, "sub1");
            inputMsg.Add(sub2, "sub2");

            FudgeMsg outputMsg = CycleMessage(inputMsg);

            Assert.NotNull(outputMsg);

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
            BinaryWriter bw = new BinaryWriter(stream);
            FudgeStreamEncoder.WriteMsg(bw, msg);

            byte[] content = stream.ToArray();

            MemoryStream stream2 = new MemoryStream(content);
            BinaryReader br = new BinaryReader(stream2);
            FudgeMsg outputMsg = FudgeStreamDecoder.ReadMsg(br);
            return outputMsg;
        }
    }
}
