using System;

namespace Eclo.nanoFramework.SIM800H
{
    /// <summary>
    /// Class with text message (SMS) properties and content.
    /// </summary>
    public class TextMessage
    {
        /// <summary>
        /// Number
        /// </summary>
        public string TelephoneNumber;
        /// <summary>
        /// Message content
        /// </summary>
        public string Text;
        /// <summary>
        /// Status of the message
        /// </summary>
        public MessageState Status;
        /// <summary>
        /// Date and time when the message was sent or received
        /// </summary>
        public DateTime Timestamp;
        /// <summary>
        /// Index of the message in the SIM card's memory
        /// </summary>
        public int Index
        {
            get
            {
                return index;
            }
            internal set
            {
                index = value;
            }
        }
        private int index;
        /// <summary>
        /// Instantiates a new SMS with empty number, and content, marks it as unsent and with the current time as the timestamp.
        /// </summary>
        public TextMessage()
        {
            TelephoneNumber = "";
            Text = "";
            Status = MessageState.StoredUnsent;
            Timestamp = DateTime.UtcNow;
            Index = -1;
        }

        /// <summary>
        /// Instantiates a new SMS message with the given parameters.
        /// </summary>
        /// <param name="number">Number</param>
        /// <param name="text">Message content</param>
        /// <param name="state"><see cref="MessageState">of message</see></param>
        /// <param name="timestamp">Time stamp of message</param>
        public TextMessage(string number, string text, MessageState state, DateTime timestamp)
        {
            this.TelephoneNumber = number;
            this.Text = text;
            this.Status = state;
            this.Timestamp = timestamp;
        }

        private TextMessage(string number, string text, MessageState state, DateTime timestamp, int index)
        {
            this.TelephoneNumber = number;
            this.Text = text;
            this.Status = state;
            this.Timestamp = timestamp;
            this.Index = index;
        }
    }
}
