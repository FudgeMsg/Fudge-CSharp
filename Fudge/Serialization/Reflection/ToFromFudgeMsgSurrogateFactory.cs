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
using System.Linq;
using System.Reflection.Emit;
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
    public class ToFromFudgeMsgSurrogateFactory
    {
        /// <summary>
        /// The base class for al lthe emmited surrogates
        /// </summary>
        public abstract class EmittedFudgeSerializationSurrogateBase : IFudgeSerializationSurrogate
        {
            public abstract void Serialize(object obj, IAppendingFudgeFieldContainer msg, IFudgeSerializer serializer);

            public object Deserialize(IFudgeFieldContainer msg, IFudgeDeserializer deserializer)
            {
                var ret = DeserializeImpl(msg,deserializer);
                deserializer.Register(msg, ret);
                return ret;
            }

            /// <summary>
            /// The stub method which will be proxied to the type
            /// </summary>
            /// <param name="msg"></param>
            /// <param name="deserializer"></param>
            /// <returns></returns>
            public abstract object DeserializeImpl(IFudgeFieldContainer msg, IFudgeDeserializer deserializer);
        }

        /// <summary>
        /// Constructs a new <see cref="ToFromFudgeMsgSurrogateFactory"/>.
        /// </summary>
        /// <param name="typeData"><see cref="TypeData"/> for the type for this surrogate.</param>
        public static IFudgeSerializationSurrogate Create(TypeData typeData)
        {
            if (typeData == null)
                throw new ArgumentNullException("typeData");
            if (!CanHandle(typeData))
                throw new ArgumentOutOfRangeException("typeData", "ToFromFudgeMsgSurrogateFactory cannot handle " + typeData.Type.FullName);

            var toFudgeMsgMethod = GetToMsg(typeData);
            var fromFudgeMsgMethod = GetFromMsg(typeData);

            var aNameString = string.Format("{0}.{1}", typeof(ToFromFudgeMsgSurrogateFactory).FullName, Guid.NewGuid());
            var aName = new AssemblyName(aNameString);
            var ab = AppDomain.CurrentDomain.DefineDynamicAssembly(aName, AssemblyBuilderAccess.Run);
            var mb = ab.DefineDynamicModule(aName.Name);
            var tb = mb.DefineType(string.Format("{0}.EmittedSerializer", aNameString), TypeAttributes.Public);

            tb.SetParent(typeof(EmittedFudgeSerializationSurrogateBase));
            
            ConstructorBuilder ctor1 = tb.DefineConstructor(MethodAttributes.Public,CallingConventions.Standard,new Type[] {});

            ILGenerator ctor1IL = ctor1.GetILGenerator();
            ctor1IL.Emit(OpCodes.Ldarg_0);
            ctor1IL.Emit(OpCodes.Call, typeof(object).GetConstructor(Type.EmptyTypes));
            ctor1IL.Emit(OpCodes.Ret);


            
            MethodBuilder meth = tb.DefineMethod("DeserializeImpl", MethodAttributes.Public | MethodAttributes.Virtual,typeof(object), 
                new[] { typeof(IFudgeFieldContainer), typeof(IFudgeDeserializer) });

            ILGenerator methIL = meth.GetILGenerator();

            methIL.Emit(OpCodes.Ldarg_1);
            methIL.Emit(OpCodes.Ldarg_2);
            methIL.Emit(OpCodes.Call, fromFudgeMsgMethod);

            methIL.Emit(OpCodes.Ret);
            MethodInfo deserializeInterfaceMethod = typeof(EmittedFudgeSerializationSurrogateBase).GetMethod("DeserializeImpl");
            tb.DefineMethodOverride(meth, deserializeInterfaceMethod);



            MethodBuilder serializeMeth = tb.DefineMethod(
                "Serialize",
                MethodAttributes.Public | MethodAttributes.Virtual,
                null,
                new[] { typeof(object), typeof(IAppendingFudgeFieldContainer), typeof(IFudgeSerializer) });

            ILGenerator serializeMethIL = serializeMeth.GetILGenerator();

            serializeMethIL.Emit(OpCodes.Ldarg_1);
            serializeMethIL.Emit(OpCodes.Ldarg_2);
            serializeMethIL.Emit(OpCodes.Ldarg_3);
            serializeMethIL.Emit(OpCodes.Callvirt, toFudgeMsgMethod);
            serializeMethIL.Emit(OpCodes.Ret);

            tb.DefineMethodOverride(serializeMeth, typeof(EmittedFudgeSerializationSurrogateBase).GetMethod("Serialize"));

            
            Type t = tb.CreateType();
            return (IFudgeSerializationSurrogate) Activator.CreateInstance(t);
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
    }
}
