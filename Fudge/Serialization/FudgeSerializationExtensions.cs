/*<!--
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

namespace Fudge.Serialization
{
    public static class FudgeSerializationExtensions
    {
        public static void Write(this IFudgeSerializer serializer, string name, object value)
        {
            serializer.Write(name, null, value);
        }

        public static void WriteSubMsg(this IFudgeSerializer serializer, string name, object value)
        {
            serializer.WriteSubMsg(name, null, value);
        }

        public static void WriteRef(this IFudgeSerializer serializer, string name, object value)
        {
            serializer.WriteRef(name, null, value);
        }

        public static void WriteIfNotNull<T>(this IFudgeSerializer serializer, string name, T value) where T : class
        {
            if (value != null)
                serializer.Write(name, value);
        }

        public static void WriteAll<T>(this IFudgeSerializer serializer, string name, IEnumerable<T> values)
        {
            WriteAll(serializer, name, null, values);
        }

        public static void WriteAll<T>(this IFudgeSerializer serializer, string name, int? ordinal, IEnumerable<T> values)
        {
            foreach (T value in values)
            {
                serializer.Write(name, ordinal, value);
            }
        }

        public static void WriteAllSubMsgs<T>(this IFudgeSerializer serializer, string name, IEnumerable<T> objects)
        {
            serializer.WriteAllSubMsgs(name, null, objects);
        }

        public static void WriteAllSubMsgs<T>(this IFudgeSerializer serializer, string name, int? ordinal, IEnumerable<T> objects)
        {
            foreach (T obj in objects)
            {
                serializer.WriteSubMsg(name, ordinal, obj);
            }
        }

        public static void WriteAllRefs<T>(this IFudgeSerializer serializer, string name, IEnumerable<T> objects)
        {
            serializer.WriteAllRefs(name, null, objects);
        }

        public static void WriteAllRefs<T>(this IFudgeSerializer serializer, string name, int? ordinal, IEnumerable<T> objects)
        {
            var result = new List<int>();
            foreach (T obj in objects)
            {
                serializer.WriteRef(name, ordinal, obj);
            }
        }
        /*
                public static IEnumerable<T> AllFromFields<T>(this IFudgeDeserializer context, IEnumerable<IFudgeField> fields) where T : class
                {
                    var result = new List<T>();
                    foreach (var field in fields)
                    {
                        result.Add(context.FromField<T>(field));
                    }
                    return result;
                }
                */
    }
}
