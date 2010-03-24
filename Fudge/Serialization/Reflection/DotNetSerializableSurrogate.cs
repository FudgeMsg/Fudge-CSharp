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
using System.Runtime.Serialization;
using System.Reflection;
using System.Globalization;
using Fudge.Types;

namespace Fudge.Serialization.Reflection
{
    /// <summary>
    /// Surrogate for classes implementing <see cref="ISerializable"/> from .net serialization.
    /// </summary>
    /// <remarks>
    /// NOTE that when deserializing data that has not been serialized through <see cref="ISerializable"/> (e.g.
    /// recieved from another platform, fields which are <c>null</c> may have been omitted.  Using the normal
    /// methods of <see cref="SerializationInfo"/> such as <see cref="SerializationInfo.GetString"/> will throw
    /// an exception in this situation as the field is missing.  The only way around this is to use
    /// <see cref="SerializationInfo.GetEnumerator"/> to process the data instead.
    /// </remarks>
    public class DotNetSerializableSurrogate : IFudgeSerializationSurrogate
    {
        private readonly FudgeContext context;
        private readonly Type type;
        private readonly ConstructorInfo constructor;
        private readonly SerializationMixin helper;

        /// <summary>
        /// Constructs a new <see cref="DotNetSerializableSurrogate"/>.
        /// </summary>
        /// <param name="context"><see cref="FudgeContext"/> to use.</param>
        /// <param name="typeData"><see cref="TypeData"/> for the type for this surrogate.</param>
        public DotNetSerializableSurrogate(FudgeContext context, TypeData typeData)
        {
            if (context == null)
                throw new ArgumentNullException("context");
            if (typeData == null)
                throw new ArgumentNullException("typeData");
            if (!CanHandle(typeData))
                throw new ArgumentOutOfRangeException("typeData", "DotNetSerializableSurrogate cannot handle " + typeData.Type.FullName);

            this.context = context;
            this.type = typeData.Type;
            this.constructor = FindConstructor(typeData);
            helper = new SerializationMixin(context, typeData.Type, new BeforeAfterMethodMixin(context, typeData));
        }

        /// <summary>
        /// Detects whether a given type can be serialized with this class.
        /// </summary>
        /// <param
        /// name="typeData">Type to test.</param>
        /// <returns><c>true</c> if this class can handle the type.</returns>
        public static bool CanHandle(TypeData typeData)
        {
            return typeof(ISerializable).IsAssignableFrom(typeData.Type) && FindConstructor(typeData) != null;
        }

        private static ConstructorInfo FindConstructor(TypeData typeData)
        {
            var constructor = typeData.Type.GetConstructor(BindingFlags.CreateInstance | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, new Type[] { typeof(SerializationInfo), typeof(StreamingContext) }, null);
            return constructor;
        }

        #region IFudgeSerializationSurrogate Members

        /// <inheritdoc/>
        public void Serialize(object obj, IAppendingFudgeFieldContainer msg, IFudgeSerializer serializer)
        {
            helper.Serialize(msg, obj, (o, si, sc) => {((ISerializable)o).GetObjectData(si, sc);});
        }

        /// <inheritdoc/>
        public object Deserialize(IFudgeFieldContainer msg, IFudgeDeserializer deserializer)
        {
            return helper.Deserialize(msg, deserializer, (obj, si, sc) =>
                {
                    var args = new object[] { si, sc };
                    constructor.Invoke(obj, args);
                });

        }

        #endregion

        internal sealed class BeforeAfterMethodMixin
        {
            private readonly StreamingContext streamingContext;
            private readonly Action<object, StreamingContext> beforeSerialize;
            private readonly Action<object, StreamingContext> afterSerialize;
            private readonly Action<object, StreamingContext> beforeDeserialize;
            private readonly Action<object, StreamingContext> afterDeserialize;

