/**
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

namespace Fudge.Taxon
{
    /// <summary>
    /// A Fudge Taxonomy is a mapping from ordinals to names for
    /// fields in a Fudge Encoded data format
    /// </summary>
    public interface IFudgeTaxonomy
    {
        /// <summary>
        /// Obtain the field name appropriate for a field with the
        /// specified ordinal within this taxonomy.
        /// </summary>
        /// <param name="ordinal">The ordinal to locate a field name.</param>
        /// <returns>he field name, or <c>null</c> if no name available for
        /// a field with the specified ordinal in this taxonomy.</returns>
        string GetFieldName(short ordinal);

        /// <summary>
        /// Obtain the field ordinal appropriate for a field with the
        /// specified name within this taxonomy.
        /// </summary>
        /// <param name="fieldName">The name to locate an ordinal for.</param>
        /// <returns>The field ordinal, or <c>null</c> if no ordinal available
        /// for a field with the specified name in this taxonomy.</returns>
        short? GetFieldOrdinal(string fieldName);
    }
}
