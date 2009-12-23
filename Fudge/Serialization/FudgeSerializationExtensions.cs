/*<!--
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

namespace Fudge.Serialization
{
    public static class FudgeSerializationExtensions
    {
        public static IEnumerable<FudgeMsg> AllAsSubMsgs<T>(this IFudgeSerializationContext context, IEnumerable<T> objects)
        {
            var result = new List<FudgeMsg>();
            foreach (T obj in objects)
            {
                result.Add(context.AsSubMsg(obj));
            }
            return result;
        }

        public static IEnumerable<int> AllAsRefs<T>(this IFudgeSerializationContext context, IEnumerable<T> objects)
        {
            var result = new List<int>();
            foreach (T obj in objects)
            {
                result.Add(context.AsRef(obj));
            }
            return result;
        }

        public static IEnumerable<T> AllFromFields<T>(this IFudgeDeserializationContext context, IEnumerable<IFudgeField> fields) where T : class
        {
            var result = new List<T>();
            foreach (var field in fields)
            {
                result.Add(context.FromField<T>(field));
            }
            return result;
        }
    }
}
