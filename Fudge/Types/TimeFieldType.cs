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
            int? timeZone;
            int seconds;
            int nanos;
            FudgeDateTimePrecision precision;

            ReadEncodedTime(input, out precision, out timeZone, out seconds, out nanos);

            if (timeZone == null)
            {
                // No timezone
                return new FudgeTime(precision, seconds, nanos);
            }
            else
            {
                // Timezone
                return new FudgeTime(precision, seconds, nanos, timeZone.Value);
            }
        }

        /// <inheritdoc/>
        public override void WriteValue(BinaryWriter output, FudgeTime value)
        {
            WriteEncodedTime(output, value.Precision, value.TimeZoneOffset, value.TotalSeconds, value.Nanoseconds);      
        }

        internal static void ReadEncodedTime(BinaryReader input, out FudgeDateTimePrecision precision, out int? timeZone, out int seconds, out int nanos)
        {
            int firstFourBytes = input.ReadInt32();
            nanos = input.ReadInt32();

            timeZone = firstFourBytes >> 24;
            precision = (FudgeDateTimePrecision)((firstFourBytes >> 20) & 0x0f);
            seconds = firstFourBytes & 0x000fffff;

            if (timeZone == -128)
            {
                // No timezone
                timeZone = null;
            }
            else
            {
                // Timezone
                timeZone = timeZone.Value * 15;
            }
        }

        internal static void WriteEncodedTime(BinaryWriter output, FudgeDateTimePrecision precision, int? timezone, int seconds, int nanos)
        {
            int firstFourBytes = (timezone.HasValue ? timezone.Value / 15 : -128) << 24;
            firstFourBytes |= ((int)precision) << 20;
            firstFourBytes |= seconds;
            output.Write(firstFourBytes);
            output.Write(nanos);
        }
    }
}
