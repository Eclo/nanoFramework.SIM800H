////
// Copyright (c) Eclo Solutions
// See LICENSE file in the project root for full license information.
////

namespace Eclo.nanoFramework.SIM800H
{
    /// <summary>
    /// Possible states of network registration
    /// </summary>
    public enum NetworkRegistrationState
    {
        /// <summary>
        /// Module couldn't find a network
        /// </summary>
        NotSearching,
        /// <summary>
        /// Module is registered to a network
        /// </summary>
        Registered,
        /// <summary>
        /// Module is searching for a network
        /// </summary>
        Searching,
        /// <summary>
        /// Module tried to register to a network, but it was denied
        /// </summary>
        RegistrationDenied,
        /// <summary>
        /// Unknown
        /// </summary>
        Unknown,
        /// <summary>
        /// Roaming
        /// </summary>
        Roaming,
        /// <summary>
        /// Error
        /// </summary>
        Error
    }

}
