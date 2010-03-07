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
    public class DateTimeFieldTypeTest
    {
        private FudgeContext context = new FudgeContext();

        [Fact]
        public void RoundTrip()
        {
            var dt = new FudgeDateTime(1999, 12, 10, 3, 4, 5, 987654321, -75, FudgeDateTimePrecision.Nanosecond);

            var msg1 = new FudgeMsg(context, new Field("dt", dt));
            var bytes = msg1.ToByteArray();
            var msg2 = context.Deserialize(bytes).Message;

            Assert.Equal("1999-12-10T03:04:05.987654321-01:15", msg2.GetValue<FudgeDateTime>("dt").ToString());
        }

        [Fact]
        public void MakeSureSizesLineUp()
        {
            // Just in case these ever change in the future
            Assert.Equal(DateTimeFieldType.Instance.FixedSize, TimeFieldType.Instance.FixedSize + DateFieldType.Instance.FixedSize);
        }

        [Fact]
        public void CheckSecondaryTypes()
        {
            var msg = new FudgeMsg(context);
            var dt = new DateTime(1987, 1, 13, 8, 6, 5);
            var dto = new DateTimeOffset(1822, 7, 20, 13, 2, 15, new TimeSpan(-2, 0, 0));

            msg.Add("dt", dt);
            msg.Add("dto", dto);

            Assert.Same(DateTimeFieldType.Instance, msg.GetByName("dt").Type);
            Assert.IsType<FudgeDateTime>(msg.GetByName("dt").Value);
            Assert.Same(DateTimeFieldType.Instance, msg.GetByName("dto").Type);
            Assert.IsType<FudgeDateTime>(msg.GetByName("dto").Value);

            Assert.Equal(dt, msg.GetValue<DateTime>("dt"));
            Assert.Equal(dto, msg.GetValue<DateTime>("dto"));
        }

        [Fact]
        public void Minimsation()
        {
            var msg = new FudgeMsg(context);
            var dt = new DateTime(1990, 2, 1);
            var fdt = new FudgeDateTime(1990, 2, 1, 0, 0, 0, 0, FudgeDateTimePrecision.Day);

            msg.Add("dt", dt);
            msg.Add("fdt", fdt);
            Assert.Same(DateFieldType.Instance, msg.GetByName("dt").Type);
            Assert.IsType<FudgeDate>(msg.GetByName("dt").Value);
            Assert.Same(DateFieldType.Instance, msg.GetByName("fdt").Type);
            Assert.IsType<FudgeDate>(msg.GetByName("fdt").Value);

            Assert.Equal(dt, msg.GetValue<DateTime>("dt"));
            Assert.Equal(fdt, msg.GetValue<FudgeDateTime>("fdt"));

            // Error cases
            FudgeFieldType type = null;
            Assert.Equal(null, DateTimeFieldType.Instance.Minimize(null, ref type));
            Assert.Throws<ArgumentException>(() => DateTimeFieldType.Instance.Minimize("fred", ref type));
        }
    }
}
