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
using Fudge.Serialization;

namespace Fudge.Tests.Unit.Serialization.Reflection
{
    public class CollectionSurrogateBaseTest
    {
        [Fact]
        public void SerializingNullsInLists_FRN52()
        {
            var context = new FudgeContext();

            var testClass = new TestClass { List = new List<SomeClass>() };
            testClass.List.Add(new SomeClass { Name = "A" });
            testClass.List.Add(null);
            testClass.List.Add(new SomeClass { Name = "B" });
            testClass.Array = new SomeInlineClass[] { new SomeInlineClass { Name = "C" }, null, new SomeInlineClass { Name = "D" } };

            var serializer = new FudgeSerializer(context);
            var msgs = serializer.SerializeToMsgs(testClass);
            var testClass2 = (TestClass)serializer.Deserialize(msgs);

            Assert.Equal("A", testClass2.List[0].Name);
            Assert.Null(testClass2.List[1]);
            Assert.Equal("B", testClass2.List[2].Name);
            Assert.Equal("C", testClass2.Array[0].Name);
            Assert.Null(testClass2.Array[1]);
            Assert.Equal("D", testClass2.Array[2].Name);
        }

        private class TestClass
        {
            public List<SomeClass> List { get; set; }
            public SomeInlineClass[] Array { get; set; }
        }

        private class SomeClass
        {
            public string Name {get;set;}
        }

        [FudgeInline]
        private class SomeInlineClass
        {
            public string Name { get; set; }
        }
    }
}
