/*
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
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fudge.Encodings
{
    public static class FudgeEncodingExtensions
    {
        /// <summary>
        /// Convenience method for reading a <see cref="FudgeMsg"/> from a <see cref="IFudgeStreamReader"/>.
        /// </summary>
        /// <param name="reader">Reader providing the data for the message.</param>
        /// <returns>New message containing data from the reader.</returns>
        public static FudgeMsg ReadToMsg(this IFudgeStreamReader reader)
        {
            var writer = new FudgeMsgStreamWriter();
            var pipe = new FudgeStreamPipe(reader, writer);
            pipe.ProcessOne();

            return writer.Messages[0];
        }
    }
}
