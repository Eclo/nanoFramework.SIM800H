namespace Eclo.nanoFramework.SIM800H
{
    /// <summary>
    /// Class with MMS (Multimedia Messaging Service) message properties and content.
    /// </summary>
    public class MmsMessage
    {
        /// <summary>
        /// Message title
        /// </summary>
        public string Title { get; internal set; }

        /// <summary>
        /// Text content to be included in the message.
        /// </summary>
        public string Text { get; internal set; }
        
        /// <summary>
        /// Image content to be included in the message.
        /// </summary>
        public byte[] Image { get; internal set; }

        /// <summary>
        /// Instantiates a new MMS message with the given parameters.
        /// </summary>
        /// <param name="title">Message title. Optional. Set to null if not used.</param>
        public MmsMessage(string title = null)
        {
            this.Title = title;
            Text = "";
            Image = new byte[]{};
        }

        /// <summary>
        /// Instantiates a new MMS message with the given parameters.
        /// </summary>
        /// <param name="text">Message content.</param>
        /// <param name="title">Message title. Optional. Set to null if not used.</param>
        public MmsMessage(string text, string title = null)
        {
            this.Title = title;
            this.Text = text;
            this.Image = new byte[] { };
        }


        /// <summary>
        /// Instantiates a new MMS message with the given parameters.
        /// </summary>
        /// <param name="image">Message image.</param>
        /// <param name="title">Message title. Optional. Set to null if not used.</param>
        public MmsMessage(byte[] image, string title = null)
        {
            this.Title = title;
            this.Text = "";
            this.Image = image;
        }

        /// <summary>
        /// Instantiates a new MMS message with the given parameters.
        /// </summary>
        /// <param name="text">Message content.</param>
        /// <param name="image">Message image.</param>
        /// <param name="title">Message title. Optional. Set to null if not used.</param>
        public MmsMessage(string text = null, byte[] image = null, string title = null)
        {
            this.Text = text;
            this.Image = image;
            this.Title = title;
        }
    }
}
