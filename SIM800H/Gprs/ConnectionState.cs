////
// Copyright (c) Eclo Solutions
// See LICENSE file in the project root for full license information.
////

namespace Eclo.nanoFramework.SIM800H
{
    internal enum SingleConnectionState
    {
        Unknown = -1,
        IpInitial = 0,
        IPStatus = 1,
        IpConfig = 2,
        IpGprsAct = 3,
        IpStatus = 4,
        ConnectingListening = 5,
        ConnectOk = 6,
        Closing = 7,
        Closed = 8,
        PdpDeact = 9
    }

    internal enum MultiIpConnectionState
    {
        Unknown = -1,
        IpInitial = 0,
        IPStart = 1,
        IpConfig = 2,
        IpGprsAct = 3,
        IpStatus = 4,
        Processing = 5,
        PdpDeact = 9
    }

    /// <summary>
    /// Connection status of GPRS socket.
    /// See SIM800H documentation.
    /// </summary>
    public enum ConnectionStatus
    {
        Unknown = -1,
        Initial = 0,
        Connecting = 1,
        Connected = 2,
        RemoteClosing = 3,
        Closing = 4,
        Closed = 5
    }
}
