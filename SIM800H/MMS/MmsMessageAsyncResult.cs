////
// Copyright (c) Eclo Solutions
// See LICENSE file in the project root for full license information.
////

using System;
using System.Diagnostics;
using System.Threading;
using Eclo.nF.Extensions;

namespace Eclo.nanoFramework.SIM800H
{
    /// <summary>
    /// An asynchronous result object returning the outcome of an attempt to send a MMS message.
    /// </summary>
    public class SendMmsMessageAsyncResult : DeviceAsyncResult
    {
        /// <summary>
        /// Result of the MMS send operation.
        /// <remarks>Result property is false if request couldn't be completed for any reason. See error code for details.</remarks>
        /// </summary>
        public bool Result = false;

        /// <summary>
        /// Error code of the send request.
        /// </summary>
        public int ErrorCode
        {
            get;
            private set;
        }

        string _destination = string.Empty;
        MmsMessage _message;
        private bool _closeConnectionOnCompletion;

        public SendMmsMessageAsyncResult(string destination, MmsMessage message, bool closeConnectionOnCompletion = true, AsyncCallback asyncCallback = null, object asyncState = null)
            : base(asyncCallback, asyncState)
        {
            if (destination.IndexOf("@") > 0)
            {
                // its an email, take it as it is
                _destination = destination;
            }
            else
            {
                // cleanup the destination if number from unwanted chars
                foreach (char c in destination.ToCharArray())
                {
                    if ((_destination == string.Empty && c == '+') ||
                        (c >= 48 && c <= 57))
                    {
                        // accept + if it's the very first char
                        // otherwise numbers only
                        _destination += c;
                    }
                }
            }

            _message = message;
            _closeConnectionOnCompletion = closeConnectionOnCompletion;
        }

        /// <summary>
        /// Finishes the asynchronous processing and throws an exception if one was generated
        /// <remarks>Blocks until the asynchronous processing has completed</remarks>
        /// </summary>
        /// <returns>Returns the result of the request.</returns>
        public new bool End()
        {
            base.End();

            return Result;
        }

