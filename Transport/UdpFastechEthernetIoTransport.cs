using System.Net;
using System.Net.Sockets;
using Dreamine.IO.Abstractions.Results;
using Dreamine.IO.Fastech.Ethernet.Options;

namespace Dreamine.IO.Fastech.Ethernet.Transport;

/// <summary>
/// \if KO
/// <para>Fastech Ezi-IO Plus-E 통신을 위한 UDP 전송을 제공합니다.</para>
/// \endif
/// \if EN
/// <para>Provides UDP transport for Fastech Ezi-IO Plus-E communication.</para>
/// \endif
/// </summary>
public sealed class UdpFastechEthernetIoTransport : IFastechEthernetIoTransport
{
    /// <summary>
    /// \if KO
    /// <para>sync Lock 값을 보관합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Stores the sync lock value.</para>
    /// \endif
    /// </summary>
    private readonly SemaphoreSlim _syncLock = new(1, 1);
    /// <summary>
    /// \if KO
    /// <para>options 값을 보관합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Stores the options value.</para>
    /// \endif
    /// </summary>
    private readonly FastechEthernetIoOptions _options;
    /// <summary>
    /// \if KO
    /// <para>udp Client 값을 보관합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Stores the udp client value.</para>
    /// \endif
    /// </summary>
    private UdpClient? _udpClient;
    /// <summary>
    /// \if KO
    /// <para>remote End Point 값을 보관합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Stores the remote end point value.</para>
    /// \endif
    /// </summary>
    private IPEndPoint? _remoteEndPoint;
    /// <summary>
    /// \if KO
    /// <para>disposed 값을 보관합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Stores the disposed value.</para>
    /// \endif
    /// </summary>
    private bool _disposed;

