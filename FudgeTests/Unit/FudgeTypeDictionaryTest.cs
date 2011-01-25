﻿/**
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
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Fudge.Types;

using Xunit;

namespace Fudge.Tests.Unit
{
    public class FudgeTypeDictionaryTest
    {
        [Fact]
        public void SimpleTypeLookup()
        {
            FudgeFieldType type = null;

            type = new FudgeTypeDictionary().GetByCSharpType(typeof(bool));
            Assert.NotNull(type);
            Assert.Equal(PrimitiveFieldTypes.BooleanType.TypeId, type.TypeId);

            type = new FudgeTypeDictionary().GetByCSharpType(typeof(Boolean));
            Assert.NotNull(type);
            Assert.Equal(PrimitiveFieldTypes.BooleanType.TypeId, type.TypeId);
        }
    }
}
