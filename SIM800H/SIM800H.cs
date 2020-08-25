using Eclo.nF.Extensions;
using System;
using System.Collections;
using System.Text;
using System.Threading;
using Windows.Devices.Gpio;
using Windows.Devices.SerialCommunication;

namespace Eclo.nanoFramework.SIM800H
{
    /// <summary>
    /// Class with methods, properties and events to work with a SIM800H module.
    /// </summary>
    public class SIM800H : IDisposable
    {
        private static readonly SIM800H instance = new SIM800H();
       
        #region global common vars

        string tString;
        string[] sString;

        #endregion

        #region serial receiver handler vars

        bool receivingSocketData, receivingPrompt, processingPrompt, receivingSmsData, receivingHttpData, httpReadPrompt, receivingHttpHeaders, httpHeadPrompt;
        char[] buffer = new char[512];
        int bufferOffset, socketIndex, dataToRead;
        int receiverMilisecondsTimeout;
        const int receiverLoopWaitTime = 100;
        string tempString;
        string[] splitString;

#if DEBUG_SERIAL_RECEIVE
        char[] debugBuffer;
        int debugBufferOffset;
#endif 

        #endregion

        private bool _disposed;

        private readonly object _lock = new object();
        private bool _externalLock;

        private Queue _asyncTaskQueue = new Queue();
        private Thread _asyncTaskQueueThread;
        private Hashtable _sockets;
        internal ArrayList responseQueue;

        private readonly AutoResetEvent promptInQueue = new AutoResetEvent(false);
        internal readonly AutoResetEvent responseReceived = new AutoResetEvent(false);

        internal bool waitingResponse = false;

        internal bool _initCompleted = false;

        #region Power On/Off


        /// <summary>
        /// Starts an asynchronous operation to run the power on sequence
        /// </summary>
        /// <param name="asyncCallback">The callback to be invoked upon completion, optional</param>
        /// <param name="asyncState">The state object to be stored against the ReadMessageAsyncResult, optional</param>
        /// <returns>The PowerSatus result of the power on sequence</returns>
        public static PowerOnAsyncResult PowerOnAsync(AsyncCallback asyncCallback = null, object asyncState = null)
        {
            return new PowerOnAsyncResult(asyncCallback, asyncState);
        }

        /// <summary>
        /// Powers off the SIM800H module
        /// </summary>
        public static void PowerOff()
        {
            // hardware power down
            Instance._powerKey.Write(GpioPinValue.High);
            Thread.Sleep(1200);
            Instance._powerKey.Write(GpioPinValue.Low);
            Thread.Sleep(500);

            // power down command
            Instance.SendATCommand(Prompts.AT + Prompts.CPOWD);

            // set module power status property
            PowerStatus = PowerStatus.Off;

            // module won't be registered in network anymore
            GsmNetworkRegistration = NetworkRegistrationState.Unknown;
            GprsNetworkRegistration = NetworkRegistrationState.Unknown;

            // all GPRS bearers must be off too
            GprsIpAppsBearerIsOpen = false;
            GprsMmsBearerIsOpen = false;
            GprsSocketsBearerIsOpen = false;

            // abort reader thread, if running
            if (Instance.readerThread != null)
            {
                try
                {
                    // will cause a ThreadAbortException, this is the expected behavior
                    Instance.readerThread.Abort();
                    // need to call Join to guarantee that the thread dies
                    Instance.readerThread.Join();
                }
                catch { };
            }

            // dispose/null all the features properties as they'll need to be instantiated again in case of reuse
            // check the storage vars rather than the public properties otherwise all those will be instantiated which is the exact opposite of what is trying to be accomplished here
            if(_gprsProvider  != null)
            {
                GprsProvider.Dispose();
                GprsProvider = null;
            }

            if (_httpClient != null)
            {
                HttpClient.Dispose();
                HttpClient = null;
            }

            if (_mmsClient != null)
            {
                MmsClient.Dispose();
                MmsClient = null;
            }

            if (_smsProvider != null)
            {
                SmsProvider.Dispose();
                SmsProvider = null;
            }

            if (_sntpClient != null)
            {
                SntpClient.Dispose();
                SntpClient = null;
            }
        }

        #endregion

        #region Private Attributes

        // Serial line that sends commands to the GSM module
        internal SerialDevice _serialDevice;
        // Power line to the GSM module
        internal Windows.Devices.Gpio.GpioPin _powerKey;

        // Serial reader thread
        internal Thread readerThread;

        #endregion

        #region public properties

        private static FileStorage _fileStorage;
        /// <summary>
        /// <see cref="FileStorage"/> property with all the methods required to access the internal file storage
        /// </summary>
        public static FileStorage FileStorage
        {
            get
            {
                if (_fileStorage == null)
                {
                    _fileStorage = new FileStorage();
                }
                return _fileStorage;
            }
            set { _fileStorage = value; }
        }

        private static GprsProvider _gprsProvider;
        /// <summary>
        /// <see cref="GprsProvider"/> property with all the methods required to use the GPRS features
        /// </summary>
        public static GprsProvider GprsProvider
        {
            get
            {
                if (_gprsProvider == null)
                {
                    _gprsProvider = new GprsProvider();
                }
                return _gprsProvider;
            }
            set { _gprsProvider = value; }
        }

        private static SmsProvider _smsProvider;
        /// <summary>
        /// <see cref="SmsProvider"/> property with all the methods required to send SMS (text) messages
        /// </summary>
        public static SmsProvider SmsProvider
        {
            get
            {
                if (_smsProvider == null)
                {
                    _smsProvider = new SmsProvider();
                }
                return _smsProvider;
            }
            set { _smsProvider = value; }
        }

        private static HttpClient _httpClient;
        /// <summary>
        /// <see cref="HttpClient"/> property with all the methods required to perform HTTP requests
        /// </summary>
        public static HttpClient HttpClient
        {
            get
            {
                if (_httpClient == null)
                {
                    _httpClient = new HttpClient();
                }
                return _httpClient;
            }
            set { _httpClient = value; }
        }

        private static SntpClient _sntpClient;
        /// <summary>
        /// <see cref="SntpClient"/> property with all the methods required to user the SNTP (Simple Network Time Protocol) client
        /// </summary>
        public static SntpClient SntpClient
        {
            get
            {
                if (_sntpClient == null)
                {
                    _sntpClient = new SntpClient();
                }
                return _sntpClient;
            }
            set { _sntpClient = value; }
        }

        private static MmsClient _mmsClient;
        /// <summary>
        /// <see cref="MmsClient"/> property with all the methods required to use the MMS (Multimedia Messaging Service) client
        /// </summary>
        public static MmsClient MmsClient
        {
            get
            {
                if (_mmsClient == null)
                {
                    try
                    {
                        _mmsClient = new MmsClient();
                    }
                    catch
                    { }
                }
                return _mmsClient;
            }
            set { _mmsClient = value; }
        }

        private static bool _GprsMmsBearerIsOpen = false;
        /// <summary>
        /// Status of MMS bearer in profile 2 of GPRS context
        /// </summary>
        public static bool GprsMmsBearerIsOpen
        {
            get { return _GprsMmsBearerIsOpen; }
            internal set
            {

                if (_GprsMmsBearerIsOpen != value)
                {
                    _GprsMmsBearerIsOpen = value;

                    // raise event for status changed on a thread
                    new Thread(() => { GprsProvider.OnMmsBearerStateChanged(GprsMmsBearerIsOpen); }).Start();
                }

            }
        }

        private static bool _GprsIpAppsBearerIsOpen = false;
        /// <summary>
        /// Status of IP apps bearer in profile 1 of GPRS context
        /// </summary>
        public static bool GprsIpAppsBearerIsOpen
        {
            get { return _GprsIpAppsBearerIsOpen; }
            internal set
            {

                if (_GprsIpAppsBearerIsOpen != value)
                {
                    _GprsIpAppsBearerIsOpen = value;

                    // raise event for status changed on a thread
                    new Thread(() => { GprsProvider.OnGprsIpAppsBearerStateChanged(GprsIpAppsBearerIsOpen); }).Start();
                }

            }
        }

        private static bool _GprsSocketsBearerIsOpen = false;
        /// <summary>
        /// Status of sockets bearer in profile 0 of GPRS context
        /// </summary>
        public static bool GprsSocketsBearerIsOpen
        {
            get { return _GprsSocketsBearerIsOpen; }
            internal set
            {

                if (_GprsSocketsBearerIsOpen != value)
                {
                    _GprsSocketsBearerIsOpen = value;

                    // raise event for status changed on a thread
                    new Thread(() => { GprsProvider.OnGprsSocketsBearerStateChanged(GprsSocketsBearerIsOpen); }).Start();
                }

            }
        }

        internal static PowerStatus _powerStatus = PowerStatus.Unknown;
        /// <summary>
        /// Power status of SIM800H device
        /// </summary>
        public static PowerStatus PowerStatus
        {
            get { return _powerStatus; }
            internal set
            {
                if (_powerStatus != value)
                {
                    // can update this only if power on sequence is not running
                    if (_powerStatus != PowerStatus.PowerOnSequenceIsRunning)
                    {
                        _powerStatus = value;

                        // raise event for power status changed on a thread
                        new Thread(() => { Instance.OnPowerStatusChanged(_powerStatus); }).Start();
                    }
                }
            }
        }

        private static NetworkRegistrationState _gsmNetworkRegistration = NetworkRegistrationState.Unknown;
        /// <summary>
        /// GSM network registration state of module
        /// </summary>
        public static NetworkRegistrationState GsmNetworkRegistration
        {
            get { return _gsmNetworkRegistration; }
            internal set
            {

                if (_gsmNetworkRegistration != value)
                {
                    _gsmNetworkRegistration = value;

                    // raise changed event on a thread 
                    new Thread(() => { Instance.OnGsmNetworkRegistrationChanged(_gsmNetworkRegistration); }).Start();
                }

            }
        }

        private static NetworkRegistrationState _gprsNetworkRegistration = NetworkRegistrationState.Unknown;
        /// <summary>
        /// GPRS network registration state of module
        /// </summary>
        public static NetworkRegistrationState GprsNetworkRegistration
        {
            get { return _gprsNetworkRegistration; }
            internal set
            {

                if (_gprsNetworkRegistration != value)
                {
                    _gprsNetworkRegistration = value;

                    // raise changed event on a thread 
                    new Thread(() => { Thread.Sleep(1000); Instance.OnGprsNetworkRegistrationChanged(_gsmNetworkRegistration); }).Start();
                }

            }
        }

        private static String _ipAddress;
        /// <summary>
        /// IP address of module
        /// </summary>
        public static String IpAddress
        {
            get { return _ipAddress; }
            internal set { _ipAddress = value; }
        }

        /// <summary>
        /// GPRS access point configuration (APN)
        /// <note type="note">This APN configuration is used in HTTP calls, SNTP and other IP applications. For MMS set <see cref="MmsAccessPointConfiguration"/>.</note>
        /// </summary>
        public static AccessPointConfiguration AccessPointConfiguration;

        /// <summary>
        /// Access point configuration (APN) for MMS
        /// <note type="note">This APN configuration is used exclusively for MMS send. For other IP applications set <see cref="AccessPointConfiguration"/>.</note>
        /// </summary>
        public static AccessPointConfiguration MmsAccessPointConfiguration;

        private static MmsConfiguration _mmsConfiguration;
        /// <summary>
        /// MMS center configuration
        /// </summary>
        public static MmsConfiguration MmsConfiguration
        {
            get { return _mmsConfiguration; }
            set { _mmsConfiguration = value; }
        }

        private string _modelIdentification = "???";
        /// <summary>
        /// Module model identification 
        /// </summary>
        public string ModelIdentification
        {
            get
            {
                // do we already have this?
                if (_modelIdentification == "???")
                {
                    // no, try update
                    var ret = SendATCommandAndWaitForResponse(Prompts.AT + Prompts.CGMM, 2000);

                    if (ret.Result == ReturnedState.OK)
                    {
                        _modelIdentification = ret.Response;
                    }
                }

                return _modelIdentification;
            }
        }

        private static string _softwareRelease = "???";
        /// <summary>
        /// Module software release 
        /// </summary>
        public static string SoftwareRelease
        {
            get
            {
                // do we already have this?
                if (_softwareRelease == "???")
                {
                    // no, try update
                    var ret = Instance.SendATCommandAndWaitForResponse(Prompts.AT + Prompts.CGMR, 2000);

                    if (ret.Result == ReturnedState.OK)
                    {
                        _softwareRelease = ret.Response.Substring(9);
                    }
                }

                return _softwareRelease;
            }
        }

