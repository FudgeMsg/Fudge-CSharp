/**
 * Copyright (C) 2009 - 2009 by OpenGamma Inc.
 *
 * Please see distribution for license.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenGamma.Fudge.Linq;
using Xunit;
using System.Xml.Linq;

namespace OpenGamma.Fudge.Tests.Unit.Linq
{
    public class Examples
    {
        private class Tick
        {
            public double Bid { get; set; }
            public double Ask { get; set; }
            public string Ticker { get; set; }
        }

        [Fact]
        public void SimpleSelect()
        {
            var msgs = new FudgeMsg[] { Create(10.3, 11.1, "FOO"), Create(2.4, 3.1, "BAR") };

            var query = from tick in msgs.AsLinq<Tick>() select tick.Ticker;

            string[] tickers = query.ToArray();
            Assert.Equal(new string[] {"FOO", "BAR"}, tickers);
        }

        [Fact]
        public void SelectWithExpression()
        {
            var msgs = new FudgeMsg[] { Create(10.3, 11.1, "FOO"), Create(2.4, 3.1, "BAR") };

            var query = from tick in msgs.AsLinq<Tick>()
                        select tick.Bid * 2;

            double[] vals = query.ToArray();
            Assert.Equal(new double[] { 20.6, 4.8 }, vals);
        }

        [Fact]
        public void WhereFilter()
        {
            var msgs = new FudgeMsg[] { Create(10.3, 11.1, "FOO"), Create(2.4, 3.1, "BAR") };

            var query = from tick in msgs.AsLinq<Tick>()
                        where tick.Bid < 5.0
                        select tick.Ticker;

            string[] tickers = query.ToArray();
            Assert.Equal(new string[] { "BAR" }, tickers);
        }

        [Fact]
        public void ComplexSelect()
        {
            var msgs = new FudgeMsg[] { Create(10.3, 11.1, "FOO"), Create(2.4, 3.1, "BAR") };

            var query = from tick in msgs.AsLinq<Tick>()
                        where tick.Ask > 4.0
                        select new { tick.Ticker, tick.Ask };

            var results = query.ToArray();
            Assert.Equal(1, results.Length);
            Assert.Equal("FOO", results[0].Ticker);
            Assert.Equal(11.1, results[0].Ask);
        }

        [Fact]
        public void XmlExample()
        {
            var msgs = new FudgeMsg[] { Create(10.3, 11.1, "FOO"), Create(2.4, 3.1, "BAR") };

            XElement tree = new XElement("Ticks", from tick in msgs.AsLinq<Tick>()
                                                  select new XElement("Tick",
                                                      new XElement("Ticker", tick.Ticker),
                                                      new XElement("Bid", tick.Bid),
                                                      new XElement("Ask", tick.Ask)));
            string s = tree.ToString();
            //<Ticks>
            //  <Tick>
            //    <Ticker>FOO</Ticker>
            //    <Bid>10.3</Bid>
            //    <Ask>11.1</Ask>
            //  </Tick>
            //  <Tick>
            //    <Ticker>BAR</Ticker>
            //    <Bid>2.4</Bid>
            //    <Ask>3.1</Ask>
            //  </Tick>
            //</Ticks>
        }

        private static FudgeMsg Create(double bid, double ask, string ticker)
        {
            FudgeMsg msg = new FudgeMsg();
            msg.Add(bid, "Bid");
            msg.Add(ask, "Ask");
            msg.Add(ticker, "Ticker");
            return msg;
        }
    }
}
