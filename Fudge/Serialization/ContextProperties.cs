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

namespace Fudge.Serialization
{
    /// <summary>
    /// Context properties that affect the behaviour of the serialization framework.
    /// </summary>
    public static class ContextProperties
    {
        /// <summary>Property of the <see cref="FudgeContext"/> that overrides the default value of <see cref="FudgeSerializer.TypeMappingStrategy"/>.</summary>
        public static readonly FudgeContextProperty TypeMappingStrategyProperty = new FudgeContextProperty("Serialization.TypeMappingStrategy", typeof(IFudgeTypeMappingStrategy));
        /// <summary>Property of the <see cref="FudgeContext"/> that sets the <see cref="FudgeFieldNameConvention"/> (<see cref="FudgeFieldNameConvention.Identity"/> by default).</summary>
        public static readonly FudgeContextProperty FieldNameConventionProperty = new FudgeContextProperty("Serialization.FieldNameConvention", typeof(FudgeFieldNameConvention));
        /// <summary>Property of the <see cref="FudgeContext"/> that specifies whether types can automatically by serialized or whether they must be explicitly
        /// registered in the <see cref="SerializationTypeMap"/>.  By default this is <c>true</c>, i.e. types do not need explicitly registering.</summary>
        public static readonly FudgeContextProperty AllowTypeDiscoveryProperty = new FudgeContextProperty("Serialization.AllowTypeDiscovery", typeof(bool));
    }
}
