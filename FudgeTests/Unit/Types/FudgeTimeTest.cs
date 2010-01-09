/* <!--
 * Copyright (C) 2009 - 2010 by OpenGamma Inc. and other contributors.
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
 * -->
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Fudge.Types;

namespace Fudge.Tests.Unit.Types
{
    public class FudgeTimeTest
    {
        [Fact]
        public void BasicConstruction()
        {
            var t = new FudgeTime(13, 4, 5, 123456789, -120, FudgeDateTime.Precision.Nanosecond);
            Assert.Equal(13, t.Hour);
            Assert.Equal(4, t.Minute);
            Assert.Equal(5, t.Second);
            Assert.Equal(123456789, t.Nanoseconds);
            Assert.True(t.HasTimeZone);
            Assert.Equal(-120, t.TimeZoneOffset);
            Assert.Equal(FudgeDateTime.Precision.Nanosecond, t.Precision);
            Assert.Equal("13:04:05.123456789 -02:00", t.ToString());

            var t2 = new FudgeTime(1, 2, 3);
            Assert.False(t2.HasTimeZone);
            Assert.Equal(FudgeDateTime.Precision.Second, t2.Precision);
            Assert.Equal("01:02:03", t2.ToString());

            var t3 = new FudgeTime(1, 2, 3, 60);
            Assert.True(t3.HasTimeZone);
            Assert.Equal(60, t3.TimeZoneOffset);
            Assert.Equal(FudgeDateTime.Precision.Second, t3.Precision);
            Assert.Equal("01:02:03 +01:00", t3.ToString());

            // Other variants
            Assert.Equal("04:01", new FudgeTime(4, 1).ToString());
            Assert.Equal("23", new FudgeTime(23).ToString());
            Assert.Equal("10:00:05.987654321", new FudgeTime(10, 0, 5, 987654321, FudgeDateTime.Precision.Nanosecond).ToString());
            Assert.Equal("01:01:01.000000123", new FudgeTime(FudgeDateTime.Precision.Nanosecond, 3661, 123).ToString());
            Assert.Equal("01:01:01.000000123 +00:30", new FudgeTime(FudgeDateTime.Precision.Nanosecond, 3661, 123, 30).ToString());
        }

        [Fact]
        public void RangeChecking()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new FudgeTime(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => new FudgeTime(24));
            Assert.Throws<ArgumentOutOfRangeException>(() => new FudgeTime(1, -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => new FudgeTime(1, -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => new FudgeTime(1, 5, -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => new FudgeTime(1, 5, 60));
            Assert.Throws<ArgumentOutOfRangeException>(() => new FudgeTime(1, 5, 13, -1, FudgeDateTime.Precision.Nanosecond));
            Assert.Throws<ArgumentOutOfRangeException>(() => new FudgeTime(1, 5, 13, 1000000000, FudgeDateTime.Precision.Nanosecond));
            Assert.Throws<ArgumentOutOfRangeException>(() => new FudgeTime(1, 5, 23, 13));      // Must be a multiple of 15
            Assert.Throws<ArgumentOutOfRangeException>(() => new FudgeTime(1, 5, 23, -1920));
            Assert.Throws<ArgumentOutOfRangeException>(() => new FudgeTime(1, 5, 23, 1920));
            Assert.Throws<ArgumentOutOfRangeException>(() => new FudgeTime(1, 5, 23, 123, FudgeDateTime.Precision.Day));

            Assert.Throws<ArgumentOutOfRangeException>(() => new FudgeTime(FudgeDateTime.Precision.Nanosecond, -1, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => new FudgeTime(FudgeDateTime.Precision.Nanosecond, 24 * 60 * 60, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => new FudgeTime(FudgeDateTime.Precision.Nanosecond, 0, -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => new FudgeTime(FudgeDateTime.Precision.Nanosecond, 0, 1000000000));
            Assert.Throws<ArgumentOutOfRangeException>(() => new FudgeTime(FudgeDateTime.Precision.Nanosecond, 0, 0, 3));
            Assert.Throws<ArgumentOutOfRangeException>(() => new FudgeTime(FudgeDateTime.Precision.Nanosecond, 0, 0, -4500));
            Assert.Throws<ArgumentOutOfRangeException>(() => new FudgeTime(FudgeDateTime.Precision.Nanosecond, 0, 0, 4500));
            Assert.Throws<ArgumentOutOfRangeException>(() => new FudgeTime(FudgeDateTime.Precision.Month, 0, 0));
        }

        [Fact]
        public void Totals()
        {
            var t = new FudgeTime(1, 2, 3, 123456789, FudgeDateTime.Precision.Nanosecond);
            Assert.Equal(3723, t.TotalSeconds);
            Assert.Equal(3723123456789L, t.TotalNanoseconds);
        }

        [Fact]
        public void StringFormats()
        {
            Assert.Equal("10:00:05.987654321", new FudgeTime(10, 0, 5, 987654321, FudgeDateTime.Precision.Nanosecond).ToString());
            Assert.Equal("10:00:05.987654", new FudgeTime(10, 0, 5, 987654321, FudgeDateTime.Precision.Microsecond).ToString());
            Assert.Equal("10:00:05.987", new FudgeTime(10, 0, 5, 987654321, FudgeDateTime.Precision.Millisecond).ToString());
            Assert.Equal("10:00:05", new FudgeTime(10, 0, 5).ToString());
            Assert.Equal("10:00", new FudgeTime(10, 0).ToString());
            Assert.Equal("10", new FudgeTime(10).ToString());

            Assert.Equal("10:00:05 -01:15", new FudgeTime(10, 0, 5, -75).ToString());
            Assert.Equal("10:00:05 +00:00", new FudgeTime(10, 0, 5, 0).ToString());
            Assert.Equal("10:00:05 +04:00", new FudgeTime(10, 0, 5, 240).ToString());
        }

        [Fact]
        public void ObjectOverrides()
        {
            Assert.Equal(new FudgeTime(1, 2, 3).GetHashCode(), new FudgeTime(1, 2, 3).GetHashCode());
            Assert.NotEqual(new FudgeTime(1, 2, 3).GetHashCode(), new FudgeTime(1, 2, 4).GetHashCode());
            Assert.NotEqual(new FudgeTime(1, 2, 3, 1234, FudgeDateTime.Precision.Nanosecond).GetHashCode(),
                            new FudgeTime(1, 2, 3, 1235, FudgeDateTime.Precision.Nanosecond).GetHashCode());
            Assert.NotEqual(new FudgeTime(1, 2, 3, 60).GetHashCode(), new FudgeTime(1, 2, 3, 45).GetHashCode());

            Assert.False(new FudgeTime(1, 2, 3).Equals(null));
            Assert.False(new FudgeTime(1, 2, 3).Equals("fred"));
            Assert.True(new FudgeTime(1, 2, 3, 45789, 60, FudgeDateTime.Precision.Nanosecond).Equals(new FudgeTime(1, 2, 3, 45789, 60, FudgeDateTime.Precision.Nanosecond)));
            Assert.False(new FudgeTime(1, 2, 4, 45789, 60, FudgeDateTime.Precision.Nanosecond).Equals(new FudgeTime(1, 2, 3, 45789, 60, FudgeDateTime.Precision.Nanosecond)));
            Assert.False(new FudgeTime(1, 2, 3, 45780, 60, FudgeDateTime.Precision.Nanosecond).Equals(new FudgeTime(1, 2, 3, 45789, 60, FudgeDateTime.Precision.Nanosecond)));
            Assert.False(new FudgeTime(1, 2, 3, 45789, 45, FudgeDateTime.Precision.Nanosecond).Equals(new FudgeTime(1, 2, 3, 45789, 60, FudgeDateTime.Precision.Nanosecond)));
            Assert.False(new FudgeTime(1, 2, 3, 45789, 60, FudgeDateTime.Precision.Millisecond).Equals(new FudgeTime(1, 2, 3, 45789, 60, FudgeDateTime.Precision.Nanosecond)));
            Assert.False(new FudgeTime(1, 2, 3, 45789, FudgeDateTime.Precision.Nanosecond).Equals(new FudgeTime(1, 2, 3, 45789, 0, FudgeDateTime.Precision.Nanosecond)));
        }
    }
}
