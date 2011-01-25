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
using System.Reflection.Emit;

namespace Fudge.Serialization.Reflection
{
    internal static class ReflectionUtil
    {
        /// <summary>
        /// Creates a delegate for a static method after specializing its generic types.
        /// </summary>
        /// <typeparam name="T">Type of delegate to create.</typeparam>
        /// <param name="type">Type that method is on</param>
        /// <param name="name">Name of method to find.</param>
        /// <param name="genericTypes">Array of types to apply to the generic parameters of the method.</param>
        /// <returns>Delegate that calls the specialized method.</returns>
        public static T CreateStaticMethodDelegate<T>(Type type, string name, Type[] genericTypes) where T : class
        {
            var unspecializedMethod = type.GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static);
            var method = unspecializedMethod.MakeGenericMethod(genericTypes);
            return Delegate.CreateDelegate(typeof(T), null, method) as T;
        }

        /// <summary>
        /// Creates a delegate for a method after specializing its generic types.
        /// </summary>
        /// <typeparam name="T">Type of delegate to create.</typeparam>
        /// <param name="obj">Instance that methods will be called on</param>
        /// <param name="name">Name of method to find.</param>
        /// <param name="genericTypes">Array of types to apply to the generic parameters of the method.</param>
        /// <returns>Delegate that calls the specialized method.</returns>
        public static T CreateInstanceMethodDelegate<T>(object obj, string name, Type[] genericTypes) where T : class
        {
            var method = obj.GetType().GetMethod(name, BindingFlags.NonPublic | BindingFlags.Instance).MakeGenericMethod(genericTypes);
            return Delegate.CreateDelegate(typeof(T), obj, method) as T;
        }

        public static Func<object, object> CreateGetterDelegate(PropertyInfo info)
        {
            // This may seem like a lot of nasty complexity, but a simple release-mode test shows that using a delegate like
            // this is over 300 times faster than calling PropertyInfo.GetValue().  Doing this via generating IL is actually
            // simpler than trying to do it by specialising generic methods (e.g. using ReflectionUtil.CreateStaticMethodDelegate).
            // See the TypeDataTest.PerformanceComparison for a comparison.
            var getMethod = info.GetGetMethod();
            if (getMethod == null)
                return null;

            // Generate a dynamic method that simply does "return getMethod(obj)", boxing if necessary
            DynamicMethod dynamicMethod = CreateDynamicMethod(info.DeclaringType, typeof(object), new Type[] { typeof(object) });
            ILGenerator ilGenerator = dynamicMethod.GetILGenerator();
            ilGenerator.Emit(OpCodes.Ldarg_0);              // Push the object onto the stack
            ilGenerator.Emit(OpCodes.Call, getMethod);      // Call the getter
            if (info.PropertyType.IsValueType)
            {
                // Need to box the result
                ilGenerator.Emit(OpCodes.Box, info.PropertyType);
            }
            ilGenerator.Emit(OpCodes.Ret);

            var getter = (Func<object, object>)dynamicMethod.CreateDelegate(typeof(Func<object, object>));
            return getter;
        }

        public static Func<object, object> CreateGetterDelegate(FieldInfo info)
        {
            // Generate a dynamic method that simply does "return obj.field", boxing if necessary
            DynamicMethod dynamicMethod = CreateDynamicMethod(info.DeclaringType, typeof(object), new Type[] { typeof(object) });
            ILGenerator ilGenerator = dynamicMethod.GetILGenerator();
            ilGenerator.Emit(OpCodes.Ldarg_0);              // Push the object onto the stack
            ilGenerator.Emit(OpCodes.Ldfld, info);          // Get the field value
            if (info.FieldType.IsValueType)
            {
                // Need to box the result
                ilGenerator.Emit(OpCodes.Box, info.FieldType);
            }
            ilGenerator.Emit(OpCodes.Ret);

            var getter = (Func<object, object>)dynamicMethod.CreateDelegate(typeof(Func<object, object>));
            return getter;
        }

        public static Action<object, object> CreateSetterDelegate(PropertyInfo info)
        {
            var setMethod = info.GetSetMethod();
            if (setMethod == null)
                return null;

            // Generate a dynamic method that simply does "setMethod(obj, value)", unboxing if necessary
            DynamicMethod dynamicMethod = CreateDynamicMethod(info.DeclaringType, typeof(void), new Type[] { typeof(object), typeof(object) });
            ILGenerator ilGenerator = dynamicMethod.GetILGenerator();
            ilGenerator.Emit(OpCodes.Ldarg_0);              // Push the object onto the stack
            ilGenerator.Emit(OpCodes.Ldarg_1);              // Push the value onto the stack
            if (info.PropertyType.IsValueType)
            {
                // Need to unbox the value
                ilGenerator.Emit(OpCodes.Unbox_Any, info.PropertyType);
            }
            ilGenerator.Emit(OpCodes.Call, setMethod);      // Call the setter
            ilGenerator.Emit(OpCodes.Ret);

            var setter = (Action<object, object>)dynamicMethod.CreateDelegate(typeof(Action<object, object>));
            return setter;
        }

        public static Action<object, object> CreateSetterDelegate(FieldInfo info)
        {
            // Generate a dynamic method that simply does "obj.field = value", unboxing if necessary
            DynamicMethod dynamicMethod = CreateDynamicMethod(info.DeclaringType, typeof(void), new Type[] { typeof(object), typeof(object) });
            ILGenerator ilGenerator = dynamicMethod.GetILGenerator();
            ilGenerator.Emit(OpCodes.Ldarg_0);              // Push the object onto the stack
            ilGenerator.Emit(OpCodes.Ldarg_1);              // Push the value onto the stack
            if (info.FieldType.IsValueType)
            {
                // Need to unbox the value
                ilGenerator.Emit(OpCodes.Unbox_Any, info.FieldType);
            }
            ilGenerator.Emit(OpCodes.Stfld, info);          // Set the field value
            ilGenerator.Emit(OpCodes.Ret);

            var setter = (Action<object, object>)dynamicMethod.CreateDelegate(typeof(Action<object, object>));
            return setter;
        }

        private static DynamicMethod CreateDynamicMethod(Type owningType, Type returnType, Type[] argTypes)
        {
            DynamicMethod dynamicMethod;
            if (owningType.IsInterface)
            {
                // An interface can't own any code, but then it can't have any private properties either so that's fine
                dynamicMethod = new DynamicMethod("", returnType, argTypes);
            }
            else
            {
                // Make the owner the class so we can see private properties
                dynamicMethod = new DynamicMethod("", returnType, argTypes, owningType, true);
            }
            return dynamicMethod;
        }
    }
}
