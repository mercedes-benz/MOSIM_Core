// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace MMICSharp.Common.Communication
{
    /// <summary>
    /// Basic json serialization class
    /// </summary>
    public class Serialization
    {
        /// <summary>
        /// Creates a json string from the data
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string ToJsonString<T>(T data)
        {
            return System.Text.Encoding.UTF8.GetString(Utf8Json.JsonSerializer.Serialize<T>(data));
        }



        /// <summary>
        /// Instantiates the object from the json string data
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public static T FromJsonString<T>(string data)
        {
            return Utf8Json.JsonSerializer.Deserialize<T>(data);
        }

        /// <summary>
        /// Serializes an object to a byte array
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serializableObject"></param>
        /// <returns></returns>
        public static byte[] SerializeBinary<T>(T serializableObject)
        {
            //Set json as defaullt since binary serialization causes trouble finding the assemblies
            //return System.Text.Encoding.UTF8.GetBytes(Serialization.ToJsonString(serializableObject));


            T obj = serializableObject;

            IFormatter formatter = new BinaryFormatter();
            using (MemoryStream stream = new MemoryStream())
            {
                formatter.Serialize(stream, obj);
                return stream.ToArray();
            }
        }

        /// <summary>
        /// Deserializes and object from a given byte array
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serializedBytes"></param>
        /// <returns></returns>
        public static T DeserializeBinary<T>(byte[] serializedBytes)
        {
            //Set json as defaullt since binary serialization causes trouble finding the assemblies
            //return Serialization.FromJsonString<T>(System.Text.Encoding.UTF8.GetString(serializedBytes));

            IFormatter formatter = new BinaryFormatter();
            using (MemoryStream stream = new MemoryStream(serializedBytes))
            {
                return (T)formatter.Deserialize(stream);
            }
        }

    }
}