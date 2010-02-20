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
    /// Implement <see cref="IFudgeTypeMappingStrategy"/> to provide a strategy for mapping types
    /// to the names that identify them in the serialization stream, and the reverse mapping.
    /// </summary>
    /// <remarks>
    /// <para>
    /// To specify a type mapper other than the default, either set this in <see cref="FudgeSerializer.TypeMappingStrategy"/>
    /// or through <see cref="FudgeContext.SetProperty"/> using <see cref="FudgeSerializer.TypeMappingStrategyProperty"/>.
    /// </para>
    /// <para>See <see cref="JavaTypeMappingStrategy"/> for an example.</para>
    /// </remarks>
    public interface IFudgeTypeMappingStrategy
    {
        /// <summary>
        /// Maps a <see cref="Type"/> to a name.
        /// </summary>
        /// <param name="type">Type to map.</param>
        /// <returns>Name to use in serialization stream.</returns>
        string GetName(Type type);

        /// <summary>
        /// Maps a name in a serialization stream to a <see cref="Type"/>.
        /// </summary>
        /// <param name="name">Name to map.</param>
        /// <returns>Corresponding <see cref="Type"/> or <c>null</c> if not found.</returns>
        Type GetType(string name);
    }
}
