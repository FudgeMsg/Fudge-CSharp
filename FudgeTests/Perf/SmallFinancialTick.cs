/**
 * Copyright (C) 2009 - 2009 by OpenGamma Inc.
 *
 * Please see distribution for license.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenGamma.Fudge.Tests.Perf
{
    /// <summary>
    /// Intended to model a very small tick, with just a few key fields.
    /// </summary>
    [Serializable]
    public class SmallFinancialTick
    {
        public double Bid { get; set; }
        public double Ask { get; set; }
        public double BidVolume { get; set; }
        public double AskVolume { get; set; }
        public long Timestamp { get; set; }

        public SmallFinancialTick()
        {
            Timestamp = long.MaxValue - short.MaxValue;
        }

        public bool Equals(SmallFinancialTick t)
        {
            return t.Bid == this.Bid &&
                   t.Ask == this.Ask &&
                   t.BidVolume == this.BidVolume &&
                   t.AskVolume == this.AskVolume &&
                   t.Timestamp == this.Timestamp;
        }

        public override bool Equals(Object obj)
        {
            if (obj is SmallFinancialTick)
            {
                return Equals((SmallFinancialTick)obj);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return this.Bid.GetHashCode();
        }
    }
}
