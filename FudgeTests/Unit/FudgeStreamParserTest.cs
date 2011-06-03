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
using Xunit;
using System.IO;

namespace Fudge.Tests.Unit
{
    public class FudgeStreamParserTest
    {
        private static readonly FudgeContext fudgeContext = new FudgeContext();

        [Fact]
        public void StandardMessageAllNames()
        {
            FudgeMsg msg = StandardFudgeMessages.CreateMessageAllNames(fudgeContext);
            CheckResultsMatch(msg);
        }

        [Fact]
        public void StandardMessageAllOrdinals()
        {
            FudgeMsg msg = StandardFudgeMessages.CreateMessageAllOrdinals(fudgeContext);
            CheckResultsMatch(msg);
        }

        [Fact]
        public void StandardMessageByteArrays()
        {
            FudgeMsg msg = StandardFudgeMessages.CreateMessageAllByteArrayLengths(fudgeContext);
            CheckResultsMatch(msg);
        }

        [Fact]
        public void StandardMessageSubMessages()
        {
            FudgeMsg msg = StandardFudgeMessages.CreateMessageWithSubMsgs(fudgeContext);
            CheckResultsMatch(msg);
        }

        [Fact]
        public void LargeMessage()
        { // FRN-91
            FudgeMsg msg = StandardFudgeMessages.CreateLargeMessage(fudgeContext);
            CheckResultsMatch(msg);
        }

        [Fact]
        public void AllMessagesSameContext()
        {
            FudgeContext fudgeContext = new FudgeContext();
            FudgeMsg msg = null;
            msg = StandardFudgeMessages.CreateMessageAllNames(fudgeContext);
            CheckResultsMatch(msg, fudgeContext);
            msg = StandardFudgeMessages.CreateMessageAllOrdinals(fudgeContext);
            CheckResultsMatch(msg, fudgeContext);
            msg = StandardFudgeMessages.CreateMessageAllByteArrayLengths(fudgeContext);
            CheckResultsMatch(msg, fudgeContext);
            msg = StandardFudgeMessages.CreateMessageWithSubMsgs(fudgeContext);
            CheckResultsMatch(msg, fudgeContext);
            msg = StandardFudgeMessages.CreateLargeMessage(fudgeContext);
            CheckResultsMatch(msg);
        }

        protected void CheckResultsMatch(FudgeMsg msg)
        {
           CheckResultsMatch(msg, new FudgeContext());
        }

        /**
         * @param msg
         */
        protected void CheckResultsMatch(FudgeMsg msg, FudgeContext fudgeContext)
        {
            FudgeMsgEnvelope result = CycleMessage(fudgeContext, msg);
            Assert.NotNull(result);
            Assert.NotNull(result.Message);
            FudgeMsg resultMsg = result.Message;
            FudgeUtils.AssertAllFieldsMatch(msg, resultMsg);
        }

        protected FudgeMsgEnvelope CycleMessage(FudgeContext context, FudgeMsg msg)
        {
            FudgeStreamParser parser = new FudgeStreamParser(context);
            byte[] msgAsBytes = context.ToByteArray(msg);
            return parser.Parse(new MemoryStream(msgAsBytes));
        }
    }
}
