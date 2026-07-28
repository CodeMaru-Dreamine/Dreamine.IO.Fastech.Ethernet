using Dreamine.IO.Abstractions.Enums;
using Dreamine.IO.Abstractions.Models;
using Dreamine.IO.Abstractions.Results;
using Dreamine.IO.Fastech.Ethernet.Controllers;
using Dreamine.IO.Fastech.Ethernet.Options;
using Dreamine.IO.Fastech.Ethernet.Protocol;
using Dreamine.IO.Fastech.Ethernet.Transport;

namespace Dreamine.IO.Fastech.Ethernet.Tests;

public sealed class FastechControllerTests
{
    [Fact]
    public async Task ConnectReadWriteDisconnect_UsesTransportAndRaisesStates()
    {
        var transport = new StubTransport();
        transport.Responses.Enqueue(IoResult<byte[]>.Success(Reply(0xC0, 0, 1, 0, 0, 0, 0, 0, 0, 0)));
        transport.Responses.Enqueue(IoResult<byte[]>.Success(Reply(0xC6, 0)));
        await using var controller = CreateController(transport);
        var states = new List<IoConnectionState>();
        controller.StateChanged += (_, state) => states.Add(state);

        Assert.True((await controller.ConnectAsync()).IsSuccess);
        var read = await controller.DigitalInputs.ReadAsync(new IoPoint(0, 0));
        var write = await controller.DigitalOutputs.WriteAsync(new IoPoint(0, 0), true);
        Assert.True((await controller.DisconnectAsync()).IsSuccess);

        Assert.True(read.IsSuccess);
        Assert.True(read.Value);
        Assert.True(write.IsSuccess);
        Assert.Equal(
            [IoConnectionState.Connecting, IoConnectionState.Connected, IoConnectionState.Disconnecting, IoConnectionState.Disconnected],
            states);
        Assert.Equal(2, transport.Requests.Count);
    }

    [Fact]
    public async Task ReadBeforeConnectAndEmptyCollections_FailWithoutSending()
    {
        var transport = new StubTransport();
        await using var controller = CreateController(transport);

        var disconnected = await controller.DigitalInputs.ReadAsync(new IoPoint(0, 0));
        await controller.ConnectAsync();
        var emptyRead = await controller.DigitalInputs.ReadAsync([]);
        var emptyWrite = await controller.DigitalOutputs.WriteAsync(new Dictionary<IoPoint, bool>());

        Assert.False(disconnected.IsSuccess);
        Assert.False(emptyRead.IsSuccess);
        Assert.False(emptyWrite.IsSuccess);
        Assert.Empty(transport.Requests);
    }

    [Fact]
    public async Task FailedTransportRetriesConfiguredNumberOfTimes()
    {
        var transport = new StubTransport();
        transport.Responses.Enqueue(IoResult<byte[]>.Failure("first"));
        transport.Responses.Enqueue(IoResult<byte[]>.Failure("second"));
        transport.Responses.Enqueue(IoResult<byte[]>.Success(Reply(0xC0, 0, 1, 0, 0, 0, 0, 0, 0, 0)));
        await using var controller = CreateController(transport, retryCount: 2);
        await controller.ConnectAsync();

        var result = await controller.DigitalInputs.ReadAsync(new IoPoint(0, 0));

        Assert.True(result.IsSuccess);
        Assert.Equal(3, transport.Requests.Count);
    }

