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
    public class ByteArrayFieldTypeTest
    {
        [Fact]
        public void MinimizeToFixedType()
        {
            var baType = ByteArrayFieldType.VariableSizedInstance;
            FudgeFieldType type;
            byte[] data;

            type = baType;
            data = new byte[4];
            Assert.Same(data, baType.Minimize(data, ref type));
            Assert.Same(ByteArrayFieldType.Length4Instance, type);

            type = baType;
            data = new byte[8];
            Assert.Same(data, baType.Minimize(data, ref type));
            Assert.Same(ByteArrayFieldType.Length8Instance, type);

            type = baType;
            data = new byte[512];
            Assert.Same(data, baType.Minimize(data, ref type));
            Assert.Same(ByteArrayFieldType.Length512Instance, type);
        }

        [Fact]
        public void MinimizeToIndicatorAndBack()
        {
            var baType = ByteArrayFieldType.VariableSizedInstance;
            FudgeFieldType type = baType;
            byte[] data = new byte[0];

            Assert.Same(IndicatorType.Instance, baType.Minimize(data, ref type));
            Assert.Same(IndicatorFieldType.Instance, type);

            Assert.Equal(data, baType.ConvertValueFrom(IndicatorType.Instance));
        }
    }
}
