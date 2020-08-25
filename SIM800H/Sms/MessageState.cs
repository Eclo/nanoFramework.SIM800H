namespace Eclo.nanoFramework.SIM800H
{
    /// <summary>
    /// Possible states for a text message
    /// </summary>
    public enum MessageState
    {
        /// <summary>
        /// Error retrieving message
        /// </summary>
        Error = 0,
        /// <summary>
        /// Messages that were received and read
        /// </summary>
        ReceivedUnread,
        /// <summary>
        /// Messages that were received but not yet read
        /// </summary>
        ReceivedRead,
        /// <summary>
        /// Messages that were created but not yet sent
        /// </summary>
        StoredUnsent,
        /// <summary>
        /// Messages that were created and sent
        /// </summary>
        StoredSent,
        /// <summary>
        /// All messages
        /// </summary>
        All
    }
}
