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
    public class FudgeSurrogateSelectorTest
    {
        private readonly FudgeContext context = new FudgeContext();

        [Fact]
        public void DirectSerializaton()
        {
            // Basic case
            var selector = new FudgeSurrogateSelector(context);
            var factory = selector.GetSurrogateFactory(typeof(DirectTest), FudgeFieldNameConvention.Identity);
            Assert.IsType<SerializableSurrogate>(factory(context));

            // Test exception thrown if no default constructor
            Assert.Throws<FudgeRuntimeException>(() => selector.GetSurrogateFactory(typeof(DirectNoDefaultConstructorTest), FudgeFieldNameConvention.Identity));
        }

        [Fact]
        public void SurrogateAttribute()
        {
            var selector = new FudgeSurrogateSelector(context);

            // SurrogateTest has a stateless surrogate, so should get back same one every time
            var factory = selector.GetSurrogateFactory(typeof(SurrogateTest), FudgeFieldNameConvention.Identity);
            var s1 = factory(context);
            var s2 = factory(context);
            Assert.IsType<SurrogateTest.SurrogateTestSurrogate>(s1);
            Assert.Same(s1, s2);

            // SurrogateTest2 is not stateless, so should get back different
            factory = selector.GetSurrogateFactory(typeof(SurrogateTest2), FudgeFieldNameConvention.Identity);
            s1 = factory(context);
            s2 = factory(context);
            Assert.IsType<SurrogateTest2.SurrogateTest2Surrogate>(s1);
            Assert.NotSame(s1, s2);

            // SurrogateTest3 has a constructor on the surrogate which takes type
            factory = selector.GetSurrogateFactory(typeof(SurrogateTest3), FudgeFieldNameConvention.Identity);
            s1 = factory(context);
            s2 = factory(context);
            Assert.IsType<SurrogateTest3.SurrogateTest3Surrogate>(s1);
            Assert.NotSame(s1, s2);
            Assert.Equal(typeof(SurrogateTest3), ((SurrogateTest3.SurrogateTest3Surrogate)s1).Type);

            // SurrogateTest4 has a constructor on the surrogate which takes context and type
            factory = selector.GetSurrogateFactory(typeof(SurrogateTest4), FudgeFieldNameConvention.Identity);
            s1 = factory(context);
            s2 = factory(context);
            Assert.IsType<SurrogateTest4.SurrogateTest4Surrogate>(s1);
            Assert.NotSame(s1, s2);
            Assert.Equal(typeof(SurrogateTest4), ((SurrogateTest4.SurrogateTest4Surrogate)s1).Type);
            Assert.Same(context, ((SurrogateTest4.SurrogateTest4Surrogate)s1).Context);
        }

        #region Test classes

        private class DirectTest : IFudgeSerializable
        {
            #region IFudgeSerializable Members

            public void Serialize(IMutableFudgeFieldContainer msg, IFudgeSerializer serializer)
            {
                throw new NotImplementedException();
            }

            public void Deserialize(IFudgeFieldContainer msg, IFudgeDeserializer deserializer)
            {
                throw new NotImplementedException();
            }

            #endregion
        }

        private class DirectNoDefaultConstructorTest : IFudgeSerializable
        {
            public DirectNoDefaultConstructorTest(int i)
            {
            }

            #region IFudgeSerializable Members

            public void Serialize(IMutableFudgeFieldContainer msg, IFudgeSerializer serializer)
            {
                throw new NotImplementedException();
            }

            public void Deserialize(IFudgeFieldContainer msg, IFudgeDeserializer deserializer)
            {
                throw new NotImplementedException();
            }

            #endregion
        }

        [FudgeSurrogate(typeof(SurrogateTestSurrogate), true)]
        private class SurrogateTest
        {
            public SurrogateTest(int n)
            {
                Number = n;
            }

            public int Number { get; private set; }

            public class SurrogateTestSurrogate : IFudgeSerializationSurrogate
            {
                #region IFudgeSerializationSurrogate Members

                public void Serialize(object obj, IMutableFudgeFieldContainer msg, IFudgeSerializer serializer)
                {
                    throw new NotImplementedException();
                }

                public object Deserialize(IFudgeFieldContainer msg, IFudgeDeserializer deserializer)
                {
                    throw new NotImplementedException();
                }

                #endregion
            }
        }

        [FudgeSurrogate(typeof(SurrogateTest2Surrogate), false)]
        private class SurrogateTest2
        {
            public class SurrogateTest2Surrogate : IFudgeSerializationSurrogate
            {
                #region IFudgeSerializationSurrogate Members

                public void Serialize(object obj, IMutableFudgeFieldContainer msg, IFudgeSerializer serializer)
                {
                    throw new NotImplementedException();
                }

                public object Deserialize(IFudgeFieldContainer msg, IFudgeDeserializer deserializer)
                {
                    throw new NotImplementedException();
                }

                #endregion
            }
        }

        [FudgeSurrogate(typeof(SurrogateTest3Surrogate), false)]
        private class SurrogateTest3
        {
            public class SurrogateTest3Surrogate : IFudgeSerializationSurrogate
            {
                public Type Type { get; set; }

                public SurrogateTest3Surrogate(Type type)
                {
                    this.Type = type;
                }

                #region IFudgeSerializationSurrogate Members

                public void Serialize(object obj, IMutableFudgeFieldContainer msg, IFudgeSerializer serializer)
                {
                    throw new NotImplementedException();
                }

                public object Deserialize(IFudgeFieldContainer msg, IFudgeDeserializer deserializer)
                {
                    throw new NotImplementedException();
                }

                #endregion
            }
        }

        [FudgeSurrogate(typeof(SurrogateTest4Surrogate), false)]
        private class SurrogateTest4
        {
            public class SurrogateTest4Surrogate : IFudgeSerializationSurrogate
            {
                public Type Type { get; set; }
                public FudgeContext Context { get; set; }

                public SurrogateTest4Surrogate(FudgeContext context, Type type)
                {
                    this.Context = context;
                    this.Type = type;
                }

                #region IFudgeSerializationSurrogate Members

                public void Serialize(object obj, IMutableFudgeFieldContainer msg, IFudgeSerializer serializer)
                {
                    throw new NotImplementedException();
                }

                public object Deserialize(IFudgeFieldContainer msg, IFudgeDeserializer deserializer)
                {
                    throw new NotImplementedException();
                }

                #endregion
            }
        }

        #endregion
    }
}
