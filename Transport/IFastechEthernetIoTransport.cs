using Dreamine.IO.Abstractions.Results;

namespace Dreamine.IO.Fastech.Ethernet.Transport;

/// <summary>
/// \if KO
/// <para>Fastech Ethernet I/O 통신의 연결 및 요청/응답 전송 계약을 정의합니다.</para>
/// \endif
/// \if EN
/// <para>Defines connection and request-response transport operations for Fastech Ethernet I/O communication.</para>
/// \endif
/// </summary>
public interface IFastechEthernetIoTransport : IAsyncDisposable
{
    /// <summary>
    /// \if KO
    /// <para>전송이 연결되어 있는지 여부를 가져옵니다.</para>
    /// \endif
    /// \if EN
    /// <para>Gets whether the transport is connected.</para>
    /// \endif
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// \if KO
    /// <para>전송을 비동기적으로 연결합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Asynchronously connects the transport.</para>
    /// \endif
    /// </summary>
    /// <param name="cancellationToken">
    /// \if KO
    /// <para>연결 취소 토큰입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A token used to cancel connection.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>연결 결과입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The connection result.</para>
    /// \endif
    /// </returns>
    Task<IoResult> ConnectAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// \if KO
    /// <para>전송 연결을 비동기적으로 해제합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Asynchronously disconnects the transport.</para>
    /// \endif
    /// </summary>
    /// <param name="cancellationToken">
    /// \if KO
    /// <para>연결 해제 취소 토큰입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A token used to cancel disconnection.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>연결 해제 결과입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The disconnection result.</para>
    /// \endif
    /// </returns>
    Task<IoResult> DisconnectAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// \if KO
    /// <para>요청 프레임을 전송하고 대응하는 응답 프레임을 수신합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Sends a request frame and receives its corresponding response frame.</para>
    /// \endif
    /// </summary>
    /// <param name="requestFrame">
    /// \if KO
    /// <para>전송할 요청 바이트입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The request bytes to send.</para>
    /// \endif
    /// </param>
    /// <param name="receiveTimeoutMs">
    /// \if KO
    /// <para>수신 시간 제한(밀리초)입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The receive timeout in milliseconds.</para>
    /// \endif
    /// </param>
    /// <param name="expectedResponseLength">
    /// \if KO
    /// <para>예상 응답 길이이며 0이면 첫 패킷을 읽습니다.</para>
    /// \endif
    /// \if EN
    /// <para>The expected response length; zero reads the first packet.</para>
    /// \endif
    /// </param>
    /// <param name="cancellationToken">
    /// \if KO
    /// <para>교환 취소 토큰입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A token used to cancel the exchange.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>수신 응답 프레임을 포함하는 결과입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A result containing the received response frame.</para>
    /// \endif
    /// </returns>
    Task<IoResult<byte[]>> SendAndReceiveAsync(
        IReadOnlyList<byte> requestFrame,
        int receiveTimeoutMs,
        int expectedResponseLength,
        CancellationToken cancellationToken = default);
}
