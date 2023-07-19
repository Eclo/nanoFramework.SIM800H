////
// Copyright (c) Eclo Solutions
// See LICENSE file in the project root for full license information.
////

using System;
using System.Runtime.CompilerServices;

namespace Eclo.nanoFramework.SIM800H
{
    /// <summary>
    /// Class with methods to perform MMS related actions.
    /// </summary>
    public class MmsClient : IDisposable
    {
        internal MmsClient()
        {
            //if (SIM800H.MmsConfiguration.MMSC == string.Empty)
            //{
            //    // can't do anything else without MMS config, better throw an exception here
            //    throw new ArgumentNullException("MMS configuration invalid or missing");
            //}

            AtCommandResult sendMessage;

            // initialize MMS service
            string command = Prompts.AT + Prompts.CMMSINIT;
            sendMessage = SIM800H.Instance.SendATCommandAndWaitForResponse(command, 2000);
            if (sendMessage.Result != ReturnedState.OK)
            {
                // process error and return
                throw new ArgumentNullException("MMS configuration failed");
            }

            // setup MMS parameters, if configuration exists
            // set the MMS center URL
            command = Prompts.AT + Prompts.CMMS + @"CURL=""" + SIM800H.MmsConfiguration.MMSC + "\"";
            sendMessage = SIM800H.Instance.SendATCommandAndWaitForResponse(command, 2000);
            if (sendMessage.Result != ReturnedState.OK)
            {
                // process error and return
                throw new ArgumentNullException("MMS configuration failed");
            }

            // set MMS to use bearer profile 2
            command = Prompts.AT + Prompts.CMMS + @"CID=2";
            sendMessage = SIM800H.Instance.SendATCommandAndWaitForResponse(command, 2000);
            if (sendMessage.Result != ReturnedState.OK)
            {
                // give it another try
                System.Threading.Thread.Sleep(2000);

                sendMessage = SIM800H.Instance.SendATCommandAndWaitForResponse(command, 2000);
                if (sendMessage.Result != ReturnedState.OK)
                {
                    throw new ArgumentNullException("MMS configuration failed");
                }
            }

            // set the IP address and port of MMS proxy
            command = Prompts.AT + Prompts.CMMSPROTO + "=\"" + SIM800H.MmsConfiguration.Proxy + "\"" + "," + SIM800H.MmsConfiguration.ProxyPort.ToString();
            sendMessage = SIM800H.Instance.SendATCommandAndWaitForResponse(command, 2000);
            if (sendMessage.Result != ReturnedState.OK)
            {
                throw new ArgumentNullException("MMS configuration failed");
            }

            // set the parameters for the MMS PDU to be sent
            // "valid" time (expiry time for message?!) set to 5 (maximum)
            // priority set to 0 (not set)
            // delivery report set to 0 (not requested)
            // receive report set to 0 (not requested)
            // send address visibility set to 0 (default)
            // MMS class set to 0 (not set)
            // subject control set to 2 (English character code)
            // notifyrsp set to 0 (waiting for HTTP response)
            command = Prompts.AT + Prompts.CMMSSENDCFG + "=" + "6,3,0,0,2,4,2,0";
            sendMessage = SIM800H.Instance.SendATCommandAndWaitForResponse(command, 2000);
            if (sendMessage.Result != ReturnedState.OK)
            {
                // command error
                throw new ArgumentNullException("MMS configuration failed");
            }

            // save MMS context
            command = Prompts.AT + Prompts.CMMSSCONT;
            SIM800H.Instance.SendATCommand(command, 2000);
            if (sendMessage.Result != ReturnedState.OK)
            {
                // command error
                throw new ArgumentNullException("MMS configuration failed");
            }

            // terminate MMS service
            command = Prompts.AT + Prompts.CMMSTERM;
            SIM800H.Instance.SendATCommand(command, 5000);
        }

        #region Disposable implementation

#pragma warning disable 1591 // disable warning for Missing XML comment
        ~MmsClient()
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
#pragma warning restore 1591

        #endregion

        /// <summary>
        /// Starts an asynchronous operation to send an MMS message.
        /// </summary>
        /// <param name="destination">Destination phone number or email. Numbers and normal chars only, no spaces, punctuation or other special chars accepted.
        /// Phone number MUST ALWAYS include the country code preceded with international prefix '+' or '00'.
        /// Email in valid format.</param>
        /// <param name="msg">MMS message object</param>
        /// <param name="closeConnectionOnCompletion">Option to close the connection when the MMS message is sent, optional with true as default</param>
        /// <param name="asyncCallback">The callback to be invoked upon completion, optional</param>
        /// <param name="asyncState">The state object to be stored against the SendMmsMessageAsyncResult, optional.</param>
        /// <returns>The SendMmsMessageAsyncResult with a </returns>
        public SendMmsMessageAsyncResult SendMmsMessageAsync(string destination, MmsMessage msg, bool closeConnectionOnCompletion = true, AsyncCallback asyncCallback = null, object asyncState = null)
        {
            // check if MMS bearer is opened
            if (SIM800H.GprsMmsBearerIsOpen)
            {
                return new SendMmsMessageAsyncResult(destination, msg, closeConnectionOnCompletion, asyncCallback, asyncState);
            }

            throw new Exception("MMS bearer is not connected");
        }
    }
}
