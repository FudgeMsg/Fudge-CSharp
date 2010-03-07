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
    /// <c>FudgeDate</c> represents a pure date in the range +-10000 years.
    /// </summary>
    /// <remarks>
    /// It is possible for the <c>FudgeDate</c> to represent a date that is not valid
    /// (e.g. 30th February), however converting to a <see cref="DateTime"/> will always
    /// return a valid date (rolling to the next valid day if appropriate).
    /// </remarks>
    public class FudgeDate : IComparable<FudgeDate>, IConvertible
    {
        private readonly int rawValue;
        private static readonly int[] monthLengths = { 31, 29, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };
        private const int MinYear = -9999;
        private const int MaxYear = 9999;

        /// <summary>
        /// Constructs a new <c>FudgeDate</c> based on a raw value in the form YYYYMMDD.
        /// </summary>
        /// <param name="rawValue">Value to use.</param>
        /// <remarks>Note that no checking is performed that the value is a valid date, so 20010230 would be accepted.</remarks>
        public FudgeDate(int rawValue)
        {
            this.rawValue = rawValue;
        }

        /// <summary>
        /// Construts a new <c>FudgeDate</c> based on a year, month and day
        /// </summary>
        /// <param name="year">Year</param>
        /// <param name="month">Month</param>
        /// <param name="day">Day</param>
        public FudgeDate(int year, int month, int day)
        {
            if (year < MinYear || year > MaxYear)
                throw new ArgumentOutOfRangeException("year", "FudgeDate years must be in the range " + MinYear + " to " + MaxYear);
            if (month < 1 || month > 12)
                throw new ArgumentOutOfRangeException("month");
            if (day < 1 || day > 31)
                throw new ArgumentOutOfRangeException("day");

            if (year >= 0)
            {
                // Note that 0 is not a valid year as the first year was 1AD, but we handle it anyway
                rawValue = year * 10000 + month * 100 + day;
            }
            else
            {
                // We need them to be of the form -10000101 for 1 Jan 1000 BC
                rawValue = year * 10000 - month * 100 - day;
            }
        }

        /// <summary>
        /// Cosntructs a new <c>FudgeDate</c> from a .net <see cref="DateTime"/>.
        /// </summary>
        /// <param name="dateTime">DateTime to base this <c>FudgeDate</c> on</param>.
        /// <remarks>Note that timezone information is ignored, and only the date part used.</remarks>
        public FudgeDate(DateTime dateTime)
            : this(dateTime.Year, dateTime.Month, dateTime.Day)
        {
            // DateTime years run from 1 to 9999, so we don't have to worry about negatives
        }

        /// <summary>
        /// Gets the year part of the <c>FudgeDate</c>.
        /// </summary>
        /// <remarks>
        /// Note that 1 AD is year 1, and 1 BC is year -1.  Year 0 is meaningless.
        /// </remarks>
        public int Year
        {
            get { return rawValue / 10000; }
        }

        /// <summary>
        /// Gets the month part of the <c>FudgeDate</c>, in the range 1 to 12 for valid dates.
        /// </summary>
        public int Month
        {
            get
            {
                return (Math.Abs(rawValue) / 100) % 100;
            }
        }

        /// <summary>
        /// Gets the day part of the <c>FudgeDate</c>, in the range 1 to 31 for valid dates.
        /// </summary>
        public int Day
        {
            get
            {
                return Math.Abs(rawValue) % 100;
            }
        }

        /// <summary>
        /// Indicates whether the <c>FudgeDate</c> represents a real date
        /// </summary>
        public bool IsValid
        {
            get
            {
                int year = Year;
                if (year == 0)
                    return false;       // There was no year zero

                int month = Month;
                int day = Day;
                if (month < 1 || month > 12)
                    return false;
                if (day < 1 || day > monthLengths[month - 1])
                    return false;
                if (month == 2 && day == 29 && !IsLeap(year))
                    return false;

                return true;
            }
        }

        /// <summary>
        /// If this date does not represent a valid real date, then returns the next valid day.
        /// </summary>
        /// <returns>Valid date.</returns>
        public FudgeDate RollToValidDate()
        {
            if (IsValid)
                return this;

            int year = Year;

            if (year == 0)
                return new FudgeDate(00010101);     // There was no year zero

            int month = Month;
            int day = 1;

            month++;
            if (month == 13)
            {
                month = 1;
                year++;
                if (year == 0)
                    year++;         // There was no year 0
            }
            return new FudgeDate(year, month, day);
        }

        /// <summary>
        /// Tests whether a given year is a leap year.
        /// </summary>
        /// <param name="year">Year to test.</param>
        /// <returns><c>True</c> if the year is a leap year.</returns>
        public static bool IsLeap(int year)
        {
            if (year < 0)
                year++;         // There was no year zero, and leaps are fairly meaningless that far back anyway so we just continue the same pattern

            if (year % 100 == 0)
                return (year % 400 == 0);
            else
                return (year % 4 == 0);
        }

        /// <summary>
        /// Returns the raw representation of this date.
        /// </summary>
        public int RawValue
        {
            get { return rawValue; }
        }

        /// <summary>
        /// Converts this <c>FudgeDate</c> into a .net <see cref="DateTime"/>.
        /// </summary>
        /// <returns><see cref="DateTime"/> corresponding to this date.</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <remarks>If the <c>FudgeDate</c> does not represent a valid date, it will be rolled to the next valid date.</remarks>
        public DateTime ToDateTime()
        {
            var validDate = RollToValidDate();
            return new DateTime(validDate.Year, validDate.Month, validDate.Day);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            FudgeDate other = obj as FudgeDate;
            if (other == null)
                return false;

            return (this.rawValue == other.rawValue);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return rawValue.GetHashCode();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return string.Format("{0:d4}-{1:d2}-{2:d2}", Year, Month, Day);
        }

        /// <summary>
        /// Converts the date to a string based on a given <see cref="FudgeDateTimePrecision"/>.
        /// </summary>
        /// <param name="precision">Precision to use.</param>
        /// <returns>Date as a string.</returns>
        public string ToString(FudgeDateTimePrecision precision)
        {
            switch (precision)
            {
                case FudgeDateTimePrecision.Century:
                    return string.Format("{0:d2}00", Year / 100);
                case FudgeDateTimePrecision.Year:
                    return string.Format("{0:d4}", Year);
                case FudgeDateTimePrecision.Month:
                    return string.Format("{0:d4}-{1:d2}", Year, Month);
                default:
                    return ToString();
            }
        }

        #region IComparable<FudgeDate> Members

        /// <inheritdoc/>
        public int CompareTo(FudgeDate other)
        {
            if (this.Year != other.Year)
            {
                return this.Year < other.Year ? -1 : 1;
            }

            int subYearDiff = (Math.Abs(this.rawValue) % 10000) - (Math.Abs(other.rawValue) % 10000);
            return Math.Sign(subYearDiff); ;
        }

        #endregion

        #region IConvertible Members

        /// <inheritdoc/>
        public TypeCode GetTypeCode()
        {
            return TypeCode.Object;
        }

        /// <inheritdoc/>
        public bool ToBoolean(IFormatProvider provider)
        {
            throw new InvalidCastException();
        }

        /// <inheritdoc/>
        public byte ToByte(IFormatProvider provider)
        {
            throw new InvalidCastException();
        }

        /// <inheritdoc/>
        public char ToChar(IFormatProvider provider)
        {
            throw new InvalidCastException();
        }

        /// <inheritdoc/>
        public DateTime ToDateTime(IFormatProvider provider)
        {
            return this.ToDateTime();
        }

        /// <inheritdoc/>
        public decimal ToDecimal(IFormatProvider provider)
        {
            throw new InvalidCastException();
        }

        /// <inheritdoc/>
        public double ToDouble(IFormatProvider provider)
        {
            throw new InvalidCastException();
        }

        /// <inheritdoc/>
        public short ToInt16(IFormatProvider provider)
        {
            throw new InvalidCastException();
        }

        /// <inheritdoc/>
        public int ToInt32(IFormatProvider provider)
        {
            throw new InvalidCastException();
        }

        /// <inheritdoc/>
        public long ToInt64(IFormatProvider provider)
        {
            throw new InvalidCastException();
        }

        /// <inheritdoc/>
        public sbyte ToSByte(IFormatProvider provider)
        {
            throw new InvalidCastException();
        }

        /// <inheritdoc/>
        public float ToSingle(IFormatProvider provider)
        {
            throw new InvalidCastException();
        }

        /// <inheritdoc/>
        public string ToString(IFormatProvider provider)
        {
            return ToString();
        }

        /// <inheritdoc/>
        public object ToType(Type conversionType, IFormatProvider provider)
        {
            if (conversionType == typeof(DateTime))
                return this.ToDateTime();
            if (conversionType == typeof(FudgeDateTime))
                return new FudgeDateTime(this, null);
            if (conversionType == typeof(DateTimeOffset))
                return new DateTimeOffset(this.ToDateTime(), TimeSpan.Zero);
            if (conversionType == typeof(string))
                return ToString();

            throw new InvalidCastException();
        }

        /// <inheritdoc/>
        public ushort ToUInt16(IFormatProvider provider)
        {
            throw new InvalidCastException();
        }

        /// <inheritdoc/>
        public uint ToUInt32(IFormatProvider provider)
        {
            throw new InvalidCastException();
        }

        /// <inheritdoc/>
        public ulong ToUInt64(IFormatProvider provider)
        {
            throw new InvalidCastException();
        }

        #endregion
    }
}