        const int _defaultMaxSockets = 6;
        static private int _maxSockets = _defaultMaxSockets;
        /// <summary>
        /// Maximum number of sockets supported. SIM800H module supports up to 6.
        /// When setting this properties, any existing socket will be closed and becomes unavailable.
        /// </summary>
        public static int MaxSockets
        {
            get { return _maxSockets; }
            set
            {
                if (value > 6 || value < 0)
                {
                    throw new ArgumentOutOfRangeException();
                }

                if (_maxSockets != value)
                {
                    _maxSockets = value;

                    // recreate sockets table
                    Instance._sockets = new Hashtable(_maxSockets);
                }
            }
        }


        //private static int _traceLevel = -1;
        //public static int TraceLevel
        //{
        //    get
        //    {
        //        return _traceLevel;
        //    }

        //    set
        //    {
        //        _traceLevel = value;
        //    }
        //}

        #endregion


        #region Constructor/Destructor and Dispose

        private SIM800H() 
        {
            // async tasks properties
            _asyncTaskQueueThread = new Thread(AsyncTaskQueueThread);
            _asyncTaskQueueThread.Start();

            responseQueue = new ArrayList();

            // initialize sockets hash table
            _sockets = new Hashtable(_defaultMaxSockets);
        }

        internal static SIM800H Instance
        {
            get
            {
                return instance;
            }
        }

        /// <summary>
        /// Configure hardware interface with the device.
        /// </summary>
        /// <param name="powerKey">The I/O signal that will be used to control the device's power key</param>
        /// <param name="serialPort">The serial port that will be used to comunicate with the device</param>
        public static void Configure(Windows.Devices.Gpio.GpioPin powerKey, ref SerialDevice serialPort)
        {
            if (Instance._initCompleted)
            {
                // device has already been initialized so we need to clean up some thing first

                try
                {
                    Instance._serialDevice.DataReceived -= Instance._serialDevice_DataReceived;

                    //Instance._serialDevice.Close();
                    Instance._serialDevice.Dispose();
                    Instance._serialDevice = null;
                }
                catch { };
            }

            // store power key GPIO
            Instance._powerKey = powerKey;

            // setup serial port
            Instance._serialDevice = serialPort;
            // serial port settings
            //Instance._serialDevice.Close();
            Instance._serialDevice.BaudRate = 115200; // possible values:  19200, 38400, 57600 and 115200 
            Instance._serialDevice.Parity = SerialParity.None;
            Instance._serialDevice.StopBits = SerialStopBitCount.One;
            Instance._serialDevice.DataBits = 8;
            Instance._serialDevice.Handshake = SerialHandshake.None;

            // open serial
            //Instance._serialDevice.Open();

            // setup read timeout
            // because we are reading from the UART it's recommended to set a read timeout
            // otherwise the reading operation doesn't return until the requested number of bytes has been read
//            Instance._serialDevice.ReadTimeout = new TimeSpan(0, 0, 4);
            // setup write timeout
            // because we are writing to the UART it's recommended to set a write timeout
            // otherwise the write operation doesn't return until the requested number of bytes has been written
//            Instance._serialDevice.WriteTimeout = new TimeSpan(0, 0, 5);

            // setup event handler for receive buffer
            Instance._serialDevice.DataReceived += Instance._serialDevice_DataReceived;

            // flag that initialization was completed
            Instance._initCompleted = true;
        }

        internal void _serialDevice_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (e.EventType != SerialData.Chars
                || _serialDevice.BytesToRead == 0)
            {
                return;
            }

            bufferOffset = 0;
            dataToRead = 0;
            socketIndex = -1;

            receivingSocketData = false;
            receivingPrompt = false;
            processingPrompt = false;
            receivingSmsData = false;

            //Microsoft.SPOT.Debug.Print("*" + _serialLine.BytesToRead.ToString());
#if DEBUG_SERIAL_RECEIVE
            ////////////////////
            Microsoft.SPOT.Debug.Print("*" + _serialLine.BytesToRead.ToString());
#endif
            //if(TraceLevel > 3)
            //{
            //    Microsoft.SPOT.Debug.Print("*" + _serialLine.BytesToRead.ToString());
            //}


