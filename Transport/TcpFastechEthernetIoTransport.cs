using System.Net.Sockets;
using Dreamine.IO.Abstractions.Results;
using Dreamine.IO.Fastech.Ethernet.Options;

namespace Dreamine.IO.Fastech.Ethernet.Transport;

/// <summary>
/// \if KO
/// <para>Fastech Ethernet I/O 통신을 위한 TCP 전송을 제공합니다.</para>
/// \endif
/// \if EN
/// <para>Provides TCP transport for Fastech Ethernet I/O communication.</para>
/// \endif
/// </summary>
public sealed class TcpFastechEthernetIoTransport : IFastechEthernetIoTransport
{
    /// <summary>
    /// \if KO
    /// <para>Default Receive Buffer Size 값을 보관합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Stores the default receive buffer size value.</para>
    /// \endif
    /// </summary>
    private const int DefaultReceiveBufferSize = 4096;

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
    /// <para>tcp Client 값을 보관합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Stores the tcp client value.</para>
    /// \endif
    /// </summary>
    private TcpClient? _tcpClient;
    /// <summary>
    /// \if KO
    /// <para>stream 값을 보관합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Stores the stream value.</para>
    /// \endif
    /// </summary>
    private NetworkStream? _stream;
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
    /// <para><see cref="TcpFastechEthernetIoTransport"/> 클래스의 새 인스턴스를 초기화합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Initializes a new instance of the <see cref="TcpFastechEthernetIoTransport"/> class.</para>
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
    public TcpFastechEthernetIoTransport(FastechEthernetIoOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// \if KO
    /// <para>TCP 클라이언트와 네트워크 스트림이 연결되어 있는지 여부를 가져옵니다.</para>
    /// \endif
    /// \if EN
    /// <para>Gets whether the TCP client and network stream are connected.</para>
    /// \endif
    /// </summary>
    public bool IsConnected => _tcpClient?.Connected == true && _stream is not null;

    /// <summary>
    /// \if KO
    /// <para>구성된 호스트와 포트에 TCP 연결을 설정합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Establishes a TCP connection to the configured host and port.</para>
    /// \endif
    /// </summary>
    /// <param name="cancellationToken">
    /// \if KO
    /// <para>연결 작업 취소를 관찰하는 토큰입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A token that observes cancellation of the connection operation.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>연결 성공 여부와 실패 메시지를 포함한 결과입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A result containing connection success or a failure message.</para>
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
            return IoResult.Failure($"Invalid TCP port: {_options.Port}.");
        }

        if (_options.ConnectTimeoutMs <= 0)
        {
            return IoResult.Failure("The connection timeout must be greater than zero.");
        }

        await _syncLock.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            ThrowIfDisposed();

            if (IsConnected)
            {
                return IoResult.Success();
            }

            await CloseCoreAsync().ConfigureAwait(false);

            _tcpClient = new TcpClient
            {
                NoDelay = true
            };

            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(_options.ConnectTimeoutMs);

            await _tcpClient.ConnectAsync(_options.Host, _options.Port, timeoutCts.Token).ConfigureAwait(false);
            _stream = _tcpClient.GetStream();

