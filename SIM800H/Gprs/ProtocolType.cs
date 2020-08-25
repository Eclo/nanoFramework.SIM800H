namespace Eclo.nanoFramework.SIM800H
{
    /// <summary>
    /// Specifies the protocols that the <see cref='GprsSocket'/> class supports.
    /// </summary>
    /// <remarks>Implementation follows .NETMF System.Net.Sockets.ProtocolType
    /// </remarks>
    public enum ProtocolType
    {
        /// <summary>
        /// Transmission control protocol 
        /// </summary>
        Tcp = 6,
        /// <summary>
        /// User datagram protocol
        /// </summary>
        Udp = 17   
    }
}
