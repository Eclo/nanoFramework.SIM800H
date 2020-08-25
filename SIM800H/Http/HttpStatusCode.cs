namespace Eclo.nanoFramework.SIM800H
{
    // Any int can be cast to a HttpStatusCode to allow checking for non http1.1
    // codes.

    /// <summary>
    /// Contains the values of status codes defined for HTTP.
    /// </summary>
    /// <remarks>
    /// <p>Status codes indicate categories, as follows:</p>
    /// <p>1xx -- Informational.</p>
    /// <p>2xx -- Successful.</p>
    /// <p>3xx -- Redirection.</p>
    /// <p>4xx -- Client Error.</p>
    /// <p>5xx -- Server Error.</p>
    /// </remarks>
    public enum HttpStatusCode
    {

        /// Informational -- 1xx.
        /// <summary>Equivalent to HTTP status 100.  Indicates that the client can continue with its
        /// request.</summary>
        Continue = 100,
        //// <summary>Equivalent to HTTP status 101.  Indicates that the protocol version or protocol
        //// is being changed.</summary>
        //SwitchingProtocols = 101,

        /// Successful -- 2xx.
        /// <summary>Equivalent to HTTP status 200.  Indicates that the request succeeded and that
        /// the requested information is in the response. This is the most common status code to
        /// receive.</summary>
        OK = 200,
        /// <summary>Equivalent to HTTP status 201.  Indicates that the request resulted in a new
        /// resource created before the response was sent.</summary>
        Created = 201,
        /// <summary>Equivalent to HTTP status 202. Indicates that the request has been accepted for
        /// further processing.</summary>
        Accepted = 202,
        ///// <summary>Equivalent to HTTP status 203.  Indicates that the returned metainformation is
        ///// from a cached copy instead of the origin server and therefore may be incorrect.</summary>
        //NonAuthoritativeInformation = 203,
        /// <summary>Equivalent to HTTP status 204.  Indicates that the request has been successfully
        /// processed and that the response is intentionally blank.</summary>
        NoContent = 204,
        ///// <summary>Equivalent to HTTP status 205.  Indicates that the client should reset (not
        ///// reload) the current resource.</summary>
        //ResetContent = 205,
        /// <summary>Equivalent to HTTP status 206.  Indicates that the response is a
        /// partial response as requested by a GET request that includes a byte range.</summary>
        PartialContent = 206,

        ///// Redirection -- 3xx.
        ///// <summary>Equivalent to HTTP status 300.  Indicates that the requested information has
        ///// multiple representations.  The default action is to treat this status as a redirect and
        ///// follow the contents of the Location header associated with this response.
        ///// <para>If the <see cref="System.Net.HttpWebRequest.AllowAutoRedirect"/> property is
        ///// <itemref>false</itemref>, <itemref>MultipleChoices</itemref> will cause an exception to
        ///// be thrown.</para>
        ///// <para><itemref>MultipleChoices</itemref> is a synonym for <itemref>Ambiguous</itemref>.</para></summary>
        //MultipleChoices = 300,
        ///// <summary>Equivalent to HTTP status 300.  Indicates that the requested
        ///// information has multiple representations.  The default action is to treat this status as
        ///// a redirect and follow the contents of the Location header associated with this response.
        ///// <para>If the <see cref="System.Net.HttpWebRequest.AllowAutoRedirect"/> property is
        ///// <itemref>false</itemref>, <itemref>Ambiguous</itemref> will cause an exception to be
        ///// thrown.</para>
        ///// <para><itemref>Ambiguous</itemref> is a synonym for <itemref>MultipleChoices</itemref>.</para></summary>
        //Ambiguous = 300,
        /// <summary>Equivalent to HTTP status 301. Indicates that the requested information has
        /// been moved to the URI specified in the Location header. The default action when this
        /// status is received is to follow the Location header associated with the response.
        /// <para><itemref>MovedPermanently</itemref> is a synonym for <itemref>Moved</itemref>.</para></summary>
        MovedPermanently = 301,
        /// <summary>Equivalent to HTTP status 301. Indicates that the requested information
        /// has been moved to the URI specified in the Location header. The default action when this
        /// status is received is to follow the Location header associated with the response. When
        /// the original request method was POST, the redirected request will use the GET method.
        /// <para><itemref>Moved</itemref> is a synonym for <itemref>MovedPermanently</itemref>.</para></summary>
        Moved = 301,
        ///// <summary>Equivalent to HTTP status 302. Indicates that the requested information is
        ///// located at the URI specified in the Location header. The default action when this status
        ///// is received is to follow the Location header associated with the response. When the
        ///// original request method was POST, the redirected request will use the GET method.
        ///// <para>If the <see cref="System.Net.HttpWebRequest.AllowAutoRedirect"/> property is
        ///// <itemref>false</itemref>, <itemref>Found</itemref> will cause an exception to be thrown.</para>
        ///// <para><itemref>Found</itemref> is a synonym for <itemref>Redirect</itemref>.</para></summary>
        //Found = 302,
        /// <summary>Equivalent to HTTP status 302. Indicates that the requested information is
        /// located at the URI specified in the Location header. The default action when this status
        /// is received is to follow the Location header associated with the response. When the
        /// original request method was POST, the redirected request will use the GET method.
        ///</summary>
        Redirect = 302,
        ///// <summary>Equivalent to HTTP status 303. Automatically redirects the client to
        ///// the URI specified in the Location header as the result of a POST. The request to the
        ///// resource specified by the Location header will be made with a GET.
        ///// </summary>
        //SeeOther = 303,
        ///// <summary>Equivalent to HTTP status 303. Automatically redirects the
        ///// client to the URI specified in the Location header as the result of a POST. The request
        ///// to the resource specified by the Location header will be made with a GET.
        ///// </summary>
        //RedirectMethod = 303,
        /// <summary>Equivalent to HTTP status 304. Indicates that the client's cached copy is
        /// up-to-date. The contents of the resource are not transferred.</summary>
        NotModified = 304,
        ///// <summary>Equivalent to HTTP status 305. Indicates that the request should use the proxy
        ///// server at the URI specified in the Location header.</summary>
        ////UseProxy = 305,
        ///// <summary>Equivalent to HTTP status 306. This value is a proposed extension to the HTTP/1.1
        ///// specification that is not fully specified.</summary>
        //Unused = 306,
        /// <summary>Equivalent to HTTP status 307. Indicates that the request information is
        /// located at the URI specified in the Location header. The default action when this status
        /// is received is to follow the Location header associated with the response. When the
        /// original request method was POST, the redirected request will also use the POST method.
        /// </summary>
        TemporaryRedirect = 307,
        ///// <summary>Equivalent to HTTP status 307. Indicates that the request
        ///// information is located at the URI specified in the Location header. The default action
        ///// when this status is received is to follow the Location header associated with the
        ///// response. When the original request method was POST, the redirected request will also
        ///// use the POST method.
        ///// <para>If the <see cref="System.Net.HttpWebRequest.AllowAutoRedirect"/> property is
        ///// <itemref>false</itemref>, <itemref>RedirectKeepVerb</itemref> will cause an exception to
        ///// be thrown.</para>
        ///// <para><itemref>RedirectKeepVerb</itemref> is a synonym for <itemref>TemporaryRedirect</itemref>.</para></summary>
        //RedirectKeepVerb = 307,

        /// Client Error -- 4xx.
        /// <summary>Equivalent to HTTP status 400. Indicates that the request could not be
        /// understood by the server. <itemref>BadRequest</itemref> is sent when no other error is
        /// applicable, or if the exact error is unknown or does not have its own error code.</summary>
        BadRequest = 400,
        /// <summary>Equivalent to HTTP status 401. Indicates that the requested
        /// resource requires authentication. The WWW-Authenticate header contains the details of
        /// how to perform the authentication.</summary>
        Unauthorized = 401,
        ///// <summary>Equivalent to HTTP status 402. Reserved for future use.</summary>
        //PaymentRequired = 402,
        /// <summary>Equivalent to HTTP status 403. Indicates that the server refuses to
        /// fulfill the request.</summary>
        Forbidden = 403,
        /// <summary>Equivalent to HTTP status 404. Indicates that the requested resource
        /// does not exist on the server.</summary>
        NotFound = 404,
        /// <summary>Equivalent to HTTP status 405. Indicates that the request
        /// method (POST or GET) is not allowed on the requested resource.</summary>
        MethodNotAllowed = 405,
        /// <summary>Equivalent to HTTP status 406. Indicates that the client has
        /// indicated with Accept headers that it will not accept any of the available
        /// representations of the resource.</summary>
        NotAcceptable = 406,
        ///// <summary>Equivalent to HTTP status 407. Indicates that the
        ///// requested proxy requires authentication. The Proxy-authenticate header contains the
        ///// details of how to perform the authentication.</summary>
        //ProxyAuthenticationRequired = 407,
        /// <summary>Equivalent to HTTP status 408. Indicates that the client did not
        /// send a request within the time the server was expecting the request.</summary>
        RequestTimeout = 408,
        ///// <summary>Equivalent to HTTP status 409. Indicates that the request could not be
        ///// carried out because of a conflict on the server.</summary>
        //Conflict = 409,
        ///// <summary>Equivalent to HTTP status 410. Indicates that the requested resource is no
        ///// longer available.</summary>
        //Gone = 410,
        ///// <summary>Equivalent to HTTP status 411. Indicates that the required
        ///// Content-length header is missing.</summary>
        //LengthRequired = 411,
        /// <summary>Equivalent to HTTP status 412. Indicates that a condition
        /// set for this request failed, and the request cannot be carried out.  Conditions are set
        /// with conditional request headers like If-Match, If-None-Match, or If-Unmodified-Since.</summary>
        PreconditionFailed = 412,
        /// <summary>Equivalent to HTTP status 413. Indicates that the request
        /// is too large for the server to process.</summary>
        RequestEntityTooLarge = 413,
        /// <summary>Equivalent to HTTP status 414. Indicates that the URI is too
        /// long.</summary>
        RequestUriTooLong = 414,
        /// <summary>Equivalent to HTTP status 415. Indicates that the request
        /// is an unsupported type.</summary>
        UnsupportedMediaType = 415,
        ///// <summary>Equivalent to HTTP status 416. Indicates that the
        ///// range of data requested from the resource cannot be returned, either because the
        ///// beginning of the range is before the beginning of the resource, or the end of the range
        ///// is after the end of the resource.</summary>
        //RequestedRangeNotSatisfiable = 416,
        ///// <summary>Equivalent to HTTP status 417. Indicates that an expectation
        ///// given in an Expect header could not be met by the server.</summary>
        //ExpectationFailed = 417,

        /// Server Error -- 5xx.
        /// <summary>Equivalent to HTTP status 500. Indicates that a generic
        /// error has occurred on the server.</summary>
        InternalServerError = 500,
        /// <summary>Equivalent to HTTP status 501. Indicates that the server does
        /// not support the requested function.</summary>
        NotImplemented = 501,
        ///// <summary>Equivalent to HTTP status 502. Indicates that an intermediate proxy
        ///// server received a bad response from another proxy or the origin server.</summary>
        //BadGateway = 502,
        /// <summary>Equivalent to HTTP status 503. Indicates that the server is
        /// temporarily unavailable, usually due to high load or maintenance.</summary>
        ServiceUnavailable = 503,
        ///// <summary>Equivalent to HTTP status 504. Indicates that an intermediate
        ///// proxy server timed out while waiting for a response from another proxy or the origin
        ///// server.</summary>
        //GatewayTimeout = 504,
        /// <summary>Equivalent to HTTP status 505. Indicates that the
        /// requested HTTP version is not supported by the server.</summary>
        HttpVersionNotSupported = 505,

    }; // enum HttpStatusCode
}
