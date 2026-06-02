using System.Net.Sockets;
using Dreamine.IO.Abstractions.Results;
using Dreamine.IO.Fastech.Ethernet.Options;

namespace Dreamine.IO.Fastech.Ethernet.Transport;

/// <summary>
/// Provides TCP transport for Fastech Ethernet I/O communication.
/// </summary>
public sealed class TcpFastechEthernetIoTransport : IFastechEthernetIoTransport
{
    private const int DefaultReceiveBufferSize = 4096;

    private readonly SemaphoreSlim _syncLock = new(1, 1);
    private readonly FastechEthernetIoOptions _options;
    private TcpClient? _tcpClient;
    private NetworkStream? _stream;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="TcpFastechEthernetIoTransport"/> class.
    /// </summary>
    /// <param name="options">The Fastech Ethernet I/O options.</param>
    public TcpFastechEthernetIoTransport(FastechEthernetIoOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <inheritdoc />
    public bool IsConnected => _tcpClient?.Connected == true && _stream is not null;

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }
}
