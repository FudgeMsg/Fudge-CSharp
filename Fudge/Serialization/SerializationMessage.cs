/**
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
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenGamma.Fudge.Serialization
{
    public class SerializationMessage
    {
        private const int serializationVersion = 1;
        private readonly IList<FudgeMsg> encodedObjects;
        private readonly SerializationTypeMap typeMap;

        public SerializationMessage(SerializationTypeMap typeMap)
        {
            this.encodedObjects = new List<FudgeMsg>();
            this.typeMap = typeMap;
        }

        public SerializationMessage(FudgeMsg message, SerializationTypeMap typeMap)
        {
            string[] typeNames = message.GetValue<string[]>("typeMap");
            int[] typeVersions = message.GetValue<int[]>("typeVersions");
            this.typeMap = typeMap.Remap(typeNames, typeVersions);
            this.encodedObjects = message.GetAllValues<FudgeMsg>("objects");
        }

        public void AddObject(FudgeMsg msg)
        {
            encodedObjects.Add(msg);
        }

        public int EncodedObjectCount
        {
            get { return encodedObjects.Count; }
        }

        public FudgeMsg GetEncodedObject(int index)
        {
            return encodedObjects[index];
        }

        public FudgeMsg ToMessage()
        {
            var result = new FudgeMsg();
            result.Add("version", serializationVersion);
            result.Add("typeMap", typeMap.GetTypeNames().ToArray());                   // TODO t0rx 2009-10-17 -- Need to make FudgeMsg able to handle lists and other collections
            result.Add("typeVersions", typeMap.GetTypeVersions().ToArray());           // TODO t0rx 2009-10-17 -- If all versions are zero, just leave out?
            result.AddAll("objects", encodedObjects);
            return result;
        }
    }
}
