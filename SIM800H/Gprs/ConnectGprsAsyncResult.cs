using System;
using System.Threading;

namespace Eclo.nanoFramework.SIM800H
{
    /// <summary>
    /// An asynchronous result returning the result of a request to open a GPRS connection
    /// </summary>
    public class ConnectGprsAsyncResult : DeviceAsyncResult
    {
        /// <summary>
        /// Result of GPRS connection 
        /// </summary>
        public ConnectGprsResult Result = ConnectGprsResult.Failed;

        internal ConnectGprsAsyncResult(AsyncCallback asyncCallback = null, object asyncState = null)
            : base(asyncCallback, asyncState)
        {
        }

        /// <summary>
        /// Finishes the asynchronous processing and throws an exception if one was generated
        /// <remarks>Blocks until the asynchronous processing has completed</remarks>
        /// </summary>
        /// <returns>Returns the result of the request to open bearer context </returns>
        public new ConnectGprsResult End()
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
            Eclo.nanoFramework.SIM800H.AtCommandResult calRet;

            try
            {
                // send command to open Gprs connection
                calRet = SIM800H.Instance.SendATCommand(Prompts.AT + Prompts.CIICR, 85000);
                if (calRet.Result == ReturnedState.OK)
                {
                    // now get IP address

                    calRet = SIM800H.Instance.SendATCommandAndWaitForResponse(Prompts.AT + Prompts.CIFSR, 5000);
                    if (calRet.Result == Eclo.nanoFramework.SIM800H.ReturnedState.OK)
                    {
                        // request to open GPRS connection successful
                        Result = ConnectGprsResult.Open;

                        // update IP address 
                        SIM800H.IpAddress = calRet.Response;

                        // raise event on a thread
                        new Thread(() =>
                        {
                            // pause for a bit to allow async result to be processed
                            Thread.Sleep(500);

                            SIM800H.GprsProvider.OnGprsSocketsBearerStateChanged(true);
                        }).Start();

                        // done here
                        return;
                    }
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
