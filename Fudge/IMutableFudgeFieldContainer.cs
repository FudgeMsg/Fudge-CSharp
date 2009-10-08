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

        void Add(string name, object value);

        void Add(int? ordinal, object value);

        void Add(string name, int? ordinal, object value);

        void Add(string name, int? ordinal, FudgeFieldType type, object value);
    }
}