        /// <summary>
        /// The method used to perform the asynchronous processing
        /// </summary>
        public override void Process()
        {
            Exception caughtException = null;
            Eclo.nanoFramework.SIM800H.AtCommandResult sendMessage = null;
            int index;
            byte[] buffer = new byte[64];
            string command = string.Empty;
            int contentLenght = 0;
            int milisecondsTimeout;

            try
            {
                // enable verbose error message
                //SIM800H.Instance._serialLine.Write("AT+CMEE=2\r");

                // check GPRS context
                if (!SIM800H.GprsMmsBearerIsOpen)
                {
                    // process error and return
                    ErrorCode = 970;
                    goto send_error;
                }

                // initialize MMS service
                command = Prompts.AT + Prompts.CMMSINIT;
                sendMessage = SIM800H.Instance.SendATCommandAndWaitForResponse(command, 2000);
                if (sendMessage.Result != ReturnedState.OK)
                {
                    // process error and return
                    ErrorCode = 971;
                    goto send_error;
                }

                // start edit command
                command = Prompts.AT + Prompts.CMMSEDIT + "=1";
                sendMessage = SIM800H.Instance.SendATCommandAndWaitForResponse(command, 1000);
                if (sendMessage.Result == ReturnedState.Error && sendMessage.Response != string.Empty)
                {
                    // process error and return
                    ErrorCode = 901;
                    goto send_error;
                }
                else if(sendMessage.Result != ReturnedState.OK)
                {
                    goto send_error;
                }

                // Add recipient
                // send command
                command = Prompts.AT + Prompts.CMMSRECP + "=\"" + _destination + "\"";
                sendMessage = SIM800H.Instance.SendATCommandAndWaitForResponse(command, 2000);
                if (sendMessage.Result == ReturnedState.Error && sendMessage.Response != string.Empty)
                {
                    // process error and return
                    ErrorCode = 908;
                    goto send_error;
                }
                else if (sendMessage.Result != ReturnedState.OK)
                {
                    ErrorCode = 909;
                    goto send_error;
                }

                // just give it some milliseconds before next command
                Thread.Sleep(50);

                // do we have a Title?
                if (_message.Title != null && _message.Title != string.Empty)
                {
                    milisecondsTimeout = 2000;

                    // send command
                    command = Prompts.AT + Prompts.CMMSDOWN + "=\"TITLE\"," + _message.Title.Length.ToString() + "," + "5000";
                    sendMessage = SIM800H.Instance.SendATCommandAndWaitForResponse(command, 5000);

                    if (sendMessage.Result == ReturnedState.OK && sendMessage.Response == Prompts.CONNECT)
                    {
                        // reset event
                        SIM800H.Instance.responseReceived.Reset();
                        // set flag
                        SIM800H.Instance.waitingResponse = true;

                        // send title directly to UART
                        SIM800H.Instance._serialDevice.Write(_message.Title);
                    }
                    else if (sendMessage.Result == ReturnedState.Error && sendMessage.Response != string.Empty)
                    {
                        Debug.WriteLine("error: " + sendMessage.Response); 

                        // process error and return
                        ErrorCode = 902;
                        goto send_error;
                    }
                    else
                    {
                        ErrorCode = 905;
                        goto send_error;
                    }

                    // wait for module response 
                    while (milisecondsTimeout > 0)
                    {
                        if (SIM800H.Instance.responseReceived.WaitOne(250, false))
                        {
                            // need to lock queue because it can be changed on another thread
                            lock (SIM800H.Instance.responseQueue)
                            {
                                // check if there is any response available 
                                if (SIM800H.Instance.responseQueue.Count > 0)
                                {
                                    if (SIM800H.Instance.responseQueue.FindAndRemove(Prompts.OK) != null)
                                    {
                                        // OK prompt received, done here
                                        break;
                                    }

                                    // ERROR response
                                    if (SIM800H.Instance.responseQueue.FindAndRemove(Prompts.ERROR) != null)
                                    {
                                        // return ERROR 
                                        ErrorCode = 906;
                                        goto send_error;
                                    }
                                }
                            }
                        }

                        // loop this each 250ms
                        milisecondsTimeout = milisecondsTimeout - 250;
                    }

                    if (milisecondsTimeout == 0)
                    {
                        // if we reach here there was no response from module, something went wrong...
                        ErrorCode = 907;
                        goto send_error;
                    }
                }

                // do we have an image to send?
                if (_message.Image.Length > 0)
                {
                    contentLenght = (int)_message.Image.Length;
                    milisecondsTimeout = (contentLenght > 100 ? (contentLenght / 100) * 100 : 2000);

                    // send command
                    command = Prompts.AT + Prompts.CMMSDOWN + "=\"PIC\"," +
                        _message.Image.Length + "," + milisecondsTimeout.ToString();

                    sendMessage = SIM800H.Instance.SendATCommandAndWaitForResponse(command, 2000);

                    if (sendMessage.Result == ReturnedState.OK && sendMessage.Response == Prompts.CONNECT)
                    {
                        // reset event
                        SIM800H.Instance.responseReceived.Reset();
                        // set flag
                        SIM800H.Instance.waitingResponse = true;

                        // write data in chunks of 64 bytes because of UART buffer size
                        index = 0;
                        int chunkSize = 64;

                        while (index < _message.Image.Length)
                        {
                            // adjust chunk size
                            chunkSize = System.Math.Min(chunkSize, (int)_message.Image.Length - index);

                            // check if UART TX buffer needs to be flushed
/*TBD                            if (SIM800H.Instance._serialDevice.BytesToWrite > chunkSize)
                            {
                                SIM800H.Instance._serialDevice.Flush();
                                //Debug.WriteLine("*"); 
                            }*/

                            // read chunk and send it directly to UART
                            SIM800H.Instance._serialDevice.WriteBytes(_message.Image, index, chunkSize);
                            
                            // update index
                            index += chunkSize;
                        }
                    }
                    else if (sendMessage.Result == ReturnedState.Error && sendMessage.Response != string.Empty)
                    {
                        Debug.WriteLine("error: " + sendMessage.Response);

                        // process error and return
                        ErrorCode = 908;
                        goto send_error;
                    }
                    else
                    {
                        ErrorCode = 910;
                        goto send_error;
                    }

                    // wait for module response 
                    while (milisecondsTimeout > 0)
                    {
                        if (SIM800H.Instance.responseReceived.WaitOne(250, false))
                        {
                            // need to lock queue because it can be changed on another thread
                            lock (SIM800H.Instance.responseQueue)
                            {
                                // check if there is any response available 
                                if (SIM800H.Instance.responseQueue.Count > 0)
                                {
                                    if (SIM800H.Instance.responseQueue.FindAndRemove(Prompts.OK) != null)
                                    {
                                        // OK prompt received, done here
                                        break;
                                    }

                                    // ERROR response
                                    if (SIM800H.Instance.responseQueue.FindAndRemove(Prompts.ERROR) != null)
                                    {
                                        // return ERROR 
                                        ErrorCode = 912;
                                        goto send_error;
                                    }
                                }
                            }
                        }

                        // loop this each 250ms
                        milisecondsTimeout = milisecondsTimeout - 250;
                    }

                    if (milisecondsTimeout == 0)
                    {
                        // if we reach here there was no response from module, something went wrong...
                        ErrorCode = 914;
                        goto send_error;
                    }

                    // adjust send timeout according to image size
                    // set MMS send timeout
                    command = Prompts.AT + Prompts.CMMSTIMEOUT + "=" + (int)((_message.Image.Length * 1.6) / 1000) + ",20";
                    sendMessage = SIM800H.Instance.SendATCommandAndWaitForResponse(command, 2000);
                    if (sendMessage.Result != ReturnedState.OK)
                    {
                        // process error and return
                        throw new ArgumentNullException("MMS configuration failed");
                    }
                }
                else
                {
                    // no image so send timeout can be shorter
                    // set MMS send timeout to default 20 seconds
                    command = Prompts.AT + Prompts.CMMSTIMEOUT + "=20,20";
                    sendMessage = SIM800H.Instance.SendATCommandAndWaitForResponse(command, 2000);
                    if (sendMessage.Result != ReturnedState.OK)
                    {
                        // process error and return
                        throw new ArgumentNullException("MMS configuration failed");
                    }
                }

                // do we have Text?
                if (_message.Text != null && _message.Text != string.Empty)
                {
                    contentLenght = (int)_message.Text.Length;
                    milisecondsTimeout = (contentLenght > 100 ? (contentLenght / 100) * 100 : 2000);

                    // send command
                    command = Prompts.AT + Prompts.CMMSDOWN + "=\"TEXT\"," + _message.Text.Length.ToString() + "," + milisecondsTimeout.ToString();
                    sendMessage = SIM800H.Instance.SendATCommandAndWaitForResponse(command, 2000);

                    if (sendMessage.Result == ReturnedState.OK && sendMessage.Response == Prompts.CONNECT)
                    {
                        // reset event
                        SIM800H.Instance.responseReceived.Reset();
                        // set flag
                        SIM800H.Instance.waitingResponse = true;

                        // write data in chunks of 64 bytes because of UART buffer size
                        index = 0;
                        int chunkSize = 64;

                        while (index < _message.Text.Length)
                        {
                            // adjust chunk size
                            chunkSize = System.Math.Min(chunkSize, (int)_message.Text.Length - index);

                            // read chunk and get UTF8 bytes from it
                            System.Text.Encoding.UTF8.GetBytes(_message.Text, index, chunkSize, buffer, 0);

                            // check if UART TX buffer needs to be flushed
/*TBD                            if (SIM800H.Instance._serialDevice.BytesToWrite > chunkSize)
                            {
                                SIM800H.Instance._serialDevice.Flush();
                                //Debug.WriteLine("*"); 
                            }*/

                            // send directly to UART
                            SIM800H.Instance._serialDevice.WriteBytes(buffer, 0, chunkSize);
                            
                            // update index
                            index += chunkSize;
                        }
                    }
                    else if(sendMessage.Result == ReturnedState.Error && sendMessage.Response != string.Empty)
                    {
                        Debug.WriteLine("error: " + sendMessage.Response);
                        
                        // process error and return
                        ErrorCode = 916;
                        goto send_error;
                    }
                    else
                    {
                        ErrorCode = 918;
                        goto send_error;
                    }

                    // wait for module response 
                    while (milisecondsTimeout > 0)
                    {
                        if (SIM800H.Instance.responseReceived.WaitOne(250, false))
                        {
                            // need to lock queue because it can be changed on another thread
                            lock (SIM800H.Instance.responseQueue)
                            {
                                // check if there is any response available 
                                if (SIM800H.Instance.responseQueue.Count > 0)
                                {
                                    if (SIM800H.Instance.responseQueue.FindAndRemove(Prompts.OK) != null)
                                    {
                                        // OK prompt received, done here
                                        break;
                                    }

                                    // ERROR response
                                    if (SIM800H.Instance.responseQueue.FindAndRemove(Prompts.ERROR) != null)
                                    {
                                        // return ERROR 
                                        ErrorCode = 912;
                                        goto send_error;
                                    }
                                }
                            }
                        }

                        // loop this each 250ms
                        milisecondsTimeout = milisecondsTimeout - 250;
                    }

                    if (milisecondsTimeout == 0)
                    {
                        // if we reach here there was no response from module, something went wrong...
                        ErrorCode = 914;
                        goto send_error;
                    }
                }

/////////////////////////

                 //View the information of sending MMS
                 //send command
                sendMessage = SIM800H.Instance.SendATCommandAndWaitForResponse(Prompts.AT + Prompts.CMMSVIEW, 1000);

                 //just give it some milliseconds before next command
                Thread.Sleep(50);

//////////////////////////

                // send MMS

                // send command
                command = Prompts.AT + Prompts.CMMSSEND;
                sendMessage = SIM800H.Instance.SendATCommandAndWaitForResponse(command, 21000);
                if (sendMessage.Result == ReturnedState.Error && sendMessage.Response != string.Empty)
                {
                    Debug.WriteLine("error: " + sendMessage.Response); 

                    // process error and return
                    ErrorCode = 920;
                    goto send_error;
                }
                else if (sendMessage.Result != ReturnedState.OK)
                {
                    ErrorCode = 922;
                    goto send_error;
                }

                // just give it some milliseconds before next command
                Thread.Sleep(50);

                // Exit from edit mode. This will cleared up MMS from buffer
                // send command
                command = Prompts.AT + Prompts.CMMSEDIT + "=0";
                sendMessage = SIM800H.Instance.SendATCommand(command, 5000);
                if (sendMessage.Result == ReturnedState.Error && sendMessage.Response != string.Empty)
                {
                    // process error and return
                    ErrorCode = 924;
                    goto send_error;
                }
                else if (sendMessage.Result != ReturnedState.OK)
                {
                    ErrorCode = 926;
                    goto send_error;
                }

                // set flag for request successful
                Result = true;
                
                // done here
                return;

                //////////////////////////////////////////////////////
                // error processing to reuse code
send_error:
                // try to save error code, if any
                try
                {
                    // check if there is anything in the Response worth parsing
                    if (sendMessage.Response != String.Empty)
                    {
                        ErrorCode = int.Parse(sendMessage.Response);
                    }
                }
                catch { }

                // done here
                return;
            }
            catch (Exception exception)
            {
                caughtException = exception;
            }
            finally
            {
                // dispose var
                sendMessage = null;
                buffer = null;
   
                // terminate MMS service
                command = Prompts.AT + Prompts.CMMSTERM;
                SIM800H.Instance.SendATCommand(command, 5000);

                // if IP bearer was opened and there is a request to close it, now it's the time
                if (SIM800H.GprsIpAppsBearerIsOpen && _closeConnectionOnCompletion)
                {
                    // try to close it
                    SIM800H.GprsProvider.CloseBearer(BearerProfile.MmsBearer);
                }

                Complete(caughtException);
            }
        }
    }
}
