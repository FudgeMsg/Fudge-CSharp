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
    public class FudgeSurrogateSelector
    {
        private readonly FudgeContext context;

        public FudgeSurrogateSelector(FudgeContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            this.context = context;
        }

        public Func<FudgeContext, IFudgeSerializationSurrogate> GetSurrogateFactory(Type type)
        {
            if (typeof(IFudgeSerializable).IsAssignableFrom(type))
            {
                // We only need one surrogate as it is stateless
                var factory = new SerializableSurrogate(type);
                return (c => factory);
            }

            var typeData = new TypeData(context, type);

            if (PropertyBasedSerializationSurrogate.CanHandle(typeData))
            {
                var surrogate = new PropertyBasedSerializationSurrogate(context, type);
                return (c => surrogate);
            }

            return null;
        }
    }
}
