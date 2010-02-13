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
    public class TypeDataCacheTest
    {
        [Fact]
        public void GettingTypeData()
        {
            var context = new FudgeContext();
            var cache = new TypeDataCache(context);

            TypeData data = cache.GetTypeData(this.GetType(), FudgeFieldNameConvention.Identity);
            Assert.NotNull(data);
            TypeData data2 = cache.GetTypeData(this.GetType(), FudgeFieldNameConvention.Identity);
            Assert.Same(data, data2);
        }

        [Fact]
        public void RangeChecking()
        {
            var context = new FudgeContext();
            var cache = new TypeDataCache(context);

            Assert.Throws<ArgumentNullException>(() => new TypeDataCache(null));
            Assert.Throws<ArgumentNullException>(() => cache.GetTypeData(null, FudgeFieldNameConvention.Identity));
        }

        [Fact]
        public void HandlesCycles()
        {
            var context = new FudgeContext();
            var cache = new TypeDataCache(context);

            var data = cache.GetTypeData(typeof(Cycle), FudgeFieldNameConvention.Identity);
            Assert.NotNull(data);
            Assert.Equal(data, data.Properties[0].TypeData);
        }

        public class Cycle
        {
            public Cycle Other { get; set; }
        }
    }
}
