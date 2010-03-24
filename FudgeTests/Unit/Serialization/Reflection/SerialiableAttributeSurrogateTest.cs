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
using Fudge.Serialization.Reflection;

namespace Fudge.Tests.Unit.Serialization.Reflection
{
    public class SerialiableAttributeSurrogateTest
    {
        private readonly FudgeContext context = new FudgeContext();

        [Fact]
        public void NonSerializedMarkerTest()
        {
            var obj1 = new SerializableClass(2, 3);

            var serializer = new FudgeSerializer(context);
            var msg = serializer.SerializeToMsg(obj1);

            var obj2 = (SerializableClass)serializer.Deserialize(msg);

            Assert.Equal(2, obj2.GetSerializableField());
            Assert.Equal(0, obj2.GetNonserializableField());
        }

        [Fact]
        public void PropertiesArentSerialized()
        {
            var obj1 = new PropertyTest();
            PropertyTest.val = 7;

            var serializer = new FudgeSerializer(context);
            var msg = serializer.SerializeToMsg(obj1);

            PropertyTest.val = 0;

            var obj2 = (PropertyTest)serializer.Deserialize(msg);
            Assert.Equal(0, PropertyTest.val);
        }

        [Fact]
        public void ConstructorArgChecking()
        {
            var typeData = new TypeData(context, new TypeDataCache(context), typeof(SerializableClass), FudgeFieldNameConvention.Identity);
            var badTypeData = new TypeData(context, new TypeDataCache(context), GetType(), FudgeFieldNameConvention.Identity);
            Assert.Throws<ArgumentNullException>(() => new SerializableAttributeSurrogate(null, typeData));
            Assert.Throws<ArgumentNullException>(() => new SerializableAttributeSurrogate(context, null));
            Assert.Throws<ArgumentOutOfRangeException>(() => new SerializableAttributeSurrogate(context, badTypeData));
        }

        #region Test classes

        [Serializable]
        private class SerializableClass
        {
            private int serializableField;

            [NonSerialized]
            private int nonSerializableField;

            public SerializableClass(int serValue, int nonSerValue)
            {
                this.serializableField = serValue;
                this.nonSerializableField = nonSerValue;
            }

            public int GetSerializableField()
            {
                return serializableField;
            }

            public int GetNonserializableField()
            {
                return nonSerializableField;
            }
        }

        [Serializable]
        private class PropertyTest
        {
            [NonSerialized]
            public static int val;

            public int Prop
            {
                get { return val; }
                set { val = value; }
            }
        }

        #endregion
    }
}
