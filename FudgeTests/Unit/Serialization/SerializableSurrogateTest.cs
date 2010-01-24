/**
 * Copyright (C) 2009 - 2009 by OpenGamma Inc. and other contributors.
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
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Fudge.Serialization;
using System.Reflection;

namespace Fudge.Tests.Unit.Serialization
{
    /*
    public class SerializableSurrogateTest
    {
        [Fact]
        public void HappyDayStory()
        {
            var surrogate = new SerializableSurrogate(typeof(Class0));
        }

        [Fact]
        public void MakeSureHasDefaultConstructor()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                {
                    var surrogate = new SerializableSurrogate(typeof(Class1));
                });
        }

        [Fact]
        public void MakeSureConstructorPublic()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                var surrogate = new SerializableSurrogate(typeof(Class2));
            });
        }

        public class Class0 : IFudgeSerializable
        {
            public Class0()
            {
            }

            #region IFudgeSerializable Members

            public void Serialize(FudgeMsg msg, IFudgeSerializer context)
            {
                throw new NotImplementedException();
            }

            public void Deserialize(FudgeMsg msg, int dataVersion, IFudgeDeserializer context)
            {
                throw new NotImplementedException();
            }

            #endregion
        }

        public class Class1 : IFudgeSerializable
        {
            public Class1(int fred)
            {
            }

            #region IFudgeSerializable Members

            public void Serialize(FudgeMsg msg, IFudgeSerializer context)
            {
                throw new NotImplementedException();
            }

            public void Deserialize(FudgeMsg msg, int dataVersion, IFudgeDeserializer context)
            {
                throw new NotImplementedException();
            }

            #endregion
        }

        public class Class2 : IFudgeSerializable
        {
            protected Class2()
            {
            }

            #region IFudgeSerializable Members

            public void Serialize(FudgeMsg msg, IFudgeSerializer context)
            {
                throw new NotImplementedException();
            }

            public void Deserialize(FudgeMsg msg, int dataVersion, IFudgeDeserializer context)
            {
                throw new NotImplementedException();
            }

            #endregion
        }
    }
     */
}
