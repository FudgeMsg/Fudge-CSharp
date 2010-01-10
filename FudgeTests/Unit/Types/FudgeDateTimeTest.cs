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
    public class FudgeDateTimeTest
    {
        [Fact]
        public void VariousConstructors()
        {
            var dt = new DateTime(1978, 4, 3, 1, 17, 34, DateTimeKind.Unspecified);
            Assert.Equal("1978-04-03 01:17:34.000000000", new FudgeDateTime(dt).ToString());
            Assert.Equal("1978-04-03 01:17", new FudgeDateTime(dt, FudgeDateTimePrecision.Minute).ToString());
            Assert.Equal("1978-04", new FudgeDateTime(dt, FudgeDateTimePrecision.Month).ToString());

            Assert.Equal("1978-04-03 01:17:34.123456789", new FudgeDateTime(1978, 4, 3, 1, 17, 34, 123456789, FudgeDateTimePrecision.Nanosecond).ToString());
            Assert.Equal("1978-04-03", new FudgeDateTime(1978, 4, 3, 1, 17, 34, 123456789, FudgeDateTimePrecision.Day).ToString());
            Assert.Equal("1978-04-03 01:17:34.123456789 -01:30", new FudgeDateTime(1978, 4, 3, 1, 17, 34, 123456789, -90, FudgeDateTimePrecision.Nanosecond).ToString());
            Assert.Equal("1978-04-03", new FudgeDateTime(1978, 4, 3, 1, 17, 34, 123456789, -90, FudgeDateTimePrecision.Day).ToString());

            Assert.Equal("1978-04-03", new FudgeDateTime(new FudgeDate(19780403), null).ToString());
            Assert.Equal("1978-04-03 01:17:34", new FudgeDateTime(new FudgeDate(19780403), new FudgeTime(1, 17, 34)).ToString());
            Assert.Equal(FudgeDateTimePrecision.Second, new FudgeDateTime(new FudgeDate(19780403), new FudgeTime(1, 17, 34)).Precision);    // Make sure it picks up the precision from the time
        }

        [Fact]
        public void Properties()
        {
            var dt = new FudgeDateTime(1234, 5, 6, 7, 8, 9, 123456789, -60, FudgeDateTimePrecision.Nanosecond);
            Assert.Equal(12340506, dt.Date.RawValue);
            Assert.Equal("07:08:09.123456789 -01:00", dt.Time.ToString());
            Assert.Equal(FudgeDateTimePrecision.Nanosecond, dt.Precision);
            Assert.Equal(1234, dt.Year);
            Assert.Equal(5, dt.Month);
            Assert.Equal(6, dt.Day);
            Assert.Equal(7, dt.Hour);
            Assert.Equal(8, dt.Minute);
            Assert.Equal(9, dt.Second);
            Assert.Equal(123456789, dt.Nanoseconds);
            Assert.Equal(-60, dt.TimeZoneOffset);
            Assert.True(dt.IsValidDate);
        }

        [Fact]
        public void BasicToString()
        {
            var dt = new FudgeDateTime(2003, 11, 13, 12, 14, 34, 987654321, 0, FudgeDateTimePrecision.Nanosecond);
            Assert.Equal("2003-11-13 12:14:34.987654321 +00:00", dt.ToString());
        }
    }
}
