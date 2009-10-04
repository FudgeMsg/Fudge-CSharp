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
    public interface IMutableFudgeFieldContainer : IFudgeFieldContainer
    {
        void Add(IFudgeField field);

        void Add(object value, string name);

        void Add(object value, short? ordinal);

        void Add(object value, string name, short? ordinal);

        void Add(FudgeFieldType type, object value, string name, short? ordinal);
    }
}
