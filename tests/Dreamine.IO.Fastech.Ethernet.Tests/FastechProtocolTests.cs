using Dreamine.IO.Abstractions.Models;
using Dreamine.IO.Fastech.Ethernet.Protocol;

namespace Dreamine.IO.Fastech.Ethernet.Tests;

public sealed class FastechProtocolTests
{
    private readonly FastechPlusE16PointProtocol _protocol = new();

    [Fact]
    public void BuildReadFrames_UseExpectedHeaderLengthTypeAndIncrementingSync()
    {
        var input = _protocol.BuildReadDigitalInputs([new IoPoint(0, 0)]);
        var output = _protocol.BuildReadDigitalOutputs([new IoPoint(0, 0)]);
        var slave = _protocol.BuildGetSlaveInfo();

        Assert.Equal([0xAA, 0x03, 0x01, 0x00, 0xC0], input);
        Assert.Equal([0xAA, 0x03, 0x02, 0x00, 0xC5], output);
        Assert.Equal([0xAA, 0x03, 0x03, 0x00, 0x01], slave);
    }

    [Fact]
    public void ParseDigitalInputs_MapsBothInputBytesAndClampsCount()
    {
        var response = Reply(0xC0, 0x00, 0b0000_0101, 0b1000_0000, 0, 0, 0, 0, 0, 0);

        var result = _protocol.ParseDigitalInputs(response, 99);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(16, result.Value.Length);
        Assert.True(result.Value[0]);
        Assert.False(result.Value[1]);
        Assert.True(result.Value[2]);
        Assert.True(result.Value[15]);
    }

    [Fact]
    public void ParseDigitalOutputs_UsesOutputBytes()
    {
        var response = Reply(0xC5, 0x00, 0, 0, 0b0000_0010, 0b0000_0100, 0, 0, 0, 0);

        var result = _protocol.ParseDigitalOutputs(response, 16);

        Assert.True(result.IsSuccess);
        Assert.False(result.Value![0]);
        Assert.True(result.Value[1]);
        Assert.True(result.Value[10]);
    }

    [Fact]
    public void BuildWriteDigitalOutputs_MapsLowAndHighChannels()
    {
        var frame = _protocol.BuildWriteDigitalOutputs(
            new Dictionary<IoPoint, bool>
            {
                [new IoPoint(0, 0)] = true,
                [new IoPoint(0, 8)] = true,
                [new IoPoint(0, 15)] = false
            });

        Assert.Equal(13, frame.Length);
        Assert.Equal(0xC6, frame[4]);
        Assert.Equal([0x00, 0x00, 0x01, 0x01], frame[5..9]);
        Assert.Equal([0xFF, 0xFF, 0xFE, 0xFE], frame[9..13]);
    }

    [Fact]
    public void BuildWriteDigitalOutputs_RejectsOutOfRangeChannel()
    {
        var values = new Dictionary<IoPoint, bool> { [new IoPoint(0, 16)] = true };

        Assert.Throws<ArgumentOutOfRangeException>(() => _protocol.BuildWriteDigitalOutputs(values));
    }

    [Theory]
    [InlineData(new byte[] { 0xAA })]
    [InlineData(new byte[] { 0xAB, 0x04, 0, 0, 0xC0, 0 })]
    [InlineData(new byte[] { 0xAA, 0x05, 0, 0, 0xC0, 0 })]
    [InlineData(new byte[] { 0xAA, 0x04, 0, 1, 0xC0, 0 })]
    [InlineData(new byte[] { 0xAA, 0x04, 0, 0, 0xC5, 0 })]
    [InlineData(new byte[] { 0xAA, 0x04, 0, 0, 0xC0, 7 })]
    public void ParseDigitalInputs_RejectsInvalidResponse(byte[] response)
    {
        var result = _protocol.ParseDigitalInputs(response, 1);

        Assert.False(result.IsSuccess);
        Assert.False(string.IsNullOrWhiteSpace(result.Message));
    }

    [Fact]
    public void ParseDigitalInputs_RejectsShortPayload()
    {
        var result = _protocol.ParseDigitalInputs(Reply(0xC0, 0, 1), 1);

        Assert.False(result.IsSuccess);
        Assert.Contains("payload", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ParseWriteResponse_AcceptsSuccessAndPreservesDeviceErrorCode()
    {
        var success = _protocol.ParseWriteResponse(Reply(0xC6, 0));
        var failure = _protocol.ParseWriteResponse(Reply(0xC6, 9));

        Assert.True(success.IsSuccess);
        Assert.False(failure.IsSuccess);
        Assert.Equal(9, failure.ErrorCode);
    }

    [Fact]
    public void ParseSlaveInfo_ReturnsTypeAndTrimmedName()
    {
        var result = _protocol.ParseSlaveInfo(Reply(0x01, 0, 4, (byte)'E', (byte)'Z', (byte)'I', 0));

        Assert.True(result.IsSuccess);
        Assert.Equal("SlaveType=4, Name=EZI", result.Value);
    }

    [Fact]
    public void AnalogMembers_ReportUnsupported()
    {
        var point = new AnalogIoPoint(0, 0);

        Assert.Throws<NotSupportedException>(() => _protocol.BuildReadAnalogInputs([point]));
        Assert.Throws<NotSupportedException>(() => _protocol.BuildReadAnalogOutputs([point]));
        Assert.Throws<NotSupportedException>(() =>
            _protocol.BuildWriteAnalogOutputs(new Dictionary<AnalogIoPoint, double> { [point] = 1 }));
        Assert.False(_protocol.ParseAnalogInputs([], 1).IsSuccess);
        Assert.False(_protocol.ParseAnalogOutputs([], 1).IsSuccess);
    }

    [Fact]
    public void UnsupportedProtocol_FailsEveryOperation()
    {
        var unsupported = new UnsupportedFastechEthernetIoProtocol();
        var digital = new IoPoint(0, 0);
        var analog = new AnalogIoPoint(0, 0);

        Assert.Throws<NotSupportedException>(() => unsupported.BuildReadDigitalInputs([digital]));
        Assert.Throws<NotSupportedException>(() => unsupported.BuildReadDigitalOutputs([digital]));
        Assert.Throws<NotSupportedException>(() =>
            unsupported.BuildWriteDigitalOutputs(new Dictionary<IoPoint, bool> { [digital] = true }));
        Assert.Throws<NotSupportedException>(() => unsupported.BuildReadAnalogInputs([analog]));
        Assert.Throws<NotSupportedException>(() => unsupported.BuildReadAnalogOutputs([analog]));
        Assert.Throws<NotSupportedException>(() =>
            unsupported.BuildWriteAnalogOutputs(new Dictionary<AnalogIoPoint, double> { [analog] = 1 }));
        Assert.False(unsupported.ParseDigitalInputs([], 1).IsSuccess);
        Assert.False(unsupported.ParseDigitalOutputs([], 1).IsSuccess);
        Assert.False(unsupported.ParseAnalogInputs([], 1).IsSuccess);
        Assert.False(unsupported.ParseAnalogOutputs([], 1).IsSuccess);
        Assert.False(unsupported.ParseWriteResponse([]).IsSuccess);
    }

    private static byte[] Reply(byte type, byte status, params byte[] payload)
    {
        var response = new byte[6 + payload.Length];
        response[0] = 0xAA;
        response[1] = checked((byte)(response.Length - 2));
        response[2] = 1;
        response[3] = 0;
        response[4] = type;
        response[5] = status;
        payload.CopyTo(response, 6);
        return response;
    }
}
