/**
 * Copyright (C) 2009 - 2009 by OpenGamma Inc.
 *
 * Please see distribution for license.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenGamma.Fudge.Taxon
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
