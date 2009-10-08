/**
 * Copyright (C) 2009 - 2009 by OpenGamma Inc.
 * 
 * Please see distribution for license.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenGamma.Fudge
{
    /// <summary>
    ///  An interface defining any arbitrary container for fields that can
    ///  be described by the Fudge specification.
    /// </summary>
    public interface IFudgeFieldContainer : IEnumerable<IFudgeField>
    {
        short GetNumFields();

        /// <summary>
        /// Return an unmodifiable list of all the fields in this message, in the index
        /// order for those fields.
        /// </summary>
        IList<IFudgeField> GetAllFields();

        IFudgeField GetByIndex(int index);

        IList<IFudgeField> GetAllByOrdinal(int ordinal);

        IFudgeField GetByOrdinal(int ordinal);

        IList<IFudgeField> GetAllByName(string name);

        IFudgeField GetByName(string name);

        object GetValue(string name);
 
        T GetValue<T>(string name);

        object GetValue(string name, Type type);

        object GetValue(int ordinal);

        T GetValue<T>(int ordinal);

        object GetValue(int ordinal, Type type);

        object GetValue(string name, int? ordinal);

        T GetValue<T>(string name, int? ordinal);

        object GetValue(string name, int? ordinal, Type type);

        /// <returns>value, or <c>null</c> if field not found.</returns>
        /// <exception cref="InvalidCastException">Field type could not be converted to a <c>double</c></exception>
        /// <exception cref="OverflowException">Field value could not fit within a <c>double</c></exception>
        double? GetDouble(string fieldName);

        /// <returns>value, or <c>null</c> if field not found.</returns>
        /// <exception cref="InvalidCastException">Field type could not be converted to a <c>double</c></exception>
        /// <exception cref="OverflowException">Field value could not fit within a <c>double</c></exception>
        double? GetDouble(int ordinal);

        /// <returns>value, or <c>null</c> if field not found.</returns>
        /// <exception cref="InvalidCastException">Field type could not be converted to a <c>float</c></exception>
        /// <exception cref="OverflowException">Field value could not fit within a <c>float</c></exception>
        float? GetFloat(string fieldName);

        /// <returns>value, or <c>null</c> if field not found.</returns>
        /// <exception cref="InvalidCastException">Field type could not be converted to a <c>float</c></exception>
        /// <exception cref="OverflowException">Field value could not fit within a <c>float</c></exception>
        float? GetFloat(int ordinal);

        /// <returns>value, or <c>null</c> if field not found.</returns>
        /// <exception cref="InvalidCastException">Field type could not be converted to a <c>long</c></exception>
        /// <exception cref="OverflowException">Field value could not fit within a <c>long</c></exception>
        long? GetLong(string fieldName);

        /// <returns>value, or <c>null</c> if field not found.</returns>
        /// <exception cref="InvalidCastException">Field type could not be converted to a <c>long</c></exception>
        /// <exception cref="OverflowException">Field value could not fit within a <c>long</c></exception>
        long? GetLong(int ordinal);

        /// <returns>value, or <c>null</c> if field not found.</returns>
        /// <exception cref="InvalidCastException">Field type could not be converted to an <c>int</c></exception>
        /// <exception cref="OverflowException">Field value could not fit within an <c>int</c></exception>
        int? GetInt(string fieldName);

        /// <returns>value, or <c>null</c> if field not found.</returns>
        /// <exception cref="InvalidCastException">Field type could not be converted to an <c>int</c></exception>
        /// <exception cref="OverflowException">Field value could not fit within an <c>int</c></exception>
        int? GetInt(int ordinal);

        /// <returns>value, or <c>null</c> if field not found.</returns>
        /// <exception cref="InvalidCastException">Field type could not be converted to a <c>short</c></exception>
        /// <exception cref="OverflowException">Field value could not fit within a <c>short</c></exception>
        short? GetShort(string fieldName);

        /// <returns>value, or <c>null</c> if field not found.</returns>
        /// <exception cref="InvalidCastException">Field type could not be converted to a <c>short</c></exception>
        /// <exception cref="OverflowException">Field value could not fit within a <c>short</c></exception>
        short? GetShort(int ordinal);

        /// <returns>value, or <c>null</c> if field not found.</returns>
        /// <exception cref="InvalidCastException">Field type could not be converted to a <c>byte</c></exception>
        /// <exception cref="OverflowException">Field value could not fit within a <c>byte</c></exception>
        sbyte? GetSByte(string fieldName);

        /// <returns>value, or <c>null</c> if field not found.</returns>
        /// <exception cref="InvalidCastException">Field type could not be converted to a <c>byte</c></exception>
        /// <exception cref="OverflowException">Field value could not fit within a <c>byte</c></exception>
        sbyte? GetSByte(int ordinal);

        /// <returns>value, or <c>null</c> if field not found.</returns>
        /// <exception cref="InvalidCastException">Field type could not be converted to a <c>bool</c></exception>
        /// <exception cref="OverflowException">Field value could not fit within a <c>bool</c></exception>
        bool? GetBoolean(string fieldName);

        /// <returns>value, or <c>null</c> if field not found.</returns>
        /// <exception cref="InvalidCastException">Field type could not be converted to a <c>bool</c></exception>
        /// <exception cref="OverflowException">Field value could not fit within a <c>bool</c></exception>
        bool? GetBoolean(int ordinal);

        /// <returns>value, or <c>null</c> if field not found.</returns>
        /// <exception cref="InvalidCastException">Field type could not be converted to a <c>string</c></exception>
        /// <exception cref="OverflowException">Field value could not fit within a <c>string</c></exception>
        string GetString(string fieldName);

        /// <returns>value, or <c>null</c> if field not found.</returns>
        /// <exception cref="InvalidCastException">Field type could not be converted to a <c>string</c></exception>
        /// <exception cref="OverflowException">Field value could not fit within a <c>string</c></exception>
        string GetString(int ordinal);
    }
}
