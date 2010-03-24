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
using System.Runtime.Serialization;
using Xunit;
using Fudge.Serialization;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using Fudge.Serialization.Reflection;

namespace Fudge.Tests.Unit.Serialization.Reflection
{
    public class DataContractSurrogateTest
    {
        private readonly FudgeContext context = new FudgeContext();

        [Fact]
        public void SimpleCase()
        {
            var obj1 = new SimpleTestClass { SerializedMember = "Serialized", UnserializedMember = "Unserialized", SerializedProperty = 1, UnserializedProperty = 2 };

            var serializer = new FudgeSerializer(context);
            var msg = serializer.SerializeToMsg(obj1);

            var obj2 = (SimpleTestClass)serializer.Deserialize(msg);

            Assert.Equal(obj1.SerializedMember, obj2.SerializedMember);
            Assert.Equal(obj1.SerializedProperty, obj2.SerializedProperty);
            
            // Make sure the others weren't serialized
            Assert.Null(obj2.UnserializedMember);
            Assert.Equal(0, obj2.UnserializedProperty);
        }

        [Fact]
        public void NamingConventionsApplied()
        {
            var obj1 = new SimpleTestClass { SerializedMember = "Serialized", UnserializedMember = "Unserialized", SerializedProperty = 1, UnserializedProperty = 2 };

            var serializer = new FudgeSerializer(context);
            serializer.TypeMap.FieldNameConvention = FudgeFieldNameConvention.AllUpperCase;

            var msg = serializer.SerializeToMsg(obj1);

            Assert.NotNull(msg.GetByName("SERIALIZEDMEMBER"));
        }

        [Fact]
        public void HonoursDataMemberName()
        {
            var obj1 = new SimpleTestClass { SerializedMember = "Serialized", UnserializedMember = "Unserialized", SerializedProperty = 1, UnserializedProperty = 2 };

            var serializer = new FudgeSerializer(context);
            var msg = serializer.SerializeToMsg(obj1);

            Assert.NotNull(msg.GetByName("Prop"));
        }

        [Fact]
        public void HonoursBeforeAndAfterSerialize_FRN78()
        {
            var obj1 = new ClassWithBeforeAndAfterMethods { Val1 = "Test" };

            var serializer = new FudgeSerializer(context);
            var msg = serializer.SerializeToMsg(obj1);
            Assert.Equal("Before|Test", msg.GetString("Val1"));
            Assert.Equal("Before|Test|After", obj1.Val1);

            var obj2 = (ClassWithBeforeAndAfterMethods)serializer.Deserialize(msg);
            Assert.Equal("Before|Test", obj2.Val1);
            Assert.Equal("null|After2", obj2.Val2);
        }

        // Logged as [FRN-79]
        //[Fact]
        //public void HonoursDataContractName_FRN79()
        //{
        //    Assert.True(false, "Test not implemented yet");
        //}

        [Fact]
        public void ConstructorArgChecking()
        {
            var typeData = new TypeData(context, new TypeDataCache(context), typeof(SimpleTestClass), FudgeFieldNameConvention.Identity);
            var badTypeData = new TypeData(context, new TypeDataCache(context), GetType(), FudgeFieldNameConvention.Identity);
            Assert.Throws<ArgumentNullException>(() => new DataContractSurrogate(null, typeData));
            Assert.Throws<ArgumentNullException>(() => new DataContractSurrogate(context, null));
            Assert.Throws<ArgumentOutOfRangeException>(() => new DataContractSurrogate(context, badTypeData));
        }

        #region Test classes

        [DataContract]
        private class SimpleTestClass
        {
            [DataMember]
            private string serializedMember;

            private string unserializedMember;

            public SimpleTestClass()
            {
            }

            public string SerializedMember
            {
                get { return serializedMember; }
                set { serializedMember = value; }
            }

            public string UnserializedMember
            {
                get { return unserializedMember; }
                set { unserializedMember = value; }
            }

            [DataMember(Name="Prop")]
            public int SerializedProperty
            {
                get;
                set;
            }

            public int UnserializedProperty
            {
                get;
                set;
            }
        }

        [DataContract]
        private class ClassWithBeforeAndAfterMethods
        {
            public ClassWithBeforeAndAfterMethods()
            {
            }

            [DataMember]
            public string Val1 { get; set; }

            public string Val2 { get; set; }

            [OnSerializing]
            private void BeforeSerialize(StreamingContext sc)
            {
                Val1 = "Before|" + Val1;
            }

            [OnSerialized]
            private void AfterSerialize(StreamingContext sc)
            {
                Val1 = Val1 + "|After";
            }

            [OnDeserializing]
            private void BeforeDeserialize(StreamingContext sc)
            {
                Val2 = Val1 ?? "null";
            }

            [OnDeserialized]
            private void AfterDeserialize(StreamingContext sc)
            {
                Val2 = Val2 + "|After2";
            }
        }

        #endregion
    }
}
