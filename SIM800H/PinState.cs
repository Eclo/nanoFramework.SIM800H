namespace Eclo.nanoFramework.SIM800H
{
    /// <summary>
    /// Possible states of the SIM card
    /// </summary>
    public enum PinState
    {
        /// <summary>
        /// Error retrieving PIN status
        /// </summary>
        Error = 0,
        /// <summary>
        /// SIM is unlocked and ready to be used.
        /// </summary>
        Ready,
        /// <summary>
        /// SIM is locked waiting for the PIN
        /// </summary>
        PIN,
        /// <summary>
        /// SIM is locked waiting for the PUK
        /// </summary>
        PUK,
        /// <summary>
        /// SIM is waiting for phone to SIM card (anti-theft) 
        /// </summary>
        PH_PIN,
        /// <summary>
        /// SIM is waiting for phone to SIM PUK (anti theft) 
        /// </summary>
        PH_PUK, 
        /// <summary>
        /// SIM is waiting for second PIN
        /// </summary>
        PIN2,
        /// <summary>
        /// SIM is waiting for second PUK
        /// </summary>
        PUK2,
        /// <summary>
        /// SIM is not present
        /// </summary>
        NotPresent
    }
}
