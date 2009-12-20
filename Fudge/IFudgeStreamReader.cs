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
using Fudge.Taxon;

namespace Fudge
{
    /// <summary>
    /// IFudgeStreamReader is implemented by classes wishing to present data streams (e.g. Fudge binary encoding, XML, JSON) as Fudge messages.
    /// </summary>
    /// <remarks>
    /// <para>
    /// If you simply wish to extract a single message from a stream into a <see cref="FudgeMsg"/> object, then use the
    /// <see cref="Fudge.Encodings.FudgeEncodingExtensions.ReadToMsg"/> extension method.
    /// </para>
    /// <para>
    /// The <see cref="Fudge.Encodings"/> namespace contains a variety of readers and writers for different file formats.
    /// </para>
    /// </remarks>
    public interface IFudgeStreamReader
    {
        /// <summary>
        /// Indicates whether there is remaining data in the stream.
        /// </summary>
        bool HasNext { get; }

        /// <summary>
        /// Moves to the next element within the stream.
        /// </summary>
        /// <returns>The type of element.</returns>
        FudgeStreamElement MoveNext();

        /// <summary>
        /// Gets the type of the current stream element.
        /// </summary>
        FudgeStreamElement CurrentElement { get; }

        /// <summary>
        /// When the current element is a field, gives the type of the field.
        /// </summary>
        FudgeFieldType FieldType { get; }

        /// <summary>
        /// When the current element is a field, gives the ordinal of the field, or <c>null</c> if none.
        /// </summary>
        int? FieldOrdinal { get; }

        /// <summary>
        /// When the current element is a field, gives the name of the field, or <c>null</c> if none.
        /// </summary>
        string FieldName { get; }

        /// <summary>
        /// When the current element is a field, gives the value of the field.
        /// </summary>
        object FieldValue { get; }
    }
}
