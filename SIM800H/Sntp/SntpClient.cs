using System;
using System.Runtime.CompilerServices;

namespace Eclo.nanoFramework.SIM800H
{
    /// <summary>
    /// SNTP client class with all methods to perform SNTP requests.
    /// </summary>
    public class SntpClient : IDisposable
    {
        internal SntpClient()
        {
            // set NTP to use bearer profile 1
            AtCommandResult ret = SIM800H.Instance.SendATCommandAndWaitForResponse(Prompts.AT + Prompts.CNTP + @"CID=1", 2000);
            if (ret.Result != ReturnedState.OK)
            {
                // give it another try
                System.Threading.Thread.Sleep(2000);

                ret = SIM800H.Instance.SendATCommandAndWaitForResponse(Prompts.AT + Prompts.CNTP + @"CID=1", 2000);
                if (ret.Result != ReturnedState.OK)
                {
                    //Console.WriteLine("failed to assign bearer profile");
                }
            }

            // dispose var
            ret = null;
        }

        #region Disposable implementation

        ~SntpClient()
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
        /// Starts an asynchronous operation to synchronize network time with SNTP server.
        /// Requires GPRS bearer profile 1 opened.
        /// </summary>
        /// <param name="sntpServer">URL or IP address of the NTP server</param>
        /// <param name="utcOffset">UTC offset of local time zone.</param>
        /// <param name="asyncCallback">The callback to be invoked upon completion, optional</param>
        /// <param name="asyncState">The state object to be stored against the OpenGprsContextAsyncResult, optional</param>
        /// <returns>The IMEIAsyncResult</returns>
        public SyncNetworkTimeAsyncResult SyncNetworkTimeAsync(string sntpServer, TimeSpan utcOffset, AsyncCallback asyncCallback = null, object asyncState = null)
        {
            return new SyncNetworkTimeAsyncResult(sntpServer, utcOffset, asyncCallback, asyncState);
        }

        #region NTP request reply received

        /// <summary>
        /// Represents the delegate used for the <see cref="SntpReplyReceived"/> event.
        /// </summary>
        /// <param name="syncResult">Result code of SNTP request</param>
        public delegate void SntpReplyReceivedHandler(SyncResult syncResult);
        /// <summary>
        /// Event raised when the module receives the reply of a SNTP sync request.
        /// </summary>
        public event SntpReplyReceivedHandler SntpReplyReceived;
        private SntpReplyReceivedHandler onSntpReplyReceived;

        /// <summary>
        /// Raises the <see cref="SntpReplyReceived"/> event.
        /// </summary>
        /// <param name="syncResult">Result code of SNTP sync request</param>
        internal virtual void OnSntpReplyReceived(SyncResult syncResult)
        {
            if (onSntpReplyReceived == null) onSntpReplyReceived = new SntpReplyReceivedHandler(OnSntpReplyReceived);
            if (SntpReplyReceived != null)
            {
                SntpReplyReceived(syncResult);
            }
        }

        #endregion
    }
}