            return IoResult.Success();
        }
        catch (OperationCanceledException ex)
        {
            await CloseCoreAsync().ConfigureAwait(false);
            return IoResult.Failure(ex.Message);
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
    /// <para>현재 TCP 연결과 스트림을 닫습니다.</para>
    /// \endif
    /// \if EN
    /// <para>Closes the current TCP connection and stream.</para>
    /// \endif
    /// </summary>
    /// <param name="cancellationToken">
    /// \if KO
    /// <para>연결 해제 작업 취소를 관찰하는 토큰입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A token that observes cancellation of the disconnection operation.</para>
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
    /// <para>요청 프레임을 전송하고 TCP 응답 프레임을 수신합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Sends a request frame and receives the TCP response frame.</para>
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
    /// <para>기대하는 응답 길이이며, 0이면 한 번에 사용 가능한 데이터를 읽습니다.</para>
    /// \endif
    /// \if EN
    /// <para>The expected response length; zero reads the data available in one operation.</para>
    /// \endif
    /// </param>
    /// <param name="cancellationToken">
    /// \if KO
    /// <para>송수신 작업 취소를 관찰하는 토큰입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A token that observes cancellation of the send/receive operation.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>수신한 바이트 또는 실패 메시지를 포함한 결과입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A result containing the received bytes or a failure message.</para>
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

        if (expectedResponseLength < 0)
        {
            return IoResult<byte[]>.Failure("The expected response length must not be negative.");
        }

        await _syncLock.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            ThrowIfDisposed();

            if (_stream is null || _tcpClient is null || !_tcpClient.Connected)
            {
                return IoResult<byte[]>.Failure("The Fastech TCP transport is not connected.");
            }

            var requestBuffer = requestFrame as byte[] ?? requestFrame.ToArray();

            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(receiveTimeoutMs);

            await _stream.WriteAsync(requestBuffer, timeoutCts.Token).ConfigureAwait(false);
            await _stream.FlushAsync(timeoutCts.Token).ConfigureAwait(false);

            var response = expectedResponseLength > 0
                ? await ReadExactlyAsync(_stream, expectedResponseLength, timeoutCts.Token).ConfigureAwait(false)
                : await ReadAvailableAsync(_stream, timeoutCts.Token).ConfigureAwait(false);

            return IoResult<byte[]>.Success(response);
        }
        catch (OperationCanceledException ex)
        {
            return IoResult<byte[]>.Failure(ex.Message);
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
    /// <para>연결과 동기화 리소스를 비동기적으로 해제합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Asynchronously releases the connection and synchronization resources.</para>
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
    /// <para>스트림에서 지정된 바이트 수를 모두 읽습니다.</para>
    /// \endif
    /// \if EN
    /// <para>Reads exactly the specified number of bytes from the stream.</para>
    /// \endif
    /// </summary>
    /// <param name="stream">
    /// \if KO
    /// <para>읽을 네트워크 스트림입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The network stream to read.</para>
    /// \endif
    /// </param>
    /// <param name="length">
    /// \if KO
    /// <para>읽어야 할 바이트 수입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The number of bytes to read.</para>
    /// \endif
    /// </param>
    /// <param name="cancellationToken">
    /// \if KO
    /// <para>읽기 취소를 관찰하는 토큰입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A token that observes cancellation of the read.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>정확히 요청된 길이의 바이트 배열입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A byte array with exactly the requested length.</para>
    /// \endif
    /// </returns>
    /// <exception cref="IOException">
    /// \if KO
    /// <para>요청된 길이를 읽기 전에 원격 끝점이 연결을 닫은 경우 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when the remote endpoint closes the connection before the requested length is read.</para>
    /// \endif
    /// </exception>
    private static async Task<byte[]> ReadExactlyAsync(
        NetworkStream stream,
        int length,
        CancellationToken cancellationToken)
    {
        var buffer = new byte[length];
        var offset = 0;

        while (offset < buffer.Length)
        {
            var read = await stream.ReadAsync(
                buffer.AsMemory(offset, buffer.Length - offset),
                cancellationToken).ConfigureAwait(false);

            if (read == 0)
            {
                throw new IOException("The remote Fastech endpoint closed the TCP connection.");
            }

            offset += read;
        }

        return buffer;
    }

    /// <summary>
    /// \if KO
    /// <para>스트림에서 현재 수신 가능한 데이터를 한 번 읽습니다.</para>
    /// \endif
    /// \if EN
    /// <para>Reads the currently available data from the stream in one operation.</para>
    /// \endif
    /// </summary>
    /// <param name="stream">
    /// \if KO
    /// <para>읽을 네트워크 스트림입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The network stream to read.</para>
    /// \endif
    /// </param>
    /// <param name="cancellationToken">
    /// \if KO
    /// <para>읽기 취소를 관찰하는 토큰입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A token that observes cancellation of the read.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>수신한 바이트 배열입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The received byte array.</para>
    /// \endif
    /// </returns>
    /// <exception cref="IOException">
    /// \if KO
    /// <para>데이터를 읽기 전에 원격 끝점이 연결을 닫은 경우 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when the remote endpoint closes the connection before data is read.</para>
    /// \endif
    /// </exception>
    private static async Task<byte[]> ReadAvailableAsync(
        NetworkStream stream,
        CancellationToken cancellationToken)
    {
        var buffer = new byte[DefaultReceiveBufferSize];
        var read = await stream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);

        if (read == 0)
        {
            throw new IOException("The remote Fastech endpoint closed the TCP connection.");
        }

        return buffer[..read];
    }

    /// <summary>
    /// \if KO
    /// <para>내부 TCP 스트림과 클라이언트를 닫고 참조를 초기화합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Closes the internal TCP stream and client and clears their references.</para>
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
        try
        {
            _stream?.Dispose();
            _tcpClient?.Dispose();
        }
        finally
        {
            _stream = null;
            _tcpClient = null;
        }

        return Task.CompletedTask;
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
