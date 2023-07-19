////
// Copyright (c) Eclo Solutions
// See LICENSE file in the project root for full license information.
////

namespace Eclo.nanoFramework.SIM800H
{
    /// <summary>
    /// Outcome of request to open bearer context
    /// </summary>
    public enum OpenBearerResult
    {
        /// <summary>
        /// Bearer context open
        /// </summary>
        Open,
        /// <summary>
        /// Bearer context is already open
        /// </summary>
        AlreadyOpen,
        /// <summary>
        /// Unspecified error when trying to open context
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
