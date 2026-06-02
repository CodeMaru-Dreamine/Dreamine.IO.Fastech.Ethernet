using System.Net;
using System.Net.Sockets;
using Dreamine.IO.Abstractions.Results;
using Dreamine.IO.Fastech.Ethernet.Options;

namespace Dreamine.IO.Fastech.Ethernet.Transport;

/// <summary>
/// Provides UDP transport for Fastech Ezi-IO Plus-E communication.
/// </summary>
public sealed class UdpFastechEthernetIoTransport : IFastechEthernetIoTransport
{
    private readonly SemaphoreSlim _syncLock = new(1, 1);
    private readonly FastechEthernetIoOptions _options;
    private UdpClient? _udpClient;
    private IPEndPoint? _remoteEndPoint;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="UdpFastechEthernetIoTransport"/> class.
    /// </summary>
    /// <param name="options">The Fastech Ethernet I/O options.</param>
    public UdpFastechEthernetIoTransport(FastechEthernetIoOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <inheritdoc />
    public bool IsConnected => _udpClient is not null;

    /// <summary>
    /// Gets the last request frame sent by this transport.
    /// </summary>
    public byte[] LastRequestFrame { get; private set; } = [];

    /// <summary>
    /// Gets the last response frame received by this transport.
    /// </summary>
    public byte[] LastResponseFrame { get; private set; } = [];

    /// <inheritdoc />
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

    private Task CloseCoreAsync()
    {
        _udpClient?.Dispose();
        _udpClient = null;
        _remoteEndPoint = null;

        return Task.CompletedTask;
    }

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

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }
}
