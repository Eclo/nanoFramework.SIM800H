using System;
using System.Collections;
using System.Text;
using System.Threading;
using Eclo.nF.Extensions;
using Windows.Storage.Streams;

namespace Eclo.nanoFramework.SIM800H
{
    /// <summary>
    /// An asynchronous result object returning the result of an HTTP Request
    /// </summary>
    public class HttpWebRequestAsyncResult : DeviceAsyncResult
    {
        /// <summary>
        /// The device HTTP response object after the call is completed.
        /// <remarks>RequestSuccessful property is false if request couldn't be completed for any reason</remarks>
        /// </summary>
        public HttpWebResponse HttpResponse = new HttpWebResponse(false);

        HttpWebRequest _httpRequest;
        HttpActionResult _actionResult;

        private bool _readResponseData;
        private bool _readResponseHeaders;
        private int _readTimeout;

        public HttpWebRequestAsyncResult(HttpWebRequest request, bool readResponseData = false, bool readResponseHeaders = false, bool closeConnectionOnCompletion = true, int readTimeout = 5000, AsyncCallback asyncCallback = null, object asyncState = null)
            : base(asyncCallback, asyncState)
        {
            _httpRequest = request;
            _readResponseData = readResponseData;
            _readResponseHeaders = readResponseHeaders;
            _readTimeout = readTimeout;
            _closeConnectionOnCompletion = closeConnectionOnCompletion;
        }

        /// <summary>
        /// Finishes the asynchronous processing and throws an exception if one was generated
        /// <remarks>Blocks until the asynchronous processing has completed</remarks>
        /// </summary>
        /// <returns>Returns the SMS </returns>
        public new HttpWebResponse End()
        {
            base.End();

            return HttpResponse;
        }

