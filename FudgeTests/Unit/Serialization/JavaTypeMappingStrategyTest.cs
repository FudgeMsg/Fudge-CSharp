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

namespace Fudge.Tests.Unit.Serialization
{
    public class JavaTypeMappingStrategyTest
    {
        [Fact]
        public void SimpleExample()
        {
            var mapper = new JavaTypeMappingStrategy("Fudge.Tests.Unit", "org.fudgemsg");
            Assert.Equal("org.fudgemsg.serialization.JavaTypeMappingStrategyTest", mapper.GetName(this.GetType()));
            Assert.Same(this.GetType(), mapper.GetType("org.fudgemsg.serialization.JavaTypeMappingStrategyTest"));
        }

        [Fact]
        public void InnerClasses()
        {
            var mapper = new JavaTypeMappingStrategy("Fudge.Tests.Unit", "org.fudgemsg");
            Assert.Equal("org.fudgemsg.serialization.JavaTypeMappingStrategyTest$Inner", mapper.GetName(typeof(Inner)));
            Assert.Same(typeof(Inner), mapper.GetType("org.fudgemsg.serialization.JavaTypeMappingStrategyTest$Inner"));
        }

        [Fact]
        public void ConstructorRangeChecking()
        {
            Assert.Throws<ArgumentNullException>(() => new JavaTypeMappingStrategy(null, ""));
            Assert.Throws<ArgumentNullException>(() => new JavaTypeMappingStrategy("", null));
        }

        private class Inner
        {
        }
    }
}
