using System;

namespace Eclo.nanoFramework.SIM800H
{
    /// <summary>
    /// Class with message status report
    /// </summary>
    public class MessageStatusReport
    {
        public int FO { get; set; }
        public int MessageReference { set; get; }
        public string ReceivingNumber { set; get; }
        public int TORA { set; get; }
        public DateTime ServiceCenterTimeStamp { set; get; }
        public DateTime DelieveredTimeStamp { set; get; }
        public int ST { set; get; }
    }
}
