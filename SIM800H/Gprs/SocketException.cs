////
// Copyright (c) Eclo Solutions
// See LICENSE file in the project root for full license information.
////

using System;

namespace Eclo.nanoFramework.SIM800H
{
    /// <summary>
    /// </summary>
    /// <remarks>Implementation follows .NETMF System.Net.Sockets.SocketException</remarks>
    [Serializable]
    public class SocketException : Exception
    {
        private int _errorCode;

        public SocketException(SocketError errorCode)
        {
            _errorCode = (int)errorCode;
        }

        public int ErrorCode
        {
            get { return _errorCode; }
        }

    }; // class SocketException
}
