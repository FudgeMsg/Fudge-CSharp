/*
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

namespace Fudge.Encodings
{
    public class FudgeMsgStreamWriter : IFudgeStreamWriter
    {
        private readonly Stack<FudgeMsg> msgStack = new Stack<FudgeMsg>();
        private readonly FudgeContext context;
        private readonly FudgeMsg top;
        private FudgeMsg current;

        public FudgeMsgStreamWriter()
        {
            context = new FudgeContext();
            top = context.NewMessage();
            current = top;
        }

        public FudgeMsg Message
        {
            get { return top; }
        }

        #region IFudgeStreamWriter Members

        public void StartSubMessage(string name, int? ordinal)
        {
            msgStack.Push(current);
            FudgeMsg newMsg = context.NewMessage();
            current.Add(name, ordinal, newMsg);
            current = newMsg;
        }

        public void WriteField(string name, int? ordinal, FudgeFieldType type, object value)
        {
            current.Add(name, ordinal, type, value);
        }

        public void WriteFields(IEnumerable<IFudgeField> fields)
        {
            foreach (var field in fields)
            {
                current.Add(field);
            }
        }

        public void EndSubMessage()
        {
            if (msgStack.Count == 0)
            {
                throw new InvalidOperationException("Ending more messages than started");
            }
            current = msgStack.Pop();
        }

        public void End()
        {
            // Noop
        }

        #endregion
    }
}
