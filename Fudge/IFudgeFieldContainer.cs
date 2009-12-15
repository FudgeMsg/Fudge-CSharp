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
using System.Text;

namespace Fudge
{
    /// <summary>
    ///  An interface defining any arbitrary container for fields that can
    ///  be described by the Fudge specification.
    /// </summary>
    public interface IFudgeFieldContainer : IEnumerable<IFudgeField>
    {
        
        /// <summary>
        /// Returns the number of fields currently in this message.
        /// </summary>
        /// <returns>number of fields</returns>
        short GetNumFields();

        /// <summary>
        /// Return an unmodifiable list of all the fields in this message, in the index
        /// order for those fields.
        /// </summary>
        /// <returns>a list of all fields in this message</returns>
        IList<IFudgeField> GetAllFields();

        /// <summary>
        /// Returns a list of all field names currently in this message. Any fields which are referenced by ordinal only are ignored.
        /// </summary>
        /// <returns>a list of all field names in this message</returns>
        IList<string> GetAllFieldNames();

        /// <summary>
        /// Returns a field at a specific index into the message. This index is the physical position at which the field was added to the message, it is not
        /// the same as the ordinal index of a field. Returns null if the index is is too great and the message doesn't contain that many fields.
        /// </summary>
        /// <param name="index">physical index of the field</param>
        /// <returns>the field</returns>
        IFudgeField GetByIndex(int index);

        /// <summary>
        /// Returns a list of all fields with the given ordinal index. Returns the empty list if there are no matching fields.
        /// </summary>
        /// <param name="ordinal">ordinal index</param>
        /// <returns>a list of matching fields</returns>
        IList<IFudgeField> GetAllByOrdinal(int ordinal);

        /// <summary>
        /// Returns a field with the given ordinal index, or null if that index does not exist. In the case of multiple fields with the same ordinal index,
        /// the first (i.e. the lowest physical index) is returned.
        /// </summary>
        /// <param name="ordinal">ordinal index</param>
        /// <returns>the field, or null if none is found</returns>
        IFudgeField GetByOrdinal(int ordinal);

        /// <summary>
        /// Returns a list of all fields with the given field name. Returns the empty list if there are no matching fields.
        /// </summary>
        /// <param name="name">field name</param>
        /// <returns>a list of matching fields</returns>
        IList<IFudgeField> GetAllByName(string name);

        /// <summary>
        /// Returns a field with the given name, or null if that name does not exist. In the case of multiple fields with the same field name, the first (i.e.
        /// the lowest physical index) is returned.
        /// </summary>
        /// <param name="name">field name</param>
        /// <returns>the field, or null if none is found</returns>
        IFudgeField GetByName(string name);

        /// <summary>
        /// Returns the value of the field that would be returned by <c>GetByName</c>, or null if there is no matching field.
        /// </summary>
        /// <param name="name">field name</param>
        /// <returns>value of matching field, or null if none is found</returns>
        object GetValue(string name);
 
        /// <summary>
        /// Returns the typed value of the field that would be returned by <c>GetByName</c>, or null if there is no matching field.
        /// </summary>
        /// <typeparam name="T">underlying .NET type of the field</typeparam>
        /// <param name="name">field name</param>
        /// <returns>value of matching field, or null if none is found</returns>
        T GetValue<T>(string name);

        /// <summary>
        /// Returns the value of the field that would be returned by <c>GetByName</c>, or null if there is no matching field. The return type is converted to
        /// the requested type.
        /// </summary>
        /// <param name="name">field name</param>
        /// <param name="type">requested .NET type of the returned value</param>
        /// <returns>value of matching field, converted to the requested type, or null if none is found</returns>
        object GetValue(string name, Type type);

        /// <summary>
        /// Returns the value of the field that would be returned by <c>GetByOrdinal</c>, or null if there is no matching field.
        /// </summary>
        /// <param name="ordinal">ordinal index</param>
        /// <returns>field value, or null if none is found</returns>
        object GetValue(int ordinal);

        /// <summary>
        /// Returns the typed value of the field that would be returned by <c>GetByOrdinal</c>, or null if there is no matching field.
        /// </summary>
        /// <typeparam name="T">underlying .NET type of the field</typeparam>
        /// <param name="ordinal">ordinal index</param>
        /// <returns>value of matching field, or null if none is found</returns>
        T GetValue<T>(int ordinal);

        /// <summary>
        /// Returns the value of the field that would be returned by <c>GetByOrdinal</c>, or null if there is no matching field.
        /// The return type is converted to the requested type.
        /// </summary>
        /// <param name="ordinal">ordinal index</param>
        /// <param name="type">requested .NET type of the returned value</param>
        /// <returns>value of matching field, or null if none is found</returns>
        object GetValue(int ordinal, Type type);

        /// <summary>
        /// Returns the value of the first field (i.e. lowest physical index) with either a matching name or ordinal index.
        /// </summary>
        /// <param name="name">field name, or null to only search by ordinal index</param>
        /// <param name="ordinal">ordinal index, or null to only search by name</param>
        /// <returns>value of matching field, or null if none is found</returns>
        object GetValue(string name, int? ordinal);

