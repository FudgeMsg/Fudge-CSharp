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
using System.Diagnostics;
using System.Linq;
using System.Reflection;
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
    public class FudgeInterOpTest
    {
        [Fact]
        public void AllNames()
        {
            FudgeMsg inputMsg = StandardFudgeMessages.CreateMessageAllNames();
            FudgeMsg outputMsg = CycleMessage(inputMsg, "allNames.dat");

            Assert.NotNull(outputMsg);

            FudgeUtils.AssertAllFieldsMatch(inputMsg, outputMsg);
        }

        [Fact]
        public void AllOrdinals()
        {
            FudgeMsg inputMsg = StandardFudgeMessages.CreateMessageAllOrdinals();
            FudgeMsg outputMsg = CycleMessage(inputMsg, "allOrdinals.dat");

            Assert.NotNull(outputMsg);

            FudgeUtils.AssertAllFieldsMatch(inputMsg, outputMsg);
        }

        [Fact]
        public void VariableWidthColumnSizes()
        {
            FudgeMsg inputMsg = new FudgeMsg(
                                    new Field("100", new byte[100]),
                                    new Field("1000", new byte[1000]),
                                    new Field("10000", new byte[100000]));          // TODO t0rx 2009-10-08 -- Fix this field name in Fudge-Java and here so the interop still works

            FudgeMsg outputMsg = CycleMessage(inputMsg, "variableWidthColumnSizes.dat");

            Assert.NotNull(outputMsg);

            FudgeUtils.AssertAllFieldsMatch(inputMsg, outputMsg);
        }

        [Fact]
        public void SubMsg() //throws IOException
        {
            var inputMsg = new FudgeMsg(   
                                new Field("sub1",
                                    new Field("bibble", "fibble"),
                                    new Field(827, "Blibble")),
                                new Field("sub2", 
                                    new Field("bibble9", 9837438),
                                    new Field(828, 82.77f)));

            FudgeMsg outputMsg = CycleMessage(inputMsg, "subMsg.dat");

            Assert.NotNull(outputMsg);

            FudgeUtils.AssertAllFieldsMatch(inputMsg, outputMsg);
        }

        [Fact]
        public void Unknown()
        {
            FudgeMsg inputMsg = new FudgeMsg(
                                    new Field("unknown", new UnknownFudgeFieldValue(new byte[10], new FudgeTypeDictionary().GetUnknownType(200))));
            FudgeMsg outputMsg = CycleMessage(inputMsg, "unknown.dat");
            FudgeUtils.AssertAllFieldsMatch(inputMsg, outputMsg);
        }

        // this was a random array, but changed for repeatability (didn't want to use a fixed seed because not sure Random impl is same on Java).
        protected byte[] CreateAscendingArray(int length)
        {
            byte[] bytes = new byte[length];
            for (int i = 0; i < length; i++)
            {
                bytes[i] = (byte)i;
            }
            return bytes;
        }

        [Fact]
        public void FixedWidthByteArrays()
        {
            FudgeMsg inputMsg = new FudgeMsg(
                                    new Field("byte[4]", CreateAscendingArray(4)),
                                    new Field("byte[8]", CreateAscendingArray(8)),
                                    new Field("byte[16]", CreateAscendingArray(16)),
                                    new Field("byte[20]", CreateAscendingArray(20)),
                                    new Field("byte[32]", CreateAscendingArray(32)),
                                    new Field("byte[64]", CreateAscendingArray(64)),
                                    new Field("byte[128]", CreateAscendingArray(128)),
                                    new Field("byte[256]", CreateAscendingArray(256)),
                                    new Field("byte[512]", CreateAscendingArray(512)),
                                    new Field("byte[28]", CreateAscendingArray(28)));

            FudgeMsg outputMsg = CycleMessage(inputMsg, "fixedWidthByteArrays.dat");
            FudgeUtils.AssertAllFieldsMatch(inputMsg, outputMsg);
        }

        protected static FudgeMsg CycleMessage(FudgeMsg msg, string filename) //throws IOException
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            Stream stream = assembly.GetManifestResourceStream("Fudge.Tests.Resources." + filename);
            BinaryReader referenceReader = new FudgeBinaryReader(stream);
            Stream memoryStream = new MemoryStream();
            // set the last parameter of the following line to true to see the full diff report between streams and not fail at the first difference.
            BinaryWriter bw = new StreamComparingBinaryNBOWriter(referenceReader, memoryStream, false);
            FudgeStreamEncoder.WriteMsg(bw, msg);
            bw.Close();

            // Reload as closed above
            stream = assembly.GetManifestResourceStream("Fudge.Tests.Resources." + filename);
            BinaryReader br = new FudgeBinaryReader(stream);                    // Load the message from the resource rather than our output
            FudgeMsg outputMsg = FudgeStreamDecoder.ReadMsg(br).Message;
            return outputMsg;
        }
    }
}
