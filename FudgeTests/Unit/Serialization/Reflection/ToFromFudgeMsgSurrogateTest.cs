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

namespace Fudge.Tests.Unit.Serialization.Reflection
{
    public class ToFromFudgeMsgSurrogateTest
    {
        private readonly FudgeContext context = new FudgeContext();
        private readonly TypeDataCache typeDataCache;

        public ToFromFudgeMsgSurrogateTest()
        {
            typeDataCache = new TypeDataCache(context);
        }

        [Fact]
        public void CanHandle()
        {
            Assert.True(ToFromFudgeMsgSurrogate.CanHandle(typeDataCache, FudgeFieldNameConvention.Identity, typeof(ExampleClass)));
            Assert.False(ToFromFudgeMsgSurrogate.CanHandle(typeDataCache, FudgeFieldNameConvention.Identity, typeof(Bogus1)));
            Assert.False(ToFromFudgeMsgSurrogate.CanHandle(typeDataCache, FudgeFieldNameConvention.Identity, typeof(Bogus2)));
            Assert.False(ToFromFudgeMsgSurrogate.CanHandle(typeDataCache, FudgeFieldNameConvention.Identity, typeof(Bogus3)));
            Assert.False(ToFromFudgeMsgSurrogate.CanHandle(typeDataCache, FudgeFieldNameConvention.Identity, typeof(Bogus4)));
        }

        [Fact]
        public void ConstuctorRangeChecking()
        {
            Assert.Throws<ArgumentNullException>(() => new ToFromFudgeMsgSurrogate(null, typeDataCache.GetTypeData(typeof(ExampleClass), FudgeFieldNameConvention.Identity)));
            Assert.Throws<ArgumentNullException>(() => new ToFromFudgeMsgSurrogate(context, null));
            Assert.Throws<ArgumentOutOfRangeException>(() => new ToFromFudgeMsgSurrogate(context, typeDataCache.GetTypeData(typeof(Bogus1), FudgeFieldNameConvention.Identity)));
        }

        [Fact]
        public void SurrogateSelectorGetsIt()
        {
            var surrogate = new FudgeSerializer(context).TypeMap.GetSurrogate(typeof(ExampleClass));
            Assert.NotNull(surrogate);
            Assert.IsType<ToFromFudgeMsgSurrogate>(surrogate);
        }

        [Fact]
        public void RoundTrip()
        {
            var serializer = new FudgeSerializer(context);

            var obj1 = new ExampleClass(7);
            var msg = serializer.SerializeToMsg(obj1);
            var obj2 = (ExampleClass)serializer.Deserialize(msg);

            Assert.NotSame(obj1, obj2);
            Assert.Equal(obj1.Number, obj2.Number);
        }

        public class ExampleClass
        {
            private readonly int number;

            public ExampleClass(int n)
            {
                this.number = n;
            }

            public int Number
            {
                get { return number; }
            }

            public static ExampleClass FromFudgeMsg(IFudgeFieldContainer msg, IFudgeDeserializer deserializer)
            {
                int? val = msg.GetInt("number");
                return new ExampleClass(val ?? -1);
            }

            public void ToFudgeMsg(IAppendingFudgeFieldContainer msg, IFudgeSerializer serializer)
            {
                msg.Add("number", number);
            }
        }

        public class Bogus1
        {
            public static Bogus1 FromFudgeMsg(IFudgeFieldContainer msg, IFudgeDeserializer deserializer, int dataVersion)
            {
                return null;
            }

            public static IFudgeFieldContainer ToFudgeMsg(IFudgeSerializer serializer)
            {
                // Shouldn't be static
                return null;
            }
        }

        public class Bogus2
        {
            public static Bogus2 FromFudgeMsg(IFudgeFieldContainer msg, IFudgeDeserializer deserializer)
            {
                // Missing arg
                return null;
            }

            public IFudgeFieldContainer ToFudgeMsg(IFudgeSerializer serializer)
            {
                return null;
            }
        }

        public class Bogus3
        {
            public static ExampleClass FromFudgeMsg(IFudgeFieldContainer msg, IFudgeDeserializer deserializer, int dataVersion)
            {
                // Wrong return type
                return null;
            }

            public IFudgeFieldContainer ToFudgeMsg(IFudgeSerializer serializer)
            {
                return null;
            }
        }

        public class Bogus4
        {
            public static Bogus4 FromFudgeMsg(FudgeMsg msg, IFudgeDeserializer deserializer, int dataVersion)
            {
                // Wrong arg type
                return null;
            }

            public IFudgeFieldContainer ToFudgeMsg(IFudgeSerializer serializer)
            {
                return null;
            }
        }
    }
}
