using System;

namespace Eclo.nanoFramework.SIM800H
{
    /// <summary>
    /// Class with information about location and time.
    /// </summary>
    public class LocationAndTime
    {
        private DateTime _dateTime;

        /// <summary>
        /// Current date time as provided from location application service
        /// </summary>
        public DateTime DateTime
        {
            get { return _dateTime; }
            internal set { _dateTime = value; }
        }

        private double _latitude;

        /// <summary>
        /// Current latitude as provided from location application service. Value is degrees.
        /// </summary>
        public double Latitude
        {
            get { return _latitude; }
            internal set { _latitude = value; }
        }

        private double _longitude;

        /// <summary>
        /// Current longitude as provided from location application service. Value is degrees.
        /// </summary>
        public double Longitude
        {
            get { return _longitude; }
            internal set { _longitude = value; }
        }

        private int _errorCode = 65535;

        /// <summary>
        /// Error code for the request.
        /// 404 - not found
        /// 408 - request timeout
        /// 601 - network error
        /// 602 - no memory
        /// 603 - DNS error
        /// 604 - Stack busy
        /// 65535 - Other error
        /// </summary>
        public int ErrorCode
        {
            get { return _errorCode; }
            set { _errorCode = value; }
        }
        
        internal LocationAndTime(DateTime dateTime)
        {
            this._dateTime = dateTime;

            this._errorCode = 0;
        }

        internal LocationAndTime(DateTime dateTime, double latitude, double longitude)
        {
            this._dateTime = dateTime;
            this._latitude = latitude;
            this._longitude = longitude;
            
            this._errorCode = 0;
        }

        internal LocationAndTime(int errorCode)
        {
            this._errorCode = errorCode;
        }

        internal LocationAndTime()
        {
        }
    }
}
