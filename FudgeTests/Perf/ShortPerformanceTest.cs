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
using System.Runtime.Serialization.Formatters.Binary;
using OpenGamma.Fudge.Util;
using OpenGamma.Fudge.Serialization;

namespace OpenGamma.Fudge.Tests.Perf
{
    /// <summary>
    /// A very short test just to establish some simple performance metrics
    /// for Fudge encoding compared with Java Serialization.
    /// </summary>
    public class ShortPerformanceTest
    {
        private const int HOT_SPOT_WARMUP_CYCLES = 1000;

        public ShortPerformanceTest()
        {
            WarmUpHotSpot();
        }

        private static void WarmUpHotSpot()
        {
            Console.Out.WriteLine("Fudge size, Names Only: " + FudgeCycle(true, false));
            Console.Out.WriteLine("Fudge size, Ordinals Only: " + FudgeCycle(false, true));
            Console.Out.WriteLine("Fudge size, Names And Ordinals: " + FudgeCycle(true, true));
            Console.Out.WriteLine("Serialization size: " + SerializationCycle());
            for (int i = 0; i < HOT_SPOT_WARMUP_CYCLES; i++)
            {
                FudgeCycle(true, false);
                FudgeCycle(false, true);
                FudgeCycle(true, true);
                SerializationCycle();
            }
        }

        [Fact]
        public void PerformanceVersusSerialization10000Cycles()
        {
            PerformanceVersusSerialization(10000);
        }

        private static void PerformanceVersusSerialization(int nCycles)
        {
            long startTime = 0;
            long endTime = 0;

            Console.Out.WriteLine("Starting Fudge names only.");
            startTime = DateTime.Now.Ticks / 10000;
            for (int i = 0; i < nCycles; i++)
            {
                FudgeCycle(true, false);
            }
            endTime = DateTime.Now.Ticks / 10000;
            long fudgeDeltaNamesOnly = endTime - startTime;
            double fudgeSplitNamesOnly = ConvertToCyclesPerSecond(nCycles, fudgeDeltaNamesOnly);
            Console.Out.WriteLine("GCing...");
            System.GC.Collect();

            Console.Out.WriteLine("Starting Fudge ordinals only.");
            startTime = DateTime.Now.Ticks / 10000;
            for (int i = 0; i < nCycles; i++)
            {
                FudgeCycle(false, true);
            }
            endTime = DateTime.Now.Ticks / 10000;
            long fudgeDeltaOrdinalsOnly = endTime - startTime;
            double fudgeSplitOrdinalsOnly = ConvertToCyclesPerSecond(nCycles, fudgeDeltaOrdinalsOnly);
            Console.Out.WriteLine("GCing...");
            System.GC.Collect();

            Console.Out.WriteLine("Starting Fudge names and ordinals.");
            startTime = DateTime.Now.Ticks / 10000;
            for (int i = 0; i < nCycles; i++)
            {
                FudgeCycle(true, true);
            }
            endTime = DateTime.Now.Ticks / 10000;
            long fudgeDeltaBoth = endTime - startTime;
            double fudgeSplitBoth = ConvertToCyclesPerSecond(nCycles, fudgeDeltaBoth);
            Console.Out.WriteLine("GCing...");
            System.GC.Collect();

            Console.Out.WriteLine("Starting Java Serialization.");
            startTime = DateTime.Now.Ticks / 10000;
            for (int i = 0; i < nCycles; i++)
            {
                SerializationCycle();
            }
            endTime = DateTime.Now.Ticks / 10000;
            long serializationDelta = endTime - startTime;
            double serializationSplit = ConvertToCyclesPerSecond(nCycles, serializationDelta);
            Console.Out.WriteLine("GCing...");
            System.GC.Collect();

            StringBuilder sb = new StringBuilder();
            sb.Append("For ").Append(nCycles).Append(" cycles");
            Console.Out.WriteLine(sb.ToString());

            sb = new StringBuilder();
            sb.Append("Fudge Names Only ").Append(fudgeDeltaNamesOnly);
            Console.Out.WriteLine(sb.ToString());
            sb = new StringBuilder();
            sb.Append("Fudge Ordinals Only ").Append(fudgeDeltaOrdinalsOnly);
            Console.Out.WriteLine(sb.ToString());
            sb = new StringBuilder();
            sb.Append("Fudge Names And Ordinals ").Append(fudgeDeltaBoth);
            Console.Out.WriteLine(sb.ToString());
            sb = new StringBuilder();
            sb.Append("ms, Serialization ").Append(serializationDelta).Append("ms");
            Console.Out.WriteLine(sb.ToString());
            sb = new StringBuilder();
            sb.Append("Fudge Names Only: ").Append(fudgeSplitNamesOnly).Append("cycles/sec");
            Console.Out.WriteLine(sb.ToString());
            sb = new StringBuilder();
            sb.Append("Fudge Ordinals Only: ").Append(fudgeSplitOrdinalsOnly).Append("cycles/sec");
            Console.Out.WriteLine(sb.ToString());
            sb = new StringBuilder();
            sb.Append("Fudge Names And Ordinals: ").Append(fudgeSplitBoth).Append("cycles/sec");
            Console.Out.WriteLine(sb.ToString());
            sb = new StringBuilder();
            sb.Append("Serialization: ").Append(serializationSplit).Append("cycles/sec");
            Console.Out.WriteLine(sb.ToString());
            Assert.True(serializationDelta > fudgeDeltaNamesOnly, "Serialization faster by " + (fudgeDeltaNamesOnly - serializationDelta) + "ms.");
        }

