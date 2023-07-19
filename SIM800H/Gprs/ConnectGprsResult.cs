////
// Copyright (c) Eclo Solutions
// See LICENSE file in the project root for full license information.
////

namespace Eclo.nanoFramework.SIM800H
{
    /// <summary>
    /// Outcome of request to open GPRS connection
    /// </summary>
    public enum ConnectGprsResult
    {
        /// <summary>
        /// GPRS connection open
        /// </summary>
        Open,
        /// <summary>
        /// GPRS connection  already open
        /// </summary>
        AlreadyOpen,
        /// <summary>
        /// Unspecified error when trying to open GPRS connection
        /// </summary>
        Error,
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
        /// Failed to open bearer context after all attempts
        /// </summary>
        Failed
    }
}
