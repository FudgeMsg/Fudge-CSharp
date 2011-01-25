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
    /// The type definition for a Fudge-encoded datetime
    /// </summary>
    public class DateTimeFieldType : FudgeFieldType<FudgeDateTime>
    {
        /// <summary>
        /// A type defintion for datetime data.
        /// </summary>
        public static readonly DateTimeFieldType Instance = new DateTimeFieldType();

        /// <summary>
        /// Creates a new date field type
        /// </summary>
        public DateTimeFieldType()
            : base(FudgeTypeDictionary.DATETIME_TYPE_ID, false, 12)
        {
        }

        /// <inheritdoc/>
        public override object Minimize(object value, ref FudgeFieldType type)
        {
            // If it's a pure date we can just use a date field instead

            if (value == null)
                return null;

            FudgeDateTime fdt = value as FudgeDateTime;
            if (fdt == null)
                throw new ArgumentException("value must be FudgeDateTime");

            if (fdt.Precision == FudgeDateTimePrecision.Day || fdt.Time.Equals(FudgeTime.Midnight))
            {
                // We can just use a date
                type = DateFieldType.Instance;
                return fdt.Date;
            }

            return fdt;
        }

        /// <inheritdoc/>
        public override FudgeDateTime ReadTypedValue(BinaryReader input, int dataSize)
        {
            FudgeDate date = DateFieldType.Instance.ReadTypedValue(input, 0);

            int? timeZone;
            int seconds;
            int nanos;
            FudgeDateTimePrecision precision;

            TimeFieldType.ReadEncodedTime(input, out precision, out timeZone, out seconds, out nanos);

            // Time can't have a precision of worse than hours
            var timePrecision = (precision > FudgeDateTimePrecision.Day) ? precision : FudgeDateTimePrecision.Hour;
            var time = new FudgeTime(timePrecision, seconds, nanos, timeZone);

            return new FudgeDateTime(date, time, precision);
        }

        /// <inheritdoc/>
        public override void WriteValue(BinaryWriter output, FudgeDateTime value)
        {
            DateFieldType.Instance.WriteValue(output, value.Date);
            TimeFieldType.WriteEncodedTime(output, value.Precision, value.TimeZoneOffset, value.Time.TotalSeconds, value.Nanoseconds);
        }
    }
}
