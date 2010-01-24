/* <!--
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
using System.IO;

namespace Fudge.Types
{
    /// <summary>
    /// The type definition for a Fudge-encoded date
    /// </summary>
    public class DateFieldType : FudgeFieldType<FudgeDate>
    {
        /// <summary>
        /// A type defintion for date data.
        /// </summary>
        public static readonly DateFieldType Instance = new DateFieldType();

        /// <summary>
        /// Creates a new date field type
        /// </summary>
        public DateFieldType()
            : base(FudgeTypeDictionary.DATE_TYPE_ID, false, 4)
        {
        }

        /// <inheritdoc/>
        public override FudgeDate ReadTypedValue(BinaryReader input, int dataSize)
        {
            int rawValue = input.ReadInt32();
            return new FudgeDate(rawValue);
        }

        /// <inheritdoc/>
        public override void WriteValue(BinaryWriter output, FudgeDate value)
        {
            output.Write(value.RawValue);
        }
    }
}
