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
/// \if KO
/// <para>전송과 장치 프로토콜을 조합하여 Fastech Ethernet I/O 채널을 제공하는 컨트롤러입니다.</para>
/// \endif
/// \if EN
/// <para>Provides a Fastech Ethernet I/O controller that composes a transport and device protocol into channel operations.</para>
/// \endif
/// </summary>
public sealed class FastechEthernetIoController : IIoController
{
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
    /// <para>transport 값을 보관합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Stores the transport value.</para>
    /// \endif
    /// </summary>
    private readonly IFastechEthernetIoTransport _transport;
    /// <summary>
    /// \if KO
    /// <para>protocol 값을 보관합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Stores the protocol value.</para>
    /// \endif
    /// </summary>
    private readonly IFastechEthernetIoProtocol _protocol;
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
    /// <para>옵션에 맞는 기본 전송과 검증된 16점 DIO 프로토콜로 컨트롤러를 초기화합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Initializes the controller with the default transport and verified 16-point DIO protocol for the options.</para>
    /// \endif
    /// </summary>
    /// <param name="options">
    /// \if KO
    /// <para>Fastech 연결 옵션입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The Fastech connection options.</para>
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
    /// <exception cref="ArgumentOutOfRangeException">
    /// \if KO
    /// <para>전송 형식이 지원되지 않는 경우 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when the transport type is unsupported.</para>
    /// \endif
    /// </exception>
    public FastechEthernetIoController(FastechEthernetIoOptions options)
        : this(
            options,
            CreateTransport(options),
            new FastechPlusE16PointProtocol())
    {
    }

    /// <summary>
    /// \if KO
    /// <para>옵션에 맞는 기본 전송과 지정한 프로토콜로 컨트롤러를 초기화합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Initializes the controller with the default transport and specified protocol.</para>
    /// \endif
    /// </summary>
    /// <param name="options">
    /// \if KO
    /// <para>Fastech 연결 옵션입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The Fastech connection options.</para>
    /// \endif
    /// </param>
    /// <param name="protocol">
    /// \if KO
    /// <para>장치 프로토콜 구현입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The device-protocol implementation.</para>
    /// \endif
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// \if KO
    /// <para>인수 중 하나가 <see langword="null"/>인 경우 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when either argument is <see langword="null"/>.</para>
    /// \endif
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// \if KO
    /// <para>전송 형식이 지원되지 않는 경우 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when the transport type is unsupported.</para>
    /// \endif
    /// </exception>
    public FastechEthernetIoController(
        FastechEthernetIoOptions options,
        IFastechEthernetIoProtocol protocol)
        : this(options, CreateTransport(options), protocol)
    {
    }

    /// <summary>
    /// \if KO
    /// <para>지정한 옵션, 전송 및 프로토콜로 컨트롤러와 네 채널을 초기화합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Initializes the controller and its four channels with the specified options, transport, and protocol.</para>
    /// \endif
    /// </summary>
    /// <param name="options">
    /// \if KO
    /// <para>Fastech 연결 옵션입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The Fastech connection options.</para>
    /// \endif
    /// </param>
    /// <param name="transport">
    /// \if KO
    /// <para>요청/응답 전송입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The request-response transport.</para>
    /// \endif
    /// </param>
    /// <param name="protocol">
    /// \if KO
    /// <para>장치 프로토콜 구현입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The device-protocol implementation.</para>
    /// \endif
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// \if KO
    /// <para>인수 중 하나가 <see langword="null"/>인 경우 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when any argument is <see langword="null"/>.</para>
    /// \endif
    /// </exception>
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
    /// \if KO
    /// <para>현재 Fastech Ethernet I/O 옵션을 가져옵니다.</para>
    /// \endif
    /// \if EN
    /// <para>Gets the current Fastech Ethernet I/O options.</para>
    /// \endif
    /// </summary>
    public FastechEthernetIoOptions Options => _options;

    /// <summary>
    /// \if KO
    /// <para>현재 연결 상태를 가져옵니다.</para>
    /// \endif
    /// \if EN
    /// <para>Gets the current connection state.</para>
    /// \endif
    /// </summary>
    public IoConnectionState State { get; private set; } = IoConnectionState.Disconnected;

    /// <summary>
    /// \if KO
    /// <para>연결 상태가 변경될 때 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Occurs when the connection state changes.</para>
    /// \endif
    /// </summary>
    public event EventHandler<IoConnectionState>? StateChanged;

