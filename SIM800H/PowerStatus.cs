////
// Copyright (c) Eclo Solutions
// See LICENSE file in the project root for full license information.
////

namespace Eclo.nanoFramework.SIM800H
{
    /// <summary>
    /// Possible power status
    /// </summary>
    public enum PowerStatus
    {
        /// <summary>
        /// Unknown power state
        /// </summary>
        Unknown,
        /// <summary>
        /// Device is on
        /// </summary>
        On,
        /// <summary>
        /// Device is off
        /// </summary>
        Off,
        /// <summary>
        /// Device is in flight mode
        /// </summary>
        FlightMode,
        /// <summary>
        /// Device is workgin with minimum functionality 
        /// </summary>
        Minimum,
        /// <summary>
        /// Power on sequence is running
        /// </summary>
        PowerOnSequenceIsRunning
    }
}
