////
// Copyright (c) Eclo Solutions
// See LICENSE file in the project root for full license information.
////

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace Eclo.nanoFramework.SIM800H
{
    /// <summary>
    /// Class with methods to perform GPRS related actions.
    /// </summary>
    public class GprsProvider : IDisposable
    {
        internal GprsProvider()
        {
            // close any connection that maybe active, just in case, otherwise is won't accept the following GPRS configurations
            SIM800H.Instance.SendATCommandAndDontWaitReply(Prompts.AT + "+CIPSHUT");

            // setup multi connection and send data mode
            var calRet = SIM800H.Instance.SendATCommand(Prompts.AT + "+CIPMUX=1;+CIPQSEND=1", 5000);
            if (calRet.Result != ReturnedState.OK)
            {
                // give it another try
                Thread.Sleep(2000);

                calRet = SIM800H.Instance.SendATCommand(Prompts.AT + "+CIPMUX=1;+CIPQSEND=1", 5000);
                if (calRet.Result != ReturnedState.OK)
                {
                    // TBD
                    //Debug.WriteLine("** failed to set GPRS prefs ***");
                }
            }

            // setup APN parameters for IP apps, if configuration exists
            if (SIM800H.AccessPointConfiguration != null)
            {
                string atCommand = string.Empty;
                if (SIM800H.AccessPointConfiguration.UserName == "" && SIM800H.AccessPointConfiguration.Password == "")
                {
                    // short config because there is only APN name without user name and password
                    atCommand = Prompts.AT + Prompts.CSTT + "=\"" + SIM800H.AccessPointConfiguration.AccessPointName + "\"";
                    calRet = SIM800H.Instance.SendATCommand(atCommand, 5000);
                }
                else
                {
                    atCommand = Prompts.AT + Prompts.CSTT + "=\"" + SIM800H.AccessPointConfiguration.AccessPointName + "\",\"" + SIM800H.AccessPointConfiguration.UserName + "\",\"" + SIM800H.AccessPointConfiguration.Password + "\"";
                    calRet = SIM800H.Instance.SendATCommand(atCommand, 5000);
                }

                if (calRet.Result != ReturnedState.OK)
                {
                    // give it another try
                    Thread.Sleep(2000);

                    calRet = SIM800H.Instance.SendATCommand(atCommand, 5000);
                    if (calRet.Result != ReturnedState.OK)
                    {
                        //  TBD
                        //Debug.WriteLine("** failed to set APN config for sockets ***");
                    }
                }

                // not checking any return values because in case of failure the device won't have GPRS connectivity and it should be reset anyway
                calRet = SIM800H.Instance.SendATCommandAndDontWaitReply(Prompts.AT + 
                    Prompts.SAPBR + @"=3,1,""Contype"",""GPRS"";" +
                    Prompts.SAPBR + @"=3,1,""APN"",""" + SIM800H.AccessPointConfiguration.AccessPointName + @""";" +
                    Prompts.SAPBR + @"=3,1,""USER"",""" + SIM800H.AccessPointConfiguration.UserName + @""";" +
                    Prompts.SAPBR + @"=3,1,""PWD"",""" + SIM800H.AccessPointConfiguration.Password + @"""");

                // dispose vars
                atCommand = null;
                calRet = null;
            }
            else
            {
                // no APN config, warn user about this 
                Debug.WriteLine("No APN configuration or invalid");
            }

            // set APN for MMS
            if (SIM800H.MmsAccessPointConfiguration != null)
            {
                // not checking any return values because in case of failure the device won't have GPRS connectivity and it should be reset anyway
                calRet = SIM800H.Instance.SendATCommandAndDontWaitReply(Prompts.AT +
                    Prompts.SAPBR + @"=3,2,""Contype"",""GPRS"";" +
                    Prompts.SAPBR + @"=3,2,""APN"",""" + SIM800H.MmsAccessPointConfiguration.AccessPointName + @""";" +
                    Prompts.SAPBR + @"=3,2,""USER"",""" + SIM800H.MmsAccessPointConfiguration.UserName + @""";" +
                    Prompts.SAPBR + @"=3,2,""PWD"",""" + SIM800H.MmsAccessPointConfiguration.Password + @"""");

                // dispose vars
                calRet = null;
            }
            else
            {
                // no APN config, warn user about this 
                Debug.WriteLine("No MMS APN configuration or invalid");
            }
        }

#region Disposable implementation

        ~GprsProvider()
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

#region methods for sockets operations

        /// <summary>
        /// Starts an asynchronous operation to open a GPRS connection.
        /// A GPRS connection is required to use sockets, IP apps and MMS.
        /// </summary>
        /// <param name="asyncCallback">The callback to be invoked upon completion, optional</param>
        /// <param name="asyncState">The state object to be stored against the OpenGprsContextAsyncResult, optional</param>
        /// <returns>The ConnectGprsAsyncResult</returns>
        public ConnectGprsAsyncResult OpenGprsConnectionAsync(AsyncCallback asyncCallback = null, object asyncState = null)
        {
            return new ConnectGprsAsyncResult(asyncCallback, asyncState);
        }

        /// <summary>
        /// Detach GPRS
        /// </summary>
        /// <returns></returns>
        public Eclo.nanoFramework.SIM800H.AtCommandResult DetachGprs()
        {
            // check if context is open
            if (SIM800H.GprsIpAppsBearerIsOpen == false)
            {
                return new AtCommandResult(ReturnedState.Error);
            }

            AtCommandResult calRet = SIM800H.Instance.SendATCommand(Prompts.AT + Prompts.CGATT + "=0", 2000);
            if (calRet.Result == ReturnedState.OK)
            {
                // request to close GPRS connection successful
                SIM800H.GprsIpAppsBearerIsOpen = false;
            }

            return calRet;
        }

        ///// <summary>
        ///// Connects to a TCP server
        ///// </summary>
        ///// <param name="server">IP address of the server</param>
        ///// <param name="port">Port in the server</param>
        ///// <returns></returns>
        //public Eclo.NETMF.CellularRadio.ATCommandResult ConnectTCP(string server, int port)
        //{
        //    return SIM800H.Instance.SendATCommand("AT+CIPSTART=\"TCP\",\"" + server + "\"," + port);
        //}

        ///// <summary>
        ///// Disconnects from TCP server
        ///// </summary>
        ///// <returns></returns>
        //public Eclo.NETMF.CellularRadio.ATCommandResult DisconnectTCP()
        //{
        //    return SIM800H.Instance.SendATCommand("AT+CIPCLOSE");
        //}

        ///// <summary>
        ///// Configure the device as a TCP server
        ///// </summary>
        ///// <param name="port">Port</param>
        ///// <returns></returns>
        //public Eclo.NETMF.CellularRadio.ATCommandResult ConfigureTCPServer(int port)
        //{
        //    return SIM800H.Instance.SendATCommand("AT+CIPSERVER=1," + port);
        //}

        /// <summary>
        /// Queries current status of a specific GPRS connection
        /// </summary>
        /// <returns></returns>
        public ConnectionStatus CheckConnectionStatus(int connection)
        {
            //Debug.WriteLine("Checking status for connection " + connection);

            AtCommandResult calRet = SIM800H.Instance.SendATCommandAndWaitForResponse(Prompts.AT + Prompts.CIPSTATUS + "=" + connection, 5000);
            if (calRet.Result == Eclo.nanoFramework.SIM800H.ReturnedState.OK)
            {
                try
                {
                    // clear response
                    string messageRaw = (new StringBuilder(calRet.Response).Replace(Prompts.CIPSTATUS + ": ", "").Replace("\"", "")).ToString();

                    // get other message details
                    string[] split = messageRaw.Split(new char[] { ',' });

                    if (split.Length == 6)
                    {
                        // status
                        switch (split[5])
                        {
                            case "INITIAL":
                                return ConnectionStatus.Initial;
                            case "CONNECTING":
                                return ConnectionStatus.Connecting;
                            case "CONNECTED":
                                return ConnectionStatus.Connected;
                            case "REMOTE CLOSING":
                                return ConnectionStatus.RemoteClosing;
                            case "CLOSING":
                                return ConnectionStatus.Closing;
                            case "CLOSED":
                                return ConnectionStatus.Closed;

                            // shouldn't reach here
                            default:
                                return ConnectionStatus.Unknown;
                        }
                    }

                }
                catch { };
            }
            else
            {
                //Debug.WriteLine("No valid response");
            }

            return ConnectionStatus.Unknown; ;
        }

        #endregion

        #region methods for IP based applications (HTTP, FTP, EMAIL, SNTP)


        [Obsolete("This method is obsolete and will be removed in a future version. Replace with OpenBearerAsync specifying a BearerProfile.")]
        public OpenBearerAsyncResult OpenBearerAsync(AsyncCallback asyncCallback = null, object asyncState = null)
        {
            return new OpenBearerAsyncResult(BearerProfile.IpAppsBearer, asyncCallback, asyncState);
        }

        /// <summary>
        /// Starts an asynchronous operation to open a GPRS bearer.
        /// A GPRS bearer is required for HTTP client, SNTP, MMS and location requests.
        /// </summary>
        /// <param name="profile">The bearer profile</param>
        /// <param name="asyncCallback">The callback to be invoked upon completion, optional</param>
        /// <param name="asyncState">The state object to be stored against the OpenGprsContextAsyncResult, optional</param>
        /// <returns>The OpenBearerAsyncResult</returns>
        public OpenBearerAsyncResult OpenBearerAsync(BearerProfile profile, AsyncCallback asyncCallback = null, object asyncState = null)
        {
            return new OpenBearerAsyncResult(profile, asyncCallback, asyncState);
        }

        /// <summary>
        /// Closes a GPRS context bearer profile
        /// </summary>
        /// <param name="profile">The bearer profile.</param>
        /// <returns></returns>
        public AtCommandResult CloseBearer(BearerProfile profile)
        {
            // check if context is open
            // update owner properties
            switch (profile)
            {
                case BearerProfile.SocketsBearer:
                    if (SIM800H.GprsSocketsBearerIsOpen == false)
                    {
                        return new AtCommandResult(ReturnedState.Error);
                    }
                    break;

                case BearerProfile.IpAppsBearer:
                    if (SIM800H.GprsIpAppsBearerIsOpen == false)
                    {
                        return new AtCommandResult(ReturnedState.Error);
                    }
                    break;

                case BearerProfile.MmsBearer:
                    if (SIM800H.GprsMmsBearerIsOpen == false)
                    {
                        return new AtCommandResult(ReturnedState.Error);
                    }
                    break;
            }

            AtCommandResult calRet = SIM800H.Instance.SendATCommandAndWaitForResponse(Prompts.AT + Prompts.SAPBR + "=0," + profile.ToString(), 2000);
            if (calRet.Result == ReturnedState.OK)
            {
                // request to close GPRS context successful
                switch (profile)
                {
                    case BearerProfile.SocketsBearer:
                        SIM800H.GprsSocketsBearerIsOpen = false;
                        break;

                    case BearerProfile.IpAppsBearer:
                        SIM800H.GprsIpAppsBearerIsOpen = false;
                        break;

                    case BearerProfile.MmsBearer:
                        SIM800H.GprsMmsBearerIsOpen = false;
                        break;
                }
            }

            return calRet;
        }

        /// <summary>
        /// Queries a GPRS bearer profile to check if it's opened.
        /// On successful execution context open status is updated in the respective GprsNNNBearerIsOpen property
        /// </summary>
        /// <param name="profile">The bearer profile.</param>
        /// <returns></returns>
        public AtCommandResult CheckBearerStatus(BearerProfile profile)
        {
            AtCommandResult calRet = SIM800H.Instance.SendATCommandAndWaitForResponse(Prompts.AT + Prompts.SAPBR + "=2," + profile.ToString(), 3000);
            if (calRet.Result == Eclo.nanoFramework.SIM800H.ReturnedState.OK)
            {
                try
                {
                    // check for empty response, because IP address may not be available yet
                    if (calRet.Response != string.Empty)
                    {
                        // clear response
                        string messageRaw = calRet.Response.Substring(Prompts.SAPBR.Length + 2);

                        // get other message details
                        string[] split = messageRaw.Split(new char[] { ',' });

                        if (split.Length == 3)
                        {
                            // parse status
                            int bearerStatus = int.Parse(split[1]);

                            // update owner properties
                            switch(profile)
                            {
                                case BearerProfile.SocketsBearer:
                                    SIM800H.GprsSocketsBearerIsOpen = bearerStatus == 1 ? true : false;
                                    // update IP on owner property
                                    SIM800H.IpAddress = bearerStatus == 1 ? split[2].Trim('"') : "";
                                    break;

                                case BearerProfile.IpAppsBearer:
                                    SIM800H.GprsIpAppsBearerIsOpen = bearerStatus == 1 ? true : false;
                                    // update IP on owner property
                                    SIM800H.IpAddress = bearerStatus == 1 ? split[2].Trim('"') : "";
                                    break;

                                case BearerProfile.MmsBearer:
                                    SIM800H.GprsMmsBearerIsOpen = bearerStatus == 1 ? true : false;
                                    // update IP on owner property
                                    SIM800H.IpAddress = bearerStatus == 1 ? split[2].Trim('"') : "";
                                    break;
                            }

                            // done here
                            return calRet;
                        }
                    }
                }
                catch { };
            }

            return calRet;
        }

#endregion

        #region Delegates and events

        #region IP apps GPRS bearer open/close changed
        /// <summary>
        /// Represents the delegate used for the <see cref="GprsIpAppsBearerStateChanged"/> event.
        /// </summary>
        /// <param name="isOpen">Current state of the IP apps bearer </param>
        public delegate void GprsIpAppsBearerStateChangedHandler(bool isOpen);
        /// <summary>
        /// Event raised when the status of the IP bearer for IP apps changes.
        /// </summary>
        public event GprsIpAppsBearerStateChangedHandler GprsIpAppsBearerStateChanged;
        private GprsIpAppsBearerStateChangedHandler onGprsIpAppsBearerStateChanged;

        /// <summary>
        /// Raises the <see cref="GprsIpAppsBearerStateChanged"/> event.
        /// </summary>
        /// <param name="isOpen">Current state of the IP apps bearer </param>
        internal virtual void OnGprsIpAppsBearerStateChanged(bool isOpen)
        {
            if (onGprsIpAppsBearerStateChanged == null) onGprsIpAppsBearerStateChanged = new GprsIpAppsBearerStateChangedHandler(OnGprsIpAppsBearerStateChanged);
            if (GprsIpAppsBearerStateChanged != null)
            {
                GprsIpAppsBearerStateChanged(isOpen);
            }
        }
        #endregion

        #region MMS GPRS bearer open/close changed

        /// <summary>
        /// Represents the delegate used for the <see cref="MmsBearerStateChanged"/> event.
        /// </summary>
        /// <param name="isOpen">Current state of the MMS bearer </param>
        public delegate void MmsBearerStateChangedHandler(bool isOpen);
        /// <summary>
        /// Event raised when the status of the MMS changes.
        /// </summary>
        public event MmsBearerStateChangedHandler MmsBearerStateChanged;
        private MmsBearerStateChangedHandler onMmsBearerStateChanged;

        /// <summary>
        /// Raises the <see cref="MmsBearerStateChanged"/> event.
        /// </summary>
        /// <param name="isOpen">Current state of the MMS bearer </param>
        internal virtual void OnMmsBearerStateChanged(bool isOpen)
        {
            if (onMmsBearerStateChanged == null) onMmsBearerStateChanged = new MmsBearerStateChangedHandler(OnMmsBearerStateChanged);
            if (MmsBearerStateChanged != null)
            {
                MmsBearerStateChanged(isOpen);
            }
        }

        #endregion

        #region sockets GPRS bearer open/close changed
        /// <summary>
        /// Represents the delegate used for the <see cref="GprsSocketsBearerStateChanged"/> event.
        /// </summary>
        /// <param name="isOpen">Current state of the sockets bearer </param>
        public delegate void GprsSocketsBearerStateChangedHandler(bool isOpen);
        /// <summary>
        /// Event raised when the status of the sockets bearer changes.
        /// </summary>
        public event GprsSocketsBearerStateChangedHandler GprsSocketsBearerStateChanged;
        private GprsSocketsBearerStateChangedHandler onGprsSocketsBearerStateChanged;

        /// <summary>
        /// Raises the <see cref="GprsSocketsBearerStateChanged"/> event.
        /// </summary>
        /// <param name="isOpen">Current state of the sockets bearer </param>
        internal virtual void OnGprsSocketsBearerStateChanged(bool isOpen)
        {
            if (onGprsSocketsBearerStateChanged == null) onGprsSocketsBearerStateChanged = new GprsSocketsBearerStateChangedHandler(OnGprsSocketsBearerStateChanged);
            if (GprsSocketsBearerStateChanged != null)
            {
                GprsSocketsBearerStateChanged(isOpen);
            }
        }
        #endregion

#endregion

    }
}
