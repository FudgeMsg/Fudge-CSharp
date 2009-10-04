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
using OpenGamma.Fudge.Util;

namespace OpenGamma.Fudge.Tests.Unit
{
    /// <summary>
    /// A test class that will encode and decode a number of different Fudge messages
    /// to test that encoding and decoding works properly.
    /// </summary>
    public class FudgeInterOpTest
    {
        [Fact]
        public void AllNames()
        {
            FudgeMsg inputMsg = FudgeMsgTest.CreateMessageAllNames();
            FudgeMsg outputMsg = CycleMessage(inputMsg, "allNames.dat");

            Assert.NotNull(outputMsg);

            FudgeTestUtils.AssertAllFieldsMatch(inputMsg, outputMsg);
        }

        [Fact]
        public void AllOrdinals()
        {
            FudgeMsg inputMsg = FudgeMsgTest.CreateMessageAllOrdinals();
            FudgeMsg outputMsg = CycleMessage(inputMsg, "allOrdinals.dat");

            Assert.NotNull(outputMsg);

            FudgeTestUtils.AssertAllFieldsMatch(inputMsg, outputMsg);
        }

        [Fact]
        public void VariableWidthColumnSizes()
        {
            FudgeMsg inputMsg = new FudgeMsg();
            inputMsg.Add(new byte[100], "100");
            inputMsg.Add(new byte[1000], "1000");
            inputMsg.Add(new byte[100000], "10000");

            FudgeMsg outputMsg = CycleMessage(inputMsg, "variableWidthColumnSizes.dat");

            Assert.NotNull(outputMsg);

            FudgeTestUtils.AssertAllFieldsMatch(inputMsg, outputMsg);
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

            FudgeMsg outputMsg = CycleMessage(inputMsg, "subMsg.dat");

            Assert.NotNull(outputMsg);

            FudgeTestUtils.AssertAllFieldsMatch(inputMsg, outputMsg);
        }

        [Fact]
        public void Unknown()
        {
            FudgeMsg inputMsg = new FudgeMsg();
            inputMsg.Add(new UnknownFudgeFieldValue(new byte[10], FudgeTypeDictionary.Instance.GetUnknownType(200)), "unknown");
            FudgeMsg outputMsg = CycleMessage(inputMsg, "unknown.dat");
            FudgeTestUtils.AssertAllFieldsMatch(inputMsg, outputMsg);
        }

        // this was a random array, but changed for repeatability (didn't want to use a fixed seed because not sure Random impl is same on Java).
        protected byte[] CreateAscendingArray(int length)
        {
            byte[] bytes = new byte[length];
            for (int i = 0; i < length; i++)
            {
                bytes[i] = (byte) (i % sbyte.MaxValue);
            }
            return bytes;
        }

        [Fact]
        public void FixedWidthByteArrays()
        {
            FudgeMsg inputMsg = new FudgeMsg();
            inputMsg.Add(CreateAscendingArray(4), "byte[4]");
            inputMsg.Add(CreateAscendingArray(8), "byte[8]");
            inputMsg.Add(CreateAscendingArray(16), "byte[16]");
            inputMsg.Add(CreateAscendingArray(20), "byte[20]");
            inputMsg.Add(CreateAscendingArray(32), "byte[32]");
            inputMsg.Add(CreateAscendingArray(64), "byte[64]");
            inputMsg.Add(CreateAscendingArray(128), "byte[128]");
            inputMsg.Add(CreateAscendingArray(256), "byte[256]");
            inputMsg.Add(CreateAscendingArray(512), "byte[512]");

            inputMsg.Add(CreateAscendingArray(28), "byte[28]");

            FudgeMsg outputMsg = CycleMessage(inputMsg, "fixedWidthByteArrays.dat");
            FudgeTestUtils.AssertAllFieldsMatch(inputMsg, outputMsg);
        }

        protected static FudgeMsg CycleMessage(FudgeMsg msg, String filename) //throws IOException
        {
            string outputDir = System.Environment.GetEnvironmentVariable("FudgeTestOutputDir");
            string fullPath;
            if (outputDir != null)
            {
                fullPath = outputDir + Path.PathSeparator + filename;
            }
            else
            {
                fullPath = filename;  // fall back to local directory if env not set.
            }
            FileStream fileStream = new FileStream(fullPath, FileMode.Create);
            BinaryWriter bw = new FudgeBinaryWriter(fileStream);
            FudgeStreamEncoder.WriteMsg(bw, msg);
            fileStream.Close();

            fileStream = new FileStream(fullPath, FileMode.Open);
            BinaryReader br = new FudgeBinaryReader(fileStream);
            FudgeMsg outputMsg = FudgeStreamDecoder.ReadMsg(br).Message;
            return outputMsg;
        }
    }
}
