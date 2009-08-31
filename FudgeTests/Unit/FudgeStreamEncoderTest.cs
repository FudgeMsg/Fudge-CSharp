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

namespace OpenGamma.Fudge.Tests.Unit
{
    public class FudgeStreamEncoderTest : FudgeStreamEncoder        // Inherit so we can test the protected methods
    {
        [Fact]
        public void fieldPrefixComposition()
        {
            Assert.Equal(0x20, ComposeFieldPrefix(false, 10, false, false));
            Assert.Equal(0x40, ComposeFieldPrefix(false, 1024, false, false));
            Assert.Equal(0x60, ComposeFieldPrefix(false, short.MaxValue + 1000, false, false));
            Assert.Equal(0x98, ComposeFieldPrefix(true, 0, true, true));
        }
    }
}
