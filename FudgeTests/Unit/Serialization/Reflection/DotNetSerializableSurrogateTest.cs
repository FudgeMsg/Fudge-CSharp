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
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Xml.Serialization;

namespace Fudge.Tests.Unit.Serialization.Reflection
{
    public class DotNetSerializableSurrogateTest
    {
        private readonly FudgeContext context = new FudgeContext();

        [Fact]
        public void SimpleCase()
        {
            var obj1 = new SimpleTestClass { Val = "Test" };

            var serializer = new FudgeSerializer(context);
            var msg = serializer.SerializeToMsg(obj1);

            Assert.Equal("Test", msg.GetString("serializedVal"));

            var obj2 = (SimpleTestClass)serializer.Deserialize(msg);
            Assert.Equal("Test", obj2.Val);
        }

        // Logged this to deal with later as FRN-73
        //[Fact]
        //public void UsesIDeserializationCallback()
        //{
        //    var obj1 = new ClassWithIDeserializationCallback { Val = "Test2" };

        //    var serializer = new FudgeSerializer(context);
        //    var msg = serializer.SerializeToMsg(obj1);

        //    var obj2 = (ClassWithIDeserializationCallback)serializer.Deserialize(msg);
        //    Assert.Equal("Test2", obj2.Val);
        //    Assert.True(obj2.OnDeserializationCalled);
        //}

        [Fact]
        public void NullHandling()
        {
            var obj1 = new ClassWithInner();        // Not setting the inner

            var serializer = new FudgeSerializer(context);
            var msg = serializer.SerializeToMsg(obj1);

            var obj2 = (ClassWithInner)serializer.Deserialize(msg);
            Assert.NotNull(obj2);
            Assert.Null(obj2.Inner);
        }

        [Fact]
        public void HandlesInnerObjects()
        {
            var obj1 = new ClassWithInner { Inner = new ClassWithInner() };

            var serializer = new FudgeSerializer(context);
            var msg = serializer.SerializeToMsg(obj1);

            var obj2 = (ClassWithInner)serializer.Deserialize(msg);

            Assert.NotNull(obj2.Inner);
            Assert.NotSame(obj2, obj2.Inner);
        }

        [Fact]
        public void TryOutSomeTypes()
        {
            var obj1 = new ClassWithSomeTypes { Array = new int[] { 7, 3, -2 }, DateTime = DateTime.Now, List = new List<string>(), String = "Str" };
            obj1.List.Add("a");
            obj1.List.Add("b");

            var serializer = new FudgeSerializer(context);
            var msg = serializer.SerializeToMsg(obj1);

            var obj2 = (ClassWithSomeTypes)serializer.Deserialize(msg);

            Assert.Equal(obj1.Array, obj2.Array);
            Assert.Equal(obj1.DateTime, obj2.DateTime);
            Assert.Equal(obj1.List, obj2.List);
            Assert.Equal(obj1.String, obj2.String);
        }

        private class SimpleTestClass : ISerializable
        {
            public SimpleTestClass()
            {
            }

            protected SimpleTestClass(SerializationInfo info, StreamingContext context)         // MSDN says it should usually be protected...
            {
                Val = info.GetString("serializedVal");
            }

            public string Val { get; set; }

            #region ISerializable Members

            public void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                info.AddValue("serializedVal", Val);
            }

            #endregion
        }

        private class ClassWithIDeserializationCallback : ISerializable, IDeserializationCallback
        {
            public ClassWithIDeserializationCallback()
            {
            }

            public ClassWithIDeserializationCallback(SerializationInfo info, StreamingContext context)
            {
                Val = info.GetString("serializedVal");
            }

            public string Val { get; set; }

            public bool OnDeserializationCalled { get; private set; }

            #region IDeserializationCallback Members

            public void OnDeserialization(object sender)
            {
                OnDeserializationCalled = true;
            }

            #endregion

            #region ISerializable Members

            public void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                info.AddValue("serializedVal", Val);
            }

            #endregion
        }

        [Serializable]
        public class ClassWithInner : ISerializable
        {
            public ClassWithInner()
            {
            }

            protected ClassWithInner(SerializationInfo info, StreamingContext context)
            {
                Inner = (ClassWithInner)info.GetValue("inner", typeof(ClassWithInner));
            }

            public ClassWithInner Inner { get; set; }

            public string Text { get; set; }

            #region ISerializable Members

            public void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                info.AddValue("inner", Inner);
            }

            #endregion
        }

        private class ClassWithSomeTypes : ISerializable
        {
            public ClassWithSomeTypes()
            {
            }

            protected ClassWithSomeTypes(SerializationInfo info, StreamingContext context)
            {
                String = info.GetString("string");
                DateTime = info.GetDateTime("dateTime");
                Array = (int[])info.GetValue("array", typeof(int[]));
                List = (List<string>)info.GetValue("list", typeof(List<string>));
            }

            #region ISerializable Members

            public void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                info.AddValue("string", String);
                info.AddValue("dateTime", DateTime);
                info.AddValue("array", Array);
                info.AddValue("list", List);
            }

            #endregion

            public string String { get; set; }
            public DateTime DateTime { get; set; }
            public int[] Array { get; set; }
            public List<string> List { get; set; }
        }

    }
}
