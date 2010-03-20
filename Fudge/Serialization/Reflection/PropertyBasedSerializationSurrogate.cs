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
        private readonly PropertySerializerMixin helper;

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

            this.helper = new PropertySerializerMixin(context, typeData, typeData.Properties, new DotNetSerializableSurrogate.BeforeAfterMethodMixin(context, typeData));
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

            helper.Deserialize(msg, deserializer, newObj);

            return newObj;
        }

        #endregion

        private class MorePropertyData
        {
            public TypeData.PropertyData PropertyData { get; set; }
            public Action<MorePropertyData, object, IAppendingFudgeFieldContainer, IFudgeSerializer> Serializer { get; set; }
            public Action<MorePropertyData, object, IFudgeField, IFudgeDeserializer> Adder { get; set; }
        }

        internal sealed class PropertySerializerMixin
        {
            private readonly FudgeContext context;
            private readonly TypeData typeData;
            private readonly Dictionary<string, MorePropertyData> propMap = new Dictionary<string, MorePropertyData>();
            private readonly DotNetSerializableSurrogate.BeforeAfterMethodMixin beforeAfterMethodHelper;

            public PropertySerializerMixin(FudgeContext context, TypeData typeData, IEnumerable<TypeData.PropertyData> properties, DotNetSerializableSurrogate.BeforeAfterMethodMixin beforeAfterMethodHelper)
            {
                this.context = context;
                this.typeData = typeData;
                this.beforeAfterMethodHelper = beforeAfterMethodHelper;

                ExtractProperties(properties);
            }

            private void ExtractProperties(IEnumerable<TypeData.PropertyData> properties)
            {
                foreach (var prop in properties)
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
                                propData.Adder = ReflectionUtil.CreateInstanceMethodDelegate<Action<MorePropertyData, object, IFudgeField, IFudgeDeserializer>>(this, "ListAppend", new Type[] { prop.TypeData.SubType });
                            }
                            propData.Serializer = this.InlineSerialize;
                            break;
                        case TypeData.TypeKind.Reference:
                            propData.Adder = this.ObjectAdd;
                            propData.Serializer = this.ReferenceSerialize;
                            break;
                    }
                    propMap.Add(prop.SerializedName, propData);
                }
            }

            public void Serialize(object obj, IAppendingFudgeFieldContainer msg, IFudgeSerializer serializer)
            {
                beforeAfterMethodHelper.CallBeforeSerialize(obj);
                foreach (var entry in propMap)
                {
                    string name = entry.Key;
                    MorePropertyData prop = entry.Value;
                    object val = prop.PropertyData.Getter(obj);

                    if (val != null)
                    {
                        prop.Serializer(prop, val, msg, serializer);
                    }
                }
                beforeAfterMethodHelper.CallAfterSerialize(obj);
            }

            /// <summary>
            /// Creates the object without calling a constructor, registers it, and deserializes the message into it
            /// </summary>
            /// <param name="msg"></param>
            /// <param name="deserializer"></param>
            /// <returns>Deserialized object</returns>
            public object CreateAndDeserialize(IFudgeFieldContainer msg, IFudgeDeserializer deserializer)
            {
                // Create without construction
                object newObj = FormatterServices.GetUninitializedObject(typeData.Type);

                // Register now in case any cycles in the object graph
                deserializer.Register(msg, newObj);

                // Deserialize the message
                Deserialize(msg, deserializer, newObj);

                // And we're done
                return newObj;
            }

            public void Deserialize(IFudgeFieldContainer msg, IFudgeDeserializer deserializer, object obj)
            {
                beforeAfterMethodHelper.CallBeforeDeserialize(obj);
                foreach (var field in msg)
                {
                    DeserializeField(field, deserializer, obj);
                }
                beforeAfterMethodHelper.CallAfterDeserialize(obj);
            }

            public bool DeserializeField(IFudgeField field, IFudgeDeserializer deserializer, object obj)
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

            private void PrimitiveSerialize(MorePropertyData prop, object val, IAppendingFudgeFieldContainer msg, IFudgeSerializer serializer)
            {
                msg.Add(prop.PropertyData.SerializedName, val);
            }

            private void PrimitiveAdd(MorePropertyData prop, object obj, IFudgeField field, IFudgeDeserializer deserializer)
            {
                object val = context.TypeHandler.ConvertType(field.Value, prop.PropertyData.Type);
                prop.PropertyData.Setter(obj, val);
            }

            private void InlineSerialize(MorePropertyData prop, object val, IAppendingFudgeFieldContainer msg, IFudgeSerializer serializer)
            {
                serializer.WriteInline(msg, prop.PropertyData.SerializedName, val);
            }

            private void ObjectAdd(MorePropertyData prop, object obj, IFudgeField field, IFudgeDeserializer deserializer)
            {
                // Handles both reference and inline
                object subObject = deserializer.FromField(field, prop.PropertyData.Type);
                prop.PropertyData.Setter(obj, subObject);
            }

            private void ListAppend<T>(MorePropertyData prop, object obj, IFudgeField field, IFudgeDeserializer deserializer) where T : class
            {
                IList<T> newList = deserializer.FromField<IList<T>>(field);
                IList<T> currentList = (IList<T>)prop.PropertyData.Getter(obj);
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
