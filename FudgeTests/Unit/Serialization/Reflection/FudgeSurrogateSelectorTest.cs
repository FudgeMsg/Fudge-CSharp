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

        #region Test classes

        private class DirectTest : IFudgeSerializable
        {
            #region IFudgeSerializable Members

            public void Serialize(IFudgeSerializer serializer)
            {
                throw new NotImplementedException();
            }

            public void BeginDeserialize(IFudgeDeserializer deserializer, int dataVersion)
            {
                throw new NotImplementedException();
            }

            public bool DeserializeField(IFudgeDeserializer deserializer, IFudgeField field, int dataVersion)
            {
                throw new NotImplementedException();
            }

            public void EndDeserialize(IFudgeDeserializer deserializer, int dataVersion)
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

            public void Serialize(IFudgeSerializer serializer)
            {
                throw new NotImplementedException();
            }

            public void BeginDeserialize(IFudgeDeserializer deserializer, int dataVersion)
            {
                throw new NotImplementedException();
            }

            public bool DeserializeField(IFudgeDeserializer deserializer, IFudgeField field, int dataVersion)
            {
                throw new NotImplementedException();
            }

            public void EndDeserialize(IFudgeDeserializer deserializer, int dataVersion)
            {
                throw new NotImplementedException();
            }

            #endregion
        }

        #endregion
    }
}
