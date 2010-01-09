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
    /// The type definition for a Fudge-encoded time
    /// </summary>
    public class TimeFieldType : FudgeFieldType<FudgeTime>
    {
        /// <summary>
        /// A type defintion for time data.
        /// </summary>
        public static readonly TimeFieldType Instance = new TimeFieldType();

        /// <summary>
        /// Creates a new time field type
        /// </summary>
        public TimeFieldType()
            : base(FudgeTypeDictionary.TIME_TYPE_ID, false, 8)
        {
        }

        /// <inheritdoc/>
        public override FudgeTime ReadTypedValue(BinaryReader input, int dataSize)
        {
            int firstFourBytes = input.ReadInt32();
            int nanos = input.ReadInt32();

            int timezone = firstFourBytes >> 24;
            var precision = (FudgeDateTime.Precision)((firstFourBytes >> 20) & 0x0f);
            var seconds = firstFourBytes & 0x000fffff;

            if (timezone == -128)
            {
                // No timezone
                return new FudgeTime(precision, seconds, nanos);
            }
            else
            {
                // Timezone
                return new FudgeTime(precision, seconds, nanos, timezone * 15);
            }
        }

        /// <inheritdoc/>
        public override void WriteValue(BinaryWriter output, FudgeTime value)
        {
            int firstFourBytes = (value.HasTimeZone ? value.TimeZoneOffset / 15 : -128) << 24;
            firstFourBytes |= ((int)value.Precision) << 20;
            firstFourBytes |= value.TotalSeconds;
            output.Write(firstFourBytes);
            output.Write(value.Nanoseconds);        
        }
    }
}
