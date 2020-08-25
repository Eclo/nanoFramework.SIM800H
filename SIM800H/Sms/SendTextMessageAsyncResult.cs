using System;
using System.Threading;

namespace Eclo.nanoFramework.SIM800H
{
    /// <summary>
    /// An asynchronous result object returning the result of an attempt to send an SMS
    /// </summary>
    public class SendTextMessageAsyncResult : DeviceAsyncResult
    {
        /// <summary>
        /// Reference of the sent SMS
        /// </summary>
        public int Reference = -1;

        string _destinationNumber = string.Empty;
        string _message;

        public SendTextMessageAsyncResult(string destinationNumber, string message, AsyncCallback asyncCallback = null, object asyncState = null)
            : base(asyncCallback, asyncState)
        {
            // cleanup the number from unwanted chars
            foreach(char c in destinationNumber.ToCharArray())
            {
                if((_destinationNumber == string.Empty && c == '+') ||
                    (c >= 48 && c <= 57))
                {
                    // accept + if it's the very first char
                    // otherwise numbers only
                    _destinationNumber += c;
                }
            }

            _message = message;
        }

        /// <summary>
        /// Finishes the asynchronous processing and throws an exception if one was generated
        /// <remarks>Blocks until the asynchronous processing has completed</remarks>
        /// </summary>
        /// <returns>Returns the sent SMS index</returns>
        public new int End()
        {
            base.End();

            return Reference;
        }

        /// <summary>
        /// The method used to perform the asynchronous processing
        /// </summary>
        public override void Process()
        {
            Exception caughtException = null;
            Eclo.nanoFramework.SIM800H.AtCommandResult sendMessage;

            try
            {
                // set event handler for Sms sent reference received
                SIM800H.SmsSentReferenceReceived += Owner_SmsSentReferenceReceived;

                // send command
                sendMessage = SIM800H.Instance.SendATCommandAndWaitForResponse(Prompts.AT + Prompts.CMGS + "=\"" + _destinationNumber + "\" ", 1000);

                if (sendMessage.Result == ReturnedState.OK && sendMessage.Response == Prompts.SendPrompt)
                {
                    // we have a send prompt
                    // send message text ended with CTRL+Z without wake-up char
                    sendMessage = SIM800H.Instance.SendATCommand(_message + (char)26, 500, false, true);

                    // this can take up to 60 seconds
                    int milisecondsTimeout = 60000;

                    // wait for +CMGS response
                    while (milisecondsTimeout > 0)
                    {
                        // loop this each 100ms
                        milisecondsTimeout = milisecondsTimeout - 500;

                        // any reference?
                        if (Reference != -1)
                        {
                            break;
                        }

                        // sleep for 500ms
                        Thread.Sleep(500);
                    }
                }
            }
            catch (Exception exception)
            {
                caughtException = exception;
            }
            finally
            {
                // remove SMS reference received handler
                SIM800H.SmsSentReferenceReceived -= Owner_SmsSentReferenceReceived;

                // dispose var
                sendMessage = null;
            }

            Complete(caughtException);
        }

        void Owner_SmsSentReferenceReceived(int reference)
        {
            Reference = reference;
        }
    }
}
