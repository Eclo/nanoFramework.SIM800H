////
// Copyright (c) Eclo Solutions
// See LICENSE file in the project root for full license information.
////

using System;

namespace Eclo.nanoFramework.SIM800H
{

    /// <summary> 
    /// Provides an object representation of a uniform resource identifier (URI) 
    /// and easy access to the parts of the URI. 
    /// </summary> 
    /// <remarks>Implementation follows .NETMF System.Uri
    /// </remarks>
    public class Uri
    {
        protected string m_OriginalUriString = null;
        protected string m_scheme = null;
        protected string m_absoluteUri = null;
        protected string m_host = "";
        protected int m_port = -1;
        protected string m_AbsolutePath = null;

        public Uri(string uriString)
        {
            ConstructAbsoluteUri(uriString);
        }

        protected void ConstructAbsoluteUri(string uriString)
        {
            // ParseUriString provides full validation including testing for
            // null.
            ParseUriString(uriString);
            m_OriginalUriString = uriString;
        }

        protected void ParseUriString(string uriString)
        {
            int startIndex = 0;
            int endIndex = 0;

            // Check for null or empty string.
            if (uriString == null || uriString.Length == 0)
            {
                throw new ArgumentNullException();
            }
            uriString = uriString.Trim();

            // Check for presence of ':'. Colon always should be present in URI.
            if (uriString.IndexOf(':') == -1)
            {
                throw new ArgumentException();
            }

            string uriStringLower = uriString.ToLower();

            // Validate Scheme
            endIndex = uriString.IndexOf(':');
            m_scheme = uriString.Substring(0, endIndex);

            // Get past the colon
            startIndex = endIndex + 1;
            if (startIndex >= uriString.Length)
            {
                throw new ArgumentException();
            }

            // Get host, port and absolute path
            bool bRooted = ParseSchemeSpecificPart(uriString, startIndex);

            m_absoluteUri = m_scheme + ":" +
                (bRooted ? "//" : string.Empty) +
                m_host;

            // http and port is not 80
            // https and port is not 443
            if ((m_scheme == "http" && m_port != -1 && m_port != 80) ||
                (m_scheme == "https" && m_port != -1 && m_port != 443))
            {
                m_absoluteUri += ":" + m_port.ToString();
            }


            m_absoluteUri += (m_AbsolutePath.Length == 0 ? "/" : string.Empty) +
                m_AbsolutePath;
        }

        protected bool ParseSchemeSpecificPart(string sUri, int iStart)
        {
            bool bRooted = sUri.Length >= iStart + 2 && sUri.Substring(iStart, 2) == "//";
            string sAuthority = string.Empty;

            switch (m_scheme)
            {
                case "amqps":
                case "http":
                case "https":

                    Split(sUri, iStart + 2, out sAuthority, out m_AbsolutePath, true);
                    break;

                default:
                    break;
            }

            int iPortSplitter = sAuthority.LastIndexOf(':');
            if (iPortSplitter < 0 || sAuthority.LastIndexOf(']') > iPortSplitter)
            {
                m_host = sAuthority;
                m_port = m_scheme == "http" ? 80 : 443;
            }
            else
            {
                m_host = sAuthority.Substring(0, iPortSplitter);
                m_port = Convert.ToInt32(sAuthority.Substring(iPortSplitter + 1));
            }

            return bRooted;
        }

        protected void Split(string sUri, int iStart, out string sAuthority, out string sPath, bool bReplaceEmptyPath)
        {
            int iSplitter = sUri.IndexOf('/', iStart);
            if (iSplitter < 0)
            {
                sAuthority = sUri.Substring(iStart);
                sPath = string.Empty;
            }
            else
            {
                sAuthority = sUri.Substring(iStart, iSplitter - iStart);
                sPath = sUri.Substring(iSplitter);
            }

            if (bReplaceEmptyPath && sPath.Length == 0)
            {
                sPath = "/";
            }
        }

        public string Scheme
        {
            get
            {
                return m_scheme;
            }
        }

        public string AbsoluteUri
        {
            get
            {
                return m_absoluteUri;
            }
        }

        public int Port { get { return m_port; } }

        public string Host { get { return m_host; } }
        public string AbsolutePath { get { return m_AbsolutePath; } }
        public string OriginalString { get { return m_OriginalUriString; } }
    }
}
