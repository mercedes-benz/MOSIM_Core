// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace MMICSharp.Common.Communication
{
    /// <summary>
    /// Extensions for the web communication
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Method opens the response stream and returns the string
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        public static string ReadResponseStream(this HttpWebResponse response)
        {
            string data = null;


            if (response == null)
                return data;

            //Read the stream
            try
            {
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    data = reader.ReadToEnd();
                }
            }
            catch (Exception)
            {
            }

            response.Close();

            return data;
        }


        /// <summary>
        /// Method opens the response stream and returns the string
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        public static T ReadResponseStream<T>(this HttpWebResponse response)
        {
            string data = response.ReadResponseStream();

            if (data == null)
                return default(T);

            return Serialization.FromJsonString<T>(data);
        }




        /// <summary>
        /// Method opens the response stream and returns the string
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        public static string ReadInputStream(this HttpListenerRequest request)
        {
            string data = null;

            try
            {
                using (StreamReader reader = new StreamReader(request.InputStream))
                {
                    data = reader.ReadToEnd();
                }
            }
            catch (Exception)
            {
                return null;
            }

            return data;
        }


        /// <summary>
        /// Reads the input stream and returns the type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request"></param>
        /// <returns></returns>
        public static T ReadInputStream<T>(this HttpListenerRequest request) where T: class
        {
            string data = request.ReadInputStream();

            if (data == null)
                return default(T);

            return Serialization.FromJsonString<T>(data);
        }


        /// <summary>
        /// Reads the network stream and returns the byte data
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static byte[] ReadStream(this NetworkStream stream, int bufferSize)
        {
            List<byte> data = new List<byte>();

            while (true)
            {
                byte[] chunkedBuffer = new byte[bufferSize];

                int amount = stream.Read(chunkedBuffer, 0, chunkedBuffer.Length);

                if (amount == 0)
                    break;

                if (amount < bufferSize)
                {
                    data.AddRange(chunkedBuffer.Take(amount).ToArray());
                    break;
                }
                else
                {
                    data.AddRange(chunkedBuffer);
                }
            }
            return data.ToArray();
        }

    }
}
