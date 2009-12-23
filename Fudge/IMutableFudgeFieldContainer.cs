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
    /// Defines a set of fields that can be modified as well as inspected.
    /// </summary>
    public interface IMutableFudgeFieldContainer : IFudgeFieldContainer
    {
        /// <summary>
        /// Adds a field to this message.
        /// </summary>
        /// <param name="field">field to add</param>
        void Add(IFudgeField field);

        /// <summary>
        /// Adds a field to this message.
        /// </summary>
        /// <param name="name">name of the field</param>
        /// <param name="value">value of the field</param>
        void Add(string name, object value);

        /// <summary>
        /// Adds a field to this message.
        /// </summary>
        /// <param name="ordinal">ordinal index of the field</param>
        /// <param name="value">value of the field</param>
        void Add(int? ordinal, object value);

        /// <summary>
        /// Adds a field to this message.
        /// </summary>
        /// <param name="name">name of the field, or null if no name is specified</param>
        /// <param name="ordinal">ordinal index of the field, or null if no index is specified</param>
        /// <param name="value">value of the field</param>
        void Add(string name, int? ordinal, object value);

        /// <summary>
        /// Adds a field to this message with an explicit type.
        /// </summary>
        /// <param name="name">name of the field, or null if no name is specified</param>
        /// <param name="ordinal">ordinal index of the field, or null if no index is specified</param>
        /// <param name="type">explicit type of the field</param>
        /// <param name="value">value of the field</param>
        void Add(string name, int? ordinal, FudgeFieldType type, object value);
    }
}
