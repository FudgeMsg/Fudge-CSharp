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

namespace Fudge.Encodings
{
    /// <summary>
    /// FudgeMultiWriter allows you to write to multiple writers simultaneously (e.g. from a <see cref="FudgeStreamPipe"/>.
    /// </summary>
    public class FudgeStreamMultiwriter : IFudgeStreamWriter
    {
        private readonly IFudgeStreamWriter[] writers;

        /// <summary>
        /// Constructs a new <see cref="FudgeStreamMultiwriter"/>.
        /// </summary>
        /// <param name="writers">The set of <see cref="IFudgeStreamWriter"/>s that this <see cref="FudgeStreamMultiwriter"/> will use.</param>
        public FudgeStreamMultiwriter(params IFudgeStreamWriter[] writers)
        {
            this.writers = writers;
        }

        #region IFudgeStreamWriter Members

        /// <inheritdoc/>
        public void StartMessage()
        {
            foreach (var writer in writers)
                writer.StartMessage();
        }

        /// <inheritdoc/>
        public void StartSubMessage(string name, int? ordinal)
        {
            foreach (var writer in writers)
                writer.StartSubMessage(name, ordinal);
        }

        /// <inheritdoc/>
        public void WriteField(string name, int? ordinal, FudgeFieldType type, object value)
        {
            foreach (var writer in writers)
                writer.WriteField(name, ordinal, type, value);
        }

        /// <inheritdoc/>
        public void WriteFields(IEnumerable<IFudgeField> fields)
        {
            foreach (var writer in writers)
                writer.WriteFields(fields);
        }

        /// <inheritdoc/>
        public void EndSubMessage()
        {
            foreach (var writer in writers)
                writer.EndSubMessage();
        }

        /// <inheritdoc/>
        public void EndMessage()
        {
            foreach (var writer in writers)
                writer.EndMessage();
        }

        #endregion
    }
}
