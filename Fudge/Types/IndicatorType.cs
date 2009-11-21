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

namespace Fudge.Types
{
    /// <summary>
    /// The only value of a field with the Indicator type.
    /// </summary>
    public sealed class IndicatorType : IConvertible
    {
        private IndicatorType() { }

        /// <summary>
        /// The only instance of this type.
        /// </summary>
        public static readonly IndicatorType Instance = new IndicatorType();

        #region IConvertible Members

        public TypeCode GetTypeCode()
        {
            return TypeCode.Object;
        }

        public bool ToBoolean(IFormatProvider provider)
        {
            return false;
        }

        public byte ToByte(IFormatProvider provider)
        {
            return 0;
        }

        public char ToChar(IFormatProvider provider)
        {
            throw new InvalidCastException();
        }

        public DateTime ToDateTime(IFormatProvider provider)
        {
            throw new InvalidCastException();
        }

        public decimal ToDecimal(IFormatProvider provider)
        {
            return 0;
        }

        public double ToDouble(IFormatProvider provider)
        {
            return 0;
        }

        public short ToInt16(IFormatProvider provider)
        {
            return 0;
        }

        public int ToInt32(IFormatProvider provider)
        {
            return 0;
        }

        public long ToInt64(IFormatProvider provider)
        {
            return 0;
        }

        public sbyte ToSByte(IFormatProvider provider)
        {
            return 0;
        }

        public float ToSingle(IFormatProvider provider)
        {
            return 0;
        }

        public string ToString(IFormatProvider provider)
        {
            return "";
        }

        public object ToType(Type conversionType, IFormatProvider provider)
        {
            throw new InvalidCastException();
        }

        public ushort ToUInt16(IFormatProvider provider)
        {
            return 0;
        }

        public uint ToUInt32(IFormatProvider provider)
        {
            return 0;
        }

        public ulong ToUInt64(IFormatProvider provider)
        {
            return 0;
        }

        #endregion
    }
}
