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

namespace Fudge.Serialization
{
    /// <summary>
    /// <c>IFudgeSerializer</c> is the interface through which objects (or their surrogates)
    /// write data during serialization.
    /// </summary>
    public interface IFudgeSerializer
    {
        // TODO 2010-01-23 t0rx -- Do we need fast versions for primitive types?

        /// <summary>
        /// Gets the <see cref="FudgeContext"/> for this deserializer.
        /// </summary>
        FudgeContext Context { get; }

        /// <summary>
        /// Writes a child object as a serialized sub-message with a given name and/or ordinal.
        /// </summary>
        /// <param name="fieldName">Name of field, may be <c>null</c>.</param>
        /// <param name="ordinal">Ordinal of field, may be <c>null</c>.</param>
        /// <param name="obj">Child object to write.</param>
        /// <remarks>If <c>obj</c> is <c>null</c> then the sub-message will be omitted.</remarks>
        void WriteInline(IMutableFudgeFieldContainer msg, string fieldName, int? ordinal, object obj);
    }
}