        /// <summary>
        /// Returns the typed value of the first field (i.e. lowest physical index) with either a matching name or ordinal index.
        /// </summary>
        /// <typeparam name="T">underlying .NET type of the field</typeparam>
        /// <param name="name">field name, or null to only search by ordinal index</param>
        /// <param name="ordinal">ordinal index, or null to only search by name</param>
        /// <returns>value of matching field, or null if none is found</returns>
        T GetValue<T>(string name, int? ordinal);

        /// <summary>
        /// Returns the value of the first field (i.e. lowest physical index) with either a matching name or ordinal index. The return type
        /// is converted to the requested type.
        /// </summary>
        /// <param name="name">field name, or null to only search by ordinal index</param>
        /// <param name="ordinal">ordinal index, or null to only search by name</param>
        /// <param name="type">requested .NET type of the returned value</param>
        /// <returns>value of matching field, or null if none is found</returns>
        object GetValue(string name, int? ordinal, Type type);

        /// <summary>Returns the value of a field as a double, or null if the field does not exist.
        /// In the case of multiple fields with the same field name, the first (i.e. the lowest physical index) is returned.</summary>
        /// <param name="fieldName">field name</param>
        /// <returns>value, or <c>null</c> if field not found.</returns>
        /// <exception cref="InvalidCastException">Field type could not be converted to a <c>double</c></exception>
        /// <exception cref="OverflowException">Field value could not fit within a <c>double</c></exception>
        double? GetDouble(string fieldName);

        /// <summary>Returns the value of a field as a double, or null if the field does not exist.
        /// In the case of multiple fields with the same ordinal index, the first (i.e. the lowest physical index) is returned.</summary>
        /// <param name="ordinal">ordinal index</param>
        /// <returns>value, or <c>null</c> if field not found.</returns>
        /// <exception cref="InvalidCastException">Field type could not be converted to a <c>double</c></exception>
        /// <exception cref="OverflowException">Field value could not fit within a <c>double</c></exception>
        double? GetDouble(int ordinal);

        /// <summary>Returns the value of a field as a float, or null if the field does not exist.
        /// In the case of multiple fields with the same field name, the first (i.e. the lowest physical index) is returned.</summary>
        /// <param name="fieldName">field name</param>
        /// <returns>value, or <c>null</c> if field not found.</returns>
        /// <exception cref="InvalidCastException">Field type could not be converted to a <c>float</c></exception>
        /// <exception cref="OverflowException">Field value could not fit within a <c>float</c></exception>
        float? GetFloat(string fieldName);

        /// <summary>Returns the value of a field as a float, or null if the field does not exist.
        /// In the case of multiple fields with the same ordinal index, the first (i.e. the lowest physical index) is returned.</summary>
        /// <param name="ordinal">ordinal index</param>
        /// <returns>value, or <c>null</c> if field not found.</returns>
        /// <exception cref="InvalidCastException">Field type could not be converted to a <c>float</c></exception>
        /// <exception cref="OverflowException">Field value could not fit within a <c>float</c></exception>
        float? GetFloat(int ordinal);

        /// <summary>Returns the value of a field as a long, or null if the field does not exist.
        /// In the case of multiple fields with the same field name, the first (i.e. the lowest physical index) is returned.</summary>
        /// <param name="fieldName">field name</param>
        /// <returns>value, or <c>null</c> if field not found.</returns>
        /// <exception cref="InvalidCastException">Field type could not be converted to a <c>long</c></exception>
        /// <exception cref="OverflowException">Field value could not fit within a <c>long</c></exception>
        long? GetLong(string fieldName);

        /// <summary>Returns the value of a field as a long, or null if the field does not exist.
        /// In the case of multiple fields with the same ordinal index, the first (i.e. the lowest physical index) is returned.</summary>
        /// <param name="ordinal">ordinal index</param>
        /// <returns>value, or <c>null</c> if field not found.</returns>
        /// <exception cref="InvalidCastException">Field type could not be converted to a <c>long</c></exception>
        /// <exception cref="OverflowException">Field value could not fit within a <c>long</c></exception>
        long? GetLong(int ordinal);

        /// <summary>Returns the value of a field as a int, or null if the field does not exist.
        /// In the case of multiple fields with the same field name, the first (i.e. the lowest physical index) is returned.</summary>
        /// <param name="fieldName">field name</param>
        /// <returns>value, or <c>null</c> if field not found.</returns>
        /// <exception cref="InvalidCastException">Field type could not be converted to an <c>int</c></exception>
        /// <exception cref="OverflowException">Field value could not fit within an <c>int</c></exception>
        int? GetInt(string fieldName);

        /// <summary>Returns the value of a field as a int, or null if the field does not exist.
        /// In the case of multiple fields with the same ordinal index, the first (i.e. the lowest physical index) is returned.</summary>
        /// <param name="ordinal">ordinal index</param>
        /// <returns>value, or <c>null</c> if field not found.</returns>
        /// <exception cref="InvalidCastException">Field type could not be converted to an <c>int</c></exception>
        /// <exception cref="OverflowException">Field value could not fit within an <c>int</c></exception>
        int? GetInt(int ordinal);

