/**
 * Copyright (C) 2009 - 2009 by OpenGamma Inc. and other contributors.
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
using Xunit;
using System.IO;
using Fudge.Taxon;

namespace Fudge.Tests.Unit
{
    public class FudgeContextTest
    {
        private static readonly int[] ORDINALS = new int[] { 5, 14, 928, 74 };
        private static readonly string[] NAMES = new string[] { "Kirk", "Wylie", "Jim", "Moores" };

        [Fact]
        public void AllNamesCodecNoTaxonomy()
        {
            FudgeMsg inputMsg = StandardFudgeMessages.CreateMessageAllNames();
            FudgeContext context = new FudgeContext();
            FudgeMsg outputMsg = CycleMessage(inputMsg, context, null);

            Assert.NotNull(outputMsg);

            FudgeMsgCodecTest.AssertAllFieldsMatch(inputMsg, outputMsg);
        }

        [Fact]
        public void AllNamesCodecWithTaxonomy()
        {
            FudgeMsg inputMsg = new FudgeMsg();
            inputMsg.Add(NAMES[0], "value1");
            inputMsg.Add(NAMES[1], "value2");
            inputMsg.Add(NAMES[2], "value3");
            inputMsg.Add(NAMES[3], "value4");

            FudgeContext context = new FudgeContext();
            var resolverMap = new Dictionary<int, IFudgeTaxonomy>();
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
            inputMsg.Add(ORDINALS[0], "value1");
            inputMsg.Add(ORDINALS[1], "value2");
            inputMsg.Add(ORDINALS[2], "value3");
            inputMsg.Add(ORDINALS[3], "value4");

            FudgeContext context = new FudgeContext();
            var resolverMap = new Dictionary<int, IFudgeTaxonomy>();
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
            FudgeMsgEnvelope outputMsgEnvelope = context.Deserialize(inputStream);
            Assert.NotNull(outputMsgEnvelope);
            Assert.NotNull(outputMsgEnvelope.Message);
            return outputMsgEnvelope.Message;
        }
    }
}
