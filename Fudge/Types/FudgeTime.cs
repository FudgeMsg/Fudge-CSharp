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

namespace Fudge.Types
{
    /// <summary>
    /// <c>FudgeTime</c> represents a pure time value (i.e. one with no date component), with up to nanosecond resolution, and
    /// optionally carrying timezone information.
    /// </summary>
    public class FudgeTime
    {
        private readonly int seconds;
        private readonly int nanos;
        private readonly int? timeZoneOffset;
        private readonly FudgeDateTimePrecision precision;
        internal const int TicksPerSecond = 10000000;                    // A .net tick is 100 nanoseconds
        internal const int NanosPerTick = 1000000000 / TicksPerSecond;

        /// <summary>The default precision assumed for .net <see cref="DateTime"/> objects.</summary>
        public const FudgeDateTimePrecision DefaultDateTimePrecision = FudgeDateTimePrecision.Nanosecond;
        /// <summary>Represents midnight with no timezone</summary>
        public static readonly FudgeTime Midnight = new FudgeTime(FudgeDateTimePrecision.Nanosecond, 0, 0);

        /// <summary>
        /// Constructs a new <c>FudgeTime</c> which is just hours, without a timezone.
        /// </summary>
        /// <param name="hour"></param>
        public FudgeTime(int hour)
            : this(hour, 0, 0, 0, FudgeDateTimePrecision.Hour)
        {
        }

        /// <summary>
        /// Constructs a new <c>FudgeTime</c> based on separate hours and minutes, without a timezone.
        /// </summary>
        /// <param name="hour"></param>
        /// <param name="minute"></param>
        public FudgeTime(int hour, int minute)
            : this(hour, minute, 0, 0, FudgeDateTimePrecision.Minute)
        {
        }

        /// <summary>
        /// Constructs a new <c>FudgeTime</c> based on separate hours, minutes, seconds without a timezone.
        /// </summary>
        /// <param name="hour"></param>
        /// <param name="minute"></param>
        /// <param name="second"></param>
        public FudgeTime(int hour, int minute, int second)
            : this(hour, minute, second, 0, FudgeDateTimePrecision.Second)
        {
        }

        /// <summary>
        /// Constructs a new <c>FudgeTime</c> based on separate hours, minutes and seconds, with a timezone.
        /// </summary>
        /// <param name="hour"></param>
        /// <param name="minute"></param>
        /// <param name="second"></param>
        /// <param name="timeZoneOffset">Timezone offset from UTC, in minutes, must be multiple of 15</param>
        public FudgeTime(int hour, int minute, int second, int timeZoneOffset)
            : this(hour, minute, second, 0, timeZoneOffset, FudgeDateTimePrecision.Second)
        {
        }

        /// <summary>
        /// Constructs a new <c>FudgeTime</c> based on separate hours, minutes, seconds and nanoseconds, without a timezone.
        /// </summary>
        /// <param name="hour"></param>
        /// <param name="minute"></param>
        /// <param name="second"></param>
        /// <param name="nanoseconds"></param>
        /// <param name="precision"></param>
        public FudgeTime(int hour, int minute, int second, int nanoseconds, FudgeDateTimePrecision precision)
            : this(hour, minute, second, nanoseconds, null, precision)
        {
        }

        /// <summary>
        /// Constructs a new <c>FudgeTime</c> based on separate hours, minutes, seconds and nanoseconds, with a timezone.
        /// </summary>
        /// <param name="hour"></param>
        /// <param name="minute"></param>
        /// <param name="second"></param>
        /// <param name="nanoseconds"></param>
        /// <param name="timeZoneOffset">Timezone offset from UTC, in minutes, must be multiple of 15</param>
        /// <param name="precision"></param>
        public FudgeTime(int hour, int minute, int second, int nanoseconds, int? timeZoneOffset, FudgeDateTimePrecision precision)
        {
            if (hour < 0 || hour > 23)
                throw new ArgumentOutOfRangeException("hour");
            if (minute < 0 || minute > 59)
                throw new ArgumentOutOfRangeException("minute");
            if (second < 0 || second > 59)
                throw new ArgumentOutOfRangeException("second");
            if (nanoseconds < 0 || nanoseconds > 999999999)
                throw new ArgumentOutOfRangeException("nanoseconds");
            if (timeZoneOffset.HasValue)
            {
                if (timeZoneOffset % 15 != 0)
                    throw new ArgumentOutOfRangeException("timeZoneOffset", "offset must be a multiple of 15 (minutes)");
                if (timeZoneOffset < -127 * 15 || timeZoneOffset > 127 * 15)
                    throw new ArgumentOutOfRangeException("timeZoneOffset");
            }
            if (precision < FudgeDateTimePrecision.Hour)
                throw new ArgumentOutOfRangeException("precision");
            this.seconds = hour * 3600 + minute * 60 + second;
            this.nanos = nanoseconds;
            this.timeZoneOffset = timeZoneOffset;
            this.precision = precision;
        }

        /// <summary>
        /// Constructs a new <c>FudgeTime</c> based on a total number of seconds and nanoseconds, without a timezone.
        /// </summary>
        /// <param name="precision"></param>
        /// <param name="totalSeconds"></param>
        /// <param name="nanoseconds"></param>
        public FudgeTime(FudgeDateTimePrecision precision, int totalSeconds, int nanoseconds)
            : this(precision, totalSeconds, nanoseconds, null)
        {
        }

