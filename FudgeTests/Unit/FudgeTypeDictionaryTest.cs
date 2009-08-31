/**
 * Copyright (C) 2009 - 2009 by OpenGamma Inc.
 *
 * Please see distribution for license.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenGamma.Fudge.Types;

using Xunit;

namespace OpenGamma.Fudge.Tests.Unit
{
    public class FudgeTypeDictionaryTest
    {
        [Fact]
        public void SimpleTypeLookup()
        {
            FudgeFieldType type = null;

            type = FudgeTypeDictionary.Instance.GetByCSharpType(typeof(bool));
            Assert.NotNull(type);
            Assert.Equal(PrimitiveFieldTypes.BooleanType.TypeId, type.TypeId);

            type = FudgeTypeDictionary.Instance.GetByCSharpType(typeof(Boolean));
            Assert.NotNull(type);
            Assert.Equal(PrimitiveFieldTypes.BooleanType.TypeId, type.TypeId);
        }
    }
}
