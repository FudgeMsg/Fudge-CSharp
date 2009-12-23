/* <!--
 * Copyright (C) 2009 - 2009 by OpenGamma Inc. and other contributors.
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
using System.Diagnostics;
using System.Reflection;

namespace Fudge.Serialization
{
    public class SerializableSurrogate : IFudgeSerializationSurrogate
    {
        private readonly Type type;
        private readonly ConstructorInfo constructor;

        public SerializableSurrogate(Type type)
        {
            if (type == null || !typeof(IFudgeSerializable).IsAssignableFrom(type))
            {
                throw new ArgumentOutOfRangeException("type");
            }
            this.type = type;
            this.constructor = type.GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null);
            if (constructor == null)
            {
                throw new ArgumentOutOfRangeException("type", "Type " + type.FullName + " does not have a public default constructor.");
            }
        }

        #region IFudgeSerializationSurrogate Members

        public void Serialize(object obj, FudgeMsg msg, IFudgeSerializationContext context)
        {
            IFudgeSerializable ser = (IFudgeSerializable)obj;
            ser.Serialize(msg, context);
        }

        public object Deserialize(FudgeMsg msg, int dataVersion, IFudgeDeserializationContext context)
        {
            IFudgeSerializable result = (IFudgeSerializable)constructor.Invoke(null);
            context.Register(result);
            result.Deserialize(msg, dataVersion, context);
            return result;
        }

        #endregion
    }
}
