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
using System.Diagnostics;

namespace Fudge.Tests.Unit.Encodings
{
    /// <summary>
    /// Handy class for debugging what your writer is getting asked, particularly if you use through a <see cref="FudgeStreamMultiwriter"/>.
    /// </summary>
    public class DebuggingWriter : IFudgeStreamWriter
    {
        #region IFudgeStreamWriter Members

        public void StartMessage()
        {
            Debug.WriteLine("Start message");
        }

        public void StartSubMessage(string name, int? ordinal)
        {
            Debug.WriteLine(string.Format("Start sub-message (\"{0}\", {1})", name, ordinal));
        }

        public void WriteField(string name, int? ordinal, FudgeFieldType type, object value)
        {
            Debug.WriteLine(string.Format("Field (\"{0}\", {1}, {2})", name, ordinal, type));
        }

        public void WriteFields(IEnumerable<IFudgeField> fields)
        {
            Debug.WriteLine(string.Format("Write fields"));
        }

        public void EndSubMessage()
        {
            Debug.WriteLine("End sub-message");
        }

        public void EndMessage()
        {
            Debug.WriteLine("End message");
        }

        #endregion
    }
}