        private static double ConvertToCyclesPerSecond(int nCycles, long delta)
        {
            double fudgeSplit = (double)delta;
            fudgeSplit = fudgeSplit / nCycles;
            fudgeSplit = fudgeSplit / 1000.0;
            fudgeSplit = 1 / fudgeSplit;
            return fudgeSplit;
        }

        private static int FudgeCycle(bool useNames, bool useOrdinals)
        {
            MemoryStream outputStream = new MemoryStream();
            var bw = new FudgeBinaryWriter(outputStream);
            SmallFinancialTick tick = new SmallFinancialTick();
            FudgeMsg msg = new FudgeMsg();
            if (useNames && useOrdinals)
            {
                msg.Add("ask", (short)1, tick.Ask);
                msg.Add("askVolume", (short)2, tick.AskVolume);
                msg.Add("bid", (short)3, tick.Bid);
                msg.Add("bidVolume", (short)4, tick.BidVolume);
                msg.Add("ts", (short)5, tick.Timestamp);
            }
            else if (useNames)
            {
                msg.Add("ask", tick.Ask);
                msg.Add("askVolume", tick.AskVolume);
                msg.Add("bid", tick.Bid);
                msg.Add("bidVolume", tick.BidVolume);
                msg.Add("ts", tick.Timestamp);
            }
            else if (useOrdinals)
            {
                msg.Add(1, tick.Ask);
                msg.Add(2, tick.AskVolume);
                msg.Add(3, tick.Bid);
                msg.Add(4, tick.BidVolume);
                msg.Add(5, tick.Timestamp);
            }
            FudgeStreamEncoder.WriteMsg(bw, msg);

            byte[] data = outputStream.ToArray();

            MemoryStream inputstream = new MemoryStream(data);
            var br = new FudgeBinaryReader(inputstream);
            msg = FudgeStreamDecoder.ReadMsg(br).Message;

            tick = new SmallFinancialTick();
            if (useOrdinals)
            {
                tick.Ask = msg.GetDouble(1).Value;
                tick.AskVolume = msg.GetDouble(2).Value;
                tick.Bid = msg.GetDouble(3).Value;
                tick.BidVolume = msg.GetDouble(4).Value;
                tick.Timestamp = msg.GetLong(5).Value;
            }
            else if (useNames)
            {
                tick.Ask = msg.GetDouble("ask").Value;
                tick.AskVolume = msg.GetDouble("askVolume").Value;
                tick.Bid = msg.GetDouble("bid").Value;
                tick.BidVolume = msg.GetDouble("bidVolume").Value;
                tick.Timestamp = msg.GetLong("ts").Value;
            }
            else
            {
                throw new InvalidOperationException("Names or ordinals, pick at least one.");
            }
            return data.Length;
        }

        private static int SerializationCycle()
        {
            MemoryStream outputStream = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();
            SmallFinancialTick tick = new SmallFinancialTick();
            formatter.Serialize(outputStream, tick);

            byte[] data = outputStream.ToArray();

            MemoryStream inputStream = new MemoryStream(data);
            formatter.Deserialize(inputStream);
            return data.Length;
        }
    }
}
