namespace Dreamine.IO.Fastech.Ethernet.Options;

/// <summary>
/// Represents the Fastech Ethernet I/O transport type.
/// </summary>
public enum FastechEthernetIoTransportType
{
    /// <summary>
    /// UDP transport for Ezi-IO Plus-E protocol communication.
    /// </summary>
    Udp = 0,

    /// <summary>
    /// TCP transport for custom or future Fastech Ethernet I/O communication.
    /// </summary>
    Tcp = 1,
}
