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
    public class FudgeSurrogateSelector
    {
        private readonly FudgeContext context;
        private readonly TypeDataCache typeDataCache;

        public FudgeSurrogateSelector(FudgeContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            this.context = context;
            this.typeDataCache = new TypeDataCache(context);
        }

        public Func<FudgeContext, IFudgeSerializationSurrogate> GetSurrogateFactory(Type type, FudgeFieldNameConvention fieldNameConvention)
        {
            var typeData = typeDataCache.GetTypeData(type, fieldNameConvention);

            // Look for FudgeSurrogate attribute
            var surrogateAttribute = typeData.CustomAttributes.FirstOrDefault(attrib => attrib is FudgeSurrogateAttribute);
            if (surrogateAttribute != null)
            {
                return BuildSurrogateFactory(type, (FudgeSurrogateAttribute)surrogateAttribute);
            }

            // For all of these known types, we only need one surrogate as it is stateless
            IFudgeSerializationSurrogate surrogate;
            if (typeof(IFudgeSerializable).IsAssignableFrom(type))
            {
                surrogate = new SerializableSurrogate(type);
            }
            else if (ArraySurrogate.CanHandle(typeData))
            {
                surrogate = new ArraySurrogate(context, typeData);
            }
            else if (DictionarySurrogate.CanHandle(typeData))
            {
                surrogate = new DictionarySurrogate(context, typeData);
            }
            else if (ListSurrogate.CanHandle(typeData))
            {
                surrogate = new ListSurrogate(context, typeData);
            }
            else if (ToFromFudgeMsgSurrogate.CanHandle(typeData))
            {
                surrogate = new ToFromFudgeMsgSurrogate(context, typeData);
            }
            else if (PropertyBasedSerializationSurrogate.CanHandle(typeData))
            {
                surrogate = new PropertyBasedSerializationSurrogate(context, typeData);
            }
            else
            {
                throw new FudgeRuntimeException("Cannot automatically determine surrogate for type " + type.FullName);
            }
            return c => surrogate;
        }

        private Func<FudgeContext, IFudgeSerializationSurrogate> BuildSurrogateFactory(Type type, FudgeSurrogateAttribute attrib)
        {
            var surrogateType = attrib.SurrogateType;
            var constructor = surrogateType.GetConstructor(new Type[] { typeof(FudgeContext), typeof(Type) });
            object[] args;
            if (constructor != null)
            {
                args = new object[] { context, type };
            }
            else if ((constructor = surrogateType.GetConstructor(new Type[] { typeof(Type) })) != null)
            {
                args = new object[] { type };
            }
            else if ((constructor = surrogateType.GetConstructor(Type.EmptyTypes)) != null)
            {
                args = new object[] { };
            }
            else
            {
                Debug.Assert(false, "Lack of suitable constructor should have been picked up by FudgeSurrogateAttribute");
                throw new FudgeRuntimeException("Surrogate type " + surrogateType + " does not have appropriate constructor");
            }

            if (attrib.Stateless)
            {
                var surrogate = (IFudgeSerializationSurrogate)constructor.Invoke(args);
                return c => surrogate;
            }
            else if (args.Length == 2)
            {
                // Have to replace the context so ignore the args we created above
                return c => (IFudgeSerializationSurrogate)constructor.Invoke(new object[] { c, type });
            }
            else
            {
                return c => (IFudgeSerializationSurrogate)constructor.Invoke(args);
            }
        }
    }
}
