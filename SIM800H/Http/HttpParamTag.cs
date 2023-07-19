////
// Copyright (c) Eclo Solutions
// See LICENSE file in the project root for full license information.
////

namespace Eclo.nanoFramework.SIM800H
{
    /// <summary>
    /// Parameters for HTTP call
    /// </summary>
    internal enum HttpParamTag
    {
        /// <summary>
        /// (mandatory parameter) Bearer profile identifier
        /// </summary>
        CID,
        /// <summary>
        /// (mandatory parameter) HTTP client URL with format "http://'server'/'path':'tcpPort'"
        /// server: FQDN or IP address
        /// path: path of file or directory
        /// tcpPort: default value is 80
        /// </summary>
        URL,
        /// <summary>
        /// Refer to IETF-RFC 2616. The user agent string is usually set by the application to identify the system.
        /// Usually this parameters include information about the OS and other system capabilities and version information.
        /// Default value is SIMCOM_MODULE
        /// </summary>
        UA,
        /// <summary>
        /// The IP address of the HTTP proxy server
        /// </summary>
        PROPIP,
        /// <summary>
        /// The port of the HTTP proxy server
        /// </summary>
        PROPORT,
        /// <summary>
        /// This flag controls the redirection mechanics of the SIM800 when it's acting as HTTP client. 
        /// If the server sends a redirect code (range 30X) the client will automatically send a new HTTP request when the flag is set to 1.
        /// Default value is 0 (no redirection)
        /// </summary>
        REDIR,
        /// <summary>
        /// Parameter for HTTP method GET used for resuming broken transfer
        /// </summary>
        BREAK,
        /// <summary>
        /// Parameter for HTTP method GET used for resuming broken transfer that is used together with BREAK. 
        /// If the value of BREAKEND is bigger than BREAK the transfer scope is from BREAK to BREAKEND.
        /// If the value of BREAKEND is smaller than BREAK the transfer scope is from BREAK to the end of the file.
        /// If both BREAKEND and BREAK are 0, the resumed broken transfer function is disabled.
        /// Note that not all servers support BREAKEND and BREAK
        /// </summary>
        BREAKEND,
        /// <summary>
        /// HTTP session timeout, scope: 30-1000 seconds. 
        /// Default value is 120 seconds.
        /// </summary>
        TIMEOUT,
        /// <summary>
        /// Sets the Content-Type HTTP header
        /// </summary>
        CONTENT,
        /// <summary>
        /// Sets the user data parameter (to set HTTP headers)
        /// </summary>
        USERDATA,
    }
}
