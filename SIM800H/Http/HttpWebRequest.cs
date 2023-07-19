////
// Copyright (c) Eclo Solutions
// See LICENSE file in the project root for full license information.
////

using System;
using System.IO;
using Windows.Storage.Streams;

namespace Eclo.nanoFramework.SIM800H
{
    internal class HttpRequestCreator : IWebRequestCreate
    {
        internal HttpRequestCreator()
        {
        }

        /// <summary>
        /// Creates an HttpWebRequest. We register
        /// for HTTP and HTTPS URLs, and this method is called when a request
        /// needs to be created for one of those.
        /// </summary>
        /// <param name="Url">Url for request being created.</param>
        /// <returns>The newly created HttpWebRequest.</returns>
        public WebRequest Create(Uri Url)
        {
            return new HttpWebRequest(Url);
        }
    }

    /// <summary>
    /// Provides an HTTP-specific implementation of the <see cref="WebRequest"/> class.
    /// </summary>
    /// <remarks>This class does the main work of the request: it collects the header information
    /// from the user, exposes the Stream for outgoing entity data, and processes the incoming
    /// request.
    /// Implementation follows .NETMF System.Net.HttpWebRequest
    /// </remarks>
    public class HttpWebRequest : WebRequest
    {
        /// <summary>
        /// The URI that we do the request for.
        /// </summary>
        private Uri _originalUrl;

        internal InMemoryRandomAccessStream _requestStream;

        /// <summary>
        /// Data to be sent in the request.
        /// Only valid for POST requests.
        /// </summary>
        public string Data { get; set; }

        /// <summary>
        /// Gets the original Uniform Resource Identifier (URI) of the request.
        /// </summary>
        /// <remarks>
        /// The URI object was created by the constructor and is always
        /// non-null.  The URI object will always be the base URI, because
        /// automatic re-directs aren't supported.
        /// </remarks>
        /// <value>A Uri that contains the URI of the Internet resource passed
        /// to the WebRequest.<see cref="WebRequest.Create(Uri)"/> method.
        /// </value>
        public Uri RequestUri
        {
            get
            {
                return _originalUrl;
            }
        }

        /// <summary>
        /// Gets or sets the type of the entity body (the value of the content
        /// type).
        /// </summary>
        /// <value>The value of the <b>Content-type</b> HTTP header.  The
        /// default value is <b>null</b>.</value>
        /// <remarks>
        /// Setting to <b>null</b> clears the content-type.
        /// </remarks>
        public String ContentType { get; set; }

        /// <summary>
        /// A collection of HTTP headers stored as name/value pairs.
        /// </summary>
        /// <value>A <b>WebHeaderCollection</b> that contains the name/value
        /// pairs that make up the headers for the HTTP request.</value>
        /// <remarks>
        /// The following header values are set through properties on the
        /// <itemref>HttpWebRequest</itemref> class: Accept, Connection,
        /// Content-Length, Content-Type, Expect, Range, Referrer,
        /// Transfer-Encoding, and User-Agent.  Trying to set these header
        /// values by using
        /// <b>WebHeaderCollection.<see cref="WebHeaderCollection.Add(string, string)"/>()</b>
        /// will raise an exception.  Date and Host are set internally.
        /// </remarks>
        public WebHeaderCollection Headers { get; set; }

        internal HttpAction _method = HttpAction.NOT_SET;

        /// <summary>
        /// Gets or sets the HTTP method of this request.
        /// Supported methods: POST, GET, HEAD and DELETE.
        /// </summary>
        /// <value>The request method to use to contact the Internet resource.
        /// The default value is GET.</value>
        /// <remarks>
        /// This method represents the initial origin verb, which is unchanged
        /// and unaffected by redirects.
        /// </remarks>
        public string Method
        {
            get
            {
                switch (_method)
                {
                    case HttpAction.POST:
                        return "POST";
                    case HttpAction.GET:
                        return "GET";
                    case HttpAction.HEAD:
                        return "HEAD";
                    case HttpAction.DELETE:
                        return "DELETE";

                    default:
                        throw new System.NotSupportedException();
                }
            }

            set
            {
                switch (value)
                {
                    case "POST":
                        _method = HttpAction.POST;
                        break;
                    case "GET":
                        _method = HttpAction.GET;
                        break;
                    case "HEAD":
                        _method = HttpAction.HEAD;
                        break;
                    case "DELETE":
                        _method = HttpAction.DELETE;
                        break;

                    default:
                        throw new NotSupportedException();
                }
            }
        }

        /// <summary>
        /// No effect in this platform. It's only implemented for compatibility with .NETMF.
        /// </summary>
        public bool KeepAlive { get; set; }

        /// <summary>
        /// No effect in this platform. It's only implemented for compatibility with .NETMF.
        /// </summary>
        public bool AllowWriteStreamBuffering { get; set; }

        /// <summary>
        /// No effect in this platform. It's only implemented for compatibility with .NETMF.
        /// </summary>
        public long ContentLength { get; set; }

        /// <summary>
        /// Implemented here for compatibility with .NETMF System.Net.HttpWebRequest.
        /// *** WARNING: DO NOT explicitly dispose the stream otherwise the request will be executed empty ***
        /// </summary>
        /// <returns>A <b>Stream</b> to use to write request data.</returns>
        /// <remarks>Used for POST requests.</remarks>
        public InMemoryRandomAccessStream GetRequestStream()
        {
            if (_requestStream == null)
            {
                _requestStream = new InMemoryRandomAccessStream();
            }

            return _requestStream;
        }

        public HttpWebRequest(Uri uri)
        {
            _originalUrl = uri;
            Headers = new WebHeaderCollection();
            UserAgent = string.Empty;
            ContentType = string.Empty;
            Data = string.Empty;
        }

        public HttpWebResponse GetResponse()
        {
            return SIM800H.HttpClient.PerformHttpWebRequestAsync(this, true, true, false).End();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}
