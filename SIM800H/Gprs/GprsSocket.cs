using Eclo.nF.Extensions;
using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Eclo.nanoFramework.SIM800H
{
    /// <summary>
    /// GPRS socket instance.
    /// </summary>
    public class GprsSocket : IDisposable
    {
        #region global common vars
        
        const int loopWaitTime = 100;

        #endregion

        ProtocolType protocolType;
        
        internal int _connectionHandle = -1;

        internal SIM800H owner;

        private ConnectionStatus _clientState = ConnectionStatus.Unknown;

        /// <summary>
        /// Connection status of a GPRS socket
        /// </summary>
        public ConnectionStatus Status
        {
            get { return _clientState; }
            internal set 
            {
                if (_clientState != value)
                {
                    _clientState = value;

                    // raise event for connected and disconnected status on a thread
                    new Thread(() =>
                    {
                        if (_clientState == ConnectionStatus.Connected)
                        {
                            this.OnSocketConnected(this);
                        }
                        else if (_clientState == ConnectionStatus.Closed)
                        {
                            this.OnSocketClosed(this);

                            // dispose object
                            ((IDisposable)this).Dispose();
                        }
                    }).Start();
                }
            }
        }

        private bool _isSsl;
        internal bool IsSsl
	    {
		    get { return _isSsl;}
		    private set { _isSsl = value;}
	    }

        internal ByteBuffer inBuffer;

        internal int BytesToRead
        {
            get { return inBuffer.Length ; }
        }
        

        #region Disposable implementation

        ~GprsSocket()
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
            if (_connectionHandle != -1)
            {
                // send AT command to 'quick' close socket 
                // don't worry with return or execution, this is just an attempt
                SIM800H.Instance.SendATCommandAndDontWaitReply(Prompts.AT + Prompts.CIPCLOSE + "=" + _connectionHandle + @",0");

                // remove from socket table
                SIM800H.RemoveSocket(this);

                _connectionHandle = -1;
            }
        }

        #endregion

        /// <summary>
        /// Class with GPRS socket methods and properties
        /// </summary>
        /// <param name="protocolType">Protocol type of the socket</param>
        /// <param name="isSslSocket">True if socket is SLL. Default is false (not SSL).</param>
        public GprsSocket(ProtocolType protocolType, bool isSslSocket = false)
        {
            // init socket
            if (protocolType == ProtocolType.Udp)
            {
                throw new NotImplementedException();
            }
            else if (protocolType == ProtocolType.Tcp)
            {

            }
            else
            {
                throw new SocketException(SocketError.ProtocolNotSupported);
            }

            // store protocol type
            this.protocolType = protocolType;

            this.IsSsl = isSslSocket;

            _connectionHandle = SIM800H.AddSocket(this);

            inBuffer = new ByteBuffer(256, true);
        }

        /// <summary>
        /// Connects a GPRS socket to the specified URL and port.
        /// This operation requires that a GPRS connection has been open previous to call this otherwise an exception will throw.
        /// </summary>
        /// <param name="remoteURL">URL where to connect</param>
        /// <param name="port">Port number where to connect</param>
        public void Connect(string remoteURL, int port)
        {
            // set SSL option
            var ret = SIM800H.Instance.SendATCommand(Prompts.AT + Prompts.CIPSSL + "=" + (IsSsl ? "1" : "0"));

            if (ret.Result == ReturnedState.OK)
            {
                // send connect command with appropriate protocol type
                ret = SIM800H.Instance.SendATCommand(Prompts.AT + Prompts.CIPSTART + "=" + _connectionHandle + @",""" + (protocolType == ProtocolType.Tcp ? "TCP" : "UDP") + @""",""" + remoteURL + @"""," + port, 500);
                if(ret.Result == ReturnedState.OK)
                {
                    return;
                }
            }

            throw new InvalidOperationException();
        }

        /// <summary>
        /// Close GPRS socket connection
        /// </summary>
        public void Close()
        {
            // close connection 
            // send command, don't care about return
            SIM800H.Instance.SendATCommand(Prompts.AT + Prompts.CIPCLOSE + "=" + _connectionHandle, 500);
        }

        public int Send(byte[] buffer)
        {
            return Send(buffer, 0, buffer.Length);
        }

        public int Send(byte[] buffer, int offset, int size)
        {
            Eclo.nanoFramework.SIM800H.AtCommandResult sendData;
            int milisecondsTimeout = 15000;
            int index;

            try
            {
                // wait if socket is not connected
                while (milisecondsTimeout > 0 && Status != ConnectionStatus.Connected)
                {
                    // timeout for next iteration
                    milisecondsTimeout = milisecondsTimeout - loopWaitTime;

                    Thread.Sleep(loopWaitTime);
                }

                if (Status != ConnectionStatus.Connected)
                {
                    //Debug.Print("connection failed");

                    return -1;
                }

                sendData = SIM800H.Instance.SendATCommandAndWaitForResponse(Prompts.AT + Prompts.CIPSEND + "=" + _connectionHandle + "," + size, 1000);

                if (sendData.Result == ReturnedState.OK && sendData.Response == Prompts.SendPrompt)
                {
                    // reset event
                    SIM800H.Instance.responseReceived.Reset();
                    // set flag
                    SIM800H.Instance.waitingResponse = true;

                    // write data in chunks of 64 bytes because of buffers size
                    index = 0;
                    int chunkSize = 64;

                    while (index < size)
                    {
                        // adjust chunk size
                        chunkSize = System.Math.Min(chunkSize, size - index);

                        // send chunk writing directly to UART
                        SIM800H.Instance._serialDevice.WriteBytes(buffer, offset + index, chunkSize);

                        // update index
                        index += chunkSize;
                    }

                    // send 
                }
                else
                {
                    // no send prompt
                    return -1;
                }

                // build accept prompt
                string acceptPrompt = Prompts.DATA_ACCEPT + _connectionHandle + "," + size;

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
                                if (SIM800H.Instance.responseQueue.FindAndRemove(acceptPrompt) != null)
                                {
                                    // done here
                                    return size;
                                }

                                // ERROR response
                                if (SIM800H.Instance.responseQueue.FindAndRemove(Prompts.ERROR) != null)
                                {
                                    // return ERROR 
                                    break;
                                }

                                // send failed prompt
                                if (SIM800H.Instance.responseQueue.FindAndRemove(Prompts.SEND_FAIL) != null)
                                {
                                    break;
                                }
                            }
                        }
                    }

                    // loop this each 250ms
                    milisecondsTimeout = milisecondsTimeout - 250;

                    // if we reach here there was no response from module, something went wrong...
                }

                //Debug.Print("failed sending");
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                //Debug.GC(true);
            }

            return -1;
        }

        public int Receive(byte[] buffer, int offset, int size)
        {
            int milisecondsTimeout = 15000;
            int toRead = 0;

            try
            {
                // start loop with timeout waiting to have enough bytes in the buffer
                while (milisecondsTimeout > 0)
                {
                    // timeout for next iteration
                    milisecondsTimeout = milisecondsTimeout - loopWaitTime;

                    if (inBuffer.Length >= size)
                    {
                        // enough bytes available to satisfy request
                        break;
                    }

                    Thread.Sleep(loopWaitTime);
                }

                // how many bytes can we get from the buffer
                lock (inBuffer.SyncRoot)
                {
                    toRead = System.Math.Min(inBuffer.Length, size);

                    Array.Copy(inBuffer.Buffer, inBuffer.Offset, buffer, offset, toRead);
                    inBuffer.Complete(toRead);

                    return toRead;
                }
            }
            catch { }
            finally 
            {
                //Debug.GC(true);
            }

            return toRead;
        }

        #region socket connected event and delegate

        /// <summary>
        /// Represents the delegate used for the <see cref="OnSocketConnected"/> event.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        public delegate void SocketConnectedHandler(GprsSocket sender);
        /// <summary>
        /// Event raised when the module emits a network registration message.
        /// </summary>
        public event SocketConnectedHandler SocketConnected;
        private SocketConnectedHandler onSocketConnected;

        /// <summary>
        /// Raises the <see cref="OnSocketConnected"/> event.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>  
        protected virtual void OnSocketConnected(GprsSocket sender)
        {
            if (onSocketConnected == null) onSocketConnected = new SocketConnectedHandler(SocketConnected);
            if (SocketConnected != null)
            {
                SocketConnected(sender);
            }
        }

        #endregion

        #region socket closed event and delegate

        /// <summary>
        /// Represents the delegate used for the <see cref="OnSocketClosed"/> event.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        public delegate void SocketClosedHandler(GprsSocket sender);
        /// <summary>
        /// Event raised when the module emits a network registration message.
        /// </summary>
        public event SocketClosedHandler SocketClosed;
        private SocketClosedHandler onSocketClosed;

        /// <summary>
        /// Raises the <see cref="OnSocketClosed"/> event.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>  
        protected virtual void OnSocketClosed(GprsSocket sender)
        {
            if (onSocketClosed == null) onSocketClosed = new SocketClosedHandler(SocketClosed);
            if (SocketClosed != null)
            {
                SocketClosed(sender);
            }
        }

        #endregion

        #region socket data received event and delegate

        /// <summary>
        /// Represents the delegate used for the <see cref="OnDataReceived"/> event.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        public delegate void DataReceivedHandler(object sender);
        /// <summary>
        /// Event raised when the module emits a network registration message.
        /// </summary>
        public event DataReceivedHandler DataReceived;
        private DataReceivedHandler onDataReceived;

        /// <summary>
        /// Raises the <see cref="OnDataReceived"/> event.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>  
        internal virtual void OnDataReceived(object sender)
        {
            if (onDataReceived == null) onDataReceived = new DataReceivedHandler(DataReceived);
            if (DataReceived != null)
            {
                DataReceived(sender);
            }
        }

        #endregion
    }
}