            try
            {
                using (var inputDataReader = new Windows.Storage.Streams.DataReader(_serialDevice.InputStream))
                {
                    inputDataReader.InputStreamOptions = Windows.Storage.Streams.InputStreamOptions.Partial;

                    while (_serialDevice.BytesToRead > 0)
                    {
                        // sanity check for buffer too small
                        if ((bufferOffset >= buffer.Length) ||
                            (receivingHttpData && (buffer.Length < dataToRead)) ||
                            (receivingHttpHeaders && (buffer.Length < dataToRead)))
                        {
                            // clone to temp buffer
                            char[] bufferTemp = (char[])buffer.Clone();

#if DEBUG_SERIAL_RECEIVE
                        Microsoft.SPOT.Debug.Print("%% nb " + (bufferTemp.Length + ((receivingHttpData || receivingHttpHeaders) ? dataToRead : _serialLine.BytesToRead) + 10));
#endif
                            //if (TraceLevel > 3)
                            //{
                            //    Microsoft.SPOT.Debug.Print("** nb " + (bufferTemp.Length + ((receivingHttpData || receivingHttpHeaders) ? dataToRead : _serialLine.BytesToRead) + 10));
                            //}

                            // resize buffer
                            buffer = new char[bufferTemp.Length + ((receivingHttpData || receivingHttpHeaders) ? dataToRead : (int)_serialDevice.BytesToRead) + 10];

                            // copy temp buffer to the new one
                            Array.Copy(bufferTemp, buffer, bufferTemp.Length);

                            // dispose temp buffer
                            bufferTemp = null;
                        }


                        // read next byte in buffer
                        buffer[bufferOffset] = (char)_serialDevice.ReadByte(inputDataReader);

                        //if (buffer[bufferOffset] == '>')
                        //{
                        //    Debug.Print(">>>>");
                        //}

                        //Debug.Print(buffer[bufferOffset].ToString());

                        // this has to start with an \r otherwise the algorithm doesn't know how to process it
                        if ((bufferOffset == 0 && buffer[0] == 0) ||
                            (buffer[0] == 'U' && bufferOffset == 0) || // UNDER_VOLTAGE_POWER_DOWN prompt doesn't start with \r
                            (buffer[0] == 'O' && bufferOffset == 0))   // OVER_VOLTAGE_POWER_DOWN prompt doesn't start with \r
                        {
                            if ((buffer[0] == 'U' && bufferOffset == 0) ||
                                (buffer[0] == 'O' && bufferOffset == 0))
                            {
                                // move buffer offset to keep first char
                                bufferOffset++;
                            }
                            // get next byte
                            // don't move buffer offset if not a special char 'U' and 'O', see comment above
                            continue;
                        }

                        if (receivingSocketData)
                        {
                            // receiving socket data

                            // check if we have a valid socket to put data
                            if (socketIndex > -1)
                            {
                                lock (((GprsSocket)_sockets[socketIndex]).inBuffer.SyncRoot)
                                {
                                    // validate if buffer has enough room to write the incoming data
                                    ((GprsSocket)_sockets[socketIndex]).inBuffer.Validate(true, dataToRead);

                                    ((GprsSocket)_sockets[socketIndex]).inBuffer.Buffer[((GprsSocket)_sockets[socketIndex]).inBuffer.WritePos] = (byte)buffer[bufferOffset];
                                    ((GprsSocket)_sockets[socketIndex]).inBuffer.Append(sizeof(byte));
                                }

#if DEBUG_SERIAL_RECEIVE
                            //////////////////
                            debugBufferOffset = 0;
                            debugBuffer[debugBufferOffset++] = buffer[bufferOffset] < 30 ? '*' : buffer[bufferOffset];
                            //////////////////
#endif
                            }

                            // decrease counter
                            dataToRead--;

                            // wait up to 2 seconds to read all required data from the buffer
                            receiverMilisecondsTimeout = 2000;

                            while (dataToRead > 0 && receiverMilisecondsTimeout > 0)
                            {

                                while (_serialDevice.BytesToRead > 0 && dataToRead > 0)
                                {
                                    buffer[bufferOffset] = (char)_serialDevice.ReadByte(inputDataReader);

#if DEBUG_SERIAL_RECEIVE
                                ////////////////////
                                debugBuffer[debugBufferOffset++] = buffer[bufferOffset] < 30 ? '*' : buffer[bufferOffset];
                                ////////////////////
#endif

                                    if (socketIndex > -1)
                                    {
                                        lock (((GprsSocket)_sockets[socketIndex]).inBuffer.SyncRoot)
                                        {
                                            ((GprsSocket)_sockets[socketIndex]).inBuffer.Buffer[((GprsSocket)_sockets[socketIndex]).inBuffer.WritePos] = (byte)buffer[bufferOffset];
                                            ((GprsSocket)_sockets[socketIndex]).inBuffer.Append(sizeof(byte));
                                        }
                                    }

                                    // decrease counter
                                    dataToRead--;
                                }

                                if (_serialDevice.BytesToRead == 0 && dataToRead > 0)
                                {
                                    // no data available and we are waiting for more bytes
                                    // timeout for next iteration
                                    receiverMilisecondsTimeout = receiverMilisecondsTimeout - receiverLoopWaitTime;

                                    // wait 
                                    Thread.Sleep(receiverLoopWaitTime);
                                }
                            }

                            // increase buffer offset
                            //bufferOffset++;

                            if (dataToRead == 0)
                            {
                                // we have all socket data

#if DEBUG_SERIAL_RECEIVE
                            StringBuilder sb = new StringBuilder();
                            foreach(char c in debugBuffer)
                            {
                                sb.Append(((byte)c).ToString("X2"));
                            }
                            Microsoft.SPOT.Debug.Print("-> " + sb.ToString());
                            sb = null;
#endif
                                //if (socketIndex > -1)
                                //{
                                //    Debug.Print("<< OK");
                                //}
                                //else
                                //{
                                //    Debug.Print("** done rcv");
                                //}
                            }
                            else
                            {
                                // some data seems to be missing
#if DEBUG_SERIAL_RECEIVE
                            Microsoft.SPOT.Debug.Print("**** " + dataToRead + "B missing");
#endif
                            }

                            // reset buffer and restart
                            buffer = new char[_serialDevice.BytesToRead + 10];
                            // clear flags and buffer
                            bufferOffset = 0;
                            processingPrompt = false;
                            receivingSocketData = false;
                            receivingPrompt = false;

                            // done here
                            continue;
                        }

                        if (receivingHttpData || receivingHttpHeaders)
                        {
                            // receiving HTTP data or HTTP headers

                            // decrease counter
                            dataToRead--;
                            // update buffer offset
                            bufferOffset++;

                            // wait up to 3 seconds to read all required data from the buffer
                            receiverMilisecondsTimeout = 3000;

                            while (dataToRead > 0 && receiverMilisecondsTimeout > 0)
                            {

                                while (_serialDevice.BytesToRead > 0 && dataToRead > 0)
                                {
                                    // get how many bytes are waiting in the UART buffer to read
                                    int bytesToRead = (int)_serialDevice.BytesToRead;

                                    // check how many bytes we are expecting to read so we don't read past the data to read length
                                    if (bytesToRead > dataToRead)
                                    {
                                        // adjustment required
                                        bytesToRead = dataToRead;
                                    }

                                    int readBytes = _serialDevice.ReadChars(inputDataReader, ref buffer, bufferOffset, bytesToRead);

                                    // adjust offset
                                    bufferOffset += readBytes;

                                    // decrease counter
                                    dataToRead -= readBytes;

                                    /////////////////////////////////////////////////////////////////////////////////
                                    // DEBUG
                                    //Microsoft.SPOT.Debug.Print("++" + readBytes.ToString() + " r " + dataToRead);
                                    /////////////////////////////////////////////////////////////////////////////////
                                }

                                if (_serialDevice.BytesToRead == 0 && dataToRead > 0)
                                {
                                    // no data available and we are waiting for more bytes
                                    // timeout for next iteration
                                    receiverMilisecondsTimeout = receiverMilisecondsTimeout - receiverLoopWaitTime;

                                    // wait 
                                    Thread.Sleep(receiverLoopWaitTime);
                                }
                            }

                            if (dataToRead == 0)
                            {
                                // we have all data

                                // add it to response queue
                                // if receiving HTTP data, trim the two initial chars from the prompt
                                //responseQueue.Add(new string(buffer, (receivingHttpData ? 2 : 0), bufferOffset - 1 - 2).Trim(new char[] { '\n', '\r' }).Trim(new char[] { '\n', '\r' }));
                                responseQueue.Add(new string(buffer, 0, bufferOffset).Trim(new char[] { '\n', '\r' }).Trim(new char[] { '\n', '\r' }));

                                if (waitingResponse)
                                {
                                    // signal response received
                                    responseReceived.Set();
                                }
                            }
                            else
                            {
                                // some data seems to be missing
                                //Debug.Print("**** " + dataToRead + "B missing");
                            }

                            // reset buffer and restart
                            buffer = new char[_serialDevice.BytesToRead + 10];
                            // clear flags and buffer
                            bufferOffset = 0;
                            receivingHttpData = false;
                            receivingHttpHeaders = false;
                            httpReadPrompt = false;
                            httpHeadPrompt = false;
                            processingPrompt = false;

                            // done here
                            continue;
                        }

                        // check if there is a RECEIVE prompt in the stream or buffer
                        // do this by checking for ',' if it's not don't bother with the rest
                        if (buffer[bufferOffset] == ',')
                        {
                            if (
                                buffer[bufferOffset - 1] == 'E' &&
                                buffer[bufferOffset - 2] == 'V' &&
                                buffer[bufferOffset - 3] == 'I' &&
                                buffer[bufferOffset - 4] == 'E' &&
                                buffer[bufferOffset - 5] == 'C' &&
                                buffer[bufferOffset - 6] == 'E' &&
                                buffer[bufferOffset - 7] == 'R' &&
                                buffer[bufferOffset - 8] == '+' &&
                                buffer[bufferOffset - 9] == '\n' &&
                                buffer[bufferOffset - 10] == '\r'
                                )
                            {
                                receivingPrompt = true;
                            }
                        }

                        // check if there is a +CMGR: prompt in the stream or buffer
                        // do this by checking for ':' 
                        if (!receivingSmsData && buffer[bufferOffset] == ':')
                        {
                            if (
                                buffer[bufferOffset - 1] == 'R' &&
                                buffer[bufferOffset - 2] == 'G' &&
                                buffer[bufferOffset - 3] == 'M' &&
                                buffer[bufferOffset - 4] == 'C' &&
                                buffer[bufferOffset - 5] == '+' &&
                                buffer[bufferOffset - 6] == '\n' &&
                                buffer[bufferOffset - 7] == '\r'
                                )
                            {
                                receivingSmsData = true;
                            }
                        }

                        // check if there is a +HTTPREAD: prompt in the stream or buffer
                        // do this by checking for ':' 
                        if (!httpReadPrompt && buffer[bufferOffset] == ':')
                        {
                            if (
                                buffer[bufferOffset - 1] == 'D' &&
                                buffer[bufferOffset - 2] == 'A' &&
                                buffer[bufferOffset - 3] == 'E' &&
                                buffer[bufferOffset - 4] == 'R' &&
                                buffer[bufferOffset - 5] == 'P' &&
                                buffer[bufferOffset - 6] == 'T' &&
                                buffer[bufferOffset - 7] == 'T' &&
                                buffer[bufferOffset - 8] == 'H' &&
                                buffer[bufferOffset - 9] == '+' &&
                                buffer[bufferOffset - 10] == '\n' &&
                                buffer[bufferOffset - 11] == '\r'
                                )
                            {
                                httpReadPrompt = true;
                            }
                        }

                        // check if there is a +HTTPHEAD: prompt in the stream or buffer
                        // do this by checking for ':' 
                        if (!httpHeadPrompt && buffer[bufferOffset] == ':')
                        {
                            if (
                                buffer[bufferOffset - 1] == 'D' &&
                                buffer[bufferOffset - 2] == 'A' &&
                                buffer[bufferOffset - 3] == 'E' &&
                                buffer[bufferOffset - 4] == 'H' &&
                                buffer[bufferOffset - 5] == 'P' &&
                                buffer[bufferOffset - 6] == 'T' &&
                                buffer[bufferOffset - 7] == 'T' &&
                                buffer[bufferOffset - 8] == 'H' &&
                                buffer[bufferOffset - 9] == '+' &&
                                buffer[bufferOffset - 10] == '\n' &&
                                buffer[bufferOffset - 11] == '\r'
                                )
                            {
                                httpHeadPrompt = true;
                            }
                        }

                        // catch the UNDER-VOLTAGE POWER DOWN prompt here
                        if (buffer[bufferOffset] == 'N' && bufferOffset == 23)
                        {
                            if (
                                buffer[bufferOffset] == 'N' &&
                                buffer[bufferOffset - 1] == 'W' &&
                                buffer[bufferOffset - 2] == 'O' &&
                                buffer[bufferOffset - 3] == 'D' &&
                                buffer[bufferOffset - 4] == ' ' &&
                                buffer[bufferOffset - 5] == 'R' &&
                                buffer[bufferOffset - 6] == 'E' &&
                                buffer[bufferOffset - 7] == 'W' &&
                                buffer[bufferOffset - 8] == 'O' &&
                                buffer[bufferOffset - 9] == 'P' &&
                                buffer[bufferOffset - 10] == ' ' &&
                                buffer[bufferOffset - 11] == 'E' &&
                                buffer[bufferOffset - 12] == 'G' &&
                                buffer[bufferOffset - 13] == 'A' &&
                                buffer[bufferOffset - 14] == 'T' &&
                                buffer[bufferOffset - 15] == 'L' &&
                                buffer[bufferOffset - 16] == 'O' &&
                                buffer[bufferOffset - 17] == 'V' &&
                                buffer[bufferOffset - 18] == '-' &&
                                buffer[bufferOffset - 19] == 'R' &&
                                buffer[bufferOffset - 20] == 'E' &&
                                buffer[bufferOffset - 21] == 'D' &&
                                buffer[bufferOffset - 22] == 'N' &&
                                buffer[bufferOffset - 23] == 'U'
                                )
                            {
                                responseQueue.Add(new string(buffer, 0, bufferOffset + 1));

                                // signal prompt in queue
                                promptInQueue.Set();

                                // done here, anything else that may be in the buffer doesn't matter
                                return;
                            }
                        }

                        // catch the OVER-VOLTAGE POWER DOWN prompt here
                        if (buffer[bufferOffset] == 'N' && bufferOffset == 22)
                        {
                            if (
                                buffer[bufferOffset] == 'N' &&
                                buffer[bufferOffset - 1] == 'W' &&
                                buffer[bufferOffset - 2] == 'O' &&
                                buffer[bufferOffset - 3] == 'D' &&
                                buffer[bufferOffset - 4] == ' ' &&
                                buffer[bufferOffset - 5] == 'R' &&
                                buffer[bufferOffset - 6] == 'E' &&
                                buffer[bufferOffset - 7] == 'W' &&
                                buffer[bufferOffset - 8] == 'O' &&
                                buffer[bufferOffset - 9] == 'P' &&
                                buffer[bufferOffset - 10] == ' ' &&
                                buffer[bufferOffset - 11] == 'E' &&
                                buffer[bufferOffset - 12] == 'G' &&
                                buffer[bufferOffset - 13] == 'A' &&
                                buffer[bufferOffset - 14] == 'T' &&
                                buffer[bufferOffset - 15] == 'L' &&
                                buffer[bufferOffset - 16] == 'O' &&
                                buffer[bufferOffset - 17] == 'V' &&
                                buffer[bufferOffset - 18] == '-' &&
                                buffer[bufferOffset - 19] == 'R' &&
                                buffer[bufferOffset - 20] == 'E' &&
                                buffer[bufferOffset - 21] == 'V' &&
                                buffer[bufferOffset - 22] == 'O'
                                )
                            {
                                responseQueue.Add(new string(buffer, 0, bufferOffset + 1));

                                // signal prompt in queue
                                promptInQueue.Set();

                                // done here, anything else that may be in the buffer doesn't matter
                                return;
                            }
                        }

                        // check for send prompt
                        if (buffer[bufferOffset] == ' ')
                        {
                            // check for '> ' sequence
                            if (buffer[bufferOffset - 1] == '>')
                            {
                                // > prompt
                                responseQueue.Add(new string(buffer, 2, bufferOffset - 1));

                                if (waitingResponse)
                                {
                                    // signal response received
                                    responseReceived.Set();
                                }
                                else
                                {
                                    // signal prompt in queue
                                    promptInQueue.Set();
                                }

                                // done here, don't process anything else anyways it shouldn't be there anything else in the buffer
                                break;
                            }
                        }

                        // check for send prompt of file storage (this one doesn't have a space after the '>' char)
                        // check for '>' char and UART buffer is empty
                        if (buffer[bufferOffset] == '>' && _serialDevice.BytesToRead == 0)
                        {
                            // > prompt
                            responseQueue.Add(new string(buffer, bufferOffset, 1));

                            if (waitingResponse)
                            {
                                // signal response received
                                responseReceived.Set();
                            }
                            else
                            {
                                // signal prompt in queue
                                promptInQueue.Set();
                            }

                            // done here, don't process anything else anyways it shouldn't be there anything else in the buffer
                            break;
                        }

                        // check if there is CONNECT prompt in the stream or buffer
                        // do this by checking for 'T' and UART buffer empty
                        if (!httpHeadPrompt && buffer[bufferOffset] == 'T')
                        {
                            if (
                                buffer[bufferOffset - 1] == 'C' &&
                                buffer[bufferOffset - 2] == 'E' &&
                                buffer[bufferOffset - 3] == 'N' &&
                                buffer[bufferOffset - 4] == 'N' &&
                                buffer[bufferOffset - 5] == 'O' &&
                                buffer[bufferOffset - 6] == 'C' &&
                                buffer[bufferOffset - 7] == '\n' &&
                                buffer[bufferOffset - 8] == '\r'
                                )
                            {
                                // CONNECT prompt
                                responseQueue.Add(new string(buffer, 2, bufferOffset - 1));

                                if (waitingResponse)
                                {
                                    // signal response received
                                    responseReceived.Set();
                                }
                                else
                                {
                                    // signal prompt in queue
                                    promptInQueue.Set();
                                }

                                // done here, don't process anything else anyways it shouldn't be there anything else in the buffer
                                break;

                            }
                        }

                        // check for start/end of prompt
                        if (buffer[bufferOffset] == '\n' && bufferOffset >= 1)
                        {
                            // check for \r\n sequence
                            if (buffer[bufferOffset - 1] == '\r')
                            {
                                if (receivingSmsData)
                                {
                                    // we are receiving SMS data
                                    // don't process this CR+LF
                                    // clear receivingSmsData flag so the next CR+LF is treated like a regular prompt
                                    receivingSmsData = false;

                                    // increase buffer offset
                                    bufferOffset++;

                                    // get next byte
                                    continue;
                                }
                                else if (!processingPrompt)
                                {
                                    // this must be a prompt start
                                    processingPrompt = true;

                                    // increase buffer offset
                                    bufferOffset++;

                                    // get next byte
                                    continue;
                                }
                                else
                                {
                                    // prompt ended

                                    // receive prompt active?
                                    if (receivingPrompt)
                                    {
                                        try
                                        {
                                            tempString = new string(buffer, 2, bufferOffset - 1 - 3);
                                            splitString = tempString.Split(',');
                                            tempString = null;

                                            // connection handler 
                                            socketIndex = int.Parse(splitString[1]);

                                            // how many data bytes we have to read 
                                            dataToRead = int.Parse(splitString[2]);

                                            // dispose var
                                            splitString = null;

                                            // set flag that we are receiving socket data
                                            receivingSocketData = true;

#if DEBUG_SERIAL_RECEIVE
                                        Microsoft.SPOT.Debug.Print("<< rcv " + dataToRead.ToString() + "B");
                                        debugBuffer = new char[dataToRead + 5];
                                        debugBufferOffset = 0;

#endif

                                            // check if socket is available and clear its buffer
                                            if (!_sockets.Contains(socketIndex))
                                            {
                                                // socket it not available!!
                                                socketIndex = -1;

                                                // still need to receive data or it will clutter the buffer
                                            }
                                        }
                                        catch { };

                                        // increase buffer offset
                                        bufferOffset++;

                                        continue;
                                    }
                                    // HTTP read prompt active?
                                    else if (httpReadPrompt)
                                    {
                                        // try to get the number of bytes to be read
                                        try
                                        {
                                            tempString = new string(buffer, 13, bufferOffset - 1 - 13);

                                            dataToRead = int.Parse(tempString);

                                            // dispose var
                                            tempString = null;

                                            // set flag that we are receiving HTTP data
                                            receivingHttpData = true;
                                        }
                                        catch { };

                                        // reset buffer offset
                                        bufferOffset = 0;

                                        continue;
                                    }
                                    // HTTP head prompt active?
                                    else if (httpHeadPrompt)
                                    {
                                        // try to get the number of bytes to be read
                                        try
                                        {
                                            tempString = new string(buffer, 13, bufferOffset - 1 - 13);

                                            dataToRead = int.Parse(tempString);

                                            // dispose var
                                            tempString = null;

                                            // set flag that we are receiving HTTP headers
                                            receivingHttpHeaders = true;
                                        }
                                        catch { };

                                        // reset buffer offset
                                        bufferOffset = 0;

                                        continue;
                                    }
                                    else
                                    {
                                        // build string from buffer and add it to list, clear all leading and trailing CR and LF
                                        responseQueue.Add(new string(buffer, 2, bufferOffset - 1 - 2).Trim(new char[] { '\n', '\r' }).Trim(new char[] { '\n', '\r' }));

                                        if (waitingResponse)
                                        {
                                            // signal response received
                                            responseReceived.Set();
                                        }
                                        else
                                        {
                                            // signal prompt in queue
                                            promptInQueue.Set();

                                            //if (TraceLevel > 3)
                                            //{
                                            //    Microsoft.SPOT.Debug.Print("*! " + bufferOffset);
                                            //}
                                        }

                                        if (_serialDevice.BytesToRead > 0)
                                        {
                                            // there are more bytes waiting to be processed in the buffer
                                            // reset buffer and restart
                                            buffer = new char[_serialDevice.BytesToRead + 10];
                                            bufferOffset = 0;

                                            processingPrompt = false;

                                            continue;
                                        }
                                        else
                                        {
                                            // done here
                                            return;
                                        }
                                    }
                                }
                            }
                        }

                        // increase buffer offset
                        bufferOffset++;
                    }
                }
            }
            catch { }
            finally
            {
                //Debug.Print(responseQueue.Count + " waiting");

                //Debug.GC(true);
            }
        }

#pragma warning disable 1591 // disable warning for Missing XML comment
        ~SIM800H()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
#pragma warning restore 1591

