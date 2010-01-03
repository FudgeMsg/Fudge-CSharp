/*
 * <!--
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
 * -->
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fudge.Encodings
{
    /// <summary>
    /// <c>FudgeMsgStreamWriter</c> allows the streaming API to be used to construct <see cref="FudgeMsg"/>s.
    /// </summary>
    public class FudgeMsgStreamWriter : IFudgeStreamWriter
    {
        private readonly Stack<FudgeMsg> msgStack = new Stack<FudgeMsg>();
        private readonly FudgeContext context;
        private FudgeMsg top;
        private FudgeMsg current;
        private readonly Queue<FudgeMsg> messages = new Queue<FudgeMsg>();

        /// <summary>
        /// Constructs a new <see cref="FudgeMsgStreamWriter"/> which will use a default <see cref="FudgeContext"/>.
        /// </summary>
        public FudgeMsgStreamWriter() : this(new FudgeContext())
        {
        }

         /// <summary>
         /// Constructs a new <see cref="FudgeMsgStreamWriter"/> using a given <see cref="FudgeContext"/>.
         /// </summary>
         /// <param name="context"><see cref="FudgeContext"/> to use to construct messages.</param>
        public FudgeMsgStreamWriter(FudgeContext context)
        {
            this.context = context;
        }

        /// <summary>
        /// Gets the list of all <see cref="FudgeMsg"/>s that have been written withing removing them from the queue.
        /// </summary>
        public IList<FudgeMsg> PeekAllMessages()
        {
            var result = new List<FudgeMsg>(messages);
            return result;
        }

        /// <summary>
        /// Dequeues the first message that has been written.
        /// </summary>
        /// <returns>First message in queue, or null if none available</returns>
        public FudgeMsg DequeueMessage()
        {
            if (messages.Count == 0)
                return null;

            return messages.Dequeue();
        }

        #region IFudgeStreamWriter Members

        /// <inheritdoc/>
        public void StartMessage()
        {
            top = context.NewMessage();
            current = top;
        }

        /// <inheritdoc/>
        public void StartSubMessage(string name, int? ordinal)
        {
            msgStack.Push(current);
            FudgeMsg newMsg = context.NewMessage();
            current.Add(name, ordinal, newMsg);
            current = newMsg;
        }

        /// <inheritdoc/>
        public void WriteField(string name, int? ordinal, FudgeFieldType type, object value)
        {
            current.Add(name, ordinal, type, value);
        }

        /// <inheritdoc/>
        public void WriteFields(IEnumerable<IFudgeField> fields)
        {
            foreach (var field in fields)
            {
                current.Add(field);
            }
        }

        /// <inheritdoc/>
        public void EndSubMessage()
        {
            if (msgStack.Count == 0)
            {
                throw new InvalidOperationException("Ending more sub-messages than started");
            }
            current = msgStack.Pop();
        }

        /// <inheritdoc/>
        public void EndMessage()
        {
            if (msgStack.Count > 0)
            {
                throw new InvalidOperationException("Ending message prematurely");
            }
            messages.Enqueue(top);
            top = null;
            current = null;
        }

        #endregion
    }
}
