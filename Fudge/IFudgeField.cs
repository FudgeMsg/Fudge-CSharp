/**
 * Copyright (C) 2009 - 2009 by OpenGamma Inc.
 *
 * Please see distribution for license.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenGamma.Fudge
{
    /// <summary>
    /// A read-only representation of a field which is contained in a fudge
    /// message, or a stream of fudge encoded data.
    /// </summary>
    public interface IFudgeField
    {
        FudgeFieldType Type { get; }
        object Value { get; }
        short? Ordinal { get; }
        string Name { get; }
    }
}