        /// <summary>
        /// Disposed of all resources
        /// </summary>
        /// <param name="disposing"><c>True</c> if managed resources should be released</param>
        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                // Release managed resources
                try
                {
                    _asyncTaskQueueThread.Abort();
                    _asyncTaskQueueThread.Join();
                    _serialDevice.Dispose();
                }
                finally
                {
                    Instance.Release();
                }
            }

            _disposed = true;
        }

#endregion

#region Async methods and handlers

        /// <summary>
        /// Waits for the current module command transaction to finish then locks the port
        /// <remarks>Blocks until the lock has been asserted, used to ensure that asynchronous transactions do not contaminate synchronous transactions</remarks>
        /// </summary>
        internal void Grab()
        {
            while (true)
            {
                lock (_lock)
                {
                    if (!_externalLock)
                    {
                        _externalLock = true;
                        //Console.WriteLine(">>>");
                        return;
                    }
                }

                Thread.Sleep(10);
            }
        }

        /// <summary>
        /// Releases the lock on the serial port 
        /// <remarks>Used to ensure that asynchronous transactions do not contaminate synchronous transactions</remarks>
        /// </summary>
        internal void Release()
        {
            lock (_lock)
            {
                _externalLock = false;
                //Console.WriteLine("<<<");
            }
        }

        /// <summary>
        /// The thread used to process asynchronous queries
        /// </summary>
        private void AsyncTaskQueueThread()
        {
            while (true)
            {
                if (_asyncTaskQueue.Count > 0 && !_externalLock)
                {
                    lock (_lock)
                    {
                        if (!_externalLock)
                        {
                            DeviceAsyncResult asyncResult = (DeviceAsyncResult)_asyncTaskQueue.Dequeue();

                            asyncResult.Process();

                            continue;
                        }
                    }
                }

                Thread.Sleep(10);
            }
        }

        /// <summary>
        /// Adds an AsyncResult to the asynchronous task queue
        /// </summary>
        /// <param name="asyncResult">the AsyncResult to add</param>
        internal void AddAsyncTask(DeviceAsyncResult asyncResult)
        {
            lock (_lock)
            {
                _asyncTaskQueue.Enqueue(asyncResult);
            }
        }

#endregion

#region Communication methods

        /// <summary>
        /// Sends an AT command to the device
        /// </summary>
        /// <returns>
        /// ReturnedState.OK - Command was sent
        /// ReturnedState.ModuleIsOff - Module is off
        /// ReturnedState.Error - Serial line is not open
        /// ReturnedState.InvalidCommand - Invalid AT command
        /// </returns>
        internal AtCommandResult SendAT()
        {
            return SendATCommand(Prompts.AT, false, false, true, false, 1000, false);
        }

        /// <summary>
        /// Sends a AT command to the device. It automatically appends the carriage return.
        /// </summary>
        /// <param name="atCommand">String with the AT command. See SIM800H manual for reference.</param>
        /// <param name="milisecondsTimeout">Timeout to complete this AT command.</param>
        /// <returns>
        /// ReturnedState.OK - Command was sent
        /// ReturnedState.ModuleIsOff - Module is off
        /// ReturnedState.Error - Serial line is not open
        /// ReturnedState.InvalidCommand - Invalid AT command
        /// </returns>
        internal AtCommandResult SendATCommandAndWaitForResponse(string atCommand, int milisecondsTimeout = 1000)
        {
            return SendATCommand(atCommand, false, true, true, false, milisecondsTimeout);
        }

        /// <summary>
        /// Sends a AT command to the device. It automatically appends the carriage return.
        /// </summary>
        /// <param name="atCommand">String with the AT command. See SIM800H manual for reference.</param>
        /// <returns>
        /// ReturnedState.OK - Command was sent
        /// ReturnedState.DeviceIsOff - Device is off
        /// ReturnedState.Error - Serial line is not open
        /// ReturnedState.InvalidCommand - Invalid AT command
        /// </returns>
        internal AtCommandResult SendATCommandAndDontWaitReply(string atCommand)
        {
            return SendATCommand(atCommand, false, false, true, true);
        }

        /// <summary>
        /// Sends a AT command to the SIM800H. It automatically appends the carriage return.
        /// </summary>
        /// <param name="atCommand">String with the AT command. See SIM800H manual for reference.</param>
        /// <param name="milisecondsTimeout">Timeout to complete this AT command.</param>
        /// <returns>
        /// ReturnedState.OK - Command was sent
        /// ReturnedState.DeviceIsOff - Device is off
        /// ReturnedState.Error - Serial line is not open
        /// ReturnedState.InvalidCommand - Invalid AT command
        /// </returns>
        internal AtCommandResult SendATCommand(string atCommand, int milisecondsTimeout = 1000)
        {
            return SendATCommand(atCommand, false, false, true, false, milisecondsTimeout);
        }

        internal AtCommandResult SendATCommand(string atCommand, int milisecondsTimeout = 1000, bool appendCR = false, bool dontWakeup = false)
        {
            return SendATCommand(atCommand, false, true, true, false, milisecondsTimeout, appendCR, dontWakeup);
        }

        internal AtCommandResult SendATCommand(string atCommand)
        {
            return SendATCommand(atCommand, true, false, true, false);
        }

        private AtCommandResult SendATCommand(string atCommand, bool privateCall, bool rawResponse, bool asyncCall, bool noReplyExpected, int milisecondsTimeout = 1000, bool appendCR = true, bool dontWakeup = false)
        {
            try
            {
                // can't execute if device is OFF and this is not a private call when power on sequence is running 
                // exception is when command is changing power mode of device
                if (!privateCall && _powerStatus != PowerStatus.PowerOnSequenceIsRunning)
                {
                    if ((atCommand.IndexOf(Prompts.CFUN_1_PROMPT) < 0) &&
                        (_powerStatus != PowerStatus.Minimum) || (_powerStatus != PowerStatus.FlightMode))
                    {
                        // OK to execute command
                    }
                    else
                    {
                        if (_powerStatus != PowerStatus.On) return new AtCommandResult(ReturnedState.DeviceIsOff);
                    }
                }

                // validate if string start with an AT
                //if (atCommand.IndexOf(Prompts.AT) < 0) return new ATCommandResult(ReturnedState.InvalidCommand);

                // Append carriage return, if requested and is not already there
                if (appendCR && atCommand.IndexOf('\r') < 0)
                {
                    atCommand += "\r";
                }

                if (!asyncCall)
                {
                    Instance.Grab();
                }

                // need to wait for a reply?
                if (!noReplyExpected)
                {
                    // reset event
                    responseReceived.Reset();

                    // set flag
                    waitingResponse = true;
                }

                //// Check if serial line is open
                //if (_serialDevice.IsOpen)
                //{
                //if (!dontWakeup && atCommand != Prompts.AT)
                //{
                //    _serialDevice.WriteBytes(new byte[] { 0 }, 0, 1);
                //    _serialDevice.WriteBytes(new byte[] { 0 }, 0, 1);
                //    _serialDevice.OutputStream.Flush();
                //    Thread.Sleep(100);
                //}

                if (atCommand.Length < 100)
                {
                    _serialDevice.Write(atCommand);
                }
                else
                {
                    // need to send this in batches
                    // write data in chunks of 64 bytes because of buffers size
                    int index = 0;
                    int chunkSize = 64;

                    while (index < atCommand.Length)
                    {
                        // adjust chunk size
                        chunkSize = System.Math.Min(chunkSize, atCommand.Length - index);

                        // send chunk writing directly to UART
                        _serialDevice.Write(atCommand.Substring(index, chunkSize));

                        // update index
                        index += chunkSize;
                    }
                }

                    //Debug.Print("SENT: " + atCommand);
                //}
                //else
                //{
                //    return new AtCommandResult(ReturnedState.Error);
                //}

                // need to wait for a reply?
                if (noReplyExpected)
                {
                    // done here
                    return new AtCommandResult(ReturnedState.OK);
                }

                int promptOKIndex = -1;

                // wait for device response 
                while (milisecondsTimeout > 0)
                {
                    if (responseReceived.WaitOne(receiverLoopWaitTime, false))
                    {
                        // need to lock queue because it can be changed on another thread
                        lock (responseQueue.SyncRoot)
                        {
                            // check if there is any response available 
                            if (responseQueue.Count > 0)
                            {
                                // OK prompt
                                promptOKIndex = responseQueue.LastIndexOf(Prompts.OK);

                                // OK response
                                if (promptOKIndex > -1)
                                {
                                    // need to send back any reply from device?
                                    if (rawResponse)
                                    {
                                        // yes, check if it's there
                                        if (promptOKIndex >= 1)
                                        {
                                            // get response data
                                            tString = (string)responseQueue[promptOKIndex - 1];
                                            // remove OK prompt from response queue
                                            responseQueue.RemoveAt(promptOKIndex);
                                            // remove response data from response queue
                                            responseQueue.RemoveAt(promptOKIndex - 1);

                                            return new AtCommandResult(ReturnedState.OK, tString);
                                        }
                                        else
                                        {
                                            // don't seem to have response data..
                                            // remove OK prompt
                                            responseQueue.RemoveAt(promptOKIndex);

                                            return new AtCommandResult(ReturnedState.OK, "");
                                        }
                                    }
                                    else
                                    {
                                        // no, just remove OK prompt
                                        responseQueue.RemoveAt(promptOKIndex);

                                        // return OK 
                                        return new AtCommandResult(ReturnedState.OK);
                                    }
                                }

                                // CONNECT response
                                if (responseQueue.FindAndRemove(Prompts.CONNECT) != null)
                                {
                                    // return CONNECT 
                                    return new AtCommandResult(ReturnedState.OK, Prompts.CONNECT);
                                }

                                // ERROR response
                                if (responseQueue.FindAndRemove(Prompts.ERROR) != null)
                                {
                                    // return ERROR 
                                    return new AtCommandResult(ReturnedState.Error);
                                }

                                // send prompt
                                if (responseQueue.FindAndRemove(Prompts.SendPrompt) != null)
                                {
                                    // return prompt 
                                    return new AtCommandResult(ReturnedState.OK, Prompts.SendPrompt);
                                }
                                
                                // send prompt for file storage FSWRITE
                                if (responseQueue.FindAndRemove(">") != null)
                                {
                                    // return prompt 
                                    return new AtCommandResult(ReturnedState.OK, Prompts.SendPrompt);
                                }

                                // send prompt
                                if (responseQueue.FindAndRemove(Prompts.DonwloadPrompt) != null)
                                {
                                    // return prompt 
                                    return new AtCommandResult(ReturnedState.OK, Prompts.DonwloadPrompt);
                                }

                                // check if this command is 'AT+CIFSR\r'
                                if (atCommand == Prompts.AT + Prompts.CIFSR + "\r")
                                {
                                    //... check for a response that looks like an IP address
                                    foreach (object s in responseQueue)
                                    {
                                        if ((promptOKIndex = responseQueue.FindItemThatLooksIpAddress()) > -1)
                                        {
                                            // get response data
                                            tString = (string)responseQueue[promptOKIndex];
                                            // remove IP from response queue
                                            responseQueue.RemoveAt(promptOKIndex);

                                            return new AtCommandResult(ReturnedState.OK, tString);
                                        }
                                    }
                                }

                                // CME ERROR response
                                promptOKIndex = responseQueue.FindItemThatContains(Prompts.CMEERROR);
                                if (promptOKIndex > -1)
                                {
                                    // get error detail
                                    tString = (string)responseQueue[promptOKIndex];
                                    // remove prompt from response queue
                                    responseQueue.RemoveAt(promptOKIndex);

                                    // return ERROR 
                                    return new AtCommandResult(ReturnedState.Error, tString.Substring(12));
                                }

                            }
                        }
                    }

                    //Thread.Sleep(250);

                    // loop this each receiverLoopWaitTime (ms)
                    milisecondsTimeout = milisecondsTimeout - receiverLoopWaitTime;

                    // if we reach here there was no response from device, something went wrong...
                }

                return new AtCommandResult(ReturnedState.NoReply);
            }
            catch { }
            finally
            {
                // clear flag
                waitingResponse = false;

                if (!asyncCall)
                {
                    Instance.Release();
                }

                if (responseQueue.Count > 0)
                {
                    // signal event for prompt in queue
                    promptInQueue.Set();
                }
            }

            return new AtCommandResult(ReturnedState.Error);
        }

