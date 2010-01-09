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

namespace Fudge.Types
{
    /// <summary>
    /// <c>FudgeDateTime</c> represents a point in time from 9999BC to 9999AD, with up to nanosecond precision and optional timezone information.
    /// </summary>
    /// <seealso cref="FudgeDate"/>
    /// <seealso cref="FudgeTime"/>
    public class FudgeDateTime
    {
        /// <summary>
        /// <c>Precision</c> expresses the resolution of a <see cref="FudgeDateTime"/> or <see cref="FudgeTime"/> object.
        /// </summary>
        public enum Precision : byte
        {
            /// <summary>The object is accurate to the nearest nanosecond.</summary>
            Nanosecond = 0,
            /// <summary>The object is accurate to the nearest microsecond.</summary>
            Microsecond,
            /// <summary>The object is accurate to the nearest millisecond.</summary>
            Millisecond,
            /// <summary>The object is accurate to the nearest second.</summary>
            Second,
            /// <summary>The object is accurate to the nearest minute.</summary>
            Minute,
            /// <summary>The object is accurate to the nearest hour.</summary>
            Hour,
            /// <summary>The object is accurate to the nearest day.</summary>
            Day,
            /// <summary>The object is accurate to the nearest month.</summary>
            Month,
            /// <summary>The object is accurate to the nearest year.</summary>
            Year,
            /// <summary>The object is accurate to the nearest century.</summary>
            Century
        }
    }
}
