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
    internal class TypeData
    {
        public TypeData(FudgeContext context, Type type)
        {
            Type = type;
            DefaultConstructor = type.GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null);
            Constructors = type.GetConstructors();
            var props = type.GetProperties();                  // Gets all the properties with a public accessor
            Properties = props.Select(prop => new PropertyData(context, prop)).ToArray();
            CustomAttributes = type.GetCustomAttributes(true);
        }

        public Type Type { get; private set; }
        public ConstructorInfo DefaultConstructor { get; private set; }
        public ConstructorInfo[] Constructors { get; private set; }
        public PropertyData[] Properties { get; private set; }
        public object[] CustomAttributes { get; private set; }

        public enum PropertyType
        {
            FudgePrimitive,
            List,
            Object
        }

        public class PropertyData
        {
            private readonly PropertyInfo info;
            private readonly string name;
            private readonly PropertyType type;
            private readonly Type valueType;
            private readonly bool hasPublicSetter;
            private string serializedName;
            private readonly Action<object, object> adderFunc;
            private readonly Action<object, IFudgeSerializer> serializer;

            public PropertyData(FudgeContext context, PropertyInfo info)
            {
                this.info = info;
                this.name = info.Name;
                this.serializedName = info.Name;
                this.type = CalcType(context, info, out this.valueType, out this.serializer, out this.adderFunc);
                this.hasPublicSetter = (info.GetSetMethod() != null);   // Can't just use CanWrite as it may be non-public
            }

            public PropertyInfo Info { get { return info; } }
            public string Name { get { return name; } }
            public string SerializedName
            {
                get { return serializedName; }
                set { serializedName = value; }
            }
            public PropertyType Type { get { return type; } }
            public Type ValueType { get { return valueType; } }         // For lists this is the type of the items
            public bool HasPublicSetter { get { return hasPublicSetter; } }
            public Action<object, IFudgeSerializer> Serializer { get { return serializer; } }
            public Action<object, object> Adder { get { return adderFunc; } }       // Used for lists

            private PropertyType CalcType(FudgeContext context, PropertyInfo prop, out Type valueType, out Action<object, IFudgeSerializer> serializer, out Action<object, object> adder)
            {
                valueType = prop.PropertyType;
                adder = null;
                serializer = null;
                if (context.TypeDictionary.GetByCSharpType(valueType) != null)
                {
                    // Just a simple field
                    adder = this.PrimitiveAdd;
                    serializer = this.PrimitiveSerialize;
                    return PropertyType.FudgePrimitive;
                }

                // Check for lists
                foreach (var interfaceType in valueType.GetInterfaces())
                {
                    if (interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(IList<>))
                    {
                        // It's a list
                        valueType = interfaceType.GetGenericArguments()[0];
                        adder = CreateMethodDelegate<Action<object, object>>("ListAdd", valueType);
                        serializer = CreateMethodDelegate<Action<object, IFudgeSerializer>>("ListSerialize", valueType);
                        return PropertyType.List;
                    }
                }

                return PropertyType.Object;
            }

            private T CreateMethodDelegate<T>(string name, Type valueType) where T : class
            {
                var method = this.GetType().GetMethod(name, BindingFlags.NonPublic | BindingFlags.Instance).MakeGenericMethod(new Type[] { valueType });
                return Delegate.CreateDelegate(typeof(T), this, method) as T;
            }

            private void PrimitiveSerialize(object obj, IFudgeSerializer serializer)
            {
                object val = Info.GetValue(obj, null);
                serializer.Write(this.SerializedName, val);
            }

            private void PrimitiveAdd(object obj, object val)
            {
                Info.SetValue(obj, val, null);
            }

            private void ListSerialize<T>(object obj, IFudgeSerializer serializer)
            {
                var list = (IList<T>)Info.GetValue(obj, null);
                foreach (T item in list)
                {
                    serializer.Write(SerializedName, item);
                }
            }

            private void ListAdd<T>(object obj, object val)
            {
                var list = (IList<T>)Info.GetValue(obj, null);
                list.Add((T)val);
            }
        }
    }
}
