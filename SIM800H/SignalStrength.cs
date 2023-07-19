////
// Copyright (c) Eclo Solutions
// See LICENSE file in the project root for full license information.
////

namespace Eclo.nanoFramework.SIM800H
{
    /// <summary>
    /// Possible values of the strength of a signal
    /// </summary>
    public enum SignalStrength
    {
        /// <summary>
        /// Error in the response from the GSM Module
        /// </summary>
        Error = 0,
        /// <summary>
        /// -115dBm or less
        /// </summary>
        VeryWeak,
        /// <summary>
        /// -111dBm
        /// </summary>
        Weak,
        /// <summary>
        /// -110 to -54dBm
        /// </summary>
        Strong,
        /// <summary>
        /// -52dBm or greater
        /// </summary>
        VeryStrong,
        /// <summary>
        /// Not known or undetectable
        /// </summary>
        Unknown
    }
}