        /// <summary>
        /// The method used to perform the asynchronous processing
        /// </summary>
        public override void Process()
        {
            Exception caughtException = null;
            Eclo.nanoFramework.SIM800H.AtCommandResult calRet;
            int index;
            string command = string.Empty;
            int contentLenght = 0;

            try
            {
                // check GPRS context
                if (!SIM800H.GprsIpAppsBearerIsOpen)
                {
                    // not open, try to open it
                    SIM800H.GprsProvider.OpenBearerAsync(BearerProfile.IpAppsBearer);
                    // wait here for 5 seconds and check again
                    Thread.Sleep(5000);

                    if (!SIM800H.GprsIpAppsBearerIsOpen)
                    {
                        // TBD
                        // probably should throw an exception
                        return;
                    }
                }

                // init HTTP service
                command = Prompts.AT + Prompts.HTTPINIT;
                calRet = SIM800H.Instance.SendATCommandAndWaitForResponse(command, 5000);
                // wait for return
                if (calRet.Result != ReturnedState.OK)
                {
                    // give it another try
                    Thread.Sleep(250);

                    calRet = SIM800H.Instance.SendATCommandAndWaitForResponse(command, 5000);
                    // wait for return
                    if (calRet.Result != ReturnedState.OK)
                    {
                        // TBD
                        // probably should throw an exception
                        return;
                    }
                }


                // set GPRS context fro HttpRequest
                calRet = SIM800H.HttpClient.SetHttpParameter(HttpParamTag.CID, 1);
                if (calRet.Result != ReturnedState.OK)
                {
                    // TBD
                    return;
                }

                // set automatic redirection
                calRet = SIM800H.HttpClient.SetHttpParameter(HttpParamTag.REDIR, 1);
                if (calRet.Result != ReturnedState.OK)
                {
                    // TBD
                    return;
                }


                // set HTTP parameters as they are in the HttpRequest

                // content type, if set
                if (_httpRequest.ContentType != null && _httpRequest.ContentType != "")
                {
                    calRet = SIM800H.HttpClient.SetHttpParameter(HttpParamTag.CONTENT, _httpRequest.ContentType);
                    // wait for return
                    if (calRet.Result != ReturnedState.OK)
                    {
                        return;
                    }
                }

                // user agent, if set
                if (_httpRequest.UserAgent != null && _httpRequest.UserAgent != "")
                {
                    calRet = SIM800H.HttpClient.SetHttpParameter(HttpParamTag.UA, _httpRequest.UserAgent);
                    if (calRet.Result != ReturnedState.OK)
                    {
                        return;
                    }
                }
                else
                {
                    // don't bother to check return
                    calRet = SIM800H.HttpClient.SetHttpParameter(HttpParamTag.UA, "ECLO_IoTM_v1");
                }

                // custom headers
                if (_httpRequest.Headers != null && _httpRequest.Headers.Count > 0)
                {
                    // build string with headers
                    StringBuilder customHeaders = new StringBuilder();

                    foreach (DictionaryEntry header in _httpRequest.Headers._headers)
                    {
                        customHeaders.Append(header.Key);
                        customHeaders.Append(":");
                        customHeaders.Append(header.Value);
                        // always add terminator, will remove the last afterwards
                        customHeaders.Append(@"\r\n");
                    }

                    // escape the '"' char in the header fields 
                    customHeaders.Replace(@"""", @"\""");

                    // set custom headers removing last '|' separator
                    calRet = SIM800H.HttpClient.SetHttpParameter(HttpParamTag.USERDATA, customHeaders.ToString().Substring(0, customHeaders.Length - 4));
                }
                else
                {
                    // clear user headers, just in case
                    calRet = SIM800H.HttpClient.SetHttpParameter(HttpParamTag.USERDATA, " ");
                }
                // wait for result
                if (calRet.Result != ReturnedState.OK)
                {
                    return;
                }

                // set HTTPS preference
                calRet = SIM800H.HttpClient.EnableSsl(_httpRequest.RequestUri.Scheme == "https");
                if (calRet.Result != ReturnedState.OK)
                {
                    return;
                }

                // set request URL
                calRet = SIM800H.HttpClient.SetHttpParameter(HttpParamTag.URL, _httpRequest.RequestUri.AbsoluteUri);
                if (calRet.Result != ReturnedState.OK)
                {
                    return;
                }

                // send POST data
                // check method
                // check also if there is data in the Data field or in the request stream of the caller
                if (_httpRequest.Method == "POST" &&
                    ((_httpRequest.Data != null && _httpRequest.Data != string.Empty) ||
                    (_httpRequest._requestStream != null &&_httpRequest._requestStream.Length > 0)))
                {
                    // compute content length

                    if (_httpRequest._requestStream != null)
                    {
                        contentLenght = (int)_httpRequest._requestStream.Length;
                    }
                    else if (_httpRequest.Data != null || _httpRequest.Data != string.Empty)
                    {
                        contentLenght = _httpRequest.Data.Length;
                    }

                    // send request to upload content data
                    // the timeout depends on the data length
                    int timeout = (contentLenght / 100) * 500;
                    // min allowed timeout is 1000
                    timeout = timeout < 1000 ? 2000 : timeout;
                    // max allowed timeout is 120000
                    command = Prompts.AT + Prompts.HTTPDATA + contentLenght.ToString() + "," + (timeout > 120000 ? 120000 : timeout);
                    calRet = SIM800H.Instance.SendATCommandAndWaitForResponse(command, 1000);

                    if (calRet.Result == ReturnedState.OK && calRet.Response == Prompts.DonwloadPrompt)
                    {
                        // reset event
                        SIM800H.Instance.responseReceived.Reset();
                        // set flag
                        SIM800H.Instance.waitingResponse = true;

                        // write data in chunks of 64 bytes because of UART buffer size
                        index = 0;
                        int chunkSize = 64;

                        if (_httpRequest._requestStream != null)
                        {
                            // caller has filled request stream

                            IBuffer buffer = (IBuffer)new Http.HttpByteBuffer(64);

                            // reset position of stream to start reading from the beginning
                            _httpRequest._requestStream.Seek(0);

                            while (index < (int)_httpRequest._requestStream.Length)
                            {
                                // adjust chunk size
                                chunkSize = System.Math.Min(chunkSize, (int)_httpRequest._requestStream.Length - index);

                                // read chunk
                                _httpRequest._requestStream.Read(buffer, (uint)chunkSize, Windows.Storage.Streams.InputStreamOptions.Partial);

                                // check if UART TX buffer needs to be flushed
                                /*TBD                                if (SIM800H.Instance._serialDevice.BytesToWrite > chunkSize)
                                                                {
                                                                    SIM800H.Instance._serialDevice.Flush();
                                                                    //Console.WriteLine("*"); 
                                                                }*/

                                // send chunk writing directly to UART
                                SIM800H.Instance._serialDevice.WriteBytes(((Http.HttpByteBuffer)buffer).Data, 0, chunkSize);

                                //Console.WriteLine("w " + _httpRequest._requestStream.Position); 

                                // update index
                                index += chunkSize;
                            }
                        }
                        else if (_httpRequest.Data != null || _httpRequest.Data != string.Empty)
                        {
                            // caller has filled Data field

                            while (index < _httpRequest.Data.Length)
                            {
                                // adjust chunk size
                                chunkSize = System.Math.Min(chunkSize, _httpRequest.Data.Length - index);

                                // check if UART TX buffer needs to be flushed
/*TBD                                if (SIM800H.Instance._serialDevice.BytesToWrite > chunkSize)
                                {
                                    SIM800H.Instance._serialDevice.Flush();
                                    //Console.WriteLine("*"); 
                                }*/

                                // send chunk writing directly to UART
                                SIM800H.Instance._serialDevice.Write(_httpRequest.Data.Substring(index, chunkSize));

                                // update index
                                index += chunkSize;
                            }
                        }
                    }
                    else
                    {
                        // no DONWLOAD prompt
                        return;
                    }
                }

                // set event handler to receive notification of HTTP action received
                SIM800H.HttpClient.HttpActionReceived += Owner_HTTPActionReceived;

                // set the HTTP action which performs the request
                calRet = SIM800H.HttpClient.SetHttpActionMethod(_httpRequest._method);
                if (calRet.Result != ReturnedState.OK)
                {
                    return;
                }

                // this is the timeout for the loop to complete, it has to depend on the content length, with a minimum of 15 seconds

                int milisecondsTimeout = (int)(contentLenght > 5000 ? contentLenght * 1.6 : 15000);
                const int loopWaitTime = 100;

                // adjust loop global timeout if read timeout is greater
                if (_readTimeout > milisecondsTimeout)
                {
                    // read timeout + headers read timeout + change
                    milisecondsTimeout = 5000 + _readTimeout + 3000;
                }

                // wait for HTTPACTION response
                while (milisecondsTimeout > 0)
                {
                    // timeout for next iteration
                    milisecondsTimeout = milisecondsTimeout - loopWaitTime;

                    // any response?
                    if (_actionResult != null)
                    {
                        // check if action matches our request
                        if (_actionResult.Action == _httpRequest._method)
                        {
                            // handle result 
                            // any data to read and we are to read response data?
                            if (_actionResult.DataLenght > 0 && _readResponseData)
                            {
                                var readExecution = SIM800H.Instance.SendATCommandAndWaitForResponse(Prompts.AT + Prompts.HTTPREAD, _readTimeout);

                                if (readExecution.Result == ReturnedState.OK)
                                {
                                    // try to parse HTTP response data
                                    try
                                    {
                                        // to get data from response string need to start after 1st line and strip it
                                        HttpResponse = new HttpWebResponse(true, (HttpStatusCode)_actionResult.StatusCode, readExecution.Response);
                                    }
                                    catch (Exception)
                                    {
                                        return;
                                    }
                                }
                                else
                                {
                                    return;
                                }
                            }
                            else
                            {
                                // no data to read or we are not to read it, set response
                                HttpResponse = new HttpWebResponse(true, (HttpStatusCode)_actionResult.StatusCode);
                            }

                            // should read response headers
                            if (_readResponseHeaders)
                            {
                                var readExecution = SIM800H.Instance.SendATCommandAndWaitForResponse(Prompts.AT + Prompts.HTTPHEAD, 5000);

                                if (readExecution.Result == ReturnedState.OK)
                                {
                                    // try to parse HTTP headers
                                    try
                                    {
                                        string[] headerList = readExecution.Response.Split(new char[] { '\r', '\n' });
                                        int separatorIndex;

                                        foreach (string header in headerList)
                                        {
                                            // find first ':'
                                            separatorIndex = header.IndexOf(':');

                                            if (separatorIndex > -1)
                                            {
                                                HttpResponse.Headers.Add(header.Substring(0, separatorIndex), header.Substring(separatorIndex + 2));
                                            }
                                        }
                                    }
                                    catch (Exception)
                                    {
                                        return;
                                    }
                                }
                                else
                                {
                                    return;
                                }

                                // done here
                                return;
                            }
                            else
                            {
                                // done here
                                break;
                            }
                        }
                    }

                    // sleep
                    Thread.Sleep(loopWaitTime);
                }

            }
            catch (Exception exception)
            {
                caughtException = exception;
            }
            finally
            {
                // remove HTTP action handler 
                SIM800H.HttpClient.HttpActionReceived -= Owner_HTTPActionReceived;

                // terminate HTTP service
                SIM800H.Instance.SendATCommandAndWaitForResponse(Prompts.AT + Prompts.HTTPTERM, 5000);

                // if IP bearer was opened and there is a request to close it, now it's the time
                if (SIM800H.GprsIpAppsBearerIsOpen && _closeConnectionOnCompletion)
                {
                    // try to close it
                    SIM800H.GprsProvider.CloseBearer(BearerProfile.IpAppsBearer);
                }

                Complete(caughtException);
            }
        }

        void Owner_HTTPActionReceived(HttpActionResult actionResult)
        {
            _actionResult = actionResult;
        }

        public bool _closeConnectionOnCompletion { get; set; }
    }
}
