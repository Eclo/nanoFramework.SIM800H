////
// Copyright (c) Eclo Solutions
// See LICENSE file in the project root for full license information.
////

using System;
using System.Threading;

namespace Eclo.nanoFramework.SIM800H
{
    /// <summary>
    /// An asynchronous result object returning the result of a request to open a bearer in GPRS context
    /// </summary>
    public class SyncNetworkTimeAsyncResult : DeviceAsyncResult
    {
        string sntpServer = "";
        string utcOffsetCalculated = "";

        /// <summary>
        /// Result of SNTP sync operation
        /// </summary>
        public SyncResult Result = SyncResult.NotSet;

        internal SyncNetworkTimeAsyncResult(string sntpServer, TimeSpan utcOffset, AsyncCallback asyncCallback = null, object asyncState = null)
            : base(asyncCallback, asyncState)
        {
            this.sntpServer = sntpServer;

            // validate UTC offset
            if (utcOffset.Hours > 12
                || utcOffset.Hours <= -12
                || utcOffset.Hours >= 11 && utcOffset.Minutes > (59)
                || utcOffset.Hours <= -11 && utcOffset.Minutes > (59))
            {
                throw new ArgumentOutOfRangeException();
            }

            // SIM800 understands UTC time offset in quarters of hour, see NTP app note
            this.utcOffsetCalculated += (((utcOffset.Hours) * 4) + ((int)Math.Floor(utcOffset.Minutes / 15.0))).ToString("D2");
        }

        /// <summary>
        /// Finishes the asynchronous processing and throws an exception if one was generated
        /// <remarks>Blocks until the asynchronous processing has completed</remarks>
        /// </summary>
        /// <returns>Returns the result of the request to open bearer context </returns>
        public new SyncResult End()
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
            AtCommandResult ret;

            try
            {
                // check if context is open
                if (!SIM800H.GprsIpAppsBearerIsOpen)
                {
                    // context not opened
                    //Debug.WriteLine("failed: context not opened");

                    Result = SyncResult.ConnectionError;
                    return;
                }

                // set NTP server and time zone
                ret = SIM800H.Instance.SendATCommand(Prompts.AT + Prompts.CNTP + @"=""" + sntpServer + @"""," + utcOffsetCalculated, 2000);
                if (ret.Result != ReturnedState.OK)
                {
                    //Debug.WriteLine("failed to set SNTP request parameters");

                    Result = SyncResult.Error;
                    return;
                }

                // set event handler to receive result of SNTP sync request 
                SIM800H.SntpClient.SntpReplyReceived += Owner_SntpReplyReceived;

                // send command to request network time sync
                ret = SIM800H.Instance.SendATCommand(Prompts.AT + Prompts.CNTP, 15000);
                if (ret.Result == ReturnedState.OK)
                {
                    // request to open GPRS context successful, start loop with timeout to query context to verify if/when open is successful
                    int milisecondsTimeout = 10000;
                    const int loopWaitTime = 500;

                    while (milisecondsTimeout > 0)
                    {
                        // timeout for next iteration
                        milisecondsTimeout = milisecondsTimeout - loopWaitTime;

                        // any response?
                        if (Result != SyncResult.NotSet)
                        {
                            // valid return value
                            break;
                        }

                        // sleep
                        Thread.Sleep(loopWaitTime);
                    }
                }
            }
            catch (Exception exception)
            {
                caughtException = exception;
            }
            finally
            {
                Complete(caughtException);
            }
        }

        void Owner_SntpReplyReceived(SyncResult syncResult)
        {
            Result = syncResult;
        }
    }
}
