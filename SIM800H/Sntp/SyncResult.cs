namespace Eclo.nanoFramework.SIM800H
{
    /// <summary>
    /// Result of SNTP sync request
    /// </summary>
    public enum SyncResult
    {
        /// <summary>
        /// value not set
        /// </summary>
        NotSet = 0,
        /// <summary>
        /// Unspecified error
        /// </summary>
        Error,
        /// <summary>
        /// Network synchronization successful
        /// </summary>
        SyncSuccessful,
        /// <summary>
        /// Device is off
        /// </summary>
        DeviceIsOff,
        /// <summary>
        /// Device is not registered at GSM network
        /// </summary>
        NotRegisteredAtGsmNetwork,
        /// <summary>
        /// Device is not registered at GPRS network
        /// </summary>
        NotRegisteredAtGprsNetwork,
        /// <summary>
        /// Network error
        /// </summary>
        NetworkError,
        /// <summary>
        /// DNS resolution error
        /// </summary>
        DnsError,
        /// <summary>
        /// Connection error
        /// </summary>
        ConnectionError,
        /// <summary>
        /// Server response error
        /// </summary>
        ServerResponseError,
        /// <summary>
        /// Server Response timeout
        /// </summary>
        ServerResponseTimeout
    }
}
