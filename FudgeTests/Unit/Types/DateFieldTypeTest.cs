﻿/* <!--
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
using Fudge.Encodings;
using System.IO;
using Fudge.Util;

namespace Fudge.Tests.Unit.Types
{
    public class DateFieldTypeTest
    {
        private FudgeContext context = new FudgeContext();

        [Fact]
        public void RoundTrip()
        {
            var d = new FudgeDate(1999, 12, 10);

            var msg1 = new FudgeMsg(context, new Field("d", d));
            var bytes = msg1.ToByteArray();
            var msg2 = context.Deserialize(bytes).Message;

            Assert.Equal("1999-12-10", msg2.GetValue<FudgeDate>("d").ToString());
        }

        [Fact]
        public void CheckActualBytes()
        {
            var d = new FudgeDate(2003, 01, 13);
            var stream = new MemoryStream();
            var writer = new FudgeBinaryWriter(stream);
            DateFieldType.Instance.WriteValue(writer, d);

            Assert.Equal("01-31-a2-a1", stream.ToArray().ToNiceString());       // That's 20030113 in hex
        }
    }
}