using System;

namespace Eclo.nanoFramework.SIM800H
{
    /// <summary>
    /// Configuration of MMS center (MMSC).
    /// </summary>
    public class MmsConfiguration
    {
        private string _mmsc = string.Empty;
        /// <summary>
        /// MMS center URL. Can't be null.
        /// <note type="note">URL without "http://" and/or port number.</note>
        /// </summary>
        public string MMSC
        {
            get { return _mmsc; }
            private set { _mmsc = value; }
        }

        // default value for SIM800H is "10.0.0.172"
        private string _proxy;
        /// <summary>
        /// Proxy. Empty if not used.
        /// <note type="note">Must be an IP address. URLs are not accepted.</note>
        /// </summary>
        public string Proxy
        {
            get { return _proxy; }
            private set { _proxy = value; }
        }

        private int _proxyPort = 80;
        /// <summary>
        /// Proxy port. Default is 80.
        /// </summary>
        public int ProxyPort
        {
            get { return _proxyPort; }
            private set { _proxyPort = value; }
        }

#pragma warning disable 1591 // disable warning for Missing XML comment
        [Obsolete("Obsolete. Replace with ProxyPort property.")]
        public int Port { get { return _proxyPort; } set{ _proxyPort = value; } }
#pragma warning restore 1591

        /// <summary>
        ///  MMS center configuration.
        /// </summary>
        /// <param name="mmsc">MMS center URL
        ///  <note type="note">URL without "http://" and/or port number.</note>
        ///  </param>
        /// <param name="proxy">MMS Proxy
        /// <note type="note">Must be an IP address. URLs are not accepted.</note>
        /// </param>
        /// <param name="proxyPort">MMS Proxy port.</param>
        public MmsConfiguration(string mmsc, string proxy, int proxyPort)
        {
            _mmsc = mmsc;
            _proxy = proxy;
            _proxyPort = proxyPort;
        }

        /// <summary>
        /// Empty  MMS center configuration.
        /// </summary>
        public MmsConfiguration(){ }

#pragma warning disable 1591 // disable warning for Missing XML comment
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            
            MmsConfiguration comp = (MmsConfiguration)obj;
            // check all 3 properties
            if (comp.MMSC != MMSC)
            {
                return false;
            }
            if (comp.Proxy != Proxy)
            {
                return false;
            }
            if (comp.ProxyPort != ProxyPort)
            {
                return false;
            }

            return true;
        }

        public static bool operator == (MmsConfiguration value1, MmsConfiguration value2)
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

        public static bool operator !=(MmsConfiguration value1, MmsConfiguration value2)
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
        
        public override int GetHashCode()
        {
            return (_mmsc + _proxy + _proxyPort).GetHashCode();
        }
#pragma warning restore 1591

    }
}
