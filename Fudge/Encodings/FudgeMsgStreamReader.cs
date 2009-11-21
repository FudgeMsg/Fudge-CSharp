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
using Fudge.Taxon;
using Fudge.Types;

namespace Fudge.Encodings
{
    public class FudgeMsgStreamReader : IFudgeStreamReader
    {
        // TODO t0rx 2009-11-13 -- What about envelopes?
        private Stack<State> stack = new Stack<State>();
        private State currentState;
        private FudgeStreamElement element = FudgeStreamElement.NoElement;
        private IFudgeField field;

        public FudgeMsgStreamReader(FudgeMsg msg)
        {
            currentState = new State(msg);
        }

        #region IFudgeStreamReader Members

        public bool HasNext
        {
            get
            {
                return (stack.Count > 0 || currentState.Fields.Count > 0);
            }
        }

        public FudgeStreamElement MoveNext()
        {
            if (currentState.Fields.Count == 0)
            {
                if (stack.Count == 0)
                    currentState = null;
                else
                    currentState = stack.Pop();
                return FudgeStreamElement.SubmessageFieldEnd;
            }
            else
            {
                field = currentState.Fields.Dequeue();
                if (field.Type == FudgeMsgFieldType.Instance)
                {
                    stack.Push(currentState);
                    currentState = new State((FudgeMsg)field.Value);
                    return FudgeStreamElement.SubmessageFieldStart;
                }
                else
                {
                    return FudgeStreamElement.SimpleField;
                }
            }
        }

        public FudgeStreamElement CurrentElement
        {
            get { return element; }
        }

        public int ProcessingDirectives
        {
            get { throw new NotImplementedException(); }
        }

        public int SchemaVersion
        {
            get { throw new NotImplementedException(); }
        }

        public int TaxonomyId
        {
            get { throw new NotImplementedException(); }
        }

        public int EnvelopeSize
        {
            get { throw new NotImplementedException(); }
        }

        public FudgeFieldType FieldType
        {
            get { return field.Type; }
        }

        public int? FieldOrdinal
        {
            get { return field.Ordinal; }
        }

        public string FieldName
        {
            get { return field.Name; }
        }

        public object FieldValue
        {
            get { return field.Value; }
        }

        public IFudgeTaxonomy Taxonomy
        {
            get { throw new NotImplementedException(); }
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
