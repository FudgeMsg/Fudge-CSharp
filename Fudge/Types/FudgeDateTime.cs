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
    /// <c>FudgeDateTime</c> represents a point in time from 9999BC to 9999AD, with up to nanosecond precision and
    /// optional timezone information.
    /// </summary>
    /// <remarks><c>FudgeDateTime</c> is most similar to .net's <see cref="DateTimeOffset"/> struct, but has
    /// better precision and range, and also allows for the offset to be optional.</remarks>
    /// <seealso cref="FudgeDate"/>
    /// <seealso cref="FudgeTime"/>
    public class FudgeDateTime
    {
        private readonly FudgeDate date;
        private readonly FudgeTime time;
        private readonly FudgeDateTimePrecision precision;

        /// <summary>
        /// Constructs a new <c>FudgeDateTime</c> based on a .net <see cref="DateTime"/>.
        /// </summary>
        /// <param name="dateTime"></param>
        public FudgeDateTime(DateTime dateTime) : this(dateTime, FudgeTime.DefaultDateTimePrecision)
        {
        }

        /// <summary>
        /// Constructs a new <c>FudgeDateTime</c> based on a .net <see cref="DateTime"/>, specifying the precision.
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="precision"></param>
        public FudgeDateTime(DateTime dateTime, FudgeDateTimePrecision precision)
        {
            this.date = new FudgeDate(dateTime);
            if (precision > FudgeDateTimePrecision.Day)
            {
                this.time = new FudgeTime(dateTime, precision);
            }
            else
            {
                this.time = FudgeTime.Midnight;
            }
            this.precision = precision;
        }

        /// <summary>
        /// Constructs a new <c>FudgeDateTime</c> based on a .net <see cref="DateTimeOffset"/>.
        /// </summary>
        /// <param name="dateTimeOffset"><see cref="DateTimeOffset"/> to use.</param>
        /// <remarks>The offset must be on a 15-minute boundary and within the +/- 30 hour range allowed by <c>FudgeDateTime</c>.</remarks>
        public FudgeDateTime(DateTimeOffset dateTimeOffset) : this (dateTimeOffset, FudgeTime.DefaultDateTimePrecision)
        {
        }

        /// <summary>
        /// Constructs a new <c>FudgeDateTime</c> based on a .net <see cref="DateTimeOffset"/>, specifying the precision.
        /// </summary>
        /// <param name="dateTimeOffset"><see cref="DateTimeOffset"/> to use.</param>
        /// <param name="precision"></param>
        /// <remarks>The offset must be on a 15-minute boundary and within the +/- 30 hour range allowed by <c>FudgeDateTime</c>.</remarks>
        public FudgeDateTime(DateTimeOffset dateTimeOffset, FudgeDateTimePrecision precision)
        {
            this.date = new FudgeDate(dateTimeOffset.Date);
            if (precision > FudgeDateTimePrecision.Day)
            {
                int seconds = (int)(dateTimeOffset.TimeOfDay.Ticks / FudgeTime.TicksPerSecond);
                int nanos = (int)(dateTimeOffset.TimeOfDay.Ticks % FudgeTime.TicksPerSecond);
                int offset = (int)(dateTimeOffset.Offset.Ticks / FudgeTime.TicksPerSecond / 60);
                this.time = new FudgeTime(precision, seconds, nanos, offset);
            }
            else
            {
                // Time and offset are irrelevant
                this.time = FudgeTime.Midnight;
            }
            this.precision = precision;
        }

        /// <summary>
        /// Constructs a <c>FudgeDateTime</c> from component values without timezone information.
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="day"></param>
        /// <param name="hour"></param>
        /// <param name="minute"></param>
        /// <param name="second"></param>
        /// <param name="nanosecond"></param>
        /// <param name="precision"></param>
        public FudgeDateTime(int year, int month, int day, int hour, int minute, int second, int nanosecond, FudgeDateTimePrecision precision)
        {
            this.date = new FudgeDate(year, month, day);
            if (precision > FudgeDateTimePrecision.Day)
            {
                this.time = new FudgeTime(hour, minute, second, nanosecond, precision);
            }
            else
            {
                this.time = FudgeTime.Midnight;
            }
            this.precision = precision;
        }

        /// <summary>
        /// Constructs a <c>FudgeDateTime</c> from component values with timezone information.
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="day"></param>
        /// <param name="hour"></param>
        /// <param name="minute"></param>
        /// <param name="second"></param>
        /// <param name="nanosecond"></param>
        /// <param name="timeZoneOffset">Timezone offset from UTC, in minutes, must be multiple of 15</param>
        /// <param name="precision"></param>
        public FudgeDateTime(int year, int month, int day, int hour, int minute, int second, int nanosecond, int timeZoneOffset, FudgeDateTimePrecision precision)
        {
            this.date = new FudgeDate(year, month, day);
            if (precision > FudgeDateTimePrecision.Day)
            {
                this.time = new FudgeTime(hour, minute, second, nanosecond, timeZoneOffset, precision);
            }
            else
            {
                this.time = FudgeTime.Midnight;
            }
            this.precision = precision;
        }

        /// <summary>
        /// Constructs a new <c>FudgeDateTime</c> using a <see cref="FudgeDate"/> and <see cref="FudgeTime"/>, taking the
        /// precision from the <see cref="FudgeTime"/>.
        /// </summary>
        /// <param name="date">Date part of the datetime.</param>
        /// <param name="time">Time part of the datetime, may be <c>null</c>.</param>
        public FudgeDateTime(FudgeDate date, FudgeTime time) : this(date, time, time == null ? FudgeDateTimePrecision.Day : time.Precision)
        {
        }

        /// <summary>
        /// Constructs a new <c>FudgeDateTime</c> using a <see cref="FudgeDate"/> and <see cref="FudgeTime"/>, specifying the precision.
        /// </summary>
        /// <param name="date">Date part of the datetime.</param>
        /// <param name="time">Time part of the datetime, may be <c>null</c>.</param>
        /// <param name="precision">Precision to use.</param>
        public FudgeDateTime(FudgeDate date, FudgeTime time, FudgeDateTimePrecision precision)
        {
            this.date = date;
            if (time == null)
            {
                this.time = FudgeTime.Midnight;
            }
            else
            {
                this.time = time;
            }
            this.precision = precision;
        }

        /// <summary>
        /// Gets the year part of the <c>FudgeDateTime</c>.
        /// </summary>
        /// <remarks>
        /// Note that 1 AD is year 1, and 1 BC is year -1.  Year 0 is meaningless.
        /// </remarks>
        public int Year
        {
            get { return date.Year; }
        }

        /// <summary>
        /// Gets the month part of the <c>FudgeDateTime</c>, in the range 1 to 12 for valid dates.
        /// </summary>
        public int Month
        {
            get { return date.Month; }
        }

        /// <summary>
        /// Gets the day part of the <c>FudgeDateTime</c>, in the range 1 to 31 for valid dates.
        /// </summary>
        public int Day
        {
            get { return date.Day; }
        }

        /// <summary>
        /// Indicates whether the <c>FudgeDateTime</c> represents a real date
        /// </summary>
        public bool IsValidDate
        {
            get { return date.IsValid; }
        }

        /// <summary>Gets the hour component of this <c>FudgeDateTime</c>.</summary>
        public int Hour
        {
            get { return time.Hour; }
        }

        /// <summary>Gets the minute component of this <c>FudgeDateTime</c>.</summary>
        public int Minute
        {
            get { return time.Minute; }
        }

        /// <summary>Gets the seconds component of this <c>FudgeDateTime</c>.</summary>
        public int Second
        {
            get { return time.Second; }
        }

        /// <summary>Gets the nanoseconds component of this <c>FudgeDateTime</c>.</summary>
        public int Nanoseconds
        {
            get { return time.Nanoseconds; }
        }

        /// <summary>Gets the offset from UTC in minutes.</summary>
        public int? TimeZoneOffset
        {
            get { return time.TimeZoneOffset; }
        }

        /// <summary>Gets the date component of this <c>FudgeDateTime</c>.</summary>
        public FudgeDate Date
        {
            get { return date; }
        }

        /// <summary>Gets the time component of this <c>FudgeDateTime</c>.</summary>
        public FudgeTime Time
        {
            get { return time; }
        }

        /// <summary>Gets the precision of this <c>FudgeDateTime</c>.</summary>
        public FudgeDateTimePrecision Precision
        {
            get { return precision; }
        }

        /// <summary>
        /// Converts the <c>FudgeDateTime</c> to a .net <see cref="DateTime"/>.
        /// </summary>
        /// <returns>Equivalent .net <see cref="DateTime"/> object.</returns>
        /// <remarks>
        /// The behaviour of this method is that datetimes with an unspecified timezone are represented using
        /// an unspecified timezone in the <see cref="DateTime"/>.  For datetimes that contain timezone information,
        /// they are converted to UTC and returned as a <see cref="DateTime"/> with kind <see cref="DateTimeKind.Utc"/>.
        /// If you want more control over this behaviour, use <see cref="ToDateTime(DateTimeKind,bool)"/>.
        /// </remarks>
        public DateTime ToDateTime()
        {
            if (TimeZoneOffset == null)
                return ToDateTime(DateTimeKind.Unspecified, false);
            else
                return ToDateTime(DateTimeKind.Utc, true);
        }

        /// <summary>
        /// Converts the <c>FudgeDateTime</c> to a .net <see cref="DateTime"/> allowing some control over timezone handling.
        /// </summary>
        /// <param name="kind"><see cref="DateTimeKind"/> for the result.</param>
        /// <param name="considerUnspecifiedAsUtc">If true then values with no timezone are considered to be UTC.</param>
        /// <returns>Equivalent .net <see cref="DateTime"/> object.</returns>
        /// <remarks>
        /// If <c>considerUnspecifiedAsUtc</c> is false, then converting to a <see cref="DateTimeKind.Local"/> <see cref="DateTime"/>
        /// will not apply any offset calculation, but simply use the time as it is.
        /// </remarks>
        public DateTime ToDateTime(DateTimeKind kind, bool considerUnspecifiedAsUtc)
        {
            if (Year < 1 || Year > 9999)
            {
                // REVIEW 2010-01-10 t0rx -- How should we handle attempts to convert to dates outside range
                throw new ArgumentOutOfRangeException("year", "Cannot convert date time of '" + this + "' to .net DateTime, as year is outside allowable range");
            }

            // Work out as ticks as all the other DateTime constructors lose accuracy beyond milliseconds
            var baseDT = new DateTime(Year, Month, Day, Hour, Minute, Second);
            long ticks = baseDT.Ticks;
            ticks += Nanoseconds / FudgeTime.NanosPerTick;

            if (TimeZoneOffset == null && !considerUnspecifiedAsUtc)
            {
                // Just return it
                return new DateTime(ticks, kind);
            }
            else
            {
                if (kind != DateTimeKind.Unspecified)
                {
                    // Convert to UTC
                    ticks -= (long)(TimeZoneOffset ?? 0) * 60 * FudgeTime.TicksPerSecond;
                }

                if (kind == DateTimeKind.Local)
                {
                    // Convert to local
                    TimeSpan tzOffset = TimeZoneInfo.Local.GetUtcOffset(baseDT);
                    ticks += tzOffset.Ticks;
                }

                return new DateTime(ticks, kind);
            }
        }

        /// <summary>
        /// Converts this <c>FudgeDateTime</c> to a .net <see cref="DateTimeOffset"/> object.
        /// </summary>
        /// <returns>Equivalent <see cref="DateTimeOffset"/>.</returns>
        public DateTimeOffset ToDateTimeOffset()
        {
            DateTime dt = ToDateTime(DateTimeKind.Unspecified, false);
            long offsetTicks = (long)(TimeZoneOffset ?? 0) * 60 * FudgeTime.TicksPerSecond;
            TimeSpan ts = new TimeSpan(offsetTicks);
            return new DateTimeOffset(dt, ts);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            string result = date.ToString(precision);
            if (precision > FudgeDateTimePrecision.Day)
                result += " " + time.ToString();
            return result;
        }
    }
}
