using System;
using System.Collections;

namespace Eclo.nanoFramework.SIM800H
{
    /// <summary>
    /// An asynchronous result object returning a list of text message indexes
    /// </summary>
    public class ListTextMessagesAsyncResult : DeviceAsyncResult
    {
        /// <summary>
        /// A <see cref="byte"/> list with the indexes of the messages that match the <see cref="MessageState"/> queried.
        /// <remarks>Returns an empty list if none couldn't be read or match the criteria</remarks>
        /// </summary>
        public ArrayList SMSList = new ArrayList();

        private MessageState _state;

        public ListTextMessagesAsyncResult(MessageState state, AsyncCallback asyncCallback = null, object asyncState = null)
            : base(asyncCallback, asyncState)
        {
            _state = state;
        }

        /// <summary>
        /// Finishes the asynchronous processing and throws an exception if one was generated.
        /// Returns a <see cref="byte"/> list with the indexes of the messages that match the <see cref="MessageState"/> queried.
        /// <remarks>Blocks until the asynchronous processing has completed</remarks>
        /// </summary>
        /// <returns></returns>
        public new ArrayList End()
        {
            base.End();

            return SMSList;
        }

        /// <summary>
        /// The method used to perform the asynchronous processing
        /// </summary>
        public override void Process()
        {
            Exception caughtException = null;

            try
            {
                // try read SMSs by index
                // valid indexes for SIM800H are from 1 to 30
                for (byte i = 1; i <= 30; i++)
                {
                    try
                    {
                        // try to read SMS at this index without marking it as read
                        var smsAttempt = SIM800H.SmsProvider.ReadTextMessage(i, false);

                        // any sms at this index?
                        if (smsAttempt.Index > -1)
                        {
                            // check type
                            if (_state == MessageState.All)
                            {
                                // reading ALL so add to list
                                SMSList.Add(i);
                            }
                            else
                            {
                                if (smsAttempt.Status == _state)
                                {
                                    // match search criteria so add to list
                                    SMSList.Add(i);
                                }
                            }
                        }
                    }
                    // don't care about errors and exceptions here
                    catch { };
                }

            }
            catch (Exception exception)
            {
                caughtException = exception;
            }

            Complete(caughtException);
        }
    }
}
