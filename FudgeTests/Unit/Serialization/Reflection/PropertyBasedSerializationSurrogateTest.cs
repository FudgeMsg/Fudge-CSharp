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
using Fudge.Serialization.Reflection;
using Fudge.Serialization;
using Fudge.Encodings;
using System.IO;
using Fudge.Types;

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

            var msg = serializer.SerializeToMsg(obj1);
            var obj2 = (SimpleExampleClass)serializer.Deserialize(msg);

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

            var msg = serializer.SerializeToMsg(obj1);
            var obj2 = (SecondaryTypeClass)serializer.Deserialize(msg);

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

            var msg = serializer.SerializeToMsg(obj1);

            // Check the serialized format
            Assert.Equal(FudgeMsgFieldType.Instance, msg.GetByName("Names").Type);
            var listMsg = msg.GetMessage("Names");
            Assert.Equal("FudgeMsg[ => Fred,  => Sheila]", listMsg.ToString());

            var obj2 = (PrimitiveListClass)serializer.Deserialize(msg);

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

            var msg = serializer.SerializeToMsg(obj1);
            var obj2 = (SubObjectClass)serializer.Deserialize(msg);

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

            var msg = serializer.SerializeToMsg(obj1);
            var obj2 = (ListOfObjectsClass)serializer.Deserialize(msg);

            Assert.NotSame(obj1, obj2);
            Assert.NotSame(obj1.Subs[0], obj2.Subs[0]);
            Assert.Equal(obj1.Subs[0].Name, obj2.Subs[0].Name);
        }

        [Fact]
        public void ArrayOfSubObjects()
        {
            Assert.True(PropertyBasedSerializationSurrogate.CanHandle(typeDataCache, FudgeFieldNameConvention.Identity, typeof(ArrayOfObjectsClass)));

            var serializer = new FudgeSerializer(context);      // We're relying on it auto-discovering the type surrogate

            var obj1 = new ArrayOfObjectsClass();
            obj1.Subs = new SimpleExampleClass[] {new SimpleExampleClass { Name = "Bob", Age = 21 }};

            var msg = serializer.SerializeToMsg(obj1);
            var obj2 = (ArrayOfObjectsClass)serializer.Deserialize(msg);

            Assert.NotSame(obj1, obj2);
            Assert.NotSame(obj1.Subs[0], obj2.Subs[0]);
            Assert.Equal(obj1.Subs[0].Name, obj2.Subs[0].Name);
        }

        [Fact]
        public void ListOfArrays()
        {
            Assert.True(PropertyBasedSerializationSurrogate.CanHandle(typeDataCache, FudgeFieldNameConvention.Identity, typeof(ListOfArraysClass)));

            var serializer = new FudgeSerializer(context);      // We're relying on it auto-discovering the type surrogate

            var obj1 = new ListOfArraysClass();
            obj1.List = new List<string[]>();
            obj1.List.Add(new string[] { "Bob", "Mavis" });

            var msg = serializer.SerializeToMsg(obj1);
            var obj2 = (ListOfArraysClass)serializer.Deserialize(msg);

            Assert.NotSame(obj1, obj2);
            Assert.NotSame(obj1.List[0], obj2.List[0]);
            Assert.Equal(obj1.List[0], obj2.List[0]);
        }

        [Fact]
        public void Dictionaries()
        {
            Assert.True(PropertyBasedSerializationSurrogate.CanHandle(typeDataCache, FudgeFieldNameConvention.Identity, typeof(DictionaryClass)));

            var serializer = new FudgeSerializer(context);      // We're relying on it auto-discovering the type surrogate

            var obj1 = new DictionaryClass();
            obj1.Map = new Dictionary<string, SimpleExampleClass>();
            obj1.Map["Fred"] = new SimpleExampleClass { Name = "Fred", Age = 23 };
            obj1.Map["Jemima"] = new SimpleExampleClass { Name = "Jemima", Age = 17 };

            var msg = serializer.SerializeToMsg(obj1);
            var obj2 = (DictionaryClass)serializer.Deserialize(msg);

            Assert.NotSame(obj1, obj2);
            Assert.NotSame(obj1.Map, obj2.Map);
            Assert.Equal(obj1.Map["Fred"], obj2.Map["Fred"]);
            Assert.Equal(obj1.Map["Jemima"], obj2.Map["Jemima"]);
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

            var msg = serializer.SerializeToMsg(obj1);

            StaticTransientClass.Static = 19;
            var obj2 = (StaticTransientClass)serializer.Deserialize(msg);

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

            var msg = serializer.SerializeToMsg(obj1);
            Assert.Null(msg.GetString("Name"));
            Assert.Equal("Albert", msg.GetString("nom"));
            Assert.Equal(72, msg.GetInt("Age"));

            var obj2 = (RenameFieldClass)serializer.Deserialize(msg);

            Assert.NotSame(obj1, obj2);
            Assert.Equal(obj1.Name, obj2.Name);
            Assert.Equal(obj1.Age, obj2.Age);
        }

        [Fact]
        public void FieldNameConventionsWithAttribute()
        {
            var obj1 = new FieldConventionAttributeClass { MyName = "Fred" };              // Specifies camelCase
            var serializer = new FudgeSerializer(context);
            
            var msg = serializer.SerializeToMsg(obj1);
            Assert.NotNull(msg.GetByName("MYNAME"));
            
            var obj2 = (FieldConventionAttributeClass)serializer.Deserialize(msg);
            Assert.Equal(obj1.MyName, obj2.MyName);
        }

        [Fact]
        public void FieldNameConventionsWithContextProperty()
        {
            var context = new FudgeContext();           // So we don't mess with other unit tests
            var obj1 = new FieldConventionClass {MyName = "Bobby", myAge = 6};
            FudgeMsg msg;
            FudgeSerializer serializer;

            // Default is identity
            serializer = new FudgeSerializer(context);
            Assert.Equal(FudgeFieldNameConvention.Identity, serializer.TypeMap.FieldNameConvention);
            msg = serializer.SerializeToMsg(obj1);
            Assert.Equal("Bobby", msg.GetString("MyName"));
            Assert.Equal(6, msg.GetInt("myAge"));
            Assert.Equal(obj1, serializer.Deserialize(msg));

            context.SetProperty(ContextProperties.FieldNameConventionProperty, FudgeFieldNameConvention.AllLowerCase);
            serializer = new FudgeSerializer(context);
            msg = serializer.SerializeToMsg(obj1);
            Assert.Equal("Bobby", msg.GetString("myname"));
            Assert.Equal(6, msg.GetInt("myage"));
            Assert.Equal(obj1, serializer.Deserialize(msg));

            context.SetProperty(ContextProperties.FieldNameConventionProperty, FudgeFieldNameConvention.AllUpperCase);
            serializer = new FudgeSerializer(context);
            msg = serializer.SerializeToMsg(obj1);
            Assert.Equal("Bobby", msg.GetString("MYNAME"));
            Assert.Equal(6, msg.GetInt("MYAGE"));
            Assert.Equal(obj1, serializer.Deserialize(msg));

            context.SetProperty(ContextProperties.FieldNameConventionProperty, FudgeFieldNameConvention.CamelCase);
            serializer = new FudgeSerializer(context);
            msg = serializer.SerializeToMsg(obj1);
            Assert.Equal("Bobby", msg.GetString("myName"));
            Assert.Equal(6, msg.GetInt("myAge"));
            Assert.Equal(obj1, serializer.Deserialize(msg));

            context.SetProperty(ContextProperties.FieldNameConventionProperty, FudgeFieldNameConvention.PascalCase);
            serializer = new FudgeSerializer(context);
            msg = serializer.SerializeToMsg(obj1);
            Assert.Equal("Bobby", msg.GetString("MyName"));
            Assert.Equal(6, msg.GetInt("MyAge"));
            Assert.Equal(obj1, serializer.Deserialize(msg));
        }

        [Fact]
        public void ConstuctorRangeChecking()
        {
            Assert.Throws<ArgumentNullException>(() => new PropertyBasedSerializationSurrogate(null, typeDataCache.GetTypeData(typeof(SimpleExampleClass), FudgeFieldNameConvention.Identity)));
            Assert.Throws<ArgumentNullException>(() => new PropertyBasedSerializationSurrogate(context, null));
            Assert.Throws<ArgumentOutOfRangeException>(() => new PropertyBasedSerializationSurrogate(context, typeDataCache.GetTypeData(typeof(NoDefaultConstructorClass), FudgeFieldNameConvention.Identity)));
        }

        public class SimpleExampleClass
        {
            public string Name { get; set; }
            public int Age { get; set; }

            public override bool Equals(object obj)
            {
                var other = (SimpleExampleClass)obj;
                return other.Name == this.Name && other.Age == this.Age;
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
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

        public class ArrayOfObjectsClass
        {
            public SimpleExampleClass[] Subs { get; set; }
        }

        public class ListOfArraysClass
        {
            public IList<string[]> List { get; set; }
        }

        public class DictionaryClass
        {
            public IDictionary<string, SimpleExampleClass> Map { get; set; }
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
            [FudgeFieldName("nom")]
            public string Name { get; set; }

            public int Age { get; set; }
        }

        [FudgeFieldNameConvention(FudgeFieldNameConvention.AllUpperCase)]
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
