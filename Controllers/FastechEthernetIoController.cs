using Dreamine.IO.Abstractions.Channels;
using Dreamine.IO.Abstractions.Controllers;
using Dreamine.IO.Abstractions.Enums;
using Dreamine.IO.Abstractions.Models;
using Dreamine.IO.Abstractions.Results;
using Dreamine.IO.Fastech.Ethernet.Channels;
using Dreamine.IO.Fastech.Ethernet.Options;
using Dreamine.IO.Fastech.Ethernet.Protocol;
using Dreamine.IO.Fastech.Ethernet.Transport;

namespace Dreamine.IO.Fastech.Ethernet.Controllers;

/// <summary>
/// Provides a Fastech Ethernet I/O controller implementation.
/// </summary>
public sealed class FastechEthernetIoController : IIoController
{
    private readonly FastechEthernetIoOptions _options;
    private readonly IFastechEthernetIoTransport _transport;
    private readonly IFastechEthernetIoProtocol _protocol;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="FastechEthernetIoController"/> class.
    /// </summary>
    /// <param name="options">The Fastech Ethernet I/O options.</param>
    public FastechEthernetIoController(FastechEthernetIoOptions options)
        : this(
            options,
            CreateTransport(options),
            new FastechPlusE16PointProtocol())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FastechEthernetIoController"/> class.
    /// </summary>
    /// <param name="options">The Fastech Ethernet I/O options.</param>
    /// <param name="protocol">The Fastech Ethernet I/O protocol implementation.</param>
    public FastechEthernetIoController(
        FastechEthernetIoOptions options,
        IFastechEthernetIoProtocol protocol)
        : this(options, CreateTransport(options), protocol)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FastechEthernetIoController"/> class.
    /// </summary>
    /// <param name="options">The Fastech Ethernet I/O options.</param>
    /// <param name="transport">The Fastech Ethernet I/O transport.</param>
    /// <param name="protocol">The Fastech Ethernet I/O protocol implementation.</param>
    public FastechEthernetIoController(
        FastechEthernetIoOptions options,
        IFastechEthernetIoTransport transport,
        IFastechEthernetIoProtocol protocol)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _transport = transport ?? throw new ArgumentNullException(nameof(transport));
        _protocol = protocol ?? throw new ArgumentNullException(nameof(protocol));

