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

namespace Fudge.Serialization.Reflection
{
    /// <summary>
    /// Describes reflection-based information about a type
    /// </summary>
    public class TypeData
    {
        private readonly TypeData subType;
        private readonly TypeData subType2;
        private readonly FudgeFieldType fieldType;

        /// <summary>
        /// Constructs a new instance
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cache"></param>
        /// <param name="type"></param>
        /// <param name="fieldNameConvention"></param>
        public TypeData(FudgeContext context, TypeDataCache cache, Type type, FudgeFieldNameConvention fieldNameConvention)
        {
            Type = type;
            DefaultConstructor = type.GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null);
            Constructors = type.GetConstructors();
            CustomAttributes = type.GetCustomAttributes(true);

            cache.RegisterTypeData(this);

            fieldNameConvention = OverrideFieldNameConvention(type, fieldNameConvention);

            var kind = CalcKind(context, cache, type, fieldNameConvention, out subType, out subType2, out fieldType);
            var inlineAttrib = GetCustomAttribute<FudgeInlineAttribute>();
            if (inlineAttrib != null)
            {
                kind = inlineAttrib.Inline ? TypeKind.Inline : TypeKind.Reference;
            }
            Kind = kind;

            ScanProperties(context, cache, fieldNameConvention);

            PublicMethods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance);
            StaticPublicMethods = type.GetMethods(BindingFlags.Public | BindingFlags.Static);
        }

        /// <summary>Gets the <see cref="Type"/> this <see cref="TypeData"/> describes.</summary>
        public Type Type { get; private set; }
        /// <summary>Gets the default constructor, if present.</summary>
        public ConstructorInfo DefaultConstructor { get; private set; }
        /// <summary>Gets all the constructors for the type.</summary>
        public ConstructorInfo[] Constructors { get; private set; }
        /// <summary>Gets <see cref="PropertyData"/> describing the properties of the type.</summary>
        public PropertyData[] Properties { get; private set; }
        /// <summary>Gets any custom attributes for the type.</summary>
        public object[] CustomAttributes { get; private set; }
        /// <summary>Gets the way that this type should be serialized by default.</summary>
        public TypeKind Kind { get; private set; }
        /// <summary>For lists and arrays, this is the <see cref="TypeData"/> for the type of elements in the list.  For dictionaries it's the key type</summary>
        public TypeData SubTypeData { get { return subType; } }
        /// <summary>For dictionaries, this is the <see cref="TypeData"/> for the type of the values</summary>
        public TypeData SubType2Data { get { return subType2; } }
        /// <summary>For lists and arrays, this is the type of elements in the list.  For dictionaries it's the key type</summary>
        public Type SubType { get { return subType == null ? null : subType.Type; } }
        /// <summary>For dictionaries, this is the type of the values</summary>
        public Type SubType2 { get { return subType2 == null ? null : subType2.Type; } }
        /// <summary>Gets the <see cref="FudgeFieldType"/> to store this type in.</summary>
        public FudgeFieldType FieldType { get { return fieldType; } }                   // If the Kind is primitive
        /// <summary>Gets all public instance methods of the type.</summary>
        public MethodInfo[] PublicMethods { get; private set; }
        /// <summary>Gets all public static methods of the type</summary>
        public MethodInfo[] StaticPublicMethods { get; private set; }

        /// <summary>
        /// Gets the first custom attribute of a given type
        /// </summary>
        /// <typeparam name="T">Type of attribute to select</typeparam>
        /// <returns>Matching attribute or <c>null</c>.</returns>
        public T GetCustomAttribute<T>() where T : Attribute
        {
            foreach (object attrib in CustomAttributes)
            {
                if (attrib.GetType() == typeof(T))
                    return (T)attrib;
            }
            return null;
        }

        /// <summary>
        /// Enunerates the different ways a type may be serialized.
        /// </summary>
        public enum TypeKind
        {
            /// <summary>Serialize as a fudge field.</summary>
            FudgePrimitive,
            /// <summary>Serialize as a sub-message.</summary>
            Inline,
            /// <summary>Serialize as a sub-message but allow references.</summary>
            Reference
        }

        private static FudgeFieldNameConvention OverrideFieldNameConvention(Type type, FudgeFieldNameConvention fieldNameConvention)
        {
            var attribs = type.GetCustomAttributes(typeof(FudgeFieldNameConventionAttribute), true);
            if (attribs.Length > 0)
                fieldNameConvention = ((FudgeFieldNameConventionAttribute)attribs[0]).Convention;
            return fieldNameConvention;
        }

        private static TypeKind CalcKind(FudgeContext context, TypeDataCache typeCache, Type type, FudgeFieldNameConvention fieldNameConvention, out TypeData subType, out TypeData subType2, out FudgeFieldType fieldType)
        {
            // REVIEW 2010-02-14 t0rx -- There seems to be some duplication here with the FudgeSurrogateSelector, should look at joining up
            subType = null;
            subType2 = null;
            fieldType = context.TypeDictionary.GetByCSharpType(type);
            if (fieldType != null)
            {
                // Just a simple field
                return TypeKind.FudgePrimitive;
            }

            // Check for arrays
            if (type.IsArray)
            {
                subType = typeCache.GetTypeData(type.GetElementType(), fieldNameConvention);
                return TypeKind.Inline;
            }

            // Check for dictionaries
            Type keyType, valueType;
            if (DictionarySurrogate.IsDictionary(type, out keyType, out valueType))
            {
                subType = typeCache.GetTypeData(keyType, fieldNameConvention);
                subType2 = typeCache.GetTypeData(valueType, fieldNameConvention);
                return TypeKind.Inline;
            }

            // Check for lists
            Type elementType;
            if (ListSurrogate.IsList(type, out elementType))
            {
                subType = typeCache.GetTypeData(elementType, fieldNameConvention);
                return TypeKind.Inline;
            }

            return TypeKind.Reference;
        }

        private void ScanProperties(FudgeContext context, TypeDataCache cache, FudgeFieldNameConvention fieldNameConvention)
        {
            var props = Type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var list = from prop in props
                       where prop.GetCustomAttributes(typeof(FudgeTransientAttribute), true).Length == 0
                       select new PropertyData(cache, fieldNameConvention, prop);
            Properties = list.ToArray();
        }

        /// <summary>
        /// Holds reflection-based data about a property of a type.
        /// </summary>
        public sealed class PropertyData
        {
            private readonly PropertyInfo info;
            private readonly string name;
            private readonly bool hasPublicSetter;
            private string serializedName;
            private readonly TypeData typeData;
            private readonly TypeKind kind;

            /// <summary>
            /// Contructs a new instance
            /// </summary>
            /// <param name="typeCache"></param>
            /// <param name="fieldNameConvention"></param>
            /// <param name="info"></param>
            public PropertyData(TypeDataCache typeCache, FudgeFieldNameConvention fieldNameConvention, PropertyInfo info)
            {
                this.info = info;
                this.name = info.Name;
                this.hasPublicSetter = (info.GetSetMethod() != null);   // Can't just use CanWrite as it may be non-public
                this.typeData = typeCache.GetTypeData(info.PropertyType, fieldNameConvention);

                var fieldNameAttrib = GetCustomAttribute<FudgeFieldNameAttribute>(info);
                if (fieldNameAttrib != null)
                {
                    // Attribute takes priority over convention
                    this.serializedName = fieldNameAttrib.Name;
                }
                else
                {
                    switch (fieldNameConvention)
                    {
                        case FudgeFieldNameConvention.Identity:
                            this.serializedName = info.Name;
                            break;
                        case FudgeFieldNameConvention.AllLowerCase:
                            this.serializedName = info.Name.ToLower();
                            break;
                        case FudgeFieldNameConvention.AllUpperCase:
                            this.serializedName = info.Name.ToUpper();
                            break;
                        case FudgeFieldNameConvention.CamelCase:
                            this.serializedName = info.Name.Substring(0, 1).ToLower() + info.Name.Substring(1);
                            break;
                        case FudgeFieldNameConvention.PascalCase:
                            this.serializedName = info.Name.Substring(0, 1).ToUpper() + info.Name.Substring(1);
                            break;
                        default:
                            throw new FudgeRuntimeException("Unknown FudgeFieldNameConvention: " + fieldNameConvention.ToString());
                    }
                }

                var inlineAttrib = GetCustomAttribute<FudgeInlineAttribute>(info);
                if (inlineAttrib == null)
                {
                    this.kind = typeData.Kind;
                }
                else
                {
                    this.kind = inlineAttrib.Inline ? TypeKind.Inline : TypeKind.Reference;
                }
            }

            /// <summary>Gets the <see cref="PropertyInfo"/> for the property.</summary>
            public PropertyInfo Info { get { return info; } }
            /// <summary>Gets the name of the property.</summary>
            public string Name { get { return name; } }
            /// <summary>Gets the <see cref="TypeData"/> describing the type of the property.</summary>
            public TypeData TypeData { get { return typeData; } }
            /// <summary>Gets the <see cref="Type"/> of the property.</summary>
            public Type Type { get { return typeData.Type; } }
            /// <summary>Gets the way the the property should be serialized.</summary>
            public TypeKind Kind { get { return kind; } }
            /// <summary>Gets whether the property has a public setter.</summary>
            public bool HasPublicSetter { get { return hasPublicSetter; } }
            /// <summary>Gets the name that should be used to serialize the property after any conventions, etc. have been applied.</summary>
            public string SerializedName
            {
                get { return serializedName; }
                set { serializedName = value; }
            }

            private T GetCustomAttribute<T>(PropertyInfo info) where T : Attribute
            {
                var attribs = info.GetCustomAttributes(typeof(T), true);
                if (attribs.Length > 0)
                    return (T)attribs[0];

                return null;
            }
        }
    }
}