        /// <summary>Returns the value of a field as a short, or null if the field does not exist.
        /// In the case of multiple fields with the same field name, the first (i.e. the lowest physical index) is returned.</summary>
        /// <param name="fieldName">field name</param>
        /// <returns>value, or <c>null</c> if field not found.</returns>
        /// <exception cref="InvalidCastException">Field type could not be converted to a <c>short</c></exception>
        /// <exception cref="OverflowException">Field value could not fit within a <c>short</c></exception>
        short? GetShort(string fieldName);

        /// <summary>Returns the value of a field as a short, or null if the field does not exist.
        /// In the case of multiple fields with the same ordinal index, the first (i.e. the lowest physical index) is returned.</summary>
        /// <param name="ordinal">ordinal index</param>
        /// <returns>value, or <c>null</c> if field not found.</returns>
        /// <exception cref="InvalidCastException">Field type could not be converted to a <c>short</c></exception>
        /// <exception cref="OverflowException">Field value could not fit within a <c>short</c></exception>
        short? GetShort(int ordinal);

        /// <summary>Returns the value of a field as a byte, or null if the field does not exist.
        /// In the case of multiple fields with the same field name, the first (i.e. the lowest physical index) is returned.</summary>
        /// <param name="fieldName">field name</param>
        /// <returns>value, or <c>null</c> if field not found.</returns>
        /// <exception cref="InvalidCastException">Field type could not be converted to a <c>byte</c></exception>
        /// <exception cref="OverflowException">Field value could not fit within a <c>byte</c></exception>
        sbyte? GetSByte(string fieldName);

        /// <summary>Returns the value of a field as a byte, or null if the field does not exist.
        /// In the case of multiple fields with the same ordinal index, the first (i.e. the lowest physical index) is returned.</summary>
        /// <param name="ordinal">ordinal index</param>
        /// <returns>value, or <c>null</c> if field not found.</returns>
        /// <exception cref="InvalidCastException">Field type could not be converted to a <c>byte</c></exception>
        /// <exception cref="OverflowException">Field value could not fit within a <c>byte</c></exception>
        sbyte? GetSByte(int ordinal);

        /// <summary>Returns the value of a field as a boolean, or null if the field does not exist.
        /// In the case of multiple fields with the same field name, the first (i.e. the lowest physical index) is returned.</summary>
        /// <param name="fieldName">field name</param>
        /// <returns>value, or <c>null</c> if field not found.</returns>
        /// <exception cref="InvalidCastException">Field type could not be converted to a <c>bool</c></exception>
        /// <exception cref="OverflowException">Field value could not fit within a <c>bool</c></exception>
        bool? GetBoolean(string fieldName);

        /// <summary>Returns the value of a field as a boolean, or null if the field does not exist.
        /// In the case of multiple fields with the same ordinal index, the first (i.e. the lowest physical index) is returned.</summary>
        /// <param name="ordinal">ordinal index</param>
        /// <returns>value, or <c>null</c> if field not found.</returns>
        /// <exception cref="InvalidCastException">Field type could not be converted to a <c>bool</c></exception>
        /// <exception cref="OverflowException">Field value could not fit within a <c>bool</c></exception>
        bool? GetBoolean(int ordinal);

        /// <summary>Returns the value of a field as a string, or null if the field does not exist.
        /// In the case of multiple fields with the same field name, the first (i.e. the lowest physical index) is returned.</summary>
        /// <param name="fieldName">field name</param>
        /// <returns>value, or <c>null</c> if field not found.</returns>
        /// <exception cref="InvalidCastException">Field type could not be converted to a <c>string</c></exception>
        /// <exception cref="OverflowException">Field value could not fit within a <c>string</c></exception>
        string GetString(string fieldName);

        /// <summary>Returns the value of a field as a string, or null if the field does not exist.
        /// In the case of multiple fields with the same ordinal index, the first (i.e. the lowest physical index) is returned.</summary>
        /// <param name="ordinal">ordinal index</param>
        /// <returns>value, or <c>null</c> if field not found.</returns>
        /// <exception cref="InvalidCastException">Field type could not be converted to a <c>string</c></exception>
        /// <exception cref="OverflowException">Field value could not fit within a <c>string</c></exception>
        string GetString(int ordinal);

        /// <summary>
        /// Returns a submessage with the given field name. In the case of multiple submessage fields with the same name,
        /// the first (i.e. lowest physical index) is returned. Fields with a matching name but are not submessages are ignored.
        /// </summary>
        /// <param name="fieldName">field name</param>
        /// <returns>value, or <c>null</c> if field not found.</returns>
        IFudgeFieldContainer GetMessage(string fieldName);

        /// <summary>
        /// Returns a submessage with the given ordinal index. In the case of multiple submessage fields with the same ordinal index,
        /// the first (i.e. lowest physical index) is returned. Fields with a matching ordinal index but are not submessages are ignored.
        /// </summary>
        /// <param name="ordinal">ordinal index</param>
        /// <returns>value, or <c>null</c> if field not found.</returns>
        IFudgeFieldContainer GetMessage(int ordinal);
    }
}
