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
    /// <summary>
    /// Base class to remove some of the boiler-plate in writing new <see cref="IFudgeStreamReader"/>s.
    /// </summary>
    public abstract class FudgeStreamReaderBase : IFudgeStreamReader
    {
        protected FudgeStreamReaderBase()
        {
            CurrentElement = FudgeStreamElement.NoElement;
        }

        #region IFudgeStreamReader Members

        public abstract bool HasNext {get;}

        public abstract FudgeStreamElement MoveNext();

        public FudgeStreamElement CurrentElement
        {
            get;
            protected set;
        }

        public FudgeFieldType FieldType
        {
            get;
            protected set;
        }

        public int? FieldOrdinal
        {
            get;
            protected set;
        }

        public string FieldName
        {
            get;
            protected set;
        }

        public object FieldValue
        {
            get;
            protected set;
        }

        #endregion
    }
}
