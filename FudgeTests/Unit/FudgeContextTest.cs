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
using System.IO;
using OpenGamma.Fudge.Taxon;

namespace OpenGamma.Fudge.Tests.Unit
{
    public class FudgeContextTest
    {
        private static readonly short[] ORDINALS = new short[] { 5, 14, 928, 74 };
        private static readonly string[] NAMES = new string[] { "Kirk", "Wylie", "Jim", "Moores" };

        [Fact]
        public void AllNamesCodecNoTaxonomy()
        {
            FudgeMsg inputMsg = FudgeMsgTest.CreateMessageAllNames();
            FudgeContext context = new FudgeContext();
            FudgeMsg outputMsg = CycleMessage(inputMsg, context, null);

            Assert.NotNull(outputMsg);

            FudgeMsgCodecTest.AssertAllFieldsMatch(inputMsg, outputMsg);
        }

        [Fact]
        public void AllNamesCodecWithTaxonomy()
        {
            FudgeMsg inputMsg = new FudgeMsg();
            inputMsg.Add("value1", NAMES[0]);
            inputMsg.Add("value2", NAMES[1]);
            inputMsg.Add("value3", NAMES[2]);
            inputMsg.Add("value4", NAMES[3]);

            FudgeContext context = new FudgeContext();
            var resolverMap = new Dictionary<short, IFudgeTaxonomy>();
            resolverMap.Add(45, new MapFudgeTaxonomy(ORDINALS, NAMES));
            context.TaxonomyResolver = new ImmutableMapTaxonomyResolver(resolverMap);

            FudgeMsg outputMsg = CycleMessage(inputMsg, context, 45);
            Assert.Equal("value1", outputMsg.GetString(NAMES[0]));
            Assert.Equal("value1", outputMsg.GetString(ORDINALS[0]));
            Assert.Equal("value2", outputMsg.GetString(NAMES[1]));
            Assert.Equal("value2", outputMsg.GetString(ORDINALS[1]));
            Assert.Equal("value3", outputMsg.GetString(NAMES[2]));
            Assert.Equal("value3", outputMsg.GetString(ORDINALS[2]));
            Assert.Equal("value4", outputMsg.GetString(NAMES[3]));
            Assert.Equal("value4", outputMsg.GetString(ORDINALS[3]));
        }

        [Fact]
        public void AllOrdinalsCodecWithTaxonomy()
        {
            FudgeMsg inputMsg = new FudgeMsg();
            inputMsg.Add("value1", ORDINALS[0]);
            inputMsg.Add("value2", ORDINALS[1]);
            inputMsg.Add("value3", ORDINALS[2]);
            inputMsg.Add("value4", ORDINALS[3]);

            FudgeContext context = new FudgeContext();
            var resolverMap = new Dictionary<short, IFudgeTaxonomy>();
            resolverMap.Add(45, new MapFudgeTaxonomy(ORDINALS, NAMES));
            context.TaxonomyResolver = new ImmutableMapTaxonomyResolver(resolverMap);

            FudgeMsg outputMsg = CycleMessage(inputMsg, context, (short)45);
            Assert.Equal("value1", outputMsg.GetString(NAMES[0]));
            Assert.Equal("value1", outputMsg.GetString(ORDINALS[0]));
            Assert.Equal("value2", outputMsg.GetString(NAMES[1]));
            Assert.Equal("value2", outputMsg.GetString(ORDINALS[1]));
            Assert.Equal("value3", outputMsg.GetString(NAMES[2]));
            Assert.Equal("value3", outputMsg.GetString(ORDINALS[2]));
            Assert.Equal("value4", outputMsg.GetString(NAMES[3]));
            Assert.Equal("value4", outputMsg.GetString(ORDINALS[3]));
        }

        private FudgeMsg CycleMessage(FudgeMsg msg, FudgeContext context, short? taxonomy)
        {
            MemoryStream outputStream = new MemoryStream();
            context.Serialize(msg, taxonomy, outputStream);

            byte[] content = outputStream.ToArray();

            MemoryStream inputStream = new MemoryStream(content);
            FudgeMsg outputMsg = context.Deserialize(inputStream);
            return outputMsg;
        }
    }
}
