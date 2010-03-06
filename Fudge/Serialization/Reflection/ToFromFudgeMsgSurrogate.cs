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
using System.Reflection;
using System.Diagnostics;

namespace Fudge.Serialization.Reflection
{
    /// <summary>
    /// A surrogate that works with classes providing <c>ToFudgeMsg</c> and static <c>FromFudgeMsg</c> methods.
    /// </summary>
    /// <remarks>
    /// The full signatures of the methods are <c>public void ToFudgeMsg(IAppendingFudgeFieldContainer msg, IFudgeSerializer serializer)</c> and
    /// <c>public static &lt;YourType&gt; FromFudgeMsg(IFudgeFieldContainer msg, IFudgeDeserializer deserializer)</c>.
    /// </remarks>
    public class ToFromFudgeMsgSurrogate : IFudgeSerializationSurrogate
    {
        private readonly FudgeContext context;
        private readonly MethodInfo toFudgeMsgMethod;
        private readonly MethodInfo fromFudgeMsgMethod;

        /// <summary>
        /// Constructs a new <see cref="ToFromFudgeMsgSurrogate"/>.
        /// </summary>
        /// <param name="context"><see cref="FudgeContext"/> to use.</param>
        /// <param name="typeData"><see cref="TypeData"/> for the type for this surrogate.</param>
        public ToFromFudgeMsgSurrogate(FudgeContext context, TypeData typeData)
        {
            if (context == null)
                throw new ArgumentNullException("context");
            if (typeData == null)
                throw new ArgumentNullException("typeData");
            if (!CanHandle(typeData))
                throw new ArgumentOutOfRangeException("typeData", "ToFromFudgeMsgSurrogate cannot handle " + typeData.Type.FullName);

            this.context = context;
            this.toFudgeMsgMethod = GetToMsg(typeData);
            this.fromFudgeMsgMethod = GetFromMsg(typeData);

            Debug.Assert(toFudgeMsgMethod != null);
            Debug.Assert(fromFudgeMsgMethod != null);
        }

        /// <summary>
        /// Determines whether this kind of surrogate can handle a given type
        /// </summary>
        /// <param name="cache"><see cref="TypeDataCache"/> for type data.</param>
        /// <param name="fieldNameConvention">Convention to use for renaming fields.</param>
        /// <param name="type">Type to test.</param>
        /// <returns>True if this kind of surrogate can handle the type.</returns>
        public static bool CanHandle(TypeDataCache cache, FudgeFieldNameConvention fieldNameConvention, Type type)
        {
            return CanHandle(cache.GetTypeData(type, fieldNameConvention));
        }

        internal static bool CanHandle(TypeData typeData)
        {
            return GetToMsg(typeData) != null && GetFromMsg(typeData) != null;
        }

        private static MethodInfo GetToMsg(TypeData typeData)
        {
            return typeData.PublicMethods.FirstOrDefault(m => m.Name == "ToFudgeMsg"
                                                           && m.ReturnType == typeof(void)
                                                           && ParamMatch(m, new Type[] { typeof(IAppendingFudgeFieldContainer), typeof(IFudgeSerializer) }));
        }

        private static MethodInfo GetFromMsg(TypeData typeData)
        {
            return typeData.StaticPublicMethods.FirstOrDefault(m => m.Name == "FromFudgeMsg"
                                                                 && m.ReturnType == typeData.Type
                                                                 && ParamMatch(m, new Type[] { typeof(IFudgeFieldContainer), typeof(IFudgeDeserializer) }));
        }

        private static bool ParamMatch(MethodInfo method, Type[] types)
        {
            var parameters = method.GetParameters();
            if (parameters.Length != types.Length)
                return false;

            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].ParameterType != types[i])
                    return false;
            }

            return true;
        }

        #region IFudgeSerializationSurrogate Members

        /// <inheritdoc/>
        public void Serialize(object obj, IAppendingFudgeFieldContainer msg, IFudgeSerializer serializer)
        {
            toFudgeMsgMethod.Invoke(obj, new object[] { msg, serializer });
        }

        /// <inheritdoc/>
        public object Deserialize(IFudgeFieldContainer msg, IFudgeDeserializer deserializer)
        {
            var result = fromFudgeMsgMethod.Invoke(null, new object[] { msg, deserializer });
            deserializer.Register(msg, result);
            return result;
        }

        #endregion
    }
}
