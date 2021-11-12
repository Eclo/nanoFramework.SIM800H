using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Eclo.nanoFramework.SIM800H
{
    /// <summary>
    /// SMS client class with methods to send and read SMSs.
    /// </summary>
    public class SmsProvider : IDisposable
    {
        internal SmsProvider()
        {
            // AT command with all configuration required for SMSs
            // CNMI=<mode>,<mt>,<bm>,<ds>,<bfr>
            // <mode> = 2 : buffer unsolicited result codes to TA
            // <mt> = 1 : no delivery reports are routed to the terminal
            // <bm> = 0 : no CBMs are routed to terminal 
            // <ds> = 1 : SMS status reports are routed to terminal
            // <bfr> = 0 : unsolicited result codes are flushed to the terminal
            // SmsMessageFormat in text mode
            // Set all SMS's to be stored in the SIM card
            // NewSmsMessageIndication
            string atCommand = Prompts.AT + "+CNMI=2,1,0,1,0;+CMGF=1;+CSCS=\"GSM\";" + Prompts.CPMS + "=\"SM\",\"SM\",\"SM\";&W0";

            var ret = SIM800H.Instance.SendATCommandAndWaitForResponse(atCommand, 6000);
            if (ret.Result != ReturnedState.OK)
            {
                // give it another try
                Thread.Sleep(2000);

                ret = SIM800H.Instance.SendATCommandAndWaitForResponse(atCommand, 6000);
                if (ret.Result != ReturnedState.OK)
                {
                    //Debug.WriteLine("failed to set SMS storage");
                }
            }

            // dispose vars
            atCommand = null;
            ret = null;
        }

        #region Disposable implementation

        ~SmsProvider()
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
        /// Starts an asynchronous operation to send a text message
        /// </summary>
        /// <param name="destinationNumber">Destination phone number. Numbers only, no spaces, punctuation or other chars accepted.
        /// Number MUST ALWAYS include the country code preceded with international prefix '+' or '00'.</param>
        /// <param name="text">Message text</param>
        /// <param name="asyncCallback">The callback to be invoked upon completion, optional</param>
        /// <param name="asyncState">The state object to be stored against the SendMessageAsyncResult, optional.</param>
        /// <returns>The IMEIAsyncResult</returns>
        public SendTextMessageAsyncResult SendTextMessageAsync(string destinationNumber, string text, AsyncCallback asyncCallback = null, object asyncState = null)
        {
            return new SendTextMessageAsyncResult(destinationNumber, text, asyncCallback, asyncState);
        }

        /// <summary>
        /// Requests to read the text message in the specified position.
        /// </summary>
        /// <param name="memoryPosition">Position in memory where the message is stored</param>
        /// <param name="markAsRead">Whether unread messages will be marked as read</param>
        /// <returns></returns>
        public TextMessage ReadTextMessage(int memoryPosition, bool markAsRead = true)
        {

            // build AT command to read msg
            // 'mode' argument is: 
            // 1 - don't change msg status of the record
            // 0 - the record status will change to 'received read' if it's 'received unread'
            AtCommandResult readMessage = SIM800H.Instance.SendATCommandAndWaitForResponse(Prompts.AT + Prompts.CMGR + "=" + memoryPosition + 
                (markAsRead ? ",0" : ",1")
                , 6000);


            // check if command was executed
            if (readMessage.Result == ReturnedState.OK)
            {
                TextMessage sms = new TextMessage();
                try
                {
                    //Debug.WriteLine("---" + readMessage.Response);
                    // check if response is empty
                    if (readMessage.Response.IndexOf(Prompts.CMGR) > -1)
                    {
                        // clear message
                        string messageRaw = readMessage.Response.Substring(7);

                        // find first new line
                        int msgStartIndex = messageRaw.IndexOf("\n");

                        // get message
                        sms.Text = (messageRaw.Substring(msgStartIndex)).Trim();

                        // get other message details
                        string[] messageDetails = messageRaw.Substring(0, msgStartIndex).Split(new char[] { ',' });

                        if (messageDetails.Length == 5)
                        {
                            // Get number
                            sms.TelephoneNumber = messageDetails[1].Trim('\"');

                            // Get status
                            if (messageDetails[0].IndexOf("REC UNREAD") > 0)
                            {
                                sms.Status = MessageState.ReceivedUnread;
                            }
                            else if (messageDetails[0].IndexOf("REC READ") > 0)
                            {
                                sms.Status = MessageState.ReceivedRead;
                            }
                            else if (messageDetails[0].IndexOf("STO UNSENT") > 0)
                            {
                                sms.Status = MessageState.StoredUnsent;
                            }
                            else if (messageDetails[0].IndexOf("STO SENT") > 0)
                            {
                                sms.Status = MessageState.StoredSent;
                            }
                            else
                            {
                                // ERROR
                                sms.Status = MessageState.All;
                            }

                            // Get time stamp

                            if ((messageDetails[3].Length < 7) || (messageDetails[4].Length < 7))
                            {
                                sms.Timestamp = new DateTime();
                            }
                            else
                            {

                                DateTime timestamp = new DateTime(int.Parse(messageDetails[3].Substring(1, 2)) + 2000, //Year
                                int.Parse(messageDetails[3].Substring(4, 2)), //Month
                                int.Parse(messageDetails[3].Substring(7, 2)), // Day
                                int.Parse(messageDetails[4].Substring(0, 2)), // Hour
                                int.Parse(messageDetails[4].Substring(3, 2)), // Minute
                                int.Parse(messageDetails[4].Substring(6, 2))); // Second
                                sms.Timestamp = timestamp;

                                sms.Index = memoryPosition;

                                return sms;
                            }
                        }
                    }
                    else
                    {

                    }
                }
                catch (Exception)
                {
                }
            }

            return new TextMessage() { Status = MessageState.Error };
        }

        /// <summary>
        /// Starts an asynchronous operation to get a list with indexes of text messages that match the <see cref="MessageState"/> criteria
        /// </summary>
        /// <param name="state"><see cref="MessageState"/> of the SMS that will be queried</param>
        /// <param name="asyncCallback">The callback to be invoked upon completion, optional</param>
        /// <param name="asyncState">The state object to be stored against the ReadMessageListAsyncResult, optional</param>
        /// <returns>The IMEIAsyncResult</returns>
        public ListTextMessagesAsyncResult ListTextMessagesAsync(MessageState state, AsyncCallback asyncCallback = null, object asyncState = null)
        {
            return new ListTextMessagesAsyncResult(state, asyncCallback, asyncState);
        }

        /// <summary>
        /// Delete a text message
        /// </summary>
        /// <param name="position">Position in memory where the message is stored</param>
        public AtCommandResult DeleteTextMessage(int position)
        {
            string atCommand = Prompts.AT + Prompts.CMGD + "=" + position;

            var ret = SIM800H.Instance.SendATCommand(atCommand, 6000);
            if (ret.Result != ReturnedState.OK)
            {
                // retry
                Thread.Sleep(250);

                ret = SIM800H.Instance.SendATCommand(atCommand, 6000);
            }

            return ret;
        }

        /// <summary>
        /// Deletes text messages according to delete option
        /// </summary>
        /// <param name="deleteOption">option to delete messages that match a criteria or are at specific storage</param>
        /// <returns></returns>
        public AtCommandResult DeleteTextMessages(MessageDeleteOption deleteOption)
        {
            string atCommand = Prompts.AT + Prompts.CMGDA + @"=""DEL ";

            switch (deleteOption)
            {
                case MessageDeleteOption.All:
                    return SIM800H.Instance.SendATCommand(atCommand + @"ALL""", 2000);

                case MessageDeleteOption.Read:
                    return SIM800H.Instance.SendATCommand(atCommand + @"READ""", 2000);

                case MessageDeleteOption.Unread:
                    return SIM800H.Instance.SendATCommand(atCommand + @"UNREAD""", 2000);

                case MessageDeleteOption.Sent:
                    return SIM800H.Instance.SendATCommand(atCommand + @"SENT""", 2000);

                case MessageDeleteOption.Unsent:
                    return SIM800H.Instance.SendATCommand(atCommand + @"UNSENT""", 2000);

                case MessageDeleteOption.Inbox:
                    return SIM800H.Instance.SendATCommand(atCommand + @"INBOX""", 2000);

                default:
                    return new AtCommandResult(ReturnedState.InvalidCommand);
            }
        }

        #region SMS Received

        /// <summary>
        /// Represents the delegate used for the <see cref="SmsReceived"/> event.
        /// </summary>
        /// <param name="messageIndex">Position index of the SMS message that has arrived</param>
        public delegate void SmsReceivedHandler(byte messageIndex);
        /// <summary>
        /// Event raised when the module receives a new SMS message.
        /// </summary>
        public event SmsReceivedHandler SmsReceived;
        private SmsReceivedHandler onSmsReceived;

        /// <summary>
        /// Raises the <see cref="SmsReceived"/> event.
        /// </summary>
        /// <param name="messageIndex">Index of the received message</param>
        internal virtual void OnSmsReceived(byte messageIndex)
        {
            if (onSmsReceived == null) onSmsReceived = new SmsReceivedHandler(OnSmsReceived);
            if (SmsReceived != null)
            {
                SmsReceived(messageIndex);
            }
        }

        #endregion

        #region SMS Status Received

        /// <summary>
        /// Represents the delegate used for the <see cref="SmsStatusReceived"/> event.
        /// </summary>
        /// <param name="status">Status report for SMS message that has arrived</param>
        public delegate void SmsStatusReceivedHandler(MessageStatusReport status);
        /// <summary>
        /// Event raised when the module receives a new SMS message.
        /// </summary>
        public event SmsStatusReceivedHandler SmsStatusReceived;
        private SmsStatusReceivedHandler onSmsStatusReceived;

        /// <summary>
        /// Raises the <see cref="SmsStatusReceived"/> event.
        /// </summary>
        /// <param name="status">Status report for SMS message that has arrived</param>
        internal virtual void OnSmsStatusReceived(MessageStatusReport status)
        {
            if (onSmsStatusReceived == null) onSmsStatusReceived = new SmsStatusReceivedHandler(SmsStatusReceived);
            if (SmsStatusReceived != null)
            {
                SmsStatusReceived(status);
            }
        }

        #endregion

    }
}
