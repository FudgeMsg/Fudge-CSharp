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
using System.Collections;
using System.Diagnostics;

namespace Fudge.Serialization
{
    /// <summary>
    /// <c>TrackingFudgeMsg</c> keeps track of which fields are fetched.
    /// </summary>
    /// <remarks>
    /// This is used for evolvable objects, whereby any fields not read during
    /// deserialisation are considered "future".  Use <see cref="GetUnvisitedFields"/>
    /// to retried these fields.
    /// </remarks>
    public class TrackingFudgeMsg : FudgeMsg
    {
        private readonly List<bool> markers;

        public TrackingFudgeMsg()
        {
            markers = new List<bool>();
        }

        public TrackingFudgeMsg(FudgeMsg other) : base(other.TypeDictionary)
        {
            // We don't use the equivalent FudgeMsg constructor as it takes a byte copy which is slow
            // and here we don't care about mutability of the source
            int nFields = other.GetNumFields();
            markers = new List<bool>(nFields);
            for (int i = 0; i < nFields; i++)
            {
                Add(other.GetByIndex(i));       // Will also add the marker
            }
        }

        public TrackingFudgeMsg(params IFudgeField[] fields)
        {
            // Can't use equivalent FudgeMsg constructor because markers is not yet
            // initialised and it will call Add() during construction
            int nFields = fields.Length;
            markers = new List<bool>(nFields);
            for (int i = 0; i < nFields; i++)
            {
                Add(fields[i]);       // Will also add the marker
            }
        }

        public IList<IFudgeField> GetUnvisitedFields()
        {
            Debug.Assert(markers.Count == base.GetNumFields(), "Tracker out of sync with base message");
            var result = new List<IFudgeField>();
            for (int i = 0; i < markers.Count; i++)
            {
                if (!markers[i])
                {
                    result.Add(GetByIndex(i));
                }
            }
            return result;
        }

        public override void Add(string name, int? ordinal, FudgeFieldType type, object value)
        {
            base.Add(name, ordinal, type, value);
            markers.Add(false);
        }

        public override object GetValue(string name)
        {
            Debug.Assert(markers.Count == base.GetNumFields(), "Tracker out of sync with base message");
            int index = GetIndex(name, null);
            if (index == -1)
            {
                return null;
            }

            markers[index] = true;
            return base.GetByIndex(index).Value;
        }

        public override object GetValue(int ordinal)
        {
            Debug.Assert(markers.Count == base.GetNumFields(), "Tracker out of sync with base message");
            int index = GetIndex(null, ordinal);
            if (index == -1)
            {
                return null;
            }

            markers[index] = true;
            return base.GetByIndex(index).Value;
        }

        public override object GetValue(string name, int? ordinal)
        {
            Debug.Assert(markers.Count == base.GetNumFields(), "Tracker out of sync with base message");
            int index = GetIndex(name, ordinal);
            if (index == -1)
            {
                return null;
            }

            markers[index] = true;
            return base.GetByIndex(index).Value;
        }

        // TODO t0rx 2009-10-18 -- Need to also deal with GetByName, GetByOrdinal, GetAllByName, GetAllByOrdinal, etc.

        private void InitializeMarkers(int size)
        {
            for (int i = 0; i < size; i++)       // Can't find a better way than this
            {
                markers.Add(false);
            }
        }
    }
}
