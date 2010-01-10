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
    public class FudgeDateTest
    {
        [Fact]
        public void VariousConstructors()
        {
            Assert.Equal(20010304, new FudgeDate(20010304).RawValue);
            Assert.Equal(19991012, new FudgeDate(1999, 10, 12).RawValue);
            Assert.Equal(-19991012, new FudgeDate(-1999, 10, 12).RawValue);

            var dateTime = new DateTime(2000, 1, 2);
            Assert.Equal(20000102, new FudgeDate(dateTime).RawValue);
        }

        [Fact]
        public void ConstructorRangeChecking()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new FudgeDate(1000000, 1, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => new FudgeDate(1000, 0, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => new FudgeDate(1000, 13, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => new FudgeDate(1000, 1, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => new FudgeDate(1000, 1, 32));
        }

        [Fact]
        public void YearMonthDay()
        {
            var date = new FudgeDate(19701211);
            Assert.Equal(1970, date.Year);
            Assert.Equal(12, date.Month);
            Assert.Equal(11, date.Day);

            var date2 = new FudgeDate(-12340102);
            Assert.Equal(-1234, date2.Year);
            Assert.Equal(1, date2.Month);
            Assert.Equal(2, date2.Day);
        }

        [Fact]
        public void IsValid()
        {
            Assert.True(new FudgeDate(20010305).IsValid);

            Assert.False(new FudgeDate(20010100).IsValid);
            Assert.False(new FudgeDate(20010132).IsValid);
            Assert.False(new FudgeDate(20010010).IsValid);
            Assert.False(new FudgeDate(20011310).IsValid);
            Assert.False(new FudgeDate(00000101).IsValid);
            Assert.False(new FudgeDate(-00000101).IsValid);
            
            Assert.False(new FudgeDate(20010230).IsValid);

            // Leap years
            Assert.True(new FudgeDate(20000229).IsValid);
            Assert.False(new FudgeDate(19990229).IsValid);
            Assert.False(new FudgeDate(19000229).IsValid);

            // Need to do negatives
        }

        [Fact]
        public void RollToValidDate()
        {
            Assert.Equal("1999-03-01", new FudgeDate(19990229).RollToValidDate().ToString());
            Assert.Equal("2000-01-01", new FudgeDate(19991232).RollToValidDate().ToString());

            Assert.Equal("0001-01-01", new FudgeDate(00000101).RollToValidDate().ToString());
            Assert.Equal("0001-01-01", new FudgeDate(-00000101).RollToValidDate().ToString());
            Assert.Equal("0001-01-01", new FudgeDate(-00011232).RollToValidDate().ToString());

            Assert.Equal("-0099-01-01", new FudgeDate(-01001232).RollToValidDate().ToString());
        }

        [Fact]
        public void ToDateTime()
        {
            var dateTime = new FudgeDate(20030104).ToDateTime();
            Assert.Equal("2003-01-04T00:00:00.0000000", dateTime.ToString("o"));
            Assert.Equal(DateTimeKind.Unspecified, dateTime.Kind);

            // Check rolls to next valid day
            var dateTime2 = new FudgeDate(20030230).ToDateTime();
            Assert.Equal("2003-03-01T00:00:00.0000000", dateTime2.ToString("o"));
        }

        [Fact]
        public void VariousToString()
        {
            Assert.Equal("1000-01-01", new FudgeDate(10000101).ToString());
            Assert.Equal("0010-01-01", new FudgeDate(00100101).ToString());
            Assert.Equal("-1000-01-01", new FudgeDate(-10000101).ToString());

            Assert.Equal("1001-01-01", new FudgeDate(10010101).ToString(FudgeDateTimePrecision.Day));
            Assert.Equal("1001-01", new FudgeDate(10010101).ToString(FudgeDateTimePrecision.Month));
            Assert.Equal("1001", new FudgeDate(10010101).ToString(FudgeDateTimePrecision.Year));
            Assert.Equal("1000", new FudgeDate(10010101).ToString(FudgeDateTimePrecision.Century));    // REVIEW 20100110 t0rx -- Is this the right behaviour for centuries?
        }

        [Fact]
        public void Comparison()
        {
            Assert.Equal(-1, new FudgeDate(19990102).CompareTo(new FudgeDate(20000102)));
            Assert.Equal(-1, new FudgeDate(19990102).CompareTo(new FudgeDate(19990202)));
            Assert.Equal(-1, new FudgeDate(19990102).CompareTo(new FudgeDate(19990103)));
            Assert.Equal(0, new FudgeDate(19990102).CompareTo(new FudgeDate(19990102)));
            Assert.Equal(1, new FudgeDate(20000102).CompareTo(new FudgeDate(19990102)));
            Assert.Equal(1, new FudgeDate(19990202).CompareTo(new FudgeDate(19990102)));
            Assert.Equal(1, new FudgeDate(19990103).CompareTo(new FudgeDate(19990102)));

            Assert.Equal(-1, new FudgeDate(-00010101).CompareTo(new FudgeDate(00010101)));

            Assert.Equal(-1, new FudgeDate(-19990102).CompareTo(new FudgeDate(-19990103)));
            Assert.Equal(-1, new FudgeDate(-19990102).CompareTo(new FudgeDate(-19990202)));
            Assert.Equal(1, new FudgeDate(-19990102).CompareTo(new FudgeDate(-20000102)));  // -1999 was after -2000
            Assert.Equal(0, new FudgeDate(-19990102).CompareTo(new FudgeDate(-19990102)));
            Assert.Equal(1, new FudgeDate(-19990103).CompareTo(new FudgeDate(-19990102)));
            Assert.Equal(1, new FudgeDate(-19990202).CompareTo(new FudgeDate(-19990102)));
            Assert.Equal(-1, new FudgeDate(-20000102).CompareTo(new FudgeDate(-19990102)));  // -1999 was after -2000
        }

        [Fact]
        public void ObjectOverrides()
        {
            Assert.True(new FudgeDate(12340310).Equals(new FudgeDate(12340310)));
            Assert.False(new FudgeDate(12340310).Equals(new FudgeDate(19990407)));
            Assert.False(new FudgeDate(12340310).Equals(null));
            Assert.False(new FudgeDate(12340310).Equals("Fred"));

            Assert.Equal(new FudgeDate(12340310).GetHashCode(), new FudgeDate(12340310).GetHashCode());
            Assert.NotEqual(new FudgeDate(12340310).GetHashCode(), new FudgeDate(12340311).GetHashCode());

            Assert.Equal("1234-10-01", new FudgeDate(12341001).ToString());
            Assert.Equal("0001-10-01", new FudgeDate(00011001).ToString());
            Assert.Equal("-1234-10-01", new FudgeDate(-12341001).ToString());
            Assert.Equal("-0001-10-01", new FudgeDate(-00011001).ToString());
        }
    }
}
