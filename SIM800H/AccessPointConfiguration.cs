using System;
using System.Text;

namespace Eclo.nanoFramework.SIM800H
{
    /// <summary>
    /// Configuration of GPRS access point (APN).
    /// </summary>
    public class AccessPointConfiguration
    {
        private string _apn = string.Empty;
        /// <summary>
        /// Access Point name. Can't be null
        /// </summary>
        public string AccessPointName
        {
            get { return _apn; }
            private set { _apn = value; }
        }

        private string _userName = string.Empty;
        /// <summary>
        /// User name. Null if not used.
        /// </summary>
        public string UserName
        {
            get { return _userName; }
            private set { _userName = value; }
        }

        private string _password = string.Empty;
        /// <summary>
        /// Password. Null if not used.
        /// </summary>
        public string Password
        {
            get { return _password; }
            private set { _password = value; }
        }
        
        /// <summary>
        /// GPRS bearer Access Point configuration.
        /// </summary>
        /// <param name="apn">Access Point name</param>
        public AccessPointConfiguration(string apn)
        {
            _apn = apn;
        }

        /// <summary>
        /// GPRS bearer Access Point configuration.
        /// </summary>
        /// <param name="apn">Access Point name</param>
        /// <param name="userName">User name</param>
        /// <param name="password">Password.</param>
        public AccessPointConfiguration(string apn, string userName, string password)
        {
            _apn = apn;
            _userName = userName;
            _password = password;
        }

        /// <summary>
        /// Empty GPRS bearer Access Point configuration.
        /// </summary>
        public AccessPointConfiguration(){}

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            
            AccessPointConfiguration comp = (AccessPointConfiguration)obj;
            // check all 3 properties
            if (comp.AccessPointName != AccessPointName)
            {
                return false;
            }
            if (comp.UserName != UserName)
            {
                return false;
            }
            if (comp.Password != Password)
            {
                return false;
            }

            return true;
        }

        public static bool operator ==(AccessPointConfiguration value1, AccessPointConfiguration value2)
        {
            if ((object)value1 == null)
            {
                if ((object)value2 == null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            return value1.Equals(value2);
        }

        public static bool operator !=(AccessPointConfiguration value1, AccessPointConfiguration value2)
        {
            if ((object)value1 == null)
            {
                if ((object)value2 == null)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }

            return !value1.Equals(value2);
        }

        /// <summary>
        /// Parse a string with a valid Access Point configuration. Expected format is "apname|user|password".
        /// User name and password are optional.
        /// </summary>
        /// <param name="s">String to be parsed in the format: "apname|user|password".</param>
        /// <returns>A new instance of <see cref="AccessPointConfiguration"/> with valid access point configuration</returns>
        static public AccessPointConfiguration Parse(string s)
        {
            // sanity check
            if (s == null || s == string.Empty)
            {
                throw new ArgumentNullException();
            }

            string[] parsedConfig = s.Split('|');

            if (parsedConfig.Length != 3)
            {
                throw new ArgumentOutOfRangeException();
            }

            return new AccessPointConfiguration(parsedConfig[0], parsedConfig[1], parsedConfig[2]);
        }

        /// <summary>
        /// String with representation of Access Point configuration in format "apname|user|password".
        /// If user and password are empty they won't be included in the configuration string.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(AccessPointName);

            if (_userName != null)
            {
                sb.Append("|" + _userName);
            }

            if ( _password != null)
            {
                sb.Append("|" + _password);
            }

            return sb.ToString();
        }

        public override int GetHashCode()
        {
            return (_apn + _userName + _password).GetHashCode();
        }
    }
}
