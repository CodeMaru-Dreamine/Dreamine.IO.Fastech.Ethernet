namespace Dreamine.IO.Fastech.Ethernet.Options;

/// <summary>
/// \if KO
/// <para>Fastech Ethernet I/O 통신에 사용할 전송 형식을 나타냅니다.</para>
/// \endif
/// \if EN
/// <para>Represents the transport type used for Fastech Ethernet I/O communication.</para>
/// \endif
/// </summary>
public enum FastechEthernetIoTransportType
{
    /// <summary>
    /// \if KO
    /// <para>Ezi-IO Plus-E 프로토콜 통신용 UDP 전송입니다.</para>
    /// \endif
    /// \if EN
    /// <para>UDP transport for Ezi-IO Plus-E protocol communication.</para>
    /// \endif
    /// </summary>
    Udp = 0,

    /// <summary>
    /// \if KO
    /// <para>사용자 지정 또는 향후 Fastech Ethernet I/O 통신용 TCP 전송입니다.</para>
    /// \endif
    /// \if EN
    /// <para>TCP transport for custom or future Fastech Ethernet I/O communication.</para>
    /// \endif
    /// </summary>
    Tcp = 1,
}
