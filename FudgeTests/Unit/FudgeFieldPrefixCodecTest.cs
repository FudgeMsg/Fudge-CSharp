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
    public class FudgeFieldPrefixCodecTest
    {
        [Fact]
        public void FieldPrefixComposition()
        {
            Assert.Equal(0x20, FudgeFieldPrefixCodec.ComposeFieldPrefix(false, 10, false, false));
            Assert.Equal(0x40, FudgeFieldPrefixCodec.ComposeFieldPrefix(false, 1024, false, false));
            Assert.Equal(0x60, FudgeFieldPrefixCodec.ComposeFieldPrefix(false, short.MaxValue + 1000, false, false));
            Assert.Equal(0x98, FudgeFieldPrefixCodec.ComposeFieldPrefix(true, 0, true, true));
        }

        [Fact]
        public void HasNameChecks()
        {
            Assert.False(FudgeFieldPrefixCodec.HasName(0x20));
            Assert.True(FudgeFieldPrefixCodec.HasName(0x98));
        }

        [Fact]
        public void fixedWidthChecks()
        {
            Assert.False(FudgeFieldPrefixCodec.IsFixedWidth(0x20));
            Assert.True(FudgeFieldPrefixCodec.IsFixedWidth(0x98));
        }

        [Fact]
        public void hasOrdinalChecks()
        {
            Assert.False(FudgeFieldPrefixCodec.HasOrdinal(0x20));
            Assert.True(FudgeFieldPrefixCodec.HasOrdinal(0x98));
        }

        [Fact]
        public void varWidthSizeChecks()
        {
            Assert.Equal(0, FudgeFieldPrefixCodec.GetFieldWidthByteCount(0x98));
            Assert.Equal(1, FudgeFieldPrefixCodec.GetFieldWidthByteCount(0x20));
            Assert.Equal(2, FudgeFieldPrefixCodec.GetFieldWidthByteCount(0x40));
            Assert.Equal(4, FudgeFieldPrefixCodec.GetFieldWidthByteCount(0x60));
        }

    }
}
