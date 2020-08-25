using System;
using System.Threading;

namespace Eclo.nanoFramework.SIM800H
{ 
    /// <summary>
    /// An asynchronous result object returning the result of a request to open a bearer in GPRS context
    /// </summary>
    public class OpenBearerAsyncResult : DeviceAsyncResult
    {
        /// <summary>
        /// Result of open bearer context 
        /// </summary>
        public OpenBearerResult Result = OpenBearerResult.Failed;

        private BearerProfile profile = BearerProfile.IpAppsBearer;

        internal OpenBearerAsyncResult(BearerProfile profile, AsyncCallback asyncCallback = null, object asyncState = null)
            : base(asyncCallback, asyncState)
        {
            this.profile = profile;
        }

        /// <summary>
        /// Finishes the asynchronous processing and throws an exception if one was generated
        /// <remarks>Blocks until the asynchronous processing has completed</remarks>
        /// </summary>
        /// <returns>Returns the result of the request to open bearer context </returns>
        public new OpenBearerResult End()
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

            try
            {
                // check if bearer is already open
                switch (profile)
                {
                    case BearerProfile.SocketsBearer:
                        if (SIM800H.GprsSocketsBearerIsOpen)
                        {
                            // context already opened
                            Result = OpenBearerResult.AlreadyOpen;
                            // done here
                            return;
                        }
                        break;

                    case BearerProfile.IpAppsBearer:
                        if (SIM800H.GprsIpAppsBearerIsOpen)
                        {
                            // context already opened
                            Result = OpenBearerResult.AlreadyOpen;
                            // done here
                            return;
                        }
                        break;

                    case BearerProfile.MmsBearer:
                        if (SIM800H.GprsMmsBearerIsOpen)
                        {
                            // context already opened
                            Result = OpenBearerResult.AlreadyOpen;
                            // done here
                            return;
                        }
                        break;
                }

                // send command to open bearer context
                Eclo.nanoFramework.SIM800H.AtCommandResult calRet = SIM800H.Instance.SendATCommand(Prompts.AT + Prompts.SAPBR + "=1," + profile.ToString(), 2000);
                if (calRet.Result == ReturnedState.OK)
                {
                    // request to open GPRS context successful, start loop with timeout to query context to verify if/when open is successful
                    int milisecondsTimeout = 10000;
                    const int loopWaitTime = 500;

                    while (milisecondsTimeout > 0)
                    {
                        // timeout for next iteration
                        milisecondsTimeout = milisecondsTimeout - loopWaitTime;

                        calRet = SIM800H.GprsProvider.CheckBearerStatus(profile);
                        if (calRet.Result == Eclo.nanoFramework.SIM800H.ReturnedState.OK)
                        {
                            switch (profile)
                            {
                                case BearerProfile.SocketsBearer:
                                    if (SIM800H.GprsSocketsBearerIsOpen)
                                    {
                                        // bearer is connected

                                        // update result
                                        Result = OpenBearerResult.Open;

                                        // done here
                                        return;
                                    }
                                    break;

                                case BearerProfile.IpAppsBearer:
                                    if (SIM800H.GprsIpAppsBearerIsOpen)
                                    {
                                        // bearer is connected

                                        // update result
                                        Result = OpenBearerResult.Open;

                                        // done here
                                        return;
                                    }
                                    break;

                                case BearerProfile.MmsBearer:
                                    if (SIM800H.GprsMmsBearerIsOpen)
                                    {
                                        // bearer is connected

                                        // update result
                                        Result = OpenBearerResult.Open;

                                        // done here
                                        return;
                                    }
                                    break;
                            }

                        }
                        else if (calRet.Result == ReturnedState.DeviceIsOff)
                        {
                            // module is off
                            Result = OpenBearerResult.DeviceIsOff;

                            // done here
                            return;
                        }

                        // sleep
                        Thread.Sleep(loopWaitTime);
                    }

                    // got here so operation has timed out
                    Result = OpenBearerResult.Failed;
                }
                else
                {
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
    }
}