    /// <summary>
    /// \if KO
    /// <para>디지털 입력 채널을 가져옵니다.</para>
    /// \endif
    /// \if EN
    /// <para>Gets the digital-input channel.</para>
    /// \endif
    /// </summary>
    public IDigitalInputChannel DigitalInputs { get; }

    /// <summary>
    /// \if KO
    /// <para>디지털 출력 채널을 가져옵니다.</para>
    /// \endif
    /// \if EN
    /// <para>Gets the digital-output channel.</para>
    /// \endif
    /// </summary>
    public IDigitalOutputChannel DigitalOutputs { get; }

    /// <summary>
    /// \if KO
    /// <para>아날로그 입력 채널을 가져옵니다.</para>
    /// \endif
    /// \if EN
    /// <para>Gets the analog-input channel.</para>
    /// \endif
    /// </summary>
    public IAnalogInputChannel AnalogInputs { get; }

    /// <summary>
    /// \if KO
    /// <para>아날로그 출력 채널을 가져옵니다.</para>
    /// \endif
    /// \if EN
    /// <para>Gets the analog-output channel.</para>
    /// \endif
    /// </summary>
    public IAnalogOutputChannel AnalogOutputs { get; }

    /// <summary>
    /// \if KO
    /// <para>전송을 연결하고 상태를 Connecting에서 Connected 또는 Faulted로 전환합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Connects the transport and transitions state from Connecting to Connected or Faulted.</para>
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
    /// <para>전송 연결 결과입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The transport-connection result.</para>
    /// \endif
    /// </returns>
    /// <exception cref="ObjectDisposedException">
    /// \if KO
    /// <para>컨트롤러가 해제된 경우 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when the controller has been disposed.</para>
    /// \endif
    /// </exception>
    public async Task<IoResult> ConnectAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        SetState(IoConnectionState.Connecting);

        var result = await _transport.ConnectAsync(cancellationToken).ConfigureAwait(false);
        SetState(result.IsSuccess ? IoConnectionState.Connected : IoConnectionState.Faulted);

