/**
 * Copyright (C) 2009 - 2009 by OpenGamma Inc.
 *
 * Please see distribution for license.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenGamma.Fudge.Taxon;

namespace OpenGamma.Fudge
{
    /// <summary>
    /// The primary interface through which <see cref="FudgeMsgField"/> and <see cref="FudgeMsg"/>
    /// can contain a {@link SizeCache}.
    /// </summary>
    public interface ISizeComputable
    {
        int ComputeSize(IFudgeTaxonomy taxonomy);
    }
}
