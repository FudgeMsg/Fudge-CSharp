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
using Xunit;
using System.IO;
using OpenGamma.Fudge.Util;
using OpenGamma.Fudge.Types;

namespace OpenGamma.Fudge.Tests.Unit.Types
{
    public class StringArrayFieldTypeTest
    {
        [Fact]
        public void SimpleExample()
        {
            string[] array = { "Fred", "Bob" };
            var fieldType = new StringArrayFieldType();

            var stream = new MemoryStream();
            var writer = new FudgeBinaryWriter(stream);

            int len = fieldType.GetVariableSize(array, null);
            fieldType.WriteValue(writer, array, null);

            byte[] bytes = stream.ToArray();

            Assert.Equal(len, bytes.Length);

            stream = new MemoryStream(bytes);
            var reader = new FudgeBinaryReader(stream);

            var array2 = fieldType.ReadTypedValue(reader, len, null);
            Assert.Equal(array, array2);
        }
    }
}