            public BeforeAfterMethodMixin(FudgeContext context, TypeData typeData)
            {
                this.streamingContext = new StreamingContext(StreamingContextStates.Persistence, context);

                var untypedDelegateCreator = ReflectionUtil.CreateStaticMethodDelegate<Func<Type, MethodInfo[], Action<object, StreamingContext>>>(GetType(), "CreateUntypedDelegate", new Type[] { typeData.Type });

                beforeSerialize = untypedDelegateCreator(typeof(OnSerializingAttribute), typeData.AllInstanceMethods);
                afterSerialize = untypedDelegateCreator(typeof(OnSerializedAttribute), typeData.AllInstanceMethods);
                beforeDeserialize = untypedDelegateCreator(typeof(OnDeserializingAttribute), typeData.AllInstanceMethods);
                afterDeserialize = untypedDelegateCreator(typeof(OnDeserializedAttribute), typeData.AllInstanceMethods);
            }

            public void CallBeforeSerialize(object obj)
            {
                beforeSerialize(obj, streamingContext);
            }

            public void CallAfterSerialize(object obj)
            {
                afterSerialize(obj, streamingContext);
            }

            public void CallBeforeDeserialize(object obj)
            {
                beforeDeserialize(obj, streamingContext);
            }

            public void CallAfterDeserialize(object obj)
            {
                afterDeserialize(obj, streamingContext);
            }

            private static Action<object, StreamingContext> CreateUntypedDelegate<T>(Type attribType, MethodInfo[] methods)
            {
                var method = GetFirstMethodWithAttribute(attribType, methods);
                if (method == null)
                    return (o, sc) => { };

                var methodDelegate = (Action<T, StreamingContext>)Delegate.CreateDelegate(typeof(Action<T, StreamingContext>), null, method);

                return (obj, sc) => { methodDelegate((T)obj, sc); };

            }

            private static MethodInfo GetFirstMethodWithAttribute(Type attribType, MethodInfo[] methods)
            {
                foreach (var method in methods)
                {
                    if (method.GetCustomAttributes(attribType, true).Length > 0)
                        return method;
                }
                return null;
            }
        }

        internal sealed class SerializationMixin
        {
            private readonly FormatterConverter formatterConverter = new FormatterConverter();
            private readonly StreamingContext streamingContext;
            private readonly Type type;
            private readonly BeforeAfterMethodMixin beforeAfterMethodHelper;

            public SerializationMixin(FudgeContext context, Type type, BeforeAfterMethodMixin beforeAfterMethodHelper)
            {
                this.streamingContext = new StreamingContext(StreamingContextStates.Persistence, context);
                this.type = type;
                this.beforeAfterMethodHelper = beforeAfterMethodHelper;
            }

            public void Serialize(IAppendingFudgeFieldContainer msg, object obj, Action<object, SerializationInfo, StreamingContext> serializeMethod)
            {
                var si = new SerializationInfo(type, formatterConverter);

                beforeAfterMethodHelper.CallBeforeSerialize(obj);
                serializeMethod(obj, si, streamingContext);
                beforeAfterMethodHelper.CallAfterSerialize(obj);

                // Pull the data out of the SerializationInfo and add to the message
                var e = si.GetEnumerator();
                while (e.MoveNext())
                {
                    string name = e.Name;
                    object val = e.Value;

                    if (val != null)
                    {
                        msg.Add(name, val);
                    }
                    else
                    {
                        // .net binary serialization still outputs the member with a null, so we have to do
                        // the same (using Indicator), otherwise deserialization blows up.
                        msg.Add(name, IndicatorType.Instance);
                    }
                }
            }

            public object Deserialize(IFudgeFieldContainer msg, IFudgeDeserializer deserializer, Action<object, SerializationInfo, StreamingContext> deserializeMethod)
            {
                // Create without construction and register before we call the constructor in case there are any cycles
                object result = FormatterServices.GetUninitializedObject(type);
                deserializer.Register(msg, result);

                var converter = new DeserializingFormatterConverter(deserializer);
                var si = new SerializationInfo(this.type, converter);
                PopulateSerializationInfo(si, msg);

