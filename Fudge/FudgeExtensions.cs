﻿/* <!--
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
 * -->
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fudge
{
    public static class FudgeExtensions
    {
        public static void AddIfNotNull(this FudgeMsg msg, string name, object value)
        {
            if (value != null)
            {
                msg.Add(name, value);
            }
        }

        public static void AddIfNotNull(this FudgeMsg msg, int ordinal, object value)
        {
            if (value != null)
            {
                msg.Add(ordinal, value);
            }
        }

        #region IFudgeField extensions

        public static string GetString(this IFudgeField field)
        {
            IConvertible value = field.Value as IConvertible;
            if (value == null)
                return null;
            else
                return value.ToString(null);
        }

        #endregion
    }
}