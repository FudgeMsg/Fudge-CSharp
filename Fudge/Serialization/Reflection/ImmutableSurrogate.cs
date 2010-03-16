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
using System.Runtime.Serialization;

namespace Fudge.Serialization.Reflection
{
    /// <summary>
    /// Surrogate for immutable objects - i.e. ones that have getters plus a rich constructor that matches the getters
    /// </summary>
    public class ImmutableSurrogate : IFudgeSerializationSurrogate
    {
        private readonly FudgeContext context;
        private readonly Type type;
        private readonly ConstructorInfo constructor;
        private readonly PropertyBasedSerializationSurrogate.PropertySerializerHelper helper;
        private readonly ConstructorParam[] constructorParams;

        /// <summary>
        /// Constructs a new <see cref="ImmutableSurrogate"/>.
        /// </summary>
        /// <param name="context"><see cref="FudgeContext"/> to use.</param>
        /// <param name="typeData"><see cref="TypeData"/> for the type for this surrogate.</param>
        public ImmutableSurrogate(FudgeContext context, TypeData typeData)
        {
            if (context == null)
                throw new ArgumentNullException("context");
            if (typeData == null)
                throw new ArgumentNullException("typeData");
            if (!CanHandle(typeData))
                throw new ArgumentOutOfRangeException("typeData", "ImmutableSurrogate cannot handle " + typeData.Type.FullName);

            this.context = context;
            this.type = typeData.Type;
            this.constructor = FindConstructor(typeData.Constructors, typeData.Properties, out constructorParams);
            Debug.Assert(constructor != null);  // Else how did it pass CanHandle?

            this.helper = new PropertyBasedSerializationSurrogate.PropertySerializerHelper(context, typeData);
        }

        #region IFudgeSerializationSurrogate Members

        /// <inheritdoc/>
        public void Serialize(object obj, IAppendingFudgeFieldContainer msg, IFudgeSerializer serializer)
        {
            helper.Serialize(obj, msg, serializer);
        }

        /// <inheritdoc/>
        public object Deserialize(IFudgeFieldContainer msg, IFudgeDeserializer deserializer)
        {
            // We create the new object without constructing it, so we can register it before trying
            // to deserialize the parameters.  This allows us to support cycles.
            object result = FormatterServices.GetUninitializedObject(type);
            deserializer.Register(msg, result);

            var args = new object[constructorParams.Length];
            foreach (var field in msg)
            {
                if (field.Name != null)
                {
                    string name = field.Name.ToLower();

                    for (int i = 0; i < constructorParams.Length; i++)
                    {
                        var constructorParam = constructorParams[i];
                        if (constructorParam.LowerName == name)
                        {
                            switch (constructorParam.Kind)
                            {
                                case TypeData.TypeKind.FudgePrimitive:
                                    args[i] = context.TypeHandler.ConvertType(field.Value, constructorParam.TypeData.Type); ;
                                    break;
                                default:
                                    args[i] = deserializer.FromField(field, constructorParam.TypeData.Type);
                                    break;
                            }
                            break;
                        }
                    }
                }
            }

            constructor.Invoke(result, args);
            return result;
        }

        #endregion

        internal static bool CanHandle(TypeData typeData)
        {
            // We're looking for no setters, but a constructor that matches the getters (by name and type)
            foreach (var propData in typeData.Properties)
            {
                if (propData.HasPublicSetter)
                    return false;               // Not for us
            }

            ConstructorParam[] constructorParams;
            return FindConstructor(typeData.Constructors, typeData.Properties, out constructorParams) != null;
        }

        private static ConstructorInfo FindConstructor(ConstructorInfo[] constructors, TypeData.PropertyData[] properties, out ConstructorParam[] constructorParams)
        {
            foreach (var constructor in constructors)
            {
                constructorParams = MatchConstructor(constructor, properties);
                if (constructorParams != null)
                    return constructor;
            }
            constructorParams = null;
            return null;
        }

        /// <summary>
        /// Checks if a constructor matches the set of properties, and initialised data about the constructor parameters at the same time.
        /// </summary>
        /// <param name="constructor">Constructor to test.</param>
        /// <param name="properties">Properties to match against.</param>
        /// <returns>Constructor parameter info, or <c>null</c> if no match.</returns>
        private static ConstructorParam[] MatchConstructor(ConstructorInfo constructor, TypeData.PropertyData[] properties)
        {
            ParameterInfo[] parameters = constructor.GetParameters();
            if (parameters.Length != properties.Length)
                return null;

            var result = new ConstructorParam[parameters.Length];

            foreach (var prop in properties)
            {
                string name = prop.Name.ToLower();      // We don't care about case

                // Not worth doing hashtable-type things, a simple scan is probably faster
                bool found = false;
                for (int i = 0; i < parameters.Length; i++)
                {
                    if (parameters[i].Name.ToLower() == name && parameters[i].ParameterType == prop.Type)
                    {
                        // Match
                        found = true;
                        result[i] = new ConstructorParam { LowerName = parameters[i].Name.ToLower(), Kind = prop.Kind, TypeData = prop.TypeData };
                        break;
                    }
                }

                if (!found)
                    return null;
            }

            return result;
        }

        private struct ConstructorParam
        {
            public string LowerName;
            public TypeData.TypeKind Kind;
            public TypeData TypeData;
        }
    }
}