#endregion

#region Serial reader thread

        internal void run()
        {
            int tInt, lenght;
            string[] splitString;
            string response;

            while (true)
            {
                // cleanup
                //Debug.GC(true);

                // wait for any response
                if (responseQueue.Count == 0)
                {
                    promptInQueue.WaitOne();
                }

                try
                {

                    lock (responseQueue.SyncRoot)
                    {
                        // get next response, if any
                        if (responseQueue.Count == 0)
                        {
                            continue;
                        }

                        //if (TraceLevel > 3)
                        //{
                        //    Debug.Print("~");
                        //}

                        // get next prompt
                        response = (string)responseQueue[0];
                        // remove it from queue
                        responseQueue.RemoveAt(0);

                        //if (TraceLevel > 4)
                        //{
                        //    Debug.Print(">> " + response);
                        //}
                    }

#region GSM Network Registration (CREG)

                    //if (response.IndexOf(Prompts.CREG) > -1)
                    if (response.IndexOf(Prompts.CREG) > -1)
                    {
                        try
                        {
                            tInt = int.Parse(response.Substring(Prompts.CREG.Length).Trim());

                            // update property
                            // property changed event is raised in the setter
                            switch (tInt)
                            {
                                // NotSearching
                                case 0:
                                    GsmNetworkRegistration = NetworkRegistrationState.NotSearching;
                                    break;

                                // Registered
                                case 1:
                                    GsmNetworkRegistration = NetworkRegistrationState.Registered;
                                    break;

                                // Searching
                                case 2:
                                    GsmNetworkRegistration = NetworkRegistrationState.Searching;
                                    break;

                                // RegistrationDenied
                                case 3:
                                    GsmNetworkRegistration = NetworkRegistrationState.RegistrationDenied;
                                    break;

                                // Roaming
                                case 5:
                                    GsmNetworkRegistration = NetworkRegistrationState.Roaming;
                                    break;

                                // Unknown
                                case 4:
                                default:
                                    GsmNetworkRegistration = NetworkRegistrationState.Unknown;
                                    break;
                            }

                            // done here
                            continue;
                        }
                        catch (Exception)
                        {
                            _gsmNetworkRegistration = NetworkRegistrationState.Error;
                        }
                    }

#endregion

#region GPRS Network Registration (CGREG)

                    //if (response.IndexOf(Prompts.CGREG) > -1)
                    if (response.IndexOf(Prompts.CGREG) > -1)
                    {
                        try
                        {
                            tInt = int.Parse(response.Substring(Prompts.CGREG.Length).Trim());

                            // update property
                            // property changed event is raised in the setter
                            switch (tInt)
                            {
                                // NotSearching
                                case 0:
                                    GprsNetworkRegistration = NetworkRegistrationState.NotSearching;

                                    // all GPRS bearers must be off too
                                    GprsIpAppsBearerIsOpen = false;
                                    GprsMmsBearerIsOpen = false;
                                    GprsSocketsBearerIsOpen = false;

                                    // clear IP address
                                    IpAddress = "";
                                   
                                    break;

                                // Registered
                                case 1:
                                    GprsNetworkRegistration = NetworkRegistrationState.Registered;

                                    // clear IP address
                                    IpAddress = "";

                                    break;

                                // Searching
                                case 2:
                                    GprsNetworkRegistration = NetworkRegistrationState.Searching;

                                    // all GPRS bearers must be off too
                                    GprsIpAppsBearerIsOpen = false;
                                    GprsMmsBearerIsOpen = false;
                                    GprsSocketsBearerIsOpen = false;

                                    // clear IP address
                                    IpAddress = "";

                                    break;

                                // RegistrationDenied
                                case 3:
                                    GprsNetworkRegistration = NetworkRegistrationState.RegistrationDenied;

                                    // all GPRS bearers must be off too
                                    GprsIpAppsBearerIsOpen = false;
                                    GprsMmsBearerIsOpen = false;
                                    GprsSocketsBearerIsOpen = false;

                                    // clear IP address
                                    IpAddress = "";

                                    break;

                                // Roaming
                                case 5:
                                    GprsNetworkRegistration = NetworkRegistrationState.Roaming;

                                    // clear IP address
                                    IpAddress = "";

                                    break;

                                // Unknown
                                case 4:
                                default:
                                    GprsNetworkRegistration = NetworkRegistrationState.Unknown;

                                    // all GPRS bearers must be off too
                                    GprsIpAppsBearerIsOpen = false;
                                    GprsMmsBearerIsOpen = false;
                                    GprsSocketsBearerIsOpen = false;

                                    // clear IP address
                                    IpAddress = "";

                                    break;
                            }

                            // done here
                            continue;
                        }
                        catch (Exception)
                        {
                            _ipAddress = "";

                            //Console.WriteLine("CGREG ERROR");
                        }
                    }

#endregion

#region Sms Received (CMTI)

                    if (response.IndexOf(Prompts.CMTI) > -1)
                    {
                        try
                        {
                            splitString = response.Substring(Prompts.CMTI.Length).Trim().Split(new char[] { ',' });

                            int index = int.Parse(splitString[1]);

                            // dispose vars
                            splitString = null;

                            // raise event on a thread 
                            new Thread(() => { Thread.Sleep(1000); SmsProvider.OnSmsReceived((byte)index); }).Start();
                            //SmsClient.OnSMSReceived(tInt);

                            // done here
                            continue;
                        }
                        catch (Exception)
                        {
                            //Console.WriteLine("CMTI retrieve ERROR");
                        }
                    }

#endregion

#region HTTP action Received (HTTPACTION)

                    if (response.IndexOf(Prompts.HTTPACTION_PROMPT) > -1)
                    {
                        try
                        {
                            splitString = response.Substring(Prompts.HTTPACTION_PROMPT.Length).Trim().Split(new char[] { ',' });

                            tInt = int.Parse(splitString[0]);

                            HttpAction method;
                            switch (tInt)
                            {
                                case 0:
                                    method = HttpAction.GET;
                                    break;

                                case 1:
                                    method = HttpAction.POST;
                                    break;

                                case 2:
                                    method = HttpAction.HEAD;
                                    break;

                                case 3:
                                    method = HttpAction.DELETE;
                                    break;

                                default:
                                    //Console.WriteLine("HTTPACTION unknown action code: " + tInt);
                                    // done here
                                    continue;
                            }

                            tInt = int.Parse(splitString[1]);
                            lenght = int.Parse(splitString[2]);

                            // dispose vars
                            splitString = null;

                            // if feature enabled...
                            if (HttpClient != null)
                            {
                                // raise event on a thread
                                new Thread(() =>
                                {
                                    HttpClient.OnHttpActionReceived(new HttpActionResult(method, tInt, lenght));
                                }).Start();
                            }

                            // done here
                            continue;
                        }
                        catch (Exception)
                        {
                            //Console.WriteLine("HTTPACTION retrieve ERROR");
                        }
                    }

#endregion

#region Send Sms Reference Result

                    if (response.IndexOf(Prompts.CMGS_PROMPT) > -1)
                    {
                        try
                        {
                            tInt = int.Parse(response.Substring(Prompts.CMGS_PROMPT.Length).Trim());

                            // raise event On a thread
                            new Thread(() => { Instance.OnSmsSentReferenceReceived(tInt); }).Start();

                            // done here
                            continue;
                        }
                        catch (Exception)
                        {
                            //Console.WriteLine("+CMGS retrieve ERROR");
                        }
                    }

#endregion

#region Sms status deliver Result

                    if (response.IndexOf(Prompts.CDS) > -1)
                    {
                        try
                        {
                            splitString = response.Trim().Split(new char[] { ',' });

                            if (splitString.Length == 9)
                            {
                                MessageStatusReport report = new MessageStatusReport();

                                report.FO = int.Parse(splitString[0].Trim());
                                report.MessageReference = int.Parse(splitString[1].Trim());
                                report.ReceivingNumber = splitString[2].Trim('\"');
                                report.TORA = int.Parse(splitString[3].Trim());

                                DateTime timestamp = new DateTime(int.Parse(splitString[4].Substring(1, 2)) + 2000, //Year
                                int.Parse(splitString[4].Substring(4, 2)), //Month
                                int.Parse(splitString[4].Substring(7, 2)), // Day
                                int.Parse(splitString[5].Substring(0, 2)), // Hour
                                int.Parse(splitString[5].Substring(3, 2)), // Minute
                                int.Parse(splitString[5].Substring(6, 2))); // Second
                                report.ServiceCenterTimeStamp = timestamp;

                                timestamp = new DateTime(int.Parse(splitString[6].Substring(1, 2)) + 2000, //Year
                                int.Parse(splitString[6].Substring(4, 2)), //Month
                                int.Parse(splitString[6].Substring(7, 2)), // Day
                                int.Parse(splitString[7].Substring(0, 2)), // Hour
                                int.Parse(splitString[7].Substring(3, 2)), // Minute
                                int.Parse(splitString[7].Substring(6, 2))); // Second
                                report.DelieveredTimeStamp = timestamp;

                                report.ST = int.Parse(splitString[8].Trim());

                                // dispose vars
                                splitString = null;

                                // raise event on a thread
                                new Thread(() => { SmsProvider.OnSmsStatusReceived(report); }).Start();
                            }

                            // done here
                            continue;
                        }
                        catch (Exception)
                        {
                            //Console.WriteLine("+CDS retrieve ERROR");
                        }
                    }

#endregion

#region Check Power On Error (NORMAL POWER DOWN)

                    if (response.IndexOf(Prompts.NORMAL_POWER_DOWN) > -1)
                    {
                        // set nodule power status
                        PowerStatus = PowerStatus.Off;

                        // done here
                        continue;
                    }

#endregion

#region Check Call Ready prompt

                    if (response.IndexOf(Prompts.Call_Ready) > -1)
                    {
                        // raise event on a thread
                        new Thread(() => { OnCallReady(); }).Start();

                        continue;
                    }

#endregion

#region Check SMS Ready prompt

                    if (response.IndexOf(Prompts.SMS_Ready) > -1)
                    {
                        // raise event on a thread
                        new Thread(() =>
                        {
                            //    Thread.Sleep(100);
                            //    //SmsClient.GetType();
                            //    //SmsClient.Init();
                            Instance.OnSmsReady();
                        }).Start();
                        //OnSmsReady(); 

                        continue;
                    }

#endregion

#region Check GPRS disconnected by network (+PDP: DEACT)

                    if (response.IndexOf(Prompts.PDP_DEACT) > -1)
                    {
                        // raise event on a thread
                        new Thread(() => { GprsProvider.OnGprsSocketsBearerStateChanged(false); }).Start();

                        // done here
                        continue;
                    }

#endregion

#region Check IP Bearer deactivated (+SAPBR)

                    if (response.IndexOf(Prompts.SAPBR_DEACT) > -1)
                    {
                        try
                        {
                            // clear IP address
                            _ipAddress = "";

                            // set Gprs context as closed, event is raised in the property setter
                            GprsSocketsBearerIsOpen = false;
                            GprsIpAppsBearerIsOpen = false;
                            GprsMmsBearerIsOpen = false;

                            // done here
                            continue;
                        }
                        catch (Exception)
                        {
                        }
                    }
#endregion

#region SIM card insert/removed (CSMINS)

                    if (response.IndexOf(Prompts.CSMINS_PROMPT) > -1)
                    {
                        try
                        {
                            splitString = response.Substring(Prompts.CSMINS_PROMPT.Length).Trim().Split(new char[] { ',' });

                            int status = int.Parse(splitString[1]);

                            // dispose vars
                            splitString = null;

                            if (status == 0)
                            {
                                // SIM card removed
                                // raise event on a thread
                                new Thread(() => { Instance.OnSimCardStatusChanged(SimCardStatus.Removed); }).Start();
                            }
                            else if (status == 1)
                            {
                                // SIM card inserted
                                // raise event on a thread
                                new Thread(() => { Instance.OnSimCardStatusChanged(SimCardStatus.Inserted); }).Start();
                            }

                            // done here
                            continue;
                        }
                        catch (Exception)
                        {
                            //Console.WriteLine("CSMINS retrieve ERROR");
                        }
                    }

#endregion

#region Voltage conditions monitor

                    if (response.IndexOf(Prompts.UNDER_VOLTAGE_WARNING) > -1)
                    {
                        // raise event on a thread
                        new Thread(() => { Instance.OnWarningConditionTriggered(WarningCondition.UnderVoltageWarning); }).Start();

                        continue;
                    }

                    if (response.IndexOf(Prompts.OVER_VOLTAGE_WARNING) > -1)
                    {
                        // raise event on a thread
                        new Thread(() => { Instance.OnWarningConditionTriggered(WarningCondition.OverVoltageWarning); }).Start();

                        continue;
                    }

                    if (response.IndexOf(Prompts.UNDER_VOLTAGE_POWER_DOWN) > -1)
                    {
                        // raise event on a thread
                        new Thread(() => { Instance.OnWarningConditionTriggered(WarningCondition.UnderVoltagePowerDown); }).Start();

                        continue;
                    }

                    if (response.IndexOf(Prompts.OVER_VOLTAGE_POWER_DOWN) > -1)
                    {
                        // raise event on a thread
                        new Thread(() => { Instance.OnWarningConditionTriggered(WarningCondition.OverVoltagePowerDown); }).Start();

                        continue;
                    }

#endregion

#region Temperature condition monitor

                    if (response.IndexOf(Prompts.CMTE) > -1)
                    {
                        try
                        {
                            tInt = int.Parse(response.Substring(Prompts.CMTE.Length).Trim());

                            if (tInt == 1 || tInt == -1 || tInt == -2 || tInt == -2)
                            {
                                // raise event on a thread
                                new Thread(() => { Instance.OnWarningConditionTriggered(WarningCondition.TemperatureWarning); }).Start();
                            }

                            // done here
                            continue;
                        }
                        catch (Exception)
                        {
                            //Console.WriteLine("CMTE retrieve ERROR");
                        }
                    }

#endregion

#region GPRS sockets prompts

                    // CONNECT OK prompt: \r\n[n], CONNECT OK\r\n
                    if (response.IndexOf(Prompts.CONNECT_OK_MUX) > -1)
                    {
                        try
                        {
                            // get connection index 
                            tInt = int.Parse(response.Substring(0, 1));

                            // update client state, if connection is available
                            if (_sockets.Contains(tInt))
                            {
                                ((GprsSocket)_sockets[tInt]).Status = ConnectionStatus.Connected;
                            }

                            // done here
                            continue;
                        }
                        catch (Exception)
                        {
                        }

                    }

                    // CONNECT FAIL prompt: \r\n[n], CONNECT FAIL\r\n
                    if (response.IndexOf(Prompts.CONNECT_FAIL_MUX) > -1)
                    {
                        try
                        {
                            // get connection index 
                            tInt = int.Parse(response.Substring(0, 1));

                            // update client state, if connection is available
                            if (_sockets.Contains(tInt))
                            {
                                ((GprsSocket)_sockets[tInt]).Status = ConnectionStatus.Initial;
                            }

                            // done here
                            continue;
                        }
                        catch (Exception)
                        {
                        }
                    }

                    // CLOSE OK prompt: \r\n[n], CLOSE OK\r\n
                    if (response.IndexOf(Prompts.CLOSE_OK_MUX) > -1)
                    {
                        try
                        {
                            // get connection index 
                            tInt = int.Parse(response.Substring(0, 1));

                            // update client state, if connection is available
                            if (_sockets.Contains(tInt))
                            {
                                ((GprsSocket)_sockets[tInt]).Status = ConnectionStatus.Closed;
                            }

                            // done here
                            continue;
                        }
                        catch (Exception)
                        {
                        }
                    }

                    // CLOSE prompt: \r\n[n], CLOSE\r\n
                    if (response.IndexOf(Prompts.CLOSED_MUX) > -1)
                    {
                        try
                        {
                            // get connection index 
                            tInt = int.Parse(response.Substring(0, 1));

                            // update client state, if connection is available
                            if (_sockets.Contains(tInt))
                            {
                                ((GprsSocket)_sockets[tInt]).Status = ConnectionStatus.Closed;
                            }

                            // done here
                            continue;
                        }
                        catch (Exception)
                        {
                        }
                    }

                    // RECEIVE prompt: \r\nRECEIVE,[n],<lenght>:\r\n
                    if (response.IndexOf(Prompts.RECEIVE) > -1)
                    {
                        try
                        {
                            splitString = response.Split(new char[] { ',' });

                            tInt = int.Parse(splitString[1]);

                            // dispose var
                            splitString = null;

                            // update client state, if socket is available
                            if (_sockets.Contains(tInt))
                            {
                                // raise event on a thread
                                new Thread(() => { ((GprsSocket)_sockets[tInt]).OnDataReceived(_sockets[tInt]); }).Start();
                            }

                            // done here
                            continue;
                        }
                        catch (Exception)
                        {
                            //Console.WriteLine("RECEIVE prompt retrieve ERROR");
                        }
                    }

#endregion

#region SNTP sync request result

                    if (response.IndexOf(Prompts.CNTP) > -1)
                    {
                        try
                        {
                            tInt = int.Parse(response.Substring(Prompts.CNTP.Length + 1).Trim());

                            SyncResult result = SyncResult.Error;

                            switch (tInt)
                            {
                                case 1:
                                    result = SyncResult.SyncSuccessful;
                                    break;
                                case 61:
                                    result = SyncResult.NetworkError;
                                    break;
                                case 62:
                                    result = SyncResult.DnsError;
                                    break;
                                case 63:
                                    result = SyncResult.ConnectionError;
                                    break;
                                case 64:
                                    result = SyncResult.ServerResponseError;
                                    break;
                                case 65:
                                    result = SyncResult.ServerResponseTimeout;
                                    break;
                            }

                            // raise event On a thread
                            new Thread(() => { SntpClient.OnSntpReplyReceived(result); }).Start();

                            // done here
                            continue;
                        }
                        catch (Exception)
                        {
                            //Console.WriteLine("+CMGS retrieve ERROR");
                        }
                    }

#endregion

                    // clear trash that might be in the buffer

                    if (response.IndexOf(Prompts.OK) > -1)
                    {
                        // done here
                        continue;
                    }

                    if (response.IndexOf(Prompts.CMTI) > -1)
                    {
                        // done here
                        continue;
                    }

                    if (response.IndexOf(Prompts.CPIN_READY) > -1)
                    {
                        // done here
                        continue;
                    }

                    if (response.IndexOf(Prompts.CPIN_NOT_INSERTED) > -1)
                    {
                        // report this
                        // raise event on a thread
                        new Thread(() => { Instance.OnSimCardStatusChanged(SimCardStatus.Removed); }).Start();

                        // done here
                        continue;
                    }

                    // RDY prompt
                    if (response.IndexOf(Prompts.RDY) > -1)
                    {
                        // only process this if power on sequence is NOT running 
                        if (PowerStatus != PowerStatus.PowerOnSequenceIsRunning)
                        {
                            // done here
                            continue;
                        }
                    }

                    // +CFUN prompt
                    if (responseQueue.IndexOf(Prompts.CFUN_PROMPT) > -1)
                    {
                        // done here
                        continue;
                    }

                    // CPMS prompt
                    if (responseQueue.IndexOf(Prompts.CPMS) > -1)
                    {
                        // done here
                        continue;
                    }

                    //if (TraceLevel > 3)
                    //{
                    //    Debug.Print("~~~");
                    //}
                }
                catch(Exception ex)
                {
                    //if (TraceLevel > 3)
                    //{
                    //    Debug.Print("## " + ex.Message);
                    //}
                }
                finally
                {

                }
            }

            //if (TraceLevel > 3)
            //{
            //    Debug.Print("####");
            //}
        }

