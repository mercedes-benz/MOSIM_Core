// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace MMICSharp.Common.Communication
{
    /// <summary>
    /// A basic class for web access functionality
    /// </summary>
    public class WebAccess
    {

        /// <summary>
        /// Sends a http put to the specified address
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static HttpWebResponse Request(string addr, Dictionary<string, string> queryString, int timeout = 1000)
        {
            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
            watch.Start();

            WebRequest.DefaultWebProxy = null;


            if (queryString.Count > 0)
                addr += "?";

            int count = 0;
            foreach (KeyValuePair<string, string> query in queryString)
            {
                if (count > 0)
                {
                    addr += "&";
                }
                addr += query.Key + "=" + query.Value;
                count++;
            }


            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(addr);
            request.Proxy = null;
            request.Method = WebRequestMethods.Http.Get;
            request.Timeout = timeout;
            request.ContentType = "text/xml";

            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                Console.WriteLine(response.StatusCode + " " + watch.Elapsed.TotalMilliseconds + " ms");

                return response;
            }
            catch (Exception)
            {
                Console.WriteLine("No response: " + watch.Elapsed.TotalMilliseconds + " ms");

                return null;
            }
        }


        /// <summary>
        /// Performs a request and directly returns the object type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="addr"></param>
        /// <param name="queryString"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static T Request<T>(string addr, Dictionary<string, string> queryString, int timeout = 1000) where T : class
        {
            using (HttpWebResponse response = WebAccess.Request(addr, queryString, timeout))
            {
                if (response == null)
                    return null;

                string data = response.ReadResponseStream();

                if (data == null)
                    return null;

                return Serialization.FromJsonString<T>(data);
            }
        }


        /// <summary>
        /// Performs a request and directly returns the object type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="addr"></param>
        /// <param name="queryString"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static void RequestOnly(string addr, Dictionary<string, string> queryString, int timeout = 1000) 
        {
            HttpWebResponse response = WebAccess.Request(addr, queryString, timeout);
            if(response!=null)
                response.Close();
        }


        /// <summary>
        /// Sends a http put to the specified address
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static HttpWebResponse PutData(string addr, string stringData, Dictionary<string, string> queryString, int timeout = 1000)
        {
            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
            watch.Start();

            WebRequest.DefaultWebProxy = null;

            if (queryString.Count > 0)
                addr += "?";

            int count = 0;
            foreach (KeyValuePair<string, string> query in queryString)
            {
                if (count > 0)
                {
                    addr += "&";
                }
                addr += query.Key + "=" + query.Value;
                count++;
            }


            byte[] data = Encoding.UTF8.GetBytes(stringData);

            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(addr);

            request.Method = WebRequestMethods.Http.Post;
            request.Timeout = timeout;
            request.Proxy = null;
            request.ContentType = "text/xml";
            request.ContentLength = data.Length;


            try
            {
                using (Stream dataStream = request.GetRequestStream())
                {
                    dataStream.Write(data, 0, data.Length);
                }

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                

                Console.WriteLine(response.StatusCode + " " + watch.Elapsed.TotalMilliseconds + " ms");
                return response;
            }

            catch (Exception)
            {
                return null;
            }

        }


        /// <summary>
        /// Sends a http put to the specified address
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static HttpWebResponse PutData<T>(string addr, T data, Dictionary<string, string> queryString, int timeout = 1000)
        {
            return WebAccess.PutData(addr, Serialization.ToJsonString<T>(data), queryString, timeout);
        }


        /// <summary>
        /// Sends a http put to the specified address
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static void PutDataOnly<T>(string addr, T data, Dictionary<string, string> queryString, int timeout = 1000)
        {
            HttpWebResponse webResponse = WebAccess.PutData(addr, Serialization.ToJsonString<T>(data), queryString, timeout);
            if (webResponse != null)
                webResponse.Close();
        }

    }
}