        return result;
    }

    /// <summary>
    /// \if KO
    /// <para>전송 연결을 해제하고 상태를 Disconnecting에서 Disconnected 또는 Faulted로 전환합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Disconnects the transport and transitions state from Disconnecting to Disconnected or Faulted.</para>
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
    /// <para>전송 연결 해제 결과입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The transport-disconnection result.</para>
    /// \endif
    /// </returns>
    /// <exception cref="ObjectDisposedException">
    /// \if KO
    /// <para>컨트롤러가 해제된 경우 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when the controller has been disposed.</para>
    /// \endif
    /// </exception>
    public async Task<IoResult> DisconnectAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        SetState(IoConnectionState.Disconnecting);

        var result = await _transport.DisconnectAsync(cancellationToken).ConfigureAwait(false);
        SetState(result.IsSuccess ? IoConnectionState.Disconnected : IoConnectionState.Faulted);

        return result;
    }

    /// <summary>
    /// \if KO
    /// <para>프로토콜을 사용하여 디지털 입력 읽기 요청을 생성·전송·파싱합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Builds, sends, and parses a digital-input read through the protocol.</para>
    /// \endif
    /// </summary>
    /// <param name="points">
    /// \if KO
    /// <para>읽을 입력 지점입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The input points to read.</para>
    /// \endif
    /// </param>
    /// <param name="cancellationToken">
    /// \if KO
    /// <para>요청 취소 토큰입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A token used to cancel the request.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>입력 상태 배열 결과입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The input-state array result.</para>
    /// \endif
    /// </returns>
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

    /// <summary>
    /// \if KO
    /// <para>디지털 출력 상태 읽기 요청을 생성·전송·파싱합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Builds, sends, and parses a digital-output state read.</para>
    /// \endif
    /// </summary>
    /// <param name="points">
    /// \if KO
    /// <para>읽을 출력 지점입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The output points to read.</para>
    /// \endif
    /// </param>
    /// <param name="cancellationToken">
    /// \if KO
    /// <para>요청 취소 토큰입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A token used to cancel the request.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>출력 상태 배열 결과입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The output-state array result.</para>
    /// \endif
    /// </returns>
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

    /// <summary>
    /// \if KO
    /// <para>디지털 출력 쓰기 요청을 생성·전송·파싱합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Builds, sends, and parses a digital-output write.</para>
    /// \endif
    /// </summary>
    /// <param name="values">
    /// \if KO
    /// <para>지점별 출력 상태입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The output states keyed by point.</para>
    /// \endif
    /// </param>
    /// <param name="cancellationToken">
    /// \if KO
    /// <para>요청 취소 토큰입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A token used to cancel the request.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>쓰기 결과입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The write result.</para>
    /// \endif
    /// </returns>
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

    /// <summary>
    /// \if KO
    /// <para>아날로그 입력 읽기 요청을 생성·전송·파싱합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Builds, sends, and parses an analog-input read.</para>
    /// \endif
    /// </summary>
    /// <param name="points">
    /// \if KO
    /// <para>읽을 입력 지점입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The input points to read.</para>
    /// \endif
    /// </param>
    /// <param name="cancellationToken">
    /// \if KO
    /// <para>요청 취소 토큰입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A token used to cancel the request.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>아날로그 입력 배열 결과입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The analog-input array result.</para>
    /// \endif
    /// </returns>
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

    /// <summary>
    /// \if KO
    /// <para>아날로그 출력 상태 읽기 요청을 생성·전송·파싱합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Builds, sends, and parses an analog-output state read.</para>
    /// \endif
    /// </summary>
    /// <param name="points">
    /// \if KO
    /// <para>읽을 출력 지점입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The output points to read.</para>
    /// \endif
    /// </param>
    /// <param name="cancellationToken">
    /// \if KO
    /// <para>요청 취소 토큰입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A token used to cancel the request.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>아날로그 출력 배열 결과입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The analog-output array result.</para>
    /// \endif
    /// </returns>
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

    /// <summary>
    /// \if KO
    /// <para>아날로그 출력 쓰기 요청을 생성·전송·파싱합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Builds, sends, and parses an analog-output write.</para>
    /// \endif
    /// </summary>
    /// <param name="values">
    /// \if KO
    /// <para>지점별 출력 값입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The output values keyed by point.</para>
    /// \endif
    /// </param>
    /// <param name="cancellationToken">
    /// \if KO
    /// <para>요청 취소 토큰입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A token used to cancel the request.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>쓰기 결과입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The write result.</para>
    /// \endif
    /// </returns>
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

    /// <summary>
    /// \if KO
    /// <para>전송을 비동기 해제하고 컨트롤러를 Disconnected 상태로 전환합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Asynchronously disposes the transport and transitions the controller to Disconnected.</para>
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

        await _transport.DisposeAsync().ConfigureAwait(false);
        _disposed = true;
        SetState(IoConnectionState.Disconnected);

        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// \if KO
    /// <para>읽기 요청 생성·전송·파싱 단계의 예외를 I/O 실패 결과로 변환합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Converts exceptions from read request building, transport, and parsing into I/O failure results.</para>
    /// \endif
    /// </summary>
    /// <typeparam name="TPoint">
    /// \if KO
    /// <para>I/O 지점 형식입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The I/O point type.</para>
    /// \endif
    /// </typeparam>
    /// <typeparam name="TValue">
    /// \if KO
    /// <para>파싱할 값 형식입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The parsed value type.</para>
    /// \endif
    /// </typeparam>
    /// <param name="points">
    /// \if KO
    /// <para>요청할 지점입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The points to request.</para>
    /// \endif
    /// </param>
    /// <param name="buildRequest">
    /// \if KO
    /// <para>요청 프레임 생성기입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The request-frame builder.</para>
    /// \endif
    /// </param>
    /// <param name="parseResponse">
    /// \if KO
    /// <para>응답 파서입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The response parser.</para>
    /// \endif
    /// </param>
    /// <param name="cancellationToken">
    /// \if KO
    /// <para>요청 취소 토큰입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A token used to cancel the request.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>파싱된 값 배열 또는 실패 결과입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The parsed value array or a failure result.</para>
    /// \endif
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// \if KO
    /// <para><paramref name="points"/>가 <see langword="null"/>인 경우 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when <paramref name="points"/> is <see langword="null"/>.</para>
    /// \endif
    /// </exception>
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
        if (!response.IsSuccess || response.Value is null)
        {
            return IoResult<TValue[]>.Failure(response.Message ?? "Failed to receive the Fastech Ethernet I/O response.", response.ErrorCode);
        }

        try
        {
            return parseResponse(response.Value);
        }
        catch (Exception ex)
        {
            return IoResult<TValue[]>.Failure(ex.Message);
        }
    }

    /// <summary>
    /// \if KO
    /// <para>쓰기 요청 생성·전송·파싱 단계의 예외를 I/O 실패 결과로 변환합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Converts exceptions from write request building, transport, and parsing into I/O failure results.</para>
    /// \endif
    /// </summary>
    /// <typeparam name="TKey">
    /// \if KO
    /// <para>출력 키 형식입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The output-key type.</para>
    /// \endif
    /// </typeparam>
    /// <typeparam name="TValue">
    /// \if KO
    /// <para>출력 값 형식입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The output-value type.</para>
    /// \endif
    /// </typeparam>
    /// <param name="values">
    /// \if KO
    /// <para>쓸 키별 값입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The values keyed by output.</para>
    /// \endif
    /// </param>
    /// <param name="buildRequest">
    /// \if KO
    /// <para>요청 프레임 생성기입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The request-frame builder.</para>
    /// \endif
    /// </param>
    /// <param name="parseResponse">
    /// \if KO
    /// <para>쓰기 응답 파서입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The write-response parser.</para>
    /// \endif
    /// </param>
    /// <param name="cancellationToken">
    /// \if KO
    /// <para>요청 취소 토큰입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A token used to cancel the request.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>쓰기 성공 또는 실패 결과입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The write success or failure result.</para>
    /// \endif
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// \if KO
    /// <para><paramref name="values"/>가 <see langword="null"/>인 경우 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when <paramref name="values"/> is <see langword="null"/>.</para>
    /// \endif
    /// </exception>
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
        if (!response.IsSuccess || response.Value is null)
        {
            return IoResult.Failure(response.Message ?? "Failed to receive the Fastech Ethernet I/O response.", response.ErrorCode);
        }

        try
        {
            return parseResponse(response.Value);
        }
        catch (Exception ex)
        {
            return IoResult.Failure(ex.Message);
        }
    }

    /// <summary>
    /// \if KO
    /// <para>연결 상태를 확인하고 구성된 횟수만큼 요청/응답 교환을 재시도합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Verifies connection state and retries the request-response exchange as configured.</para>
    /// \endif
    /// </summary>
    /// <param name="request">
    /// \if KO
    /// <para>전송할 요청 프레임입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The request frame to send.</para>
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
    /// <para>첫 성공 응답 또는 마지막 실패 결과입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The first successful response or final failure result.</para>
    /// \endif
    /// </returns>
    /// <exception cref="ObjectDisposedException">
    /// \if KO
    /// <para>컨트롤러가 해제된 경우 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when the controller has been disposed.</para>
    /// \endif
    /// </exception>
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

    /// <summary>
    /// \if KO
    /// <para>연결 상태가 달라진 경우 갱신하고 상태 변경 이벤트를 발생시킵니다.</para>
    /// \endif
    /// \if EN
    /// <para>Updates the connection state and raises its event when the value changes.</para>
    /// \endif
    /// </summary>
    /// <param name="state">
    /// \if KO
    /// <para>새 연결 상태입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The new connection state.</para>
    /// \endif
    /// </param>
    private void SetState(IoConnectionState state)
    {
        if (State == state)
        {
            return;
        }

        State = state;
        StateChanged?.Invoke(this, state);
    }

    /// <summary>
    /// \if KO
    /// <para>컨트롤러가 이미 해제되었으면 예외를 발생시킵니다.</para>
    /// \endif
    /// \if EN
    /// <para>Throws when the controller has already been disposed.</para>
    /// \endif
    /// </summary>
    /// <exception cref="ObjectDisposedException">
    /// \if KO
    /// <para>컨트롤러가 해제된 경우 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when the controller is disposed.</para>
    /// \endif
    /// </exception>
    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }

    /// <summary>
    /// \if KO
    /// <para>옵션의 전송 형식에 맞는 UDP 또는 TCP 전송을 만듭니다.</para>
    /// \endif
    /// \if EN
    /// <para>Creates a UDP or TCP transport matching the configured transport type.</para>
    /// \endif
    /// </summary>
    /// <param name="options">
    /// \if KO
    /// <para>전송 구성 옵션입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The transport configuration options.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>새 전송 구현입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The new transport implementation.</para>
    /// \endif
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// \if KO
    /// <para><paramref name="options"/>가 <see langword="null"/>인 경우 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when <paramref name="options"/> is <see langword="null"/>.</para>
    /// \endif
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// \if KO
    /// <para>전송 형식이 지원되지 않는 경우 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when the transport type is unsupported.</para>
    /// \endif
    /// </exception>
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
