////
// Copyright (c) Eclo Solutions
// See LICENSE file in the project root for full license information.
////

using System;
using System.Runtime.CompilerServices;

namespace Eclo.nanoFramework.SIM800H
{
    /// <summary>
    /// Class with methods to perform HTTP client requests.
    /// </summary>
    public class HttpClient : IDisposable
    {
        internal HttpClient()
        {
        }

        #region Disposable implementation

        ~HttpClient()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        [MethodImplAttribute(MethodImplOptions.Synchronized)]
        protected virtual void Dispose(bool disposing)
        {
        }

        #endregion

        /// <summary>
        /// Set HTTP parameter values used in HTTP service
        /// </summary>
        /// <param name="parameter">Parameter to set value</param>
        /// <param name="value">Value of parameter</param>
        /// <returns></returns>
        internal AtCommandResult SetHttpParameter(HttpParamTag parameter, string value)
        {
            string atCommand = Prompts.AT + Prompts.HTTPPARA;

            switch (parameter)
            {
                case HttpParamTag.URL:
                    atCommand += @"""URL"",";
                    break;

                case HttpParamTag.UA:
                    atCommand += @"""UA"",";
                    break;

                case HttpParamTag.PROPIP:
                    atCommand += @"""PROPIP"",";
                    break;

                case HttpParamTag.PROPORT:
                    atCommand += @"""PROPORT"",";
                    break;

                case HttpParamTag.CONTENT:
                    atCommand += @"""CONTENT"",";
                    break;

                case HttpParamTag.USERDATA:
                    atCommand += @"""USERDATA"",";
                    break;

                default:
                    return new AtCommandResult(ReturnedState.InvalidCommand);
            }

            return SIM800H.Instance.SendATCommand(atCommand + @"""" + value + @"""");
        }

        /// <summary>
        /// Set HTTP parameter values used in HTTP service
        /// </summary>
        /// <param name="parameter">Parameter to set value</param>
        /// <param name="value">Value of parameter</param>
        /// <returns></returns>
        internal AtCommandResult SetHttpParameter(HttpParamTag parameter, int value)
        {
            string atCommand = Prompts.AT + Prompts.HTTPPARA;

            switch (parameter)
            {
                case HttpParamTag.CID:
                    atCommand += @"""CID"",";
                    break;

                case HttpParamTag.REDIR:
                    atCommand += @"""REDIR"",";
                    break;

                case HttpParamTag.BREAK:
                    atCommand += @"""BREAK"",";
                    break;

                case HttpParamTag.BREAKEND:
                    atCommand += @"""BREAKEND"",";
                    break;

                case HttpParamTag.TIMEOUT:
                    atCommand += @"""TIMEOUT"",";
                    break;

                default:
                    return new AtCommandResult(ReturnedState.InvalidCommand);
            }

            return SIM800H.Instance.SendATCommand(atCommand + value);
        }

        /// <summary>
        /// Set Http action method
        /// </summary>
        /// <param name="method">action method</param>
        /// <returns></returns>
        internal AtCommandResult SetHttpActionMethod(HttpAction method)
        {
            switch (method)
            {
                case HttpAction.GET:
                case HttpAction.HEAD:
                case HttpAction.POST:
                case HttpAction.DELETE:
                    return SIM800H.Instance.SendATCommandAndWaitForResponse(Prompts.AT + Prompts.HTTPACTION + method.ToString(), 6000);

                default:
                    return new AtCommandResult(ReturnedState.InvalidCommand);
            }
        }

        /// <summary>
        /// Saves HTTP application context. When system is rebooted the parameters will be loaded automatically
        /// </summary>
        /// <returns></returns>
        public AtCommandResult SaveHttpContext()
        {
            return SIM800H.Instance.SendATCommand(Prompts.AT + Prompts.HTTPSCONT);
        }

        /// <summary>
        /// Requests information about the current <see cref="HttpCurrentStatus"/> that will raise an event that contains the HTTP service current status.
        /// </summary>
        /// <returns></returns>
        internal AtCommandResult ReadHttpStatus()
        {
            return SIM800H.Instance.SendATCommand(Prompts.AT + Prompts.HTTPSTATUS);
        }

        /// <summary>
        /// Enable/disable SSL for HTTP
        /// </summary>
        /// <param name="enable"></param>
        /// <returns></returns>
        internal AtCommandResult EnableSsl(bool enable)
        {
            return SIM800H.Instance.SendATCommandAndWaitForResponse(Prompts.AT + Prompts.HTTPSSL + (enable ? "1" : "0") , 5000);
        }

        /// <summary>
        /// Performs an asynchronous HttpWebrequest
        /// </summary>
        /// <param name="request">The <c>HttpWebRequest</c> request to be performed</param>
        /// <param name="readResponseData">Option to read the response data if any, optional with false as default</param>
        /// <param name="closeConnectionOnCompletion">Option to close the connection when HTTP request is completed, optional with true as default</param>
        /// <param name="readHeaders">Option to read the response headers, if any, optional with false as default</param>
        /// <param name="readTimeout">Timeout (in milliseconds) to complete a read operation. This is used in HTTP GET operations and when read response headers option is enabled. The default is 5s.</param>
        /// <param name="asyncCallback">The callback to be invoked upon completion, optional</param>
        /// <param name="asyncState">The state object to be stored against the ReadSMSAsyncResult, optional</param>
        /// <returns>The IMEIAsyncResult</returns>
        public HttpWebRequestAsyncResult PerformHttpWebRequestAsync(HttpWebRequest request, bool readResponseData = false, bool readHeaders = false, bool closeConnectionOnCompletion = true, int readTimeout = 5000, AsyncCallback asyncCallback = null, object asyncState = null)
        {
            return new HttpWebRequestAsyncResult(request, readResponseData, readHeaders, closeConnectionOnCompletion, readTimeout, asyncCallback, asyncState);
        }

        #region Delegates and events

        #region HTTPAction Received

        /// <summary>
        /// Represents the delegate used for the <see cref="HttpActionReceived"/> event.
        /// </summary>
        /// <param name="actionResult">HTTP action result of the request</param>
        public delegate void HttpActionReceivedHandler(HttpActionResult actionResult);
        /// <summary>
        /// Event raised when the module receives an HTTP action prompt.
        /// </summary>
        public event HttpActionReceivedHandler HttpActionReceived;
        private HttpActionReceivedHandler onHttpActionReceived;

        /// <summary>
        /// Raises the <see cref="HttpActionReceived"/> event.
        /// </summary>
        /// <param name="actionResult">HTTP action result of the request</param>
        internal virtual void OnHttpActionReceived(HttpActionResult actionResult)
        {
            if (onHttpActionReceived == null) onHttpActionReceived = new HttpActionReceivedHandler(OnHttpActionReceived);
            if (HttpActionReceived != null)
            {
                HttpActionReceived(actionResult);
            }
        }

        #endregion

        #endregion

    }
}
