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
    internal class SerializationHeader
    {
        private const int serializationVersion = 1;
        private readonly SerializationTypeMap typeMap;

        public SerializationHeader(SerializationTypeMap typeMap)
        {
            this.typeMap = typeMap;
        }

        public SerializationHeader(FudgeMsg message, SerializationTypeMap typeMap)
        {
            string[] typeNames = message.GetAllValues<string>("typeMap").ToArray();
            int[] typeVersions = message.GetAllValues<int>("typeVersions").ToArray();
            this.typeMap = typeMap.Remap(typeNames, typeVersions);
        }

        public FudgeMsg ToMessage()
        {
            var result = new FudgeMsg();
            result.Add("version", serializationVersion);
            result.AddAll("typeMap", typeMap.GetTypeNames().ToArray());                   // TODO t0rx 2009-10-17 -- Need to make FudgeMsg able to handle lists and other collections
            result.AddAll("typeVersions", typeMap.GetTypeVersions().ToArray());           // TODO t0rx 2009-10-17 -- If all versions are zero, just leave out?
            return result;
        }
    }
}
