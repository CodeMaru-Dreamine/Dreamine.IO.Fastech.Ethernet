using Dreamine.IO.Abstractions.Enums;
using Dreamine.IO.Abstractions.Options;

namespace Dreamine.IO.Fastech.Ethernet.Options;

/// <summary>
/// \if KO
/// <para>Fastech Ethernet I/O 연결, 시간 제한 및 프로토콜 확장 옵션을 나타냅니다.</para>
/// \endif
/// \if EN
/// <para>Represents Fastech Ethernet I/O connection, timeout, and protocol-extension options.</para>
/// \endif
/// </summary>
public sealed class FastechEthernetIoOptions
{
    /// <summary>
    /// \if KO
    /// <para>대상 호스트 이름 또는 IP 주소를 가져오거나 설정합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Gets or sets the target host name or IP address.</para>
    /// \endif
    /// </summary>
    public string Host { get; set; } = "127.0.0.1";

    /// <summary>
    /// \if KO
    /// <para>대상 포트를 가져오거나 설정합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Gets or sets the target port.</para>
    /// \endif
    /// </summary>
    public int Port { get; set; } = 3001;

    /// <summary>
    /// \if KO
    /// <para>로컬 UDP 포트를 가져오거나 설정합니다. 0이면 Windows가 임시 원본 포트를 선택합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Gets or sets the local UDP port. Zero lets Windows choose an ephemeral source port.</para>
    /// \endif
    /// </summary>
    public int LocalPort { get; set; }

    /// <summary>
    /// \if KO
    /// <para>Ethernet 전송 형식을 가져오거나 설정합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Gets or sets the Ethernet transport type.</para>
    /// \endif
    /// </summary>
    public FastechEthernetIoTransportType TransportType { get; set; } = FastechEthernetIoTransportType.Udp;

    /// <summary>
    /// \if KO
    /// <para>보드, 장치 또는 컨트롤러 인덱스를 가져오거나 설정합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Gets or sets the board, device, or controller index.</para>
    /// \endif
    /// </summary>
    public int DeviceIndex { get; set; }

    /// <summary>
    /// \if KO
    /// <para>연결 시간 제한을 밀리초 단위로 가져오거나 설정합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Gets or sets the connection timeout in milliseconds.</para>
    /// \endif
    /// </summary>
    public int ConnectTimeoutMs { get; set; } = 3000;

    /// <summary>
    /// \if KO
    /// <para>수신 시간 제한을 밀리초 단위로 가져오거나 설정합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Gets or sets the receive timeout in milliseconds.</para>
    /// \endif
    /// </summary>
    public int ReceiveTimeoutMs { get; set; } = 3000;

    /// <summary>
    /// \if KO
    /// <para>실패 후 재시도 횟수를 가져오거나 설정합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Gets or sets the retry count after a failure.</para>
    /// \endif
    /// </summary>
    public int RetryCount { get; set; } = 1;

    /// <summary>
    /// \if KO
    /// <para>예상 응답 길이를 가져오거나 설정합니다. 0이면 첫 수신 패킷이 끝날 때까지 읽습니다.</para>
    /// \endif
    /// \if EN
    /// <para>Gets or sets the expected response length. Zero reads until the first available packet ends.</para>
    /// \endif
    /// </summary>
    public int ExpectedResponseLength { get; set; }

    /// <summary>
    /// \if KO
    /// <para>구체 프로토콜 구현만 해석해야 하는 공급자별 속성 사전을 가져옵니다.</para>
    /// \endif
    /// \if EN
    /// <para>Gets provider-specific properties interpreted only by the concrete protocol implementation.</para>
    /// \endif
    /// </summary>
    public IDictionary<string, string> Properties { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// \if KO
    /// <para>현재 설정을 공급자 독립 I/O 연결 옵션으로 변환합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Converts the current settings to provider-neutral I/O connection options.</para>
    /// \endif
    /// </summary>
    /// <returns>
    /// \if KO
    /// <para>Fastech 공급자와 연결 속성을 포함하는 새 옵션입니다.</para>
    /// \endif
    /// \if EN
    /// <para>New options containing the Fastech provider and connection properties.</para>
    /// \endif
    /// </returns>
    public IoConnectionOptions ToIoConnectionOptions()
    {
        var options = new IoConnectionOptions
        {
            Provider = IoProvider.Fastech,
            DeviceIndex = DeviceIndex,
            Name = Host
        };

        options.Properties["Host"] = Host;
        options.Properties["Port"] = Port.ToString();
        options.Properties["LocalPort"] = LocalPort.ToString();
        options.Properties["ConnectTimeoutMs"] = ConnectTimeoutMs.ToString();
        options.Properties["ReceiveTimeoutMs"] = ReceiveTimeoutMs.ToString();
        options.Properties["RetryCount"] = RetryCount.ToString();

        foreach (var property in Properties)
        {
            options.Properties[property.Key] = property.Value;
        }

        return options;
    }
}
