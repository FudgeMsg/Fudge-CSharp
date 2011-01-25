/* <!--
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
 * -->
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Fudge.Taxon;

namespace Fudge.Types
{
    /// <summary>
    /// The type definition for an array of 16-bit integers.
    /// </summary>
    public class ShortArrayFieldType : FudgeArrayFieldTypeBase<short>
    {
        /// <summary>
        /// A type definition for arrays of signed 16-bit integers.
        /// </summary>
        public static readonly ShortArrayFieldType Instance = new ShortArrayFieldType();

        /// <summary>
        /// Creates a type definition for arrays of signed 16-bit integers.
        /// </summary>
        public ShortArrayFieldType()
            : base(FudgeTypeDictionary.SHORT_ARRAY_TYPE_ID, 2, (w, e) => w.Write(e), r => r.ReadInt16())
        {
        }
    }
}
