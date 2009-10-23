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

namespace Fudge.Tests.Perf
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
