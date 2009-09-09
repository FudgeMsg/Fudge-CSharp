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
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Util;
using System.Diagnostics;
using OpenGamma.Fudge.Tests.Perf;

namespace OpenGamma.Fudge.Tests.Rabbit
{
    /// <summary>
    /// A very short test just to establish some simple performance metrics
    /// for Fudge encoding compared with Java Serialization.
    /// </summary>
    public class WireTest
    {
        public WireTest()
        {
            Console.Out.WriteLine("WireTest Constructor");
        }
        private static readonly string CONN_ADDRESS = "127.0.0.1";

        private static void SendFudgeMessage(FudgeMsg message, string exchange, string routingKey)
        {
            MemoryStream outputStream = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(outputStream);
            FudgeStreamEncoder.WriteMsg(bw, message);
            byte[] data = outputStream.ToArray();

            using (IConnection conn = new ConnectionFactory().CreateConnection(CONN_ADDRESS))
            {
                using (IModel ch = conn.CreateModel())
                {
                    ch.BasicPublish("", "TestQueue", null, data);
                }
            }
        }

        private static FudgeMsg GetFudgeMessage(IModel ch, string queueName)
        {
            
            BasicGetResult result = ch.BasicGet(queueName, false);
            while(result == null) 
            {
                Console.WriteLine("No message available.");
                System.Threading.Thread.Sleep(200);
                result = ch.BasicGet(queueName, false);
            }
            ch.BasicAck(result.DeliveryTag, false);
            Console.WriteLine("Message:");
            DebugUtil.DumpProperties(result, Console.Out, 0);
            byte[] payload = result.Body;
            MemoryStream inputstream = new MemoryStream(payload);
            BinaryReader br = new BinaryReader(inputstream);
            return FudgeStreamDecoder.ReadMsg(br);
        }

        public static void LogConnClose(IConnection conn, ShutdownEventArgs reason)
        {
            Console.Error.WriteLine("Closing connection " + conn + " with reason " + reason);
        }

        public static void sendTick(IModel ch, SmallFinancialTick tick, String queueName, bool useOrdinals, bool useNames)
        {
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
            SendFudgeMessage(msg, "", queueName);
        }

        private static SmallFinancialTick receiveTick(IModel ch, string queueName, bool useOrdinals, bool useNames)
        {
            FudgeMsg msg = GetFudgeMessage(ch, queueName);

            SmallFinancialTick tick = new SmallFinancialTick();
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
            return tick;
        }
     
        [Fact]
        private static void BasicTest()
        {
            using (IConnection conn = new ConnectionFactory().CreateConnection(CONN_ADDRESS)) {
                conn.ConnectionShutdown += new ConnectionShutdownEventHandler(LogConnClose);
                //conn.AutoClose = true;
                using (IModel ch = conn.CreateModel())
                {
                    ch.QueueDeclare("TestQueue");
                    Console.Out.WriteLine("Starting BasicTest()");
                    Console.Out.Flush();
                    for (int i = 0; i < 1000; i++)
                    {
                        SmallFinancialTick tick = new SmallFinancialTick();

                        sendTick(ch, tick, "TestQueue", true, true);
                        SmallFinancialTick tick2;
                        tick2 = receiveTick(ch, "TestQueue", true, true);

                        Xunit.Assert.True(tick.Equals(tick2));
                    }
                    Console.Out.WriteLine("Ended BasicTest()");
                    Console.Out.Flush();
                }
                conn.Close();
            }
        }


    }
}