#endregion

#region Delegates and events

#region GSM Network Registration Changed
        /// <summary>
        /// Represents the delegate used for the <see cref="GsmNetworkRegistrationChanged"/> event.
        /// </summary>
        /// <param name="networkState">Current state of the GSM network registration</param>
        public delegate void GsmNetworkRegistrationChangedHandler(NetworkRegistrationState networkState);
        /// <summary>
        /// Event raised when the status of the GSM network registration changes.
        /// </summary>
        public static event GsmNetworkRegistrationChangedHandler GsmNetworkRegistrationChanged;
        private static GsmNetworkRegistrationChangedHandler onGsmNetworkRegistrationChanged;

        /// <summary>
        /// Raises the <see cref="GsmNetworkRegistrationChanged"/> event.
        /// </summary>
        /// <param name="networkState">Current state of the GSM network registration</param>
        protected virtual void OnGsmNetworkRegistrationChanged(NetworkRegistrationState networkState)
        {
            if (onGsmNetworkRegistrationChanged == null) onGsmNetworkRegistrationChanged = new GsmNetworkRegistrationChangedHandler(OnGsmNetworkRegistrationChanged);
            if (GsmNetworkRegistrationChanged != null)
            {
                GsmNetworkRegistrationChanged(networkState);
            }
        }
#endregion

#region GPRS Network Registration Changed
        /// <summary>
        /// Represents the delegate used for the <see cref="GprsNetworkRegistrationChanged"/> event.
        /// </summary>
        /// <param name="networkState">Current state of the GPRS network registration</param>
        public delegate void GprsNetworkRegistrationChangedHandler(NetworkRegistrationState networkState);
        /// <summary>
        /// Event raised when the status of the GPRS registration changes.
        /// </summary>
        public static event GprsNetworkRegistrationChangedHandler GprsNetworkRegistrationChanged;
        private GprsNetworkRegistrationChangedHandler onGprsNetworkRegistrationChanged;

        /// <summary>
        /// Raises the <see cref="GprsNetworkRegistrationChanged"/> event.
        /// </summary>
        /// <param name="networkState">Current state of the GPRS network registration</param>
        protected virtual void OnGprsNetworkRegistrationChanged(NetworkRegistrationState networkState)
        {
            if (onGprsNetworkRegistrationChanged == null) onGprsNetworkRegistrationChanged = new GprsNetworkRegistrationChangedHandler(OnGprsNetworkRegistrationChanged);
            if (GprsNetworkRegistrationChanged != null)
            {
                GprsNetworkRegistrationChanged(networkState);
            }
        }