    /// <summary>
    /// \if KO
    /// <para><see cref="UdpFastechEthernetIoTransport"/> 클래스의 새 인스턴스를 초기화합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Initializes a new instance of the <see cref="UdpFastechEthernetIoTransport"/> class.</para>
    /// \endif
    /// </summary>
    /// <param name="options">
    /// \if KO
    /// <para>Fastech Ethernet I/O 연결 옵션입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The Fastech Ethernet I/O connection options.</para>
    /// \endif
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// \if KO
    /// <para><paramref name="options"/>가 <see langword="null"/>인 경우 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when <paramref name="options"/> is <see langword="null"/>.</para>
    /// \endif
    /// </exception>
    public UdpFastechEthernetIoTransport(FastechEthernetIoOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// \if KO
    /// <para>UDP 클라이언트가 생성되어 있는지 여부를 가져옵니다.</para>
    /// \endif
    /// \if EN
    /// <para>Gets whether the UDP client has been created.</para>
    /// \endif
    /// </summary>
    public bool IsConnected => _udpClient is not null;

    /// <summary>
    /// \if KO
    /// <para>이 전송에서 마지막으로 보낸 요청 프레임을 가져옵니다.</para>
    /// \endif
    /// \if EN
    /// <para>Gets the last request frame sent by this transport.</para>
    /// \endif
    /// </summary>
    public byte[] LastRequestFrame { get; private set; } = [];

    /// <summary>
    /// \if KO
    /// <para>이 전송에서 마지막으로 받은 응답 프레임을 가져옵니다.</para>
    /// \endif
    /// \if EN
    /// <para>Gets the last response frame received by this transport.</para>
    /// \endif
    /// </summary>
    public byte[] LastResponseFrame { get; private set; } = [];

    /// <summary>
    /// \if KO
    /// <para>구성된 원격 끝점을 확인하고 UDP 클라이언트를 준비합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Resolves the configured remote endpoint and prepares the UDP client.</para>
    /// \endif
    /// </summary>
    /// <param name="cancellationToken">
    /// \if KO
    /// <para>연결 준비 취소를 관찰하는 토큰입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A token that observes cancellation of connection preparation.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>준비 성공 여부와 실패 메시지를 포함한 결과입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A result containing preparation success or a failure message.</para>
    /// \endif
    /// </returns>
    /// <exception cref="OperationCanceledException">
    /// \if KO
    /// <para>동기화 잠금을 기다리는 동안 <paramref name="cancellationToken"/>이 취소된 경우 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when <paramref name="cancellationToken"/> is canceled while waiting for the synchronization lock.</para>
    /// \endif
    /// </exception>
    public async Task<IoResult> ConnectAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.Host))
        {
            return IoResult.Failure("The Fastech host must not be empty.");
        }

        if (_options.Port is <= 0 or > 65535)
        {
            return IoResult.Failure($"Invalid UDP port: {_options.Port}.");
        }

        if (_options.LocalPort is < 0 or > 65535)
        {
            return IoResult.Failure($"Invalid local UDP port: {_options.LocalPort}.");
        }

        await _syncLock.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            ThrowIfDisposed();

            if (IsConnected)
            {
                return IoResult.Success();
            }

            var remoteAddress = await ResolveRemoteAddressAsync(cancellationToken).ConfigureAwait(false);
            _remoteEndPoint = new IPEndPoint(remoteAddress, _options.Port);
            _udpClient = _options.LocalPort == 0
                ? new UdpClient(remoteAddress.AddressFamily)
                : new UdpClient(_options.LocalPort, remoteAddress.AddressFamily);

            DisableUdpConnectionReset(_udpClient);

            return IoResult.Success();
        }
        catch (Exception ex)
        {
            await CloseCoreAsync().ConfigureAwait(false);
            return IoResult.Failure(ex.Message);
        }
        finally
        {
            _syncLock.Release();
        }
    }

    /// <summary>
    /// \if KO
    /// <para>현재 UDP 클라이언트와 원격 끝점 정보를 해제합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Releases the current UDP client and remote endpoint information.</para>
    /// \endif
    /// </summary>
    /// <param name="cancellationToken">
    /// \if KO
    /// <para>연결 해제 취소를 관찰하는 토큰입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A token that observes cancellation of disconnection.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>연결 해제 성공 여부와 실패 메시지를 포함한 결과입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A result containing disconnection success or a failure message.</para>
    /// \endif
    /// </returns>
    /// <exception cref="OperationCanceledException">
    /// \if KO
    /// <para>동기화 잠금을 기다리는 동안 <paramref name="cancellationToken"/>이 취소된 경우 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when <paramref name="cancellationToken"/> is canceled while waiting for the synchronization lock.</para>
    /// \endif
    /// </exception>
    public async Task<IoResult> DisconnectAsync(CancellationToken cancellationToken = default)
    {
        await _syncLock.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            await CloseCoreAsync().ConfigureAwait(false);
            return IoResult.Success();
        }
        catch (Exception ex)
        {
            return IoResult.Failure(ex.Message);
        }
        finally
        {
            _syncLock.Release();
        }
    }

    /// <summary>
    /// \if KO
    /// <para>요청 데이터그램을 보내고 하나의 UDP 응답 데이터그램을 받습니다.</para>
    /// \endif
    /// \if EN
    /// <para>Sends a request datagram and receives one UDP response datagram.</para>
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
    /// <para>밀리초 단위의 수신 제한 시간입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The receive timeout in milliseconds.</para>
    /// \endif
    /// </param>
    /// <param name="expectedResponseLength">
    /// \if KO
    /// <para>인터페이스 호환성을 위한 기대 응답 길이이며 UDP에서는 사용하지 않습니다.</para>
    /// \endif
    /// \if EN
    /// <para>The expected response length retained for interface compatibility; UDP does not use it.</para>
    /// \endif
    /// </param>
    /// <param name="cancellationToken">
    /// \if KO
    /// <para>송수신 취소를 관찰하는 토큰입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A token that observes cancellation of send/receive.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>수신 데이터그램 또는 실패 메시지를 포함한 결과입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A result containing the received datagram or a failure message.</para>
    /// \endif
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// \if KO
    /// <para><paramref name="requestFrame"/>이 <see langword="null"/>인 경우 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when <paramref name="requestFrame"/> is <see langword="null"/>.</para>
    /// \endif
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// \if KO
    /// <para>동기화 잠금을 기다리는 동안 <paramref name="cancellationToken"/>이 취소된 경우 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when <paramref name="cancellationToken"/> is canceled while waiting for the synchronization lock.</para>
    /// \endif
    /// </exception>
    public async Task<IoResult<byte[]>> SendAndReceiveAsync(
        IReadOnlyList<byte> requestFrame,
        int receiveTimeoutMs,
        int expectedResponseLength,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(requestFrame);

        if (requestFrame.Count == 0)
        {
            return IoResult<byte[]>.Failure("The request frame must not be empty.");
        }

        if (receiveTimeoutMs <= 0)
        {
            return IoResult<byte[]>.Failure("The receive timeout must be greater than zero.");
        }

        await _syncLock.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            ThrowIfDisposed();

            if (_udpClient is null || _remoteEndPoint is null)
            {
                return IoResult<byte[]>.Failure("The Fastech UDP transport is not connected.");
            }

            var requestBuffer = requestFrame as byte[] ?? requestFrame.ToArray();
            LastRequestFrame = requestBuffer;
            LastResponseFrame = [];

            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(receiveTimeoutMs);

            await _udpClient.SendAsync(requestBuffer, _remoteEndPoint, timeoutCts.Token).ConfigureAwait(false);
            var response = await _udpClient.ReceiveAsync(timeoutCts.Token).ConfigureAwait(false);
            LastResponseFrame = response.Buffer;

            return IoResult<byte[]>.Success(response.Buffer);
        }
        catch (OperationCanceledException ex)
        {
            return IoResult<byte[]>.Failure($"No UDP response from {_options.Host}:{_options.Port} within {receiveTimeoutMs} ms. {ex.Message}");
        }
        catch (SocketException ex) when (ex.SocketErrorCode == SocketError.ConnectionReset)
        {
            return IoResult<byte[]>.Failure(
                $"No UDP listener responded at {_options.Host}:{_options.Port}. Windows reported UDP reset/ICMP port unreachable. {ex.Message}");
        }
        catch (Exception ex)
        {
            return IoResult<byte[]>.Failure(ex.Message);
        }
        finally
        {
            _syncLock.Release();
        }
    }

    /// <summary>
    /// \if KO
    /// <para>UDP 클라이언트와 동기화 리소스를 비동기적으로 해제합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Asynchronously releases the UDP client and synchronization resources.</para>
    /// \endif
    /// </summary>
    /// <returns>
    /// \if KO
    /// <para>비동기 해제 작업입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The asynchronous disposal operation.</para>
    /// \endif
    /// </returns>
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        await DisconnectAsync().ConfigureAwait(false);
        _syncLock.Dispose();
        _disposed = true;

        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// \if KO
    /// <para>내부 UDP 클라이언트를 닫고 끝점 참조를 초기화합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Closes the internal UDP client and clears the endpoint reference.</para>
    /// \endif
    /// </summary>
    /// <returns>
    /// \if KO
    /// <para>완료된 비동기 작업입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A completed asynchronous operation.</para>
    /// \endif
    /// </returns>
    private Task CloseCoreAsync()
    {
        _udpClient?.Dispose();
        _udpClient = null;
        _remoteEndPoint = null;

        return Task.CompletedTask;
    }

    /// <summary>
    /// \if KO
    /// <para>구성된 호스트 이름 또는 IP 문자열을 원격 IP 주소로 확인합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Resolves the configured host name or IP literal to a remote IP address.</para>
    /// \endif
    /// </summary>
    /// <param name="cancellationToken">
    /// \if KO
    /// <para>DNS 조회 취소를 관찰하는 토큰입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A token that observes cancellation of DNS resolution.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>확인된 IPv4 우선 IP 주소입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The resolved IP address, preferring IPv4.</para>
    /// \endif
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// \if KO
    /// <para>호스트에서 사용할 수 있는 주소를 확인하지 못한 경우 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when no usable address can be resolved for the host.</para>
    /// \endif
    /// </exception>
    private async Task<IPAddress> ResolveRemoteAddressAsync(CancellationToken cancellationToken)
    {
        if (IPAddress.TryParse(_options.Host, out var address))
        {
            return address;
        }

        var addresses = await Dns.GetHostAddressesAsync(_options.Host, cancellationToken).ConfigureAwait(false);
        var resolvedAddress = addresses.FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork)
            ?? addresses.FirstOrDefault();

        return resolvedAddress ?? throw new InvalidOperationException($"Unable to resolve UDP host: {_options.Host}.");
    }

    /// <summary>
    /// \if KO
    /// <para>Windows에서 ICMP 포트 연결 불가 응답에 따른 UDP 연결 재설정 동작을 비활성화합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Disables UDP connection-reset behavior caused by ICMP port-unreachable replies on Windows.</para>
    /// \endif
    /// </summary>
    /// <param name="client">
    /// \if KO
    /// <para>설정할 UDP 클라이언트입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The UDP client to configure.</para>
    /// \endif
    /// </param>
    private static void DisableUdpConnectionReset(UdpClient client)
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        try
        {
            const int sioUdpConnReset = -1744830452;
            client.Client.IOControl(sioUdpConnReset, [0], null);
        }
        catch (SocketException)
        {
        }
    }

    /// <summary>
    /// \if KO
    /// <para>이 전송 객체가 이미 해제되었으면 예외를 발생시킵니다.</para>
    /// \endif
    /// \if EN
    /// <para>Throws when this transport has already been disposed.</para>
    /// \endif
    /// </summary>
    /// <exception cref="ObjectDisposedException">
    /// \if KO
    /// <para>객체가 이미 해제된 경우 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when the object has already been disposed.</para>
    /// \endif
    /// </exception>
    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }
}
