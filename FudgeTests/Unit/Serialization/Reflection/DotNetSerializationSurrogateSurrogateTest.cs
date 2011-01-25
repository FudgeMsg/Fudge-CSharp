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
using Fudge.Serialization.Reflection;

namespace Fudge.Tests.Unit.Serialization.Reflection
{
    public class DotNetSerializationSurrogateSurrogateTest
    {
        private readonly FudgeContext context = new FudgeContext();

        [Fact]
        public void UsingISerializationSurrogates()
        {
            var surrogateSelector = new SurrogateSelector();
            var streamingContext = new StreamingContext(StreamingContextStates.All);
            surrogateSelector.AddSurrogate(typeof(ClassWithSurrogate), streamingContext, new SurrogateClass());
            var serializer = new FudgeSerializer(context);
            serializer.TypeMap.RegisterSurrogateSelector(surrogateSelector);

            // Check out the surrogate
            var surrogate = serializer.TypeMap.GetSurrogate(typeof(ClassWithSurrogate));
            Assert.IsType<DotNetSerializationSurrogateSurrogate>(surrogate);
            Assert.IsType<SurrogateClass>(((DotNetSerializationSurrogateSurrogate)surrogate).SerializationSurrogate);

            var obj1 = new ClassWithSurrogate { A = 22 };
            var msg = serializer.SerializeToMsg(obj1);
            var obj2 = (ClassWithSurrogate)serializer.Deserialize(msg);

            Assert.Equal(obj1.A, obj2.A);
        }

        [Fact]
        public void ConstructorArgChecking()
        {
            var typeData = new TypeData(context, new TypeDataCache(context), GetType(), FudgeFieldNameConvention.Identity);
            var surrogate = new SurrogateClass();
            var selector = new SurrogateSelector();
            Assert.Throws<ArgumentNullException>(() => new DotNetSerializationSurrogateSurrogate(null, typeData, surrogate, selector));
            Assert.Throws<ArgumentNullException>(() => new DotNetSerializationSurrogateSurrogate(context, null, surrogate, selector));
            Assert.Throws<ArgumentNullException>(() => new DotNetSerializationSurrogateSurrogate(context, typeData, null, selector));
        }

        #region Test classes

        private class ClassWithSurrogate
        {
            public ClassWithSurrogate()
            {
            }

            public int A { get; set; }
        }

        private class SurrogateClass : ISerializationSurrogate
        {
            #region ISerializationSurrogate Members

            public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
            {
                var realObj = (ClassWithSurrogate)obj;
                info.AddValue("a", realObj.A);
            }

            public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
            {
                var realObj = (ClassWithSurrogate)obj;
                realObj.A = info.GetInt32("a");
                return realObj;
            }

            #endregion
        }

        #endregion
    }
}