#endregion

        //#region Incoming Call
        ///// <summary>
        ///// Represents the delegate used for the <see cref="IncomingCall"/> event.
        ///// </summary>
        ///// <param name="caller">Number of the caller</param>
        //public delegate void IncomingCallHandler(string caller);
        ///// <summary>
        ///// Event raised when the device receives an incoming call.
        ///// </summary>
        //public static event IncomingCallHandler IncomingCall;
        //private IncomingCallHandler onIncomingCall;

        ///// <summary>
        ///// Raises the <see cref="IncomingCall"/> event.
        ///// </summary>
        ///// <param name="caller">Number of the caller</param>
        //protected virtual void OnIncomingCall(string caller)
        //{
        //    if (onIncomingCall == null) onIncomingCall = new IncomingCallHandler(IncomingCall);
        //    if (IncomingCall != null)
        //    {
        //        IncomingCall(caller);
        //    }
        //}
        //#endregion

        //#region Call Ended
        ///// <summary>
        ///// Represents the delegate used for the <see cref="CallEnded"/> event.
        ///// </summary>
        ///// <param name="reason">The reason the call has ended</param>
        //public delegate void CallEndedHandler(CallEndType reason);
        ///// <summary>
        ///// Event raised when the device receives a phone activity message.
        ///// </summary>
        //public static event CallEndedHandler CallEnded;
        //private CallEndedHandler onCallEnded;

        ///// <summary>
        ///// Raises the <see cref="CallEnded"/> event.
        ///// </summary>
        ///// <param name="reason">The reason the call has ended</param>
        //protected virtual void OnCallEnded(CallEndType reason)
        //{
        //    if (onCallEnded == null) onCallEnded = new CallEndedHandler(CallEnded);
        //    if (CallEnded != null)
        //    {
        //        CallEnded(reason);
        //    }
        //}
        //#endregion

        //#region Call Connected
        ///// <summary>
        ///// Represents the delegate used for the <see cref="CallConnected"/> event.
        ///// </summary>
        ///// <param name="number"> Number to which the device is connected</param>
        //public delegate void CallConnectedHandler(string number);
        ///// <summary>
        ///// Event raised when the device receives a phone activity message.
        ///// </summary>
        //public static event CallConnectedHandler CallConnected;
        //private CallConnectedHandler onCallConnected;

        ///// <summary>
        ///// Raises the <see cref="CallConnected"/> event.
        ///// </summary>
        ///// <param name="number">Number to which the device is connected</param>
        //protected virtual void OnCallConnected(string number)
        //{
        //    if (onCallConnected == null) onCallConnected = new CallConnectedHandler(CallConnected);
        //    if (CallConnected != null)
        //    {
        //        CallConnected(number);
        //    }
        //}
        //#endregion

#region Call Ready

        /// <summary>
        /// Represents the delegate used for the <see cref="CallReady"/> event.
        /// </summary>
        public delegate void CallReadyHandler();
        /// <summary>
        /// Event raised when the device reports that is ready for calls.
        /// </summary>
        public static event CallReadyHandler CallReady;
        private static CallReadyHandler onCallReady;

        /// <summary>
        /// Raises the <see cref="CallReady"/> event.
        /// </summary>
        protected static void OnCallReady()
        {
            if (onCallReady == null) onCallReady = new CallReadyHandler(CallReady);
            if (CallReady != null)
            {
                CallReady();
            }
        }

#endregion

#region SMS Ready

        /// <summary>
        /// Represents the delegate used for the <see cref="SmsReady"/> event.
        /// </summary>
        public delegate void SmsReadyHandler();
        /// <summary>
        /// Event raised when the device reports that the SMS features are ready.
        /// </summary>
        public static event SmsReadyHandler SmsReady;
        private SmsReadyHandler onSmsReady;

        /// <summary>
        /// Raises the <see cref="SmsReady"/> event.
        /// </summary>
        protected virtual void OnSmsReady()
        {
            if (onSmsReady == null) onSmsReady = new SmsReadyHandler(SmsReady);
            if (SmsReady != null)
            {
                SmsReady();
            }
        }

#endregion


#region Sms Sent reference Received
        /// <summary>
        /// Represents the delegate used for the <see cref="SmsSentReferenceReceived"/> event.
        /// </summary>
        /// <param name="reference">reference of the Sms sent</param>
        public delegate void SmsSentReferenceReceivedHandler(int reference);
        /// <summary>
        /// Event raised when the device receives a new SMS message.
        /// </summary>
        public static event SmsSentReferenceReceivedHandler SmsSentReferenceReceived;
        private SmsSentReferenceReceivedHandler onSmsSentReferenceReceived;

        /// <summary>
        /// Raises the <see cref="SmsSentReferenceReceived"/> event.
        /// </summary>
        /// <param name="reference">reference of the Sms sent</param>
        protected virtual void OnSmsSentReferenceReceived(int reference)
        {
            if (onSmsSentReferenceReceived == null) onSmsSentReferenceReceived = new SmsSentReferenceReceivedHandler(OnSmsSentReferenceReceived);
            if (SmsSentReferenceReceived != null)
            {
                SmsSentReferenceReceived(reference);
            }
        }
#endregion

#region Power State changed

        /// <summary>
        /// Represents the delegate used for the <see cref="PowerStatusChanged"/> event.
        /// </summary>
        /// <param name="powerStatus"> new power status of the device</param>
        public delegate void PowerStatusChangedHandler(PowerStatus powerStatus);
        /// <summary>
        /// Event raised when the power status of the device changes.
        /// </summary>
        public static event PowerStatusChangedHandler PowerStatusChanged;
        private PowerStatusChangedHandler onPowerStatusChanged;

        /// <summary>
        /// Raises the <see cref="PowerStatusChanged"/> event.
        /// </summary>
        /// <param name="powerStatus"> new power status of the device</param>
        protected virtual void OnPowerStatusChanged(PowerStatus powerStatus)
        {
            if (onPowerStatusChanged == null) onPowerStatusChanged = new PowerStatusChangedHandler(PowerStatusChanged);
            if (PowerStatusChanged != null)
            {
                PowerStatusChanged(powerStatus);
            }
        }

#endregion

#region SIM card inserted/removed

        /// <summary>
        /// Represents the delegate used for the <see cref="SimCardStatusChanged"/> event.
        /// </summary>
        /// <param name="simCardStatus"> new SIM card status of the device</param>
        public delegate void SimCardStatusChangedHandler(SimCardStatus simCardStatus);
        /// <summary>
        /// Event raised when the status of the SIM card changes.
        /// </summary>
        public static event SimCardStatusChangedHandler SimCardStatusChanged;
        private SimCardStatusChangedHandler onSimCardStatusChanged;

        /// <summary>
        /// Raises the <see cref="SimCardStatusChanged"/> event.
        /// </summary>
        /// <param name="simCardStatus"> new status of the SIM card</param>
        protected virtual void OnSimCardStatusChanged(SimCardStatus simCardStatus)
        {
            if (onSimCardStatusChanged == null) onSimCardStatusChanged = new SimCardStatusChangedHandler(SimCardStatusChanged);
            if (SimCardStatusChanged != null)
            {
                SimCardStatusChanged(simCardStatus);
            }
        }

#endregion

#region Warning event

        /// <summary>
        /// Represents the delegate used for the <see cref="WarningConditionTriggered"/> event.
        /// </summary>
        /// <param name="warningCondition">The warning condition that was triggered</param>
        public delegate void WarningConditionTriggeredHandler(WarningCondition warningCondition);
        /// <summary>
        /// Event raised when there is a warning condition reported by the device.
        /// </summary>
        public static event WarningConditionTriggeredHandler WarningConditionTriggered;
        private WarningConditionTriggeredHandler onWarningConditionTriggered;

        /// <summary>
        /// Raises the <see cref="WarningConditionTriggered"/> event.
        /// </summary>
        /// <param name="warningCondition">The warning condition that was triggered</param>
        protected virtual void OnWarningConditionTriggered(WarningCondition warningCondition)
        {
            if (onWarningConditionTriggered == null) onWarningConditionTriggered = new WarningConditionTriggeredHandler(WarningConditionTriggered);
            if (WarningConditionTriggered != null)
            {
                WarningConditionTriggered(warningCondition);
            }
        }

#endregion

#endregion