        DigitalInputs = new FastechDigitalInputChannel(this);
        DigitalOutputs = new FastechDigitalOutputChannel(this);
        AnalogInputs = new FastechAnalogInputChannel(this);
        AnalogOutputs = new FastechAnalogOutputChannel(this);
    }

    /// <summary>
    /// Gets the Fastech Ethernet I/O options.
    /// </summary>
    public FastechEthernetIoOptions Options => _options;

    /// <inheritdoc />
    public IoConnectionState State { get; private set; } = IoConnectionState.Disconnected;

    /// <inheritdoc />
    public event EventHandler<IoConnectionState>? StateChanged;

    /// <inheritdoc />
    public IDigitalInputChannel DigitalInputs { get; }

    /// <inheritdoc />
    public IDigitalOutputChannel DigitalOutputs { get; }

    /// <inheritdoc />
    public IAnalogInputChannel AnalogInputs { get; }

    /// <inheritdoc />
    public IAnalogOutputChannel AnalogOutputs { get; }

    /// <inheritdoc />
    public async Task<IoResult> ConnectAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        SetState(IoConnectionState.Connecting);

        var result = await _transport.ConnectAsync(cancellationToken).ConfigureAwait(false);
        SetState(result.IsSuccess ? IoConnectionState.Connected : IoConnectionState.Faulted);

        return result;
    }

    /// <inheritdoc />
    public async Task<IoResult> DisconnectAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        SetState(IoConnectionState.Disconnecting);

        var result = await _transport.DisconnectAsync(cancellationToken).ConfigureAwait(false);
        SetState(result.IsSuccess ? IoConnectionState.Disconnected : IoConnectionState.Faulted);

        return result;
    }

    internal Task<IoResult<bool[]>> ReadDigitalInputsAsync(
        IReadOnlyList<IoPoint> points,
        CancellationToken cancellationToken)
    {
        return SendReadAsync(
            points,
            _protocol.BuildReadDigitalInputs,
            response => _protocol.ParseDigitalInputs(response, points.Count),
            cancellationToken);
    }

    internal Task<IoResult<bool[]>> ReadDigitalOutputsAsync(
        IReadOnlyList<IoPoint> points,
        CancellationToken cancellationToken)
    {
        return SendReadAsync(
            points,
            _protocol.BuildReadDigitalOutputs,
            response => _protocol.ParseDigitalOutputs(response, points.Count),
            cancellationToken);
    }

    internal Task<IoResult> WriteDigitalOutputsAsync(
        IReadOnlyDictionary<IoPoint, bool> values,
        CancellationToken cancellationToken)
    {
        return SendWriteAsync(
            values,
            _protocol.BuildWriteDigitalOutputs,
            _protocol.ParseWriteResponse,
            cancellationToken);
    }

    internal Task<IoResult<double[]>> ReadAnalogInputsAsync(
        IReadOnlyList<AnalogIoPoint> points,
        CancellationToken cancellationToken)
    {
        return SendReadAsync(
            points,
            _protocol.BuildReadAnalogInputs,
            response => _protocol.ParseAnalogInputs(response, points.Count),
            cancellationToken);
    }

    internal Task<IoResult<double[]>> ReadAnalogOutputsAsync(
        IReadOnlyList<AnalogIoPoint> points,
        CancellationToken cancellationToken)
    {
        return SendReadAsync(
            points,
            _protocol.BuildReadAnalogOutputs,
            response => _protocol.ParseAnalogOutputs(response, points.Count),
            cancellationToken);
    }

    internal Task<IoResult> WriteAnalogOutputsAsync(
        IReadOnlyDictionary<AnalogIoPoint, double> values,
        CancellationToken cancellationToken)
    {
        return SendWriteAsync(
            values,
            _protocol.BuildWriteAnalogOutputs,
            _protocol.ParseWriteResponse,
            cancellationToken);
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        await _transport.DisposeAsync().ConfigureAwait(false);
        _disposed = true;
        SetState(IoConnectionState.Disconnected);

        GC.SuppressFinalize(this);
    }

    private async Task<IoResult<TValue[]>> SendReadAsync<TPoint, TValue>(
        IReadOnlyList<TPoint> points,
        Func<IReadOnlyList<TPoint>, byte[]> buildRequest,
        Func<IReadOnlyList<byte>, IoResult<TValue[]>> parseResponse,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(points);

        if (points.Count == 0)
        {
            return IoResult<TValue[]>.Failure("At least one I/O point is required.");
        }

        byte[] request;
        try
        {
            request = buildRequest(points);
        }
        catch (Exception ex)
        {
            return IoResult<TValue[]>.Failure(ex.Message);
        }

        var response = await SendAsync(request, cancellationToken).ConfigureAwait(false);
        return !response.IsSuccess || response.Value is null
            ? IoResult<TValue[]>.Failure(response.Message ?? "Failed to receive the Fastech Ethernet I/O response.", response.ErrorCode)
            : parseResponse(response.Value);
    }

    private async Task<IoResult> SendWriteAsync<TKey, TValue>(
        IReadOnlyDictionary<TKey, TValue> values,
        Func<IReadOnlyDictionary<TKey, TValue>, byte[]> buildRequest,
        Func<IReadOnlyList<byte>, IoResult> parseResponse,
        CancellationToken cancellationToken)
        where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(values);

        if (values.Count == 0)
        {
            return IoResult.Failure("At least one I/O value is required.");
        }

        byte[] request;
        try
        {
            request = buildRequest(values);
        }
        catch (Exception ex)
        {
            return IoResult.Failure(ex.Message);
        }

        var response = await SendAsync(request, cancellationToken).ConfigureAwait(false);
        return !response.IsSuccess || response.Value is null
            ? IoResult.Failure(response.Message ?? "Failed to receive the Fastech Ethernet I/O response.", response.ErrorCode)
            : parseResponse(response.Value);
    }

    private async Task<IoResult<byte[]>> SendAsync(byte[] request, CancellationToken cancellationToken)
    {
        ThrowIfDisposed();

        if (State != IoConnectionState.Connected || !_transport.IsConnected)
        {
            return IoResult<byte[]>.Failure("The Fastech Ethernet I/O controller is not connected.");
        }

        IoResult<byte[]>? lastResult = null;

        for (var attempt = 0; attempt <= _options.RetryCount; attempt++)
        {
            lastResult = await _transport.SendAndReceiveAsync(
                request,
                _options.ReceiveTimeoutMs,
                _options.ExpectedResponseLength,
                cancellationToken).ConfigureAwait(false);

            if (lastResult.IsSuccess)
            {
                return lastResult;
            }
        }

        return lastResult ?? IoResult<byte[]>.Failure("The Fastech Ethernet I/O request was not sent.");
    }

    private void SetState(IoConnectionState state)
    {
        if (State == state)
        {
            return;
        }

        State = state;
        StateChanged?.Invoke(this, state);
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }

    private static IFastechEthernetIoTransport CreateTransport(FastechEthernetIoOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        return options.TransportType switch
        {
            FastechEthernetIoTransportType.Udp => new UdpFastechEthernetIoTransport(options),
            FastechEthernetIoTransportType.Tcp => new TcpFastechEthernetIoTransport(options),
            _ => throw new ArgumentOutOfRangeException(nameof(options), options.TransportType, "Unsupported Fastech Ethernet I/O transport type.")
        };
    }
}
