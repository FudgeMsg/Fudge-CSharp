/* <!--
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
    /// <summary>
    /// Used by <see cref="IFudgeStreamReader"/>s to indicate the type of element at the current position
    /// in a stream.
    /// </summary>
    public enum FudgeStreamElement
    {
        /// <summary>State before message has started, or at end of stream.</summary>
        NoElement,
        /// <summary>Issued when a new outermost message is started.</summary>
        MessageStart,
        /// <summary>Issued when an outermost message is completed.</summary>
        MessageEnd,
        /// <summary>Issued when a simple (non-hierarchical) field is encountered.</summary>
        SimpleField,
        /// <summary>Issued when a sub-Message field is encountered.</summary>
        SubmessageFieldStart,
        /// <summary>Issued when the end of a sub-Message field is reached.</summary>
        SubmessageFieldEnd
    }
}