#region Cellular device management

        /// <summary>
        /// Get the signal strength (RSSI) of the cellular network
        /// </summary>
        /// <returns>An instance of <see cref="SignalStrength"/> which contains a representation of the strength of the network signal</returns>
        public static SignalStrength RetrieveSignalStrength()
        {
            Eclo.nanoFramework.SIM800H.AtCommandResult signalStrenghtResult = Instance.SendATCommandAndWaitForResponse(Prompts.AT + Prompts.CSQ);

            string responseRaw = String.Empty;
            string[] response = null;

            // check if command was executed
            if (signalStrenghtResult.Result == Eclo.nanoFramework.SIM800H.ReturnedState.OK)
            {
                try
                {
                    // prompt +CSQ: <rssi>,<ber>
                    // remove prompt from response
                    responseRaw = signalStrenghtResult.Response.Substring(6);

                    // get other message details
                    response = responseRaw.Split(new char[] { ',' });

                    if (response.Length == 2)
                    {
                        int signal = int.Parse(response[0]);

                        switch (signal)
                        {
                            case 0:
                                return SignalStrength.VeryWeak;

                            case 1:
                                return SignalStrength.Weak;

                            case 31:
                                return SignalStrength.VeryStrong;

                            case 99:
                                return SignalStrength.Unknown;

                            default:
                                if (signal >= 2 && signal <= 30)
                                    return SignalStrength.Strong;
                                break;
                        }

                    }

                }
                catch (Exception)
                {
                    //Console.WriteLine("Signal Strength Error");
                }
                finally
                {
                    // dispose vars
                    responseRaw = null;
                    response = null;
                }
            }
            else if (signalStrenghtResult.Result == Eclo.nanoFramework.SIM800H.ReturnedState.NoReply)
            {
                return SignalStrength.Unknown;
            }

            return SignalStrength.Error;
        }

        /// <summary>
        /// Retrieves the operator wich the device is registered to
        /// </summary>
        /// <returns>The operator which the device is registered to</returns>
        public static string RetrieveOperator()
        {
            Eclo.nanoFramework.SIM800H.AtCommandResult operatorResult = Instance.SendATCommandAndWaitForResponse(Prompts.AT + Prompts.COPS, 2000);

            // check if command was executed
            if (operatorResult.Result == Eclo.nanoFramework.SIM800H.ReturnedState.OK)
            {
                // format: +COPS: <mode>[,<format>,<oper>]
                Instance.tString = operatorResult.Response.Substring(7);

                // get other message details
                Instance.sString = Instance.tString.Split(new char[] { ',' });

                if (Instance.sString.Length == 3)
                {
                    // clear double quotes in operator name
                    return Instance.sString[2].Trim('"');

                }

                // dispose vars
                Instance.sString = null;
                Instance.tString = null;
            }

            return "";
        }

        /// <summary>
        /// Retrieves the pin state of the SIM
        /// </summary>
        /// <returns>An instance of <see cref="PinState"/> with the current state of the PIN</returns>
        public static PinState RetrievePinState()
        {
            Eclo.nanoFramework.SIM800H.AtCommandResult pinStatusResult = Instance.SendATCommandAndDontWaitReply(Prompts.AT + Prompts.CPIN);

            // check if command was executed
            if (pinStatusResult.Result == Eclo.nanoFramework.SIM800H.ReturnedState.OK)
            {
                // prompt +CPIN:
                Instance.tString = pinStatusResult.Response.Substring(6).Trim();

                if (Instance.tString.IndexOf("READY") > -1)
                {
                    return PinState.Ready;
                }
                else if (Instance.tString.IndexOf("SIM PIN2") > -1)
                {
                    return PinState.PIN2;
                }
                else if (Instance.tString.IndexOf("SIM PUK2") > -1)
                {
                    return PinState.PUK2;
                }
                else if (Instance.tString.IndexOf("PH_SIM PIN") > -1)
                {
                    return PinState.PH_PIN;
                }
                else if (Instance.tString.IndexOf("PH_SIM PUK") > -1)
                {
                    return PinState.PH_PUK;
                }
                else if (Instance.tString.IndexOf("SIM PIN") > -1)
                {
                    return PinState.PIN;
                }
                else if (Instance.tString.IndexOf("SIM PUK") > -1)
                {
                    return PinState.PUK;
                }
                else
                {
                    return PinState.NotPresent;
                }
            }

            return PinState.Error;
        }

        /// <summary>
        /// Enable Sms status report 
        /// </summary>
        public bool SmsStatusReport
        {
            set
            {
                // get current status
                Eclo.nanoFramework.SIM800H.AtCommandResult opResult = SendATCommandAndWaitForResponse(Prompts.AT + Prompts.CSMP + "?", 2000);

                // check execution
                if (opResult.Result == Eclo.nanoFramework.SIM800H.ReturnedState.OK)
                {
                    // format: +CSMP: <fo>,<vp>,<pid>,<dcs>
                    tString = opResult.Response.Substring(7);

                    // get other message details
                    sString = tString.Split(new char[] { ',' });

                    if (sString.Length == 4)
                    {
                        // build command to set status report as requested
                        StringBuilder cmd = new StringBuilder(Prompts.AT + Prompts.CSMP + "=");

                        // set fo
                        if (value)
                        {
                            cmd.Append("49,");
                        }
                        else
                        {
                            // default value is 17
                            cmd.Append("17,");
                        }

                        // set vp
                        cmd.Append(sString[1] + ",");
                        // set pid
                        cmd.Append(sString[2] + ",");
                        // set dcs
                        cmd.Append(sString[3]);

                        opResult = SendATCommand(cmd.ToString(), 2000);
                        if (opResult.Result == Eclo.nanoFramework.SIM800H.ReturnedState.OK)
                        {
                            return;
                        }
                    }

                    // dispose vars
                    sString = null;
                    tString = null;
                }

                // failure probably should throw an exception
            }
        }

        /// <summary>
        /// Retrieves the device's IMEI 
        /// </summary>
        /// <returns>IMEI of the device</returns>
        public static string IMEI
        {
            get
            {
                var ret = Instance.SendATCommandAndWaitForResponse(Prompts.AT + Prompts.GSN, 2000);

                if (ret.Result == ReturnedState.OK)
                {
                    return ret.Response;
                }
                else
                {
                    return "";
                }
            }
        }

        /// <summary>
        /// Set phone funcionality 
        /// </summary>
        /// <param name="mode">See phone funcionality options</param>
        /// <param name="resetBeforeChange">True to reset device before changes are made effective</param>
        /// <returns>returns ATCommandResult</returns>
        public static AtCommandResult SetPhoneFuncionality(PhoneFuncionality mode, bool resetBeforeChange = false)
        {
            string atCommand = Prompts.AT + Prompts.CFUN + "=";

            // prepare command according to target functionality
            switch (mode)
            {
                case PhoneFuncionality.Minimum:
                    atCommand += @"0";
                    break;

                case PhoneFuncionality.Full:
                    atCommand += @"1";

                    // clear buffer
                    Instance.responseQueue.Clear();

                    break;

                case PhoneFuncionality.FligthMode:
                    atCommand += @"4";
                    break;

                default:
                    return new AtCommandResult(ReturnedState.InvalidCommand);
            }

            // send command
            var ret = Instance.SendATCommand(atCommand + (resetBeforeChange ? ",1" : ",0"), 3000);

            // check result 
            if (ret.Result == ReturnedState.OK)
            {
                switch (mode)
                {
                    case PhoneFuncionality.Minimum:
                        // update power status 
                        PowerStatus = PowerStatus.Minimum;

                        // device won't be registered in network anymore
                        GsmNetworkRegistration = NetworkRegistrationState.Unknown;
                        GprsNetworkRegistration = NetworkRegistrationState.Unknown;

                        // GPRS bearers must be off too
                        GprsSocketsBearerIsOpen = false;
                        GprsIpAppsBearerIsOpen = false;
                        GprsMmsBearerIsOpen = false;

                        // clear buffer
                        Instance.responseQueue.Clear();

                        break;

                    case PhoneFuncionality.Full:
                        // update power status 
                        PowerStatus = PowerStatus.On;
                        break;

                    case PhoneFuncionality.FligthMode:
                        // update power status 
                        PowerStatus = PowerStatus.FlightMode;

                        // device won't be registered in network anymore
                        GsmNetworkRegistration = NetworkRegistrationState.Unknown;
                        GprsNetworkRegistration = NetworkRegistrationState.Unknown;

                        // GPRS bearers must be off too
                        GprsSocketsBearerIsOpen = false;
                        GprsIpAppsBearerIsOpen = false;
                        GprsMmsBearerIsOpen = false;

                        // clear buffer
                        Instance.responseQueue.Clear();

                        break;

                    default:
                        // should never reach here
                        break;
                }

                // something unexpected occured, don't update power status
                return ret;
            }
            else
            {
                // something went wrong, don't update power status
                return ret;
            }
        }

        /// <summary>
        /// Set baud rate for serial interface
        /// </summary>
        /// <param name="newBaudRate">New baud rate. Valid values are 1200, 2400, 4800, 9600, 19200, 38400, 57600 and 115200. Set to 0 for auto baud rate. The new baud rate is stored to flash after command is issued. When setting to auto baud rate a reset is required/recommended.</param>
        /// <returns>ATCommandResult</returns>
        private AtCommandResult SetSerialInterfaceBaudRate(int newBaudRate)
        {
            return SendATCommand(Prompts.AT + Prompts.IPR + "=" + newBaudRate, 2000);
        }

        /// <summary>
        /// Retrieves baud rate for serial interface 
        /// </summary>
        /// <returns>An integer with the serial port baud rate (-1 when this command couldn't be executed)</returns>
        public int SerialInterfaceBaudRate
        {
            get
            {
                var ret = SendATCommand(Prompts.AT + Prompts.IPR + "?", 2000);

                if (ret.Result == ReturnedState.OK)
                {
                    // format: +IPR: rate

                    // clear response and double quotes
                    string messageRaw = (new StringBuilder(ret.Response).Replace("+IPR: ", "")).ToString();

                    return int.Parse(messageRaw);
                }

                // default is failure response
                return -1;
            }
        }


        /// <summary>
        /// Get SIM card status 
        /// </summary>
        /// <returns>SIMCardStatus</returns>
        public SimCardStatus SIMCardStatus
        {
            get
            {
                var ret = SendATCommandAndWaitForResponse(Prompts.AT + Prompts.CSMINS + "?", 2000);

                if (ret.Result == ReturnedState.OK)
                {
                    try
                    {
                        sString = ret.Response.ToString().Trim().Split(new char[] { ',' });
                        int status = int.Parse(sString[1]);

                        // dispose var
                        sString = null;

                        if (status == 0)
                        {
                            // SIM card removed
                            return SimCardStatus.Removed;
                        }
                        else if (status == 1)
                        {
                            // SIM card inserted
                            return SimCardStatus.Inserted;
                        }
                    }
                    catch (Exception)
                    {
                        //Console.WriteLine("CSMINS retrieve ERROR");
                    }
                }

                // default is unknown response
                return SimCardStatus.Unknown;
            }
        }

        /// <summary>
        /// Configure all relevant settings and options of the device
        /// </summary>
        /// <returns>True if configuration was successful</returns>
        internal bool SendConfigurationToDevice()
        {
            AtCommandResult ret = new AtCommandResult(ReturnedState.Error);
            StringBuilder sb = new StringBuilder(Prompts.AT);

            // go through all configuration preferences and build a combined command

            // CommandEchoMode is disabled
            sb.Append("E0");

            // SimInsertedStatusReporting enabled
            sb.Append(";" + Prompts.CSMINS + "=1");

            // GsmNetworkRegistrationStatus enabled
            sb.Append(";+CREG=1");

            // GprsNetworkRegistrationStatus enabled
            sb.Append(";+CGREG=1");

            // SlowClockConfiguration to automatic
            // SlowClock.Automatic
            sb.Append(";+CSCLK=2");

            // data flow control
            // System.IO.Ports.Handshake.None
            //cmd.Append(";+IFC=0,0");
            // System.IO.Ports.Handshake.RequestToSend
            //sb.Append(";+IFC=2,2";

            // set FIXED baud-rate to 115200bps
            // device is to save configuration 
            sb.Append(";+IFC=2,2;+IPR=115200;&W0");

            // transmit command
            ret = Instance.SendATCommand(sb.ToString(), 4000);

            // check execution, but it won't work because we are changing baud rate and other stuff
            if (ret.Result != ReturnedState.OK)
            {
                // check if baud rate is different
                if (_serialDevice.BaudRate != 115200)
                {
                    throw new Exception("Baud rate changed, need to reset device");
                }
            }

            // done here
            return true;
        }

        /// <summary>
        /// Retrieves date time from device's clock.
        /// For correct date time the clock must be set either programatically or using SNTP service
        /// </summary>
        /// <returns>Date time from device's clock</returns>
        public static DateTime GetDateTime()
        {
            var ret = Instance.SendATCommandAndWaitForResponse(Prompts.AT + Prompts.CCLK, 2000);

            if (ret.Result == ReturnedState.OK)
            {
                // response is in format +CCLK: "yy/mm/dd,hh:mm:ss+zz"

                // parse date time
                var dt = new DateTime(
                    int.Parse(ret.Response.Substring(8 + 3 * 0, 2)) + 2000,
                    int.Parse(ret.Response.Substring(8 + 3 * 1, 2)),
                    int.Parse(ret.Response.Substring(8 + 3 * 2, 2)),
                    int.Parse(ret.Response.Substring(8 + 3 * 3, 2)),
                    int.Parse(ret.Response.Substring(8 + 3 * 4, 2)),
                    int.Parse(ret.Response.Substring(8 + 3 * 5, 2))
                    );

                return dt;
            }
            else
            {
                return DateTime.MinValue;
            }
        }

        /// <summary>
        /// Retrieves time and optionally location of the device, as reported by the time and location service.
        /// Needs to have GPRS connection active.
        /// </summary>
        /// <param name="getLocation">True to return also the location of the device.</param>
        /// <returns>The device's time and location</returns>
        public static LocationAndTime GetTimeAndLocation(bool getLocation = true)
        {
            var ret = Instance.SendATCommandAndWaitForResponse(Prompts.AT + Prompts.CIPGSMLOC + "=" + (getLocation ? "1" : "2") + ",1", 60000);
            if (ret.Result == ReturnedState.OK)
            {
                // parse result
                // prompt format is: +CIPGSMLOC: 0,-8.804843,39.745911,2015/06/08,16:11:17

                try
                {
                    if (ret.Response.Length > 12)
                    {
                        // get other message details
                        string[] messageDetails = ret.Response.Substring(12).Split(new char[] { ',' });

                        int result = int.Parse(messageDetails[0].Trim());

                        if (result == 0)
                        {
                            double latitude = double.Parse(messageDetails[2].Trim());
                            double longitude = double.Parse(messageDetails[1].Trim());

                            string[] date = messageDetails[3].Split('/');
                            string[] time = messageDetails[4].Split(':');

                            DateTime timeStamp = new DateTime(
                                int.Parse(date[0]),     //Year
                                int.Parse(date[1]),     //Month
                                int.Parse(date[2]),     // Day
                                int.Parse(time[0]),     // Hour
                                int.Parse(time[1]),     // Minute
                                int.Parse(time[2]));    // Second

                            return new LocationAndTime(timeStamp, latitude, longitude);
                        }
                        return new LocationAndTime(result);
                    }
                    else
                    {
                        return new LocationAndTime();
                    }
                }
                catch
                {
                }
            }

            return new LocationAndTime();
        }

        /// <summary>
        /// Retrieves supply voltage.
        /// </summary>
        /// <returns>Supply voltage in mV</returns>
        public static UInt16 SupplyVoltage
        {
            get
            {
                var ret = Instance.SendATCommandAndWaitForResponse(Prompts.AT + Prompts.CBC, 2000);

                if (ret.Result == ReturnedState.OK)
                {
                    // response is in format +CBC: <bcs>,<bcl>,<voltage>

                    try
                    {
                        // get battery charge details
                        string[] batteryChargeDetails = ret.Response.Substring(5).Split(new char[] { ',' });

                        // parse voltage
                        return UInt16.Parse(batteryChargeDetails[2].Trim());
                    }
                    catch { };
                }

                return 0;
            }
        }

        #endregion

        #region GPRS Socket Helper methods

        static internal int AddSocket(GprsSocket socket)
        {
            // is there room for another socket?
            if (Instance._sockets.Count < _maxSockets)
            {
                // check for a free connection
                for (int i = 0; i < _maxSockets; i++)
                {
                    if (!Instance._sockets.Contains(i))
                    {
                        //  found one
                        Instance._sockets.Add(i, socket);
                        socket.owner = Instance;

                        return i;
                    }
                }
            }

            // all connections busy, can't add another
            throw new SocketException(SocketError.TooManyOpenSockets);
        }

        static internal void RemoveSocket(GprsSocket socket)
        {
            Instance._sockets.Remove(socket._connectionHandle);
        }

#endregion

    }
}
