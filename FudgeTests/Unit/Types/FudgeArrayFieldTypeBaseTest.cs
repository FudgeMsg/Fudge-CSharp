/**
 * Copyright (C) 2009 - 2009 by OpenGamma Inc.
 *
 * Please see distribution for license.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using OpenGamma.Fudge.Types;

namespace OpenGamma.Fudge.Tests.Unit.Types
{
    public class FudgeArrayFieldTypeBaseTest
    {
        [Fact]
        public void Minimization()
        {
            int[] data = new int[0];
            FudgeFieldType type = IntArrayFieldType.Instance;

            object minData = type.Minimize(data, ref type);
            Assert.Same(IndicatorType.Instance, minData);
            Assert.Same(IndicatorFieldType.Instance, type);

            object newData = IntArrayFieldType.Instance.ConvertValueFrom(minData);
            Assert.Equal(new int[0], newData);
        }
    }
}
