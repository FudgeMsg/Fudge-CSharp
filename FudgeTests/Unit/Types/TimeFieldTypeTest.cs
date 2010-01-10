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
using System.IO;
using Fudge.Util;

namespace Fudge.Tests.Unit.Types
{
    public class TimeFieldTypeTest
    {
        private readonly FudgeContext context = new FudgeContext();

        [Fact]
        public void TestVariousOption()
        {
            Cycle(new FudgeTime(1, 2, 3, 123456789, 45, FudgeDateTimePrecision.Nanosecond));
            Cycle(new FudgeTime(1, 2, 3, 123456789, 45, FudgeDateTimePrecision.Millisecond));
            Cycle(new FudgeTime(1, 2, 3, 123456789, -45, FudgeDateTimePrecision.Millisecond));
            Cycle(new FudgeTime(1, 2, 3));
        }

        [Fact]
        public void CheckActualBytes()
        {
            var t = new FudgeTime(1, 2, 3, 123456789, 60, FudgeDateTimePrecision.Microsecond);
            var stream = new MemoryStream();
            var writer = new FudgeBinaryWriter(stream);
            TimeFieldType.Instance.WriteValue(writer, t);

            Assert.Equal("04-10-0e-8b-07-5b-cd-15", stream.ToArray().ToNiceString());
        }

        private void Cycle(FudgeTime t)
        {
            var msg1 = new FudgeMsg(context, new Field("t", t));
            var bytes = msg1.ToByteArray();
            var msg2 = context.Deserialize(bytes).Message;

            Assert.Equal(t, msg2.GetValue<FudgeTime>("t"));
        }
    }
}
