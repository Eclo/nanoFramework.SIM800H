////
// Copyright (c) Eclo Solutions
// See LICENSE file in the project root for full license information.
////

namespace Eclo.nanoFramework.SIM800H
{
    /// <summary>
    /// HTTP status of HTTP service
    /// </summary>
    internal enum HttpStatus
    {
        Unknown = -1,
        Idle = 0,
        Receiving = 1,
        Sending = 2
    }
}