    [Fact]
    public async Task ProtocolBuilderAndParserExceptionsBecomeFailures()
    {
        var transport = new StubTransport();
        transport.Responses.Enqueue(IoResult<byte[]>.Success([1]));
        await using var controller = new FastechEthernetIoController(
            new FastechEthernetIoOptions(),
            transport,
            new ThrowingProtocol());
        await controller.ConnectAsync();

        var buildFailure = await controller.DigitalInputs.ReadAsync(new IoPoint(0, 0));
        var parseFailure = await controller.DigitalOutputs.ReadAsync([new IoPoint(0, 0)]);

        Assert.False(buildFailure.IsSuccess);
        Assert.Contains("build", buildFailure.Message, StringComparison.OrdinalIgnoreCase);
        Assert.False(parseFailure.IsSuccess);
        Assert.Contains("parse", parseFailure.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task AnalogChannelsExposeProtocolUnsupportedResult()
    {
        var transport = new StubTransport();
        await using var controller = CreateController(transport);
        await controller.ConnectAsync();

        var input = await controller.AnalogInputs.ReadAsync(new AnalogIoPoint(0, 0));
        var output = await controller.AnalogOutputs.ReadAsync(new AnalogIoPoint(0, 0));
        var write = await controller.AnalogOutputs.WriteAsync(new AnalogIoPoint(0, 0), 3.14);

        Assert.False(input.IsSuccess);
        Assert.False(output.IsSuccess);
        Assert.False(write.IsSuccess);
    }

    [Fact]
    public async Task DisposeIsIdempotentAndFurtherConnectThrows()
    {
        var controller = CreateController(new StubTransport());

        await controller.DisposeAsync();
        await controller.DisposeAsync();

        await Assert.ThrowsAsync<ObjectDisposedException>(() => controller.ConnectAsync());
    }

    [Fact]
    public void OptionsConvertToNeutralValuesAndAllowOverrides()
    {
        var options = new FastechEthernetIoOptions
        {
            Host = "10.0.0.7",
            Port = 3002,
            LocalPort = 4000,
            DeviceIndex = 2,
            ConnectTimeoutMs = 100,
            ReceiveTimeoutMs = 200,
            RetryCount = 3
        };
        options.Properties["Port"] = "9999";
        options.Properties["Custom"] = "value";

        var neutral = options.ToIoConnectionOptions();

        Assert.Equal(IoProvider.Fastech, neutral.Provider);
        Assert.Equal(2, neutral.DeviceIndex);
        Assert.Equal("10.0.0.7", neutral.Name);
        Assert.Equal("9999", neutral.Properties["Port"]);
        Assert.Equal("value", neutral.Properties["Custom"]);
    }

    private static FastechEthernetIoController CreateController(StubTransport transport, int retryCount = 0)
    {
        return new FastechEthernetIoController(
            new FastechEthernetIoOptions { RetryCount = retryCount },
            transport,
            new FastechPlusE16PointProtocol());
    }

    private static byte[] Reply(byte type, byte status, params byte[] payload)
    {
        var response = new byte[6 + payload.Length];
        response[0] = 0xAA;
        response[1] = checked((byte)(response.Length - 2));
        response[3] = 0;
        response[4] = type;
        response[5] = status;
        payload.CopyTo(response, 6);
        return response;
    }

    private sealed class StubTransport : IFastechEthernetIoTransport
    {
        public bool IsConnected { get; private set; }
        public Queue<IoResult<byte[]>> Responses { get; } = new();
        public List<byte[]> Requests { get; } = [];

        public Task<IoResult> ConnectAsync(CancellationToken cancellationToken = default)
        {
            IsConnected = true;
            return Task.FromResult(IoResult.Success());
        }

        public Task<IoResult> DisconnectAsync(CancellationToken cancellationToken = default)
        {
            IsConnected = false;
            return Task.FromResult(IoResult.Success());
        }

        public Task<IoResult<byte[]>> SendAndReceiveAsync(
            IReadOnlyList<byte> requestFrame,
            int receiveTimeoutMs,
            int expectedResponseLength,
            CancellationToken cancellationToken = default)
        {
            Requests.Add(requestFrame.ToArray());
            return Task.FromResult(Responses.Count > 0
                ? Responses.Dequeue()
                : IoResult<byte[]>.Failure("No response configured."));
        }

        public ValueTask DisposeAsync()
        {
            IsConnected = false;
            return ValueTask.CompletedTask;
        }
    }

    private sealed class ThrowingProtocol : IFastechEthernetIoProtocol
    {
        public byte[] BuildReadDigitalInputs(IReadOnlyList<IoPoint> points) => throw new InvalidOperationException("build failed");
        public IoResult<bool[]> ParseDigitalInputs(IReadOnlyList<byte> responseFrame, int count) => throw new InvalidOperationException("parse failed");
        public byte[] BuildReadDigitalOutputs(IReadOnlyList<IoPoint> points) => [1];
        public IoResult<bool[]> ParseDigitalOutputs(IReadOnlyList<byte> responseFrame, int count) => throw new InvalidOperationException("parse failed");
        public byte[] BuildWriteDigitalOutputs(IReadOnlyDictionary<IoPoint, bool> values) => [1];
        public byte[] BuildReadAnalogInputs(IReadOnlyList<AnalogIoPoint> points) => [1];
        public IoResult<double[]> ParseAnalogInputs(IReadOnlyList<byte> responseFrame, int count) => IoResult<double[]>.Success([1]);
        public byte[] BuildReadAnalogOutputs(IReadOnlyList<AnalogIoPoint> points) => [1];
        public IoResult<double[]> ParseAnalogOutputs(IReadOnlyList<byte> responseFrame, int count) => IoResult<double[]>.Success([1]);
        public byte[] BuildWriteAnalogOutputs(IReadOnlyDictionary<AnalogIoPoint, double> values) => [1];
        public IoResult ParseWriteResponse(IReadOnlyList<byte> responseFrame) => IoResult.Success();
    }
}
