////
// Copyright (c) Eclo Solutions
// See LICENSE file in the project root for full license information.
////

namespace Eclo.nanoFramework.SIM800H
{
    /// <summary>
    /// Available options to delete text messages
    /// </summary>
    public enum MessageDeleteOption
    {
        /// <summary>
        /// Delete all read messages
        /// </summary>
        Read,
        /// <summary>
        /// Delete all unread messages
        /// </summary>
        Unread,
        /// <summary>
        /// Delete all sent messages
        /// </summary>
        Sent,
        /// <summary>
        /// Delete all unsent messages
        /// </summary>
        Unsent,
        /// <summary>
        /// Delete all received messages
        /// </summary>
        Inbox,
        /// <summary>
        /// Delete all messages
        /// </summary>
        All
    }
}
