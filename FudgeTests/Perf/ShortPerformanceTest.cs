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
using System.Runtime.Serialization.Formatters.Binary;

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
            BinaryWriter bw = new BinaryWriter(outputStream);
            SmallFinancialTick tick = new SmallFinancialTick();
            FudgeMsg msg = new FudgeMsg();
            if (useNames && useOrdinals)
            {
                msg.Add(tick.Ask, "ask", (short)1);
                msg.Add(tick.AskVolume, "askVolume", (short)2);
                msg.Add(tick.Bid, "bid", (short)3);
                msg.Add(tick.BidVolume, "bidVolume", (short)4);
                msg.Add(tick.Timestamp, "ts", (short)5);
            }
            else if (useNames)
            {
                msg.Add(tick.Ask, "ask");
                msg.Add(tick.AskVolume, "askVolume");
                msg.Add(tick.Bid, "bid");
                msg.Add(tick.BidVolume, "bidVolume");
                msg.Add(tick.Timestamp, "ts");
            }
            else if (useOrdinals)
            {
                msg.Add(tick.Ask, 1);
                msg.Add(tick.AskVolume, 2);
                msg.Add(tick.Bid, 3);
                msg.Add(tick.BidVolume, 4);
                msg.Add(tick.Timestamp, 5);
            }
            FudgeStreamEncoder.WriteMsg(bw, msg);

            byte[] data = outputStream.ToArray();

            MemoryStream inputstream = new MemoryStream(data);
            BinaryReader br = new BinaryReader(inputstream);
            msg = FudgeStreamDecoder.ReadMsg(br);

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
