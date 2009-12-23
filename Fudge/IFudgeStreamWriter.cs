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

namespace Fudge
{
    /// <summary>
    /// <c>IFudgeStreamWriter</c> is implemented by classes wishing to write data streams (e.g. Fudge binary encoding, XML, JSON) of Fudge messages.
    /// </summary>
    /// <remarks>
    /// The <see cref="Fudge.Encodings"/> namespace contains a variety of readers and writers for different file formats.
    /// </remarks>
    public interface IFudgeStreamWriter
    {
        /// <summary>
        /// Starts a new top-level message.
        /// </summary>
        void StartMessage();

        /// <summary>
        /// Starts a sub-message within the current message.
        /// </summary>
        /// <param name="name">Name of the field, or <c>null</c> if none.</param>
        /// <param name="ordinal">Ordinal of the field, or <c>null</c> if none.</param>
        void StartSubMessage(string name, int? ordinal);

        /// <summary>
        /// Writes a simple field to the data stream.
        /// </summary>
        /// <param name="name">Name of the field, or <c>null</c> if none.</param>
        /// <param name="ordinal">Ordinal of the field, or <c>null</c> if none.</param>
        /// <param name="type">Type of the field, as a <see cref="FudgeFieldType"/>.</param>
        /// <param name="value">Value of the field.</param>
        void WriteField(string name, int? ordinal, FudgeFieldType type, object value);     // TODO t0rx 2009-11-12 -- Do we need overloads of this, and auto-derivation of type? 

        /// <summary>
        /// Writes multiple fields to the data stream.
        /// </summary>
        /// <param name="fields">Fields to write.</param>
        void WriteFields(IEnumerable<IFudgeField> fields);

        /// <summary>
        /// Tells the writer that the current sub-message is finished.
        /// </summary>
        void EndSubMessage();

        /// <summary>
        /// Tells the writer that we have finished with the whole top-level message.
        /// </summary>
        void EndMessage();
    }
}
