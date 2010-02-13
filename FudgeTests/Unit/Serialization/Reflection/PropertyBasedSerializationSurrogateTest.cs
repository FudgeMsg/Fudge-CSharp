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
        private readonly TypeDataCache typeDataCache;

        public PropertyBasedSerializationSurrogateTest()
        {
            typeDataCache = new TypeDataCache(context);
        }

        [Fact]
        public void SimpleExample()
        {
            Assert.True(PropertyBasedSerializationSurrogate.CanHandle(typeDataCache, FudgeFieldNameConvention.Identity, typeof(SimpleExampleClass)));

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
            Assert.True(PropertyBasedSerializationSurrogate.CanHandle(typeDataCache, FudgeFieldNameConvention.Identity, typeof(SecondaryTypeClass)));

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
            Assert.True(PropertyBasedSerializationSurrogate.CanHandle(typeDataCache, FudgeFieldNameConvention.Identity, typeof(PrimitiveListClass)));

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
            Assert.True(PropertyBasedSerializationSurrogate.CanHandle(typeDataCache, FudgeFieldNameConvention.Identity, typeof(SubObjectClass)));

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
        public void ListOfSubObjects()
        {
            Assert.True(PropertyBasedSerializationSurrogate.CanHandle(typeDataCache, FudgeFieldNameConvention.Identity, typeof(ListOfObjectsClass)));

            var serializer = new FudgeSerializer(context);      // We're relying on it auto-discovering the type surrogate

            var obj1 = new ListOfObjectsClass();
            obj1.Subs.Add(new SimpleExampleClass { Name = "Bob", Age = 21 });

            var msgs = serializer.SerializeToMsgs(obj1);
            var obj2 = (ListOfObjectsClass)serializer.Deserialize(msgs);

            Assert.NotSame(obj1, obj2);
            Assert.NotSame(obj1.Subs[0], obj2.Subs[0]);
            Assert.Equal(obj1.Subs[0].Name, obj2.Subs[0].Name);
        }

        [Fact]
        public void UnhandleableCases()
        {
            Assert.False(PropertyBasedSerializationSurrogate.CanHandle(typeDataCache, FudgeFieldNameConvention.Identity, typeof(NoDefaultConstructorClass)));
            Assert.False(PropertyBasedSerializationSurrogate.CanHandle(typeDataCache, FudgeFieldNameConvention.Identity, typeof(NoSetterClass)));
        }

        [Fact]
        public void StaticAndTransient()
        {
            Assert.True(PropertyBasedSerializationSurrogate.CanHandle(typeDataCache, FudgeFieldNameConvention.Identity, typeof(StaticTransientClass)));

            var serializer = new FudgeSerializer(context);      // We're relying on it auto-discovering the type surrogate

            StaticTransientClass.Static = 17;
            var obj1 = new StaticTransientClass {Transient = "Hello"};

            var msgs = serializer.SerializeToMsgs(obj1);

            StaticTransientClass.Static = 19;
            var obj2 = (StaticTransientClass)serializer.Deserialize(msgs);

            Assert.NotSame(obj1, obj2);
            Assert.Equal(null, obj2.Transient);
            Assert.Equal(19, StaticTransientClass.Static);
        }

        [Fact]
        public void RenamingFields()
        {
            Assert.True(PropertyBasedSerializationSurrogate.CanHandle(typeDataCache, FudgeFieldNameConvention.Identity, typeof(RenameFieldClass)));

            var serializer = new FudgeSerializer(context);      // We're relying on it auto-discovering the type surrogate

            var obj1 = new RenameFieldClass { Name = "Albert", Age = 72 };

            var msgs = serializer.SerializeToMsgs(obj1);
            Assert.Null(msgs[1].GetString("Name"));
            Assert.Equal("Albert", msgs[1].GetString("name"));
            Assert.Equal(72, msgs[1].GetInt("Age"));

            var obj2 = (RenameFieldClass)serializer.Deserialize(msgs);

            Assert.NotSame(obj1, obj2);
            Assert.Equal(obj1.Name, obj2.Name);
            Assert.Equal(obj1.Age, obj2.Age);
        }

        [Fact]
        public void FieldNameConventionsWithAttribute()
        {
            var obj1 = new FieldConventionAttributeClass { MyName = "Fred" };              // Specifies camelCase
            var serializer = new FudgeSerializer(context);
            
            var msgs = serializer.SerializeToMsgs(obj1);
            Assert.NotNull(msgs[1].GetByName("myName"));
            
            var obj2 = (FieldConventionAttributeClass)serializer.Deserialize(msgs);
            Assert.Equal(obj1.MyName, obj2.MyName);
        }

        [Fact]
        public void FieldNameConventionsWithContextProperty()
        {
            var context = new FudgeContext();           // So we don't mess with other unit tests
            var obj1 = new FieldConventionClass {MyName = "Bobby", myAge = 6};
            IList<FudgeMsg> msgs;
            FudgeSerializer serializer;

            serializer = new FudgeSerializer(context);
            Assert.Equal(FudgeFieldNameConvention.Identity, serializer.TypeMap.FieldNameConvention);
            msgs = serializer.SerializeToMsgs(obj1);
            Assert.Equal("Bobby", msgs[1].GetString("MyName"));
            Assert.Equal(6, msgs[1].GetInt("myAge"));
            Assert.Equal(obj1, serializer.Deserialize(msgs));

            context.SetProperty(SerializationTypeMap.FieldNameConventionProperty, FudgeFieldNameConvention.AllLowerCase);
            serializer = new FudgeSerializer(context);
            msgs = serializer.SerializeToMsgs(obj1);
            Assert.Equal("Bobby", msgs[1].GetString("myname"));
            Assert.Equal(6, msgs[1].GetInt("myage"));
            Assert.Equal(obj1, serializer.Deserialize(msgs));

            context.SetProperty(SerializationTypeMap.FieldNameConventionProperty, FudgeFieldNameConvention.AllUpperCase);
            serializer = new FudgeSerializer(context);
            msgs = serializer.SerializeToMsgs(obj1);
            Assert.Equal("Bobby", msgs[1].GetString("MYNAME"));
            Assert.Equal(6, msgs[1].GetInt("MYAGE"));
            Assert.Equal(obj1, serializer.Deserialize(msgs));

            context.SetProperty(SerializationTypeMap.FieldNameConventionProperty, FudgeFieldNameConvention.CamelCase);
            serializer = new FudgeSerializer(context);
            msgs = serializer.SerializeToMsgs(obj1);
            Assert.Equal("Bobby", msgs[1].GetString("myName"));
            Assert.Equal(6, msgs[1].GetInt("myAge"));
            Assert.Equal(obj1, serializer.Deserialize(msgs));

            context.SetProperty(SerializationTypeMap.FieldNameConventionProperty, FudgeFieldNameConvention.PascalCase);
            serializer = new FudgeSerializer(context);
            msgs = serializer.SerializeToMsgs(obj1);
            Assert.Equal("Bobby", msgs[1].GetString("MyName"));
            Assert.Equal(6, msgs[1].GetInt("MyAge"));
            Assert.Equal(obj1, serializer.Deserialize(msgs));
        }

        // TODO 2010-02-02 t0rx -- Test arrays
        // TODO 2010-02-02 t0rx -- Test maps
        // TODO 2010-02-02 t0rx -- Test object references

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

        public class ListOfObjectsClass
        {
            private readonly List<SimpleExampleClass> subs = new List<SimpleExampleClass>();

            public IList<SimpleExampleClass> Subs { get { return subs; } }
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

        public class StaticTransientClass
        {
            public static int Static { get; set; }

            [FudgeTransient]
            public string Transient { get; set; }
        }

        public class RenameFieldClass
        {
            [FudgeFieldName("name")]
            public string Name { get; set; }

            public int Age { get; set; }
        }

        [FudgeFieldNameConvention(FudgeFieldNameConvention.CamelCase)]
        public class FieldConventionAttributeClass
        {
            public string MyName { get; set; }
        }

        public class FieldConventionClass
        {
            // Mix up the cases
            public string MyName { get; set; }
            public int myAge { get; set; }

            public override bool Equals(object obj)
            {
                var other = (FieldConventionClass)obj;
                return MyName == other.MyName && myAge == other.myAge;
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
        }
    }
}
