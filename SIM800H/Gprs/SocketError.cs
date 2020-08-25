namespace Eclo.nanoFramework.SIM800H
{
    /// <summary>
    /// Defines socket error constants.
    /// </summary>
    /// <remarks>Implementation follows .NETMF System.Net.Sockets.SocketError </remarks>
    public enum SocketError : int
    {
        /// <summary>
        /// The operation completed successfully.
        /// </summary>
        Success = 0,

        /// <summary>
        /// The socket has an error.
        /// </summary>
        SocketError = (-1),
        /*
            * Windows Sockets definitions of regular Microsoft C error constants
            */
        ///// <summary>
        ///// A blocking socket call was canceled.
        ///// </summary>
        //Interrupted = (10000 + 4),      //WSAEINTR
        ///// <summary>                     
        ///// [To be supplied.]
        ///// </summary>
        //WSAEBADF               = (10000+9),   //
        ///// <summary>
        ///// Permission denied.
        ///// </summary>
        //AccessDenied = (10000 + 13),      //WSAEACCES
        ///// <summary>
        ///// Bad address.
        ///// </summary>
        //Fault = (10000 + 14),        //WSAEFAULT
        ///// <summary>
        ///// Invalid argument.
        ///// </summary>
        //InvalidArgument = (10000 + 22),    //WSAEINVAL
        ///// <summary>
        ///// Too many open
        ///// files.
        ///// </summary>
        TooManyOpenSockets = (10000 + 24),  //WSAEMFILE

        /*
            * Windows Sockets definitions of regular Berkeley error constants
            */
        ///// <summary>
        ///// Resource temporarily unavailable.
        ///// </summary>
        //WouldBlock = (10000 + 35),   //WSAEWOULDBLOCK
        ///// <summary>
        ///// Operation now in progress.
        ///// </summary>
        //InProgress = (10000 + 36),  // WSAEINPROGRESS
        ///// <summary>
        ///// Operation already in progress.
        ///// </summary>
        //AlreadyInProgress = (10000 + 37),  //WSAEALREADY
        ///// <summary>
        ///// Socket operation on nonsocket.
        ///// </summary>
        //NotSocket = (10000 + 38),   //WSAENOTSOCK
        ///// <summary>
        ///// Destination address required.
        ///// </summary>
        //DestinationAddressRequired = (10000 + 39), //WSAEDESTADDRREQ
        ///// <summary>
        ///// Message too long.
        ///// </summary>
        //MessageSize = (10000 + 40),  //WSAEMSGSIZE
        ///// <summary>
        ///// Protocol wrong type for socket.
        ///// </summary>
        //ProtocolType = (10000 + 41), //WSAEPROTOTYPE
        ///// <summary>
        ///// Bad protocol option.
        ///// </summary>
        //ProtocolOption = (10000 + 42), //WSAENOPROTOOPT
        ///// <summary>
        ///// Protocol not supported.
        ///// </summary>
        ProtocolNotSupported = (10000 + 43), //WSAEPROTONOSUPPORT
        ///// <summary>
        ///// Socket type not supported.
        ///// </summary>
        //SocketNotSupported = (10000 + 44), //WSAESOCKTNOSUPPORT
        ///// <summary>
        ///// Operation not supported.
        ///// </summary>
        //OperationNotSupported = (10000 + 45), //WSAEOPNOTSUPP
        ///// <summary>
        ///// Protocol family not supported.
        ///// </summary>
        //ProtocolFamilyNotSupported = (10000 + 46), //WSAEPFNOSUPPORT
        ///// <summary>
        ///// Address family not supported by protocol family.
        ///// </summary>
        //AddressFamilyNotSupported = (10000 + 47), //WSAEAFNOSUPPORT
        ///// <summary>
        /////    Address already in use.
        ///// </summary>
        //AddressAlreadyInUse = (10000 + 48), // WSAEADDRINUSE
        ///// <summary>
        ///// Cannot assign requested address.
        ///// </summary>
        //AddressNotAvailable = (10000 + 49), //WSAEADDRNOTAVAIL
        ///// <summary>
        ///// Network is down.
        ///// </summary>
        //NetworkDown = (10000 + 50), //WSAENETDOWN
        ///// <summary>
        ///// Network is unreachable.
        ///// </summary>
        //NetworkUnreachable = (10000 + 51), //WSAENETUNREACH
        ///// <summary>
        ///// Network dropped connection on reset.
        ///// </summary>
        //NetworkReset = (10000 + 52), //WSAENETRESET
        ///// <summary>
        ///// Software caused connection to abort.
        ///// </summary>
        //ConnectionAborted = (10000 + 53), //WSAECONNABORTED
        ///// <summary>
        ///// Connection reset by peer.
        ///// </summary>
        //ConnectionReset = (10000 + 54), //WSAECONNRESET
        ///// <summary>
        ///// No buffer space available.
        ///// </summary>
        //NoBufferSpaceAvailable = (10000 + 55), //WSAENOBUFS
        ///// <summary>
        ///// Socket is already connected.
        ///// </summary>
        //IsConnected = (10000 + 56), //WSAEISCONN
        ///// <summary>
        ///// Socket is not connected.
        ///// </summary>
        //NotConnected = (10000 + 57), //WSAENOTCONN
        ///// <summary>
        ///// Cannot send after socket shutdown.
        ///// </summary>
        //Shutdown = (10000 + 58), //WSAESHUTDOWN
        ///// <summary>
        ///// Connection timed out.
        ///// </summary>
        //TimedOut = (10000 + 60), //WSAETIMEDOUT
        ///// <summary>
        ///// Connection refused.
        ///// </summary>
        //ConnectionRefused = (10000 + 61), //WSAECONNREFUSED
        ///// <summary>
        ///// Host is down.
        ///// </summary>
        //HostDown = (10000 + 64), //WSAEHOSTDOWN
        ///// <summary>
        ///// No route to host.
        ///// </summary>
        //HostUnreachable = (10000 + 65), //WSAEHOSTUNREACH
        ///// <summary>
        ///// Too many processes.
        ///// </summary>
        //ProcessLimit = (10000 + 67), //WSAEPROCLIM

        /*
            * Extended Windows Sockets error constant definitions
            */
        ///// <summary>
        ///// Network subsystem is unavailable.
        ///// </summary>
        //SystemNotReady = (10000 + 91), //WSASYSNOTREADY
        ///// <summary>
        ///// Winsock.dll out of range.
        ///// </summary>
        //VersionNotSupported = (10000 + 92), //WSAVERNOTSUPPORTED
        ///// <summary>
        ///// Successful startup not yet performed.
        ///// </summary>
        //NotInitialized = (10000 + 93), //WSANOTINITIALISED

        // WSAEREMOTE             = (10000+71),
        ///// <summary>
        ///// Graceful shutdown in progress.
        ///// </summary>
        //Disconnecting = (10000 + 101), //WSAEDISCON

        //TypeNotFound = (10000 + 109), //WSATYPE_NOT_FOUND

        /*
            * Error return codes from gethostbyname() and gethostbyaddr()
            *              = (when using the resolver). Note that these errors are
            * retrieved via WSAGetLastError() and must therefore follow
            * the rules for avoiding clashes with error numbers from
            * specific implementations or language run-time systems.
            * For this reason the codes are based at 10000+1001.
            * Note also that [WSA]NO_ADDRESS is defined only for
            * compatibility purposes.
            */

        ///// <summary>
        ///// Host not found (Authoritative Answer: Host not found).
        ///// </summary>
        //HostNotFound = (10000 + 1001), //WSAHOST_NOT_FOUND
        ///// <summary>
        ///// Nonauthoritative host not found (Non-Authoritative: Host not found, or SERVERFAIL).
        ///// </summary>
        //TryAgain = (10000 + 1002), //WSATRY_AGAIN
        ///// <summary>
        ///// This is a nonrecoverable error (Non recoverable errors, FORMERR, REFUSED, NOTIMP).
        ///// </summary>
        //NoRecovery = (10000 + 1003), //WSANO_RECOVERY
        ///// <summary>
        ///// Valid name, no data record of requested type.
        ///// </summary>
        //NoData = (10000 + 1004), //WSANO_DATA
    }
}
