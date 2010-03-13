﻿/* <!--
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
    /// Used to handle classes which are bean-style (i.e. have matching getters and setters and a default constructor).
    /// </summary>
    /// <remarks>
    /// For lists (i.e. properties that implement <see cref="IList{T}"/>, no setter is required but the object itself must construct the list
    /// so that values can be added to it.
    /// </remarks>
    public class PropertyBasedSerializationSurrogate : IFudgeSerializationSurrogate
    {
        private readonly FudgeContext context;
        private readonly Type type;
        private readonly ConstructorInfo constructor;
        private readonly PropertySerializerHelper helper;

        /// <summary>
        /// Constructs a new <see cref="PropertyBasedSerializationSurrogate"/>.
        /// </summary>
        /// <param name="context"><see cref="FudgeContext"/> to use.</param>
        /// <param name="typeData"><see cref="TypeData"/> for the type for this surrogate.</param>
        public PropertyBasedSerializationSurrogate(FudgeContext context, TypeData typeData)
        {
            if (context == null)
                throw new ArgumentNullException("context");
            if (typeData == null)
                throw new ArgumentNullException("typeData");
            if (!CanHandle(typeData))
                throw new ArgumentOutOfRangeException("typeData", "PropertyBasedSerializationSurrogate cannot handle " + typeData.Type.FullName);

            this.context = context;
            this.type = typeData.Type;

            Debug.Assert(typeData.DefaultConstructor != null);      // Should have been caught in CanHandle()
            this.constructor = typeData.DefaultConstructor;

            this.helper = new PropertySerializerHelper(context, typeData);
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
            if (typeData.DefaultConstructor == null)
                return false;
            foreach (var prop in typeData.Properties)
            {
                switch (prop.Kind)
                {
                    case TypeData.TypeKind.FudgePrimitive:
                    case TypeData.TypeKind.Inline:
                    case TypeData.TypeKind.Reference:
                        // OK
                        break;
                    default:
                        // Unknown
                        return false;
                }

                if (!prop.HasPublicSetter && !ListSurrogate.IsList(prop.Type))      // Special case for lists, which we can just append to if no setter present
                {
                    // Not bean-style
                    return false;
                }
            }
            return true;
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
            object newObj = constructor.Invoke(null);

            // Register now in case any cycles in the object graph
            deserializer.Register(msg, newObj);

            foreach (var field in msg)
            {
                helper.DeserializeField(deserializer, field, newObj);
            }

            return newObj;
        }

        #endregion

        private class MorePropertyData
        {
            public TypeData.PropertyData PropertyData { get; set; }
            public Action<MorePropertyData, object, IAppendingFudgeFieldContainer, IFudgeSerializer> Serializer { get; set; }
            public Action<MorePropertyData, object, IFudgeField, IFudgeDeserializer> Adder { get; set; }
        }

        internal class PropertySerializerHelper
        {
            private readonly FudgeContext context;
            private readonly Dictionary<string, MorePropertyData> propMap = new Dictionary<string, MorePropertyData>();

            public PropertySerializerHelper(FudgeContext context, TypeData typeData)
            {
                this.context = context;

                // Pull out all the properties
                foreach (var prop in typeData.Properties)
                {
                    var propData = new MorePropertyData { PropertyData = prop };
                    switch (prop.Kind)
                    {
                        case TypeData.TypeKind.FudgePrimitive:
                            propData.Adder = this.PrimitiveAdd;
                            propData.Serializer = this.PrimitiveSerialize;
                            break;
                        case TypeData.TypeKind.Inline:
                            if (propData.PropertyData.HasPublicSetter)
                                propData.Adder = this.ObjectAdd;
                            else
                            {
                                // Must be a list
                                propData.Adder = CreateMethodDelegate<Action<MorePropertyData, object, IFudgeField, IFudgeDeserializer>>("ListAppend", prop.TypeData.SubType);
                            }
                            propData.Serializer = this.InlineSerialize;
                            break;
                        case TypeData.TypeKind.Reference:
                            propData.Adder = this.ObjectAdd;
                            propData.Serializer = this.ReferenceSerialize;
                            break;
                        default:
                            throw new FudgeRuntimeException("Invalid property type for " + typeData.Type.FullName + "." + prop.Name);
                    }
                    propMap.Add(prop.SerializedName, propData);
                }
            }

            public void Serialize(object obj, IAppendingFudgeFieldContainer msg, IFudgeSerializer serializer)
            {
                foreach (var entry in propMap)
                {
                    string name = entry.Key;
                    MorePropertyData prop = entry.Value;
                    object val = prop.PropertyData.Info.GetValue(obj, null);

                    if (val != null)
                    {
                        prop.Serializer(prop, val, msg, serializer);
                    }
                }
            }

            public bool DeserializeField(IFudgeDeserializer deserializer, IFudgeField field, object obj)
            {
                if (field.Name == null)
                    return false;           // Can't process without a name (yet)

                MorePropertyData prop;
                if (propMap.TryGetValue(field.Name, out prop))
                {
                    prop.Adder(prop, obj, field, deserializer);
                    return true;
                }
                return false;
            }

            private T CreateMethodDelegate<T>(string name, Type valueType) where T : class
            {
                var method = this.GetType().GetMethod(name, BindingFlags.NonPublic | BindingFlags.Instance).MakeGenericMethod(new Type[] { valueType });
                return Delegate.CreateDelegate(typeof(T), this, method) as T;
            }

            private void PrimitiveSerialize(MorePropertyData prop, object val, IAppendingFudgeFieldContainer msg, IFudgeSerializer serializer)
            {
                msg.Add(prop.PropertyData.SerializedName, val);
            }

            private void PrimitiveAdd(MorePropertyData prop, object obj, IFudgeField field, IFudgeDeserializer deserializer)
            {
                object val = context.TypeHandler.ConvertType(field.Value, prop.PropertyData.Type);
                prop.PropertyData.Info.SetValue(obj, val, null);
            }

            private void InlineSerialize(MorePropertyData prop, object val, IAppendingFudgeFieldContainer msg, IFudgeSerializer serializer)
            {
                serializer.WriteInline(msg, prop.PropertyData.SerializedName, val);
            }

            private void ObjectAdd(MorePropertyData prop, object obj, IFudgeField field, IFudgeDeserializer deserializer)
            {
                // Handles both reference and inline
                object subObject = deserializer.FromField(field, prop.PropertyData.Type);
                prop.PropertyData.Info.SetValue(obj, subObject, null);
            }

            private void ListAppend<T>(MorePropertyData prop, object obj, IFudgeField field, IFudgeDeserializer deserializer) where T : class
            {
                IList<T> newList = deserializer.FromField<IList<T>>(field);
                IList<T> currentList = (IList<T>)prop.PropertyData.Info.GetValue(obj, null);
                foreach (T item in newList)
                    currentList.Add(item);
            }

            private void ReferenceSerialize(MorePropertyData prop, object val, IAppendingFudgeFieldContainer msg, IFudgeSerializer serializer)
            {
                // Serializer will in-line or not as appropriate
                msg.Add(prop.PropertyData.SerializedName, null, prop.PropertyData.TypeData.FieldType, val);
            }
        }
    }
}
