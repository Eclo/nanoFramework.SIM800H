////
// Copyright (c) Eclo Solutions
// See LICENSE file in the project root for full license information.
////

namespace Eclo.nanoFramework.SIM800H
{
    /// <summary>
    /// Bearer Profiles for GPRS context
    /// </summary>
    public enum BearerProfile
    {
        /// <summary>
        /// Invalid bearer
        /// </summary>
        None = -1,

        /// <summary>
        /// Sockets bearer
        /// </summary>
        SocketsBearer = 0,
        /// <summary>
        /// IP apps bearer
        /// </summary>
        IpAppsBearer = 1,
        /// <summary>
        /// MMS bearer
        /// </summary>
        MmsBearer = 2
    }
}
