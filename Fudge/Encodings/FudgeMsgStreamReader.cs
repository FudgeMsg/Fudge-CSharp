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
using Fudge.Taxon;
using Fudge.Types;

namespace Fudge.Encodings
{
    /// <summary>
    /// <c>FudgeMsgStreamReader</c> allows a <see cref="FudgeMsg"/> to be read as if it were a stream source of data.
    /// </summary>
    public class FudgeMsgStreamReader : IFudgeStreamReader
    {
        private readonly FudgeContext context;
        private Stack<State> stack = new Stack<State>();
        private State currentState;
        private FudgeStreamElement element = FudgeStreamElement.NoElement;
        private IFudgeField field;
        private IEnumerator<FudgeMsg> messageSource;
        private FudgeMsg nextMessage;

        /// <summary>
        /// Constructs a new <see cref="FudgeMsgStreamReader"/> using a given <see cref="FudgeMsg"/> for data.
        /// </summary>
        /// <param name="context">Context to control behaviours.</param>
        /// <param name="msg"><see cref="FudgeMsg"/> to provide as a stream.</param>
        public FudgeMsgStreamReader(FudgeContext context, FudgeMsg msg)
            : this(context, new FudgeMsg[] { msg })
        {
        }

        /// <summary>
        /// Constructs a new <see cref="FudgeMsgStreamReader"/> using a set of <see cref="FudgeMsg"/>s for data.
        /// </summary>
        /// <param name="context">Context to control behaviours.</param>
        /// <param name="messages">Set <see cref="FudgeMsg"/>s to provide as a stream.</param>
        public FudgeMsgStreamReader(FudgeContext context, IEnumerable<FudgeMsg> messages)
        {
            if (context == null)
                throw new ArgumentNullException("context");
            this.context = context;

            messageSource = messages.GetEnumerator();
            currentState = null;
        }

        #region IFudgeStreamReader Members

        /// <inheritdoc/>
        public bool HasNext
        {
            get
            {
                if (stack.Count > 0 || currentState != null || nextMessage != null)
                    return true;

                // See if there's another message
                if (!messageSource.MoveNext())
                    return false;

                nextMessage = messageSource.Current;
                return true;
            }
        }

        /// <inheritdoc/>
        public FudgeStreamElement MoveNext()
        {
            if (currentState == null)
            {
                if (!HasNext)       // Will fetch the next if required
                {
                    element = FudgeStreamElement.NoElement;
                }
                else
                {
                    currentState = new State(nextMessage);
                    nextMessage = null;
                    element = FudgeStreamElement.MessageStart;
                }
            }
            else if (currentState.Fields.Count == 0)
            {
                if (stack.Count == 0)
                {
                    // Finished the message
                    currentState = null;
                    element = FudgeStreamElement.MessageEnd;
                }
                else
                {
                    currentState = stack.Pop();
                    element = FudgeStreamElement.SubmessageFieldEnd;
                }
            }
            else
            {
                field = currentState.Fields.Dequeue();
                if (field.Type == FudgeMsgFieldType.Instance)
                {
                    stack.Push(currentState);
                    currentState = new State((FudgeMsg)field.Value);
                    element = FudgeStreamElement.SubmessageFieldStart;
                }
                else
                {
                    element = FudgeStreamElement.SimpleField;
                }
            }
            return element;
        }

        /// <inheritdoc/>
        public FudgeStreamElement CurrentElement
        {
            get { return element; }
        }

        /// <inheritdoc/>
        public FudgeFieldType FieldType
        {
            get { return field.Type; }
        }

        /// <inheritdoc/>
        public int? FieldOrdinal
        {
            get { return field.Ordinal; }
        }

        /// <inheritdoc/>
        public string FieldName
        {
            get { return field.Name; }
        }

        /// <inheritdoc/>
        public object FieldValue
        {
            get { return field.Value; }
        }

        #endregion

        private class State
        {
            public readonly FudgeMsg Msg;
            public readonly Queue<IFudgeField> Fields;

            public State(FudgeMsg msg)
            {
                Msg = msg;
                Fields = new Queue<IFudgeField>(msg.GetAllFields());
            }
        }
    }
}
