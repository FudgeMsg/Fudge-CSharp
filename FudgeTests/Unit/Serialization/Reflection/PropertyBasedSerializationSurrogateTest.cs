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
using Fudge.Serialization.Reflection;
using Fudge.Serialization;
using Fudge.Encodings;
using System.IO;

namespace Fudge.Tests.Unit.Serialization.Reflection
{
    public class PropertyBasedSerializationSurrogateTest
    {
        private readonly FudgeContext context = new FudgeContext();

        [Fact]
        public void SimpleExample()
        {
            Assert.True(PropertyBasedSerializationSurrogate.CanHandle(context, typeof(SimpleExampleClass)));

            var serializer = new FudgeSerializer(context);      // We're relying on it auto-discovering the type surrogate

            var obj1 = new SimpleExampleClass { Name = "Dennis", Age = 37 };

            var msgs = serializer.SerializeToMsgs(obj1);
            var obj2 = (SimpleExampleClass)serializer.Deserialize(msgs);

            Assert.NotSame(obj1, obj2);
            Assert.Equal(obj1.Name, obj2.Name);
            Assert.Equal(obj1.Age, obj2.Age);
        }

        [Fact]
        public void SecondaryTypes()
        {
            Assert.True(PropertyBasedSerializationSurrogate.CanHandle(context, typeof(SecondaryTypeClass)));

            var serializer = new FudgeSerializer(context);      // We're relying on it auto-discovering the type surrogate

            var obj1 = new SecondaryTypeClass { Id = Guid.NewGuid() };

            var msgs = serializer.SerializeToMsgs(obj1);
            var obj2 = (SecondaryTypeClass)serializer.Deserialize(msgs);

            Assert.NotSame(obj1, obj2);
            Assert.Equal(obj1.Id, obj2.Id);
        }

        [Fact]
        public void PrimitiveLists()
        {
            Assert.True(PropertyBasedSerializationSurrogate.CanHandle(context, typeof(PrimitiveListClass)));

            var serializer = new FudgeSerializer(context);      // We're relying on it auto-discovering the type surrogate

            var obj1 = new PrimitiveListClass();
            obj1.Names.Add("Fred");
            obj1.Names.Add("Sheila");

            var msgs = serializer.SerializeToMsgs(obj1);
            var obj2 = (PrimitiveListClass)serializer.Deserialize(msgs);

            Assert.NotSame(obj1, obj2);
            Assert.Equal(obj1.Names, obj2.Names);
        }

        [Fact]
        public void SubObjects()
        {
            Assert.True(PropertyBasedSerializationSurrogate.CanHandle(context, typeof(SubObjectClass)));

            var serializer = new FudgeSerializer(context);      // We're relying on it auto-discovering the type surrogate

            var obj1 = new SubObjectClass();
            obj1.Number = 17;
            obj1.Sub = new SimpleExampleClass { Name = "Bob", Age = 21 };

            var msgs = serializer.SerializeToMsgs(obj1);
            var obj2 = (SubObjectClass)serializer.Deserialize(msgs);

            Assert.NotSame(obj1, obj2);
            Assert.NotSame(obj1.Sub, obj2.Sub);
            Assert.Equal(obj1.Sub.Name, obj2.Sub.Name);
        }

        [Fact]
        public void UnhandleableCases()
        {
            Assert.False(PropertyBasedSerializationSurrogate.CanHandle(context, typeof(NoDefaultConstructorClass)));
            Assert.False(PropertyBasedSerializationSurrogate.CanHandle(context, typeof(NoSetterClass)));
        }

        // TODO 20100202 t0rx -- Test arrays
        // TODO 20100202 t0rx -- Test maps
        // TODO 20100202 t0rx -- Test object references

        public class SimpleExampleClass
        {
            public string Name { get; set; }
            public int Age { get; set; }
        }

        public class SecondaryTypeClass
        {
            public Guid Id { get; set; }
        }

        public class PrimitiveListClass
        {
            private List<string> names = new List<string>();

            public List<string> Names { get { return names; } }
        }

        public class SubObjectClass
        {
            public int Number { get; set; }
            public SimpleExampleClass Sub { get; set; }
        }

        public class NoDefaultConstructorClass
        {
            public NoDefaultConstructorClass(int i) { }
            public string Name { get; set; }
        }

        public class NoSetterClass
        {
            public string Name { get; private set; }
        }
    }
}
