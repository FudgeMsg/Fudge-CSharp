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

            Assert.Equal("1999-12-10 03:04:05.987654321 -01:15", msg2.GetValue<FudgeDateTime>("dt").ToString());
        }

        [Fact]
        public void MakeSureSizesLineUp()
        {
            // Just in case these ever change in the future
            Assert.Equal(DateTimeFieldType.Instance.FixedSize, TimeFieldType.Instance.FixedSize + DateFieldType.Instance.FixedSize);
        }
    }
}
