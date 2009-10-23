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
using Fudge.Types;

namespace Fudge
{
    /// <summary>
    /// Container for a variable-sized field with a type that the current
    /// installation of Fudge cannot handle on decoding.
    /// In general, while Fudge supports an infinite number of
    /// <see cref="UnknownFudgeFieldType"/> instances with a particular type ID, it
    /// is optimal to use the factory method <see cref="FudgeTypeDictionary.GetUnknownType(int)"/>
    /// to obtain one for a particular context, which is what the Fudge decoding
    /// routines will do. 
    /// </summary>
    public class UnknownFudgeFieldValue
    {

        private readonly byte[] contents;
        private readonly UnknownFudgeFieldType type;

        public UnknownFudgeFieldValue(byte[] contents, UnknownFudgeFieldType type)
        {
            if (contents == null)
            {
                throw new ArgumentNullException("Contents must be provided");
            }
            if (type == null)
            {
                throw new ArgumentNullException("A valid UnknownFudgeFieldType must be specified");
            }
            this.contents = contents;
            this.type = type;
        }

        public byte[] Contents
        {
            get { return contents; }
        }

        public UnknownFudgeFieldType Type
        {
            get { return type; }
        }
    }
}
