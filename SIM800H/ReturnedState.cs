////
// Copyright (c) Eclo Solutions
// See LICENSE file in the project root for full license information.
////

namespace Eclo.nanoFramework.SIM800H
{
    /// <summary>
    /// Possible returned states by methods
    /// </summary>
    public enum ReturnedState
    {
        /// <summary>
        /// Operation successful
        /// </summary>
        OK,
        /// <summary>
        /// Error in the operation. See method documentation.
        /// </summary>
        Error,
        /// <summary>
        /// Device is off
        /// </summary>
        DeviceIsOff,
        /// <summary>
        /// Command syntax is incorrect
        /// </summary>
        InvalidCommand,
        /// <summary>
        /// Device is busy
        /// </summary>
        DeviceIsBusy,
        /// <summary>
        /// No reply returned
        /// </summary>
        NoReply

    }
}
