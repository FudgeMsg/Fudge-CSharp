﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fudge.Types
{
    /// <summary>
    /// <c>FudgeDateTimePrecision</c> expresses the resolution of a <see cref="FudgeDateTime"/> or <see cref="FudgeTime"/> object.
    /// </summary>
    /// <remarks>
    /// The values are as defined in the specification at http://www.fudgemsg.org/display/FDG/DateTime+encoding
    /// </remarks>
    public enum FudgeDateTimePrecision : byte
    {
        /// <summary>The object is accurate to the nearest millennium.</summary>
        Millennium,
        /// <summary>The object is accurate to the nearest century.</summary>
        Century,
        /// <summary>The object is accurate to the nearest year.</summary>
        Year,
        /// <summary>The object is accurate to the nearest month.</summary>
        Month,
        /// <summary>The object is accurate to the nearest day.</summary>
        Day,
        /// <summary>The object is accurate to the nearest hour.</summary>
        Hour,
        /// <summary>The object is accurate to the nearest minute.</summary>
        Minute,
        /// <summary>The object is accurate to the nearest second.</summary>
        Second,
        /// <summary>The object is accurate to the nearest millisecond.</summary>
        Millisecond,
        /// <summary>The object is accurate to the nearest microsecond.</summary>
        Microsecond,
        /// <summary>The object is accurate to the nearest nanosecond.</summary>
        Nanosecond
    }
}
