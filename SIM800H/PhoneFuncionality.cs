////
// Copyright (c) Eclo Solutions
// See LICENSE file in the project root for full license information.
////

namespace Eclo.nanoFramework.SIM800H
{
    /// <summary>
    /// Phone Funcionality 
    /// </summary>
    public enum PhoneFuncionality
    {
        /// <summary>
        /// Minimum funcionality: minimum current consumption, RF and SIM are off
        /// </summary>
        Minimum,
        /// <summary>
        /// Full funcionality (default)
        /// </summary>
        Full,
        /// <summary>
        /// Flight mode: disable tx and rx RF circuits
        /// </summary>
        FligthMode
    }
}
