////
// Copyright (c) Eclo Solutions
// See LICENSE file in the project root for full license information.
////

using System;
using System.IO;
using System.Text;

namespace Eclo.nanoFramework.SIM800H
{
    /// <summary>
    /// Handles retrieval of HTTP Response headers, and handles data reads.
    /// </summary>
    /// <remarks>Implementation follows .NETMF System.Net.HttpWebResponse</remarks>
    public class HttpWebResponse : IDisposable
    {
        /// <summary>
        /// True if the request was successfully executed.
        /// </summary>
        public bool RequestSuccessful { get; private set; }

        /// <summary>
        /// <see cref="HttpStatusCode"/> of the request response
        /// </summary>
        public HttpStatusCode StatusCode { get; private set; }

        /// <summary>
        /// Body data, if any, received from the request execution.
        /// It's null or empty if the request was performed with the option of not reading the response data.
        /// </summary>
        public string BodyData { get; private set; }

        /// <summary>
        /// Headers collection received from the request response.
        /// It's empty if the request was performed with the option of not reading the response headers.
        /// </summary>
        public WebHeaderCollection Headers { get; set; }

        internal HttpWebResponse(bool success, HttpStatusCode statusCode, string data)
        {
            this.RequestSuccessful = success;
            this.StatusCode = statusCode;
            this.BodyData = data;
            this.Headers = new WebHeaderCollection();
        }

        internal HttpWebResponse(bool success, HttpStatusCode statusCode)
        {
            this.RequestSuccessful = success;
            this.StatusCode = statusCode;
            this.BodyData = string.Empty;
            this.Headers = new WebHeaderCollection();
        }

        internal HttpWebResponse(bool success)
        {
            this.RequestSuccessful = success;
            this.BodyData = string.Empty;
            this.Headers = new WebHeaderCollection();
        }

        public MemoryStream GetResponseStream()
        {
            MemoryStream stream = new MemoryStream();
            if (this.BodyData != string.Empty)
            {
                var buff = Encoding.UTF8.GetBytes(this.BodyData);
                stream.Write(buff, 0, buff.Length);//, 0, this.BodyData.Length);
            }
            // need to reset stream position to allow reading
            stream.Seek(0, SeekOrigin.Begin);

            return stream;
        }

        ~HttpWebResponse()
        {

        }

        public void Close()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Dispose() calls Dispose(true)
        public void Dispose()
        {
            Close();
        }

        // The bulk of the clean-up code is implemented in Dispose(bool)
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // free managed resources
                //if (managedResource != null)
                //{
                //    managedResource.Dispose();
                //    managedResource = null;
                //}
            }
            //// free native resources if there are any.
            //if (nativeResource != IntPtr.Zero) 
            //{
            //    Marshal.FreeHGlobal(nativeResource);
            //    nativeResource = IntPtr.Zero;
            //}
        }

        void IDisposable.Dispose()
        {

        }
    }
}
