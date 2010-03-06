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
    /// Implement <c>IFudgeSerializable</c> to allow your class to serialize and deserialize to
    /// Fudge message streams.
    /// </summary>
    /// <example>
    /// This example shows a class implementing <see cref="IFudgeSerializable"/> directly:
    /// <code>
    /// public class Person : IFudgeSerializable
    /// {
    ///     public string Name { get; set; }
    ///     public Address MainAddress { get; set; }
    ///
    ///     public Person()
    ///     {
    ///     }
    ///
    ///     #region IFudgeSerializable Members
    ///
    ///     public virtual void Serialize(IMutableFudgeFieldContainer msg, IFudgeSerializer serializer)
    ///     {
    ///         msg.Add("name", Name);
    ///         msg.AddIfNotNull("mainAddress", MainAddress);
    ///     }
    ///
    ///     public virtual void Deserialize(IFudgeFieldContainer msg, IFudgeDeserializer deserializer)
    ///     {
    ///         foreach (IFudgeField field in msg)
    ///         {
    ///             DeserializeField(deserializer, field);
    ///         }
    ///     }
    ///
    ///     #endregion
    ///
    ///     protected virtual bool DeserializeField(IFudgeDeserializer deserializer, IFudgeField field)
    ///     {
    ///         switch (field.Name)
    ///         {
    ///             case "name":
    ///                 Name = field.GetString();
    ///                 return true;
    ///             case "mainAddress":
    ///                 MainAddress = deserializer.FromField&lt;Address&gt;(field);
    ///                 return true;
    ///         }
    ///
    ///         // Field not recognised
    ///         return false;
    ///     }
    /// }
    /// </code>
    /// The code for the <c>Address</c> class is not shown here, but it could similarly implement <see cref="IFudgeSerializable"/> or alternatively
    /// use one of the other approaches to serialization - see the <see cref="Fudge.Serialization"/> namespace for more info.
    /// </example>
    public interface IFudgeSerializable
    {
        /// <summary>
        /// Serializes the object into a message.
        /// </summary>
        /// <param name="msg">Message to serialize the object into.</param>
        /// <param name="serializer">Serializer to receive the data.</param>
        void Serialize(IMutableFudgeFieldContainer msg, IFudgeSerializer serializer);

        /// <summary>
        /// Deserializes a message into the object.
        /// </summary>
        /// <param name="msg">Message containing the data.</param>
        /// <param name="deserializer">Deserializer providing the data.</param>
        void Deserialize(IFudgeFieldContainer msg, IFudgeDeserializer deserializer);
    }
}
