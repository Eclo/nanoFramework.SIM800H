using System;

namespace Eclo.nanoFramework.SIM800H
{
    /// <summary>
    /// Makes a request to a Uniform Resource Identifier (URI). This is an abstract class.
    /// </summary>
    /// <remarks>
    /// This is the base class of all Web resource/protocol objects.  This class
    /// provides common methods, data and properties for making the top-level
    /// request.
    /// Implementation follows .NETMF System.Net.WebRequest
    /// </remarks>
    public abstract class WebRequest : IDisposable
    {
        /// <summary>
        /// Gets or sets the value of the <b>User-Agent</b> HTTP header.
        /// </summary>
        /// <value>The value of the <b>User-agent</b> HTTP header.  The default
        /// value is <b>null</b>.</value>
        public string UserAgent { get; set; }

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="Eclo.nanoFramework.SIM800H.WebRequest"/> class.
        /// </summary>
        protected WebRequest()
        {

        }

        ~WebRequest()
        {
            Dispose(false);
        }
        /// <summary>
        /// Creates a <itemref>WebRequest</itemref>.
        /// </summary>
        /// <param name="requestUri">A <see cref="Uri"/> containing the
        /// URI of the requested resource.</param>
        /// <return>A <itemref>WebRequest</itemref> descendant for the specified
        /// URI scheme.</return>
        /// <remarks>
        /// This is the main creation routine. The specified Uri is looked up
        /// in the prefix match table, and the appropriate handler is invoked to
        /// create the object.
        /// </remarks>
        public static WebRequest Create(Uri requestUri)
        {
            return CreateInternal(requestUri);
        }

        private static WebRequest CreateInternal(Uri requestUri)
        {
            if (requestUri == null) { throw new ArgumentNullException(); }

            return new HttpRequestCreator().Create(requestUri);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
        }
    }
}