                beforeAfterMethodHelper.CallBeforeDeserialize(result);
                deserializeMethod(result, si, streamingContext);
                beforeAfterMethodHelper.CallAfterDeserialize(result);

                return result;
            }

            public void PopulateSerializationInfo(SerializationInfo si, IFudgeFieldContainer msg)
            {
                foreach (var field in msg)
                {
                    if (field.Name != null)
                    {
                        if (field.Type == IndicatorFieldType.Instance)
                        {
                            // This is actually a null
                            si.AddValue(field.Name, null);
                        }
                        else
                        {
                            si.AddValue(field.Name, field.Value);
                        }
                    }
                }
            }

            private class DeserializingFormatterConverter : IFormatterConverter
            {
                private readonly IFudgeDeserializer deserializer;

                public DeserializingFormatterConverter(IFudgeDeserializer deserializer)
                {
                    this.deserializer = deserializer;
                }

                #region IFormatterConverter Members

                public object Convert(object value, TypeCode typeCode)
                {
                    return System.Convert.ChangeType(value, typeCode, CultureInfo.InvariantCulture);
                }

                public object Convert(object value, Type type)
                {
                    var fieldType = deserializer.Context.TypeHandler.DetermineTypeFromValue(value);
                    var field = new TemporaryField(fieldType, value);
                    object result = deserializer.FromField(field, type);
                    return result;
                }

                public bool ToBoolean(object value)
                {
                    return System.Convert.ToBoolean(value, CultureInfo.InvariantCulture);
                }

                public byte ToByte(object value)
                {
                    return System.Convert.ToByte(value, CultureInfo.InvariantCulture);
                }

                public char ToChar(object value)
                {
                    return System.Convert.ToChar(value, CultureInfo.InvariantCulture);
                }

                public DateTime ToDateTime(object value)
                {
                    return System.Convert.ToDateTime(value, CultureInfo.InvariantCulture);
                }

                public decimal ToDecimal(object value)
                {
                    return System.Convert.ToDecimal(value, CultureInfo.InvariantCulture);
                }

                public double ToDouble(object value)
                {
                    return System.Convert.ToDouble(value, CultureInfo.InvariantCulture);
                }

                public short ToInt16(object value)
                {
                    return System.Convert.ToInt16(value, CultureInfo.InvariantCulture);
                }

                public int ToInt32(object value)
                {
                    return System.Convert.ToInt32(value, CultureInfo.InvariantCulture);
                }

                public long ToInt64(object value)
                {
                    return System.Convert.ToInt64(value, CultureInfo.InvariantCulture);
                }

                public sbyte ToSByte(object value)
                {
                    return System.Convert.ToSByte(value, CultureInfo.InvariantCulture);
                }

                public float ToSingle(object value)
                {
                    return System.Convert.ToSingle(value, CultureInfo.InvariantCulture);
                }

                public string ToString(object value)
                {
                    return System.Convert.ToString(value, CultureInfo.InvariantCulture);
                }

                public ushort ToUInt16(object value)
                {
                    return System.Convert.ToUInt16(value, CultureInfo.InvariantCulture);
                }

                public uint ToUInt32(object value)
                {
                    return System.Convert.ToUInt32(value, CultureInfo.InvariantCulture);
                }

                public ulong ToUInt64(object value)
                {
                    return System.Convert.ToUInt64(value, CultureInfo.InvariantCulture);
                }

                #endregion

                private class TemporaryField : IFudgeField
                {
                    private readonly FudgeFieldType type;
                    private readonly object value;

                    public TemporaryField(FudgeFieldType type, object value)
                    {
                        this.type = type;
                        this.value = value;
                    }

                    #region IFudgeField Members

                    public FudgeFieldType Type
                    {
                        get { return type; }
                    }

                    public object Value
                    {
                        get { return value; }
                    }

                    public short? Ordinal
                    {
                        get { return null; }
                    }

                    public string Name
                    {
                        get { return null; }
                    }

                    #endregion
                }
            }
        }
    }
}
