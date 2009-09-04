/**
 * Copyright (C) 2009 - 2009 by OpenGamma Inc.
 *
 * Please see distribution for license.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenGamma.Fudge.Types
{
    /// <summary>
    /// The only value of a field with the Indicator type.
    /// </summary>
    public sealed class IndicatorType
    {
        private IndicatorType() { }

        /// <summary>
        /// The only instance of this type.
        /// </summary>
        public static readonly IndicatorType Instance = new IndicatorType();
    }
}