        /// <summary>
        /// Constructs a new <c>FudgeTime</c> based on a total number of seconds and nanoseconds, with a timezone.
        /// </summary>
        /// <param name="precision"></param>
        /// <param name="totalSeconds"></param>
        /// <param name="nanoseconds"></param>
        /// <param name="timeZoneOffset">Timezone offset from UTC, in minutes, must be multiple of 15</param>
        public FudgeTime(FudgeDateTimePrecision precision, int totalSeconds, int nanoseconds, int? timeZoneOffset)
        {
            if (totalSeconds < 0 || totalSeconds >= 24 * 60 * 60)
                throw new ArgumentOutOfRangeException("totalseconds");
            if (nanoseconds < 0 || nanoseconds > 999999999)
                throw new ArgumentOutOfRangeException("nanoseconds");
            if (timeZoneOffset.HasValue)
            {
                if (timeZoneOffset % 15 != 0)
                    throw new ArgumentOutOfRangeException("timeZoneOffset", "offset must be a multiple of 15 (minutes)");
                if (timeZoneOffset < -127 * 15 || timeZoneOffset > 127 * 15)
                    throw new ArgumentOutOfRangeException("timeZoneOffset");
            }
            if (precision < FudgeDateTimePrecision.Hour)
                throw new ArgumentOutOfRangeException("precision");
            this.precision = precision;
            this.seconds = totalSeconds;
            this.nanos = nanoseconds;
            this.timeZoneOffset = timeZoneOffset;
        }

        /// <summary>
        /// Constructs a <c>FudgeTime</c> from the time components of a .net <see cref="DateTime"/>.
        /// </summary>
        /// <param name="dateTime"><see cref="DateTime"/> to use.</param>
        public FudgeTime(DateTime dateTime)
            : this(dateTime, DefaultDateTimePrecision)
        {
        }

        /// <summary>
        /// Constructs a <c>FudgeTime</c> from the time components of a .net <see cref="DateTime"/>, specifying the precision.
        /// </summary>
        /// <param name="dateTime"><see cref="DateTime"/> to use.</param>
        /// <param name="precision"><see cref="FudgeDateTimePrecision"/> for this <c>FudgeTime</c>.</param>
        public FudgeTime(DateTime dateTime, FudgeDateTimePrecision precision)
        {
            if (precision < FudgeDateTimePrecision.Hour)
                throw new ArgumentOutOfRangeException("precision");
            TimeSpan time = dateTime.TimeOfDay;
            this.precision = precision;
            this.seconds = (int)(time.Ticks / TicksPerSecond);
            this.nanos = (int)(time.Ticks % TicksPerSecond) * NanosPerTick;
            switch (dateTime.Kind)
            {
                case DateTimeKind.Utc:
                    this.timeZoneOffset = 0;
                    break;
                case DateTimeKind.Unspecified:
                    this.timeZoneOffset = null;
                    break;
                case DateTimeKind.Local:
                    var tzOffset = TimeZoneInfo.Local.GetUtcOffset(dateTime);
                    this.timeZoneOffset = tzOffset.Hours * 60 + tzOffset.Minutes;
                    break;
            }
        }

        /// <summary>Gets the hour component of this time.</summary>
        public int Hour
        {
            get { return seconds / 3600; }
        }

        /// <summary>Gets the minute component of this time.</summary>
        public int Minute
        {
            get { return (seconds / 60) % 60; }
        }

        /// <summary>Gets the seconds component of this time.</summary>
        public int Second
        {
            get { return seconds % 60; }
        }

        /// <summary>Gets the total number of whole seconds since midnight that this time represents.</summary>
        public int TotalSeconds
        {
            get { return seconds; }
        }

        /// <summary>Gets the nanoseconds component of this time.</summary>
        public int Nanoseconds
        {
            get { return nanos; }
        }

        /// <summary>Gets the total number of nanoseconds since midnight that this time represents.</summary>
        public long TotalNanoseconds
        {
            get { return seconds * 1000000000L + nanos; }
        }

        /// <summary>Gets the offset from UTC in minutes, or returns <c>null</c> if there is no timezone.</summary>
        public int? TimeZoneOffset
        {
            get { return timeZoneOffset; }
        }

        /// <summary>Gets the precision of this time, as a <see cref="FudgeDateTimePrecision"/>.</summary>
        public FudgeDateTimePrecision Precision
        {
            get { return precision; }
        }

        #region Overrides from object

        private static readonly string[] precisionFormatters =
            {
                "",
                "",
                "",
                "",
                "{0:d2}",
                "{0:d2}:{1:d2}",
                "{0:d2}:{1:d2}:{2:d2}",
                "{0:d2}:{1:d2}:{2:d2}.{3:d3}",
                "{0:d2}:{1:d2}:{2:d2}.{3:d6}",
                "{0:d2}:{1:d2}:{2:d2}.{3:d9}",
            };
        private static readonly int[] nanoDividers = { 1, 1, 1, 1, 1, 1, 1, 1000000, 1000, 1 };

        /// <inheritdoc/>
        public override string ToString()
        {
            int subSecond = nanos / nanoDividers[(int)precision];
            string result = string.Format(precisionFormatters[(int)precision], Hour, Minute, Second, subSecond);
            if (timeZoneOffset.HasValue)
            {
                int mins = Math.Abs(timeZoneOffset.Value) % 60;
                int hours = Math.Abs(timeZoneOffset.Value) / 60;
                char prefix = (timeZoneOffset < 0) ? '-' : '+';
                result += string.Format(" {0}{1:d2}:{2:d2}", prefix, hours, mins);
            }
            return result;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            var other = obj as FudgeTime;
            if (other == null)
                return false;
            return this.seconds == other.seconds
                && this.nanos == other.nanos
                && this.timeZoneOffset == other.timeZoneOffset
                && this.precision == other.precision;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return seconds ^ nanos ^ (timeZoneOffset ?? int.MinValue) ^ (int)precision;
        }

        #endregion
    }
}
