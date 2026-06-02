using Dreamine.IO.Abstractions.Models;
using Dreamine.IO.Abstractions.Results;

namespace Dreamine.IO.Fastech.Ethernet.Protocol;

/// <summary>
/// Provides the Fastech Ezi-IO Plus-E 16-point digital I/O UDP protocol.
/// </summary>
public sealed class FastechPlusE16PointProtocol : IFastechEthernetIoProtocol
{
    private const byte Header = 0xAA;
    private const byte Reserved = 0x00;
    private const byte StatusOk = 0x00;
    private const byte GetInputFrameType = 0xC0;
    private const byte GetOutputFrameType = 0xC5;
    private const byte SetOutputFrameType = 0xC6;
    private const byte GetSlaveInfoFrameType = 0x01;
    private const uint Digital16PointResetMask = 0xFFFF_FFFF;
    private readonly object _syncLock = new();
    private byte _syncNo;

    /// <inheritdoc />
    public byte[] BuildGetSlaveInfo()
    {
        return BuildFrame(GetSlaveInfoFrameType, []);
    }

    /// <summary>
    /// Parses a slave information response.
    /// </summary>
    /// <param name="responseFrame">The response frame.</param>
    /// <returns>The slave information text.</returns>
    public IoResult<string> ParseSlaveInfo(IReadOnlyList<byte> responseFrame)
    {
        var payload = ParseReply(responseFrame, GetSlaveInfoFrameType, 1);
        if (!payload.IsSuccess || payload.Value is null)
        {
            return IoResult<string>.Failure(payload.Message ?? "Failed to parse Fastech slave information response.", payload.ErrorCode);
        }

        var slaveType = payload.Value[0];
        var name = payload.Value.Length > 1
            ? System.Text.Encoding.ASCII.GetString(payload.Value, 1, payload.Value.Length - 1).TrimEnd('\0')
            : string.Empty;

        return IoResult<string>.Success($"SlaveType={slaveType}, Name={name}");
    }

    /// <inheritdoc />
    public byte[] BuildReadDigitalInputs(IReadOnlyList<IoPoint> points)
    {
        ArgumentNullException.ThrowIfNull(points);

        return BuildFrame(GetInputFrameType, []);
    }

    /// <inheritdoc />
    public IoResult<bool[]> ParseDigitalInputs(IReadOnlyList<byte> responseFrame, int count)
    {
        var payload = ParseReply(responseFrame, GetInputFrameType, 8);
        if (!payload.IsSuccess || payload.Value is null)
        {
            return IoResult<bool[]>.Failure(payload.Message ?? "Failed to parse Fastech input response.", payload.ErrorCode);
        }

        return IoResult<bool[]>.Success(ToInputBits(payload.Value, count));
    }

    /// <inheritdoc />
    public byte[] BuildReadDigitalOutputs(IReadOnlyList<IoPoint> points)
    {
        ArgumentNullException.ThrowIfNull(points);

        return BuildFrame(GetOutputFrameType, []);
    }

    /// <inheritdoc />
    public IoResult<bool[]> ParseDigitalOutputs(IReadOnlyList<byte> responseFrame, int count)
    {
        var payload = ParseReply(responseFrame, GetOutputFrameType, 8);
        if (!payload.IsSuccess || payload.Value is null)
        {
            return IoResult<bool[]>.Failure(payload.Message ?? "Failed to parse Fastech output response.", payload.ErrorCode);
        }

        return IoResult<bool[]>.Success(ToOutputBits(payload.Value, count));
    }

    /// <inheritdoc />
    public byte[] BuildWriteDigitalOutputs(IReadOnlyDictionary<IoPoint, bool> values)
    {
        ArgumentNullException.ThrowIfNull(values);

        uint setMask = 0;
        uint clearMask = Digital16PointResetMask;

        foreach (var value in values)
        {
            if (value.Key.Channel is < 0 or > 15)
            {
                throw new ArgumentOutOfRangeException(nameof(values), value.Key.Channel, "Channel must be between 0 and 15.");
            }

            var bit = GetDigital16PointOutputWriteBit(value.Key.Channel);
            if (value.Value)
            {
                setMask |= bit;
                clearMask &= ~bit;
            }
        }

        var data = new byte[8];
        WriteUInt32BigEndian(setMask, data, 0);
        WriteUInt32BigEndian(clearMask, data, 4);

        return BuildFrame(SetOutputFrameType, data);
    }

    /// <inheritdoc />
    public byte[] BuildReadAnalogInputs(IReadOnlyList<AnalogIoPoint> points)
    {
        throw new NotSupportedException("Fastech Ezi-IO Plus-E 16-point DIO protocol does not support analog input.");
    }

    /// <inheritdoc />
    public IoResult<double[]> ParseAnalogInputs(IReadOnlyList<byte> responseFrame, int count)
    {
        return IoResult<double[]>.Failure("Fastech Ezi-IO Plus-E 16-point DIO protocol does not support analog input.");
    }

    /// <inheritdoc />
    public byte[] BuildReadAnalogOutputs(IReadOnlyList<AnalogIoPoint> points)
    {
        throw new NotSupportedException("Fastech Ezi-IO Plus-E 16-point DIO protocol does not support analog output.");
    }

    /// <inheritdoc />
    public IoResult<double[]> ParseAnalogOutputs(IReadOnlyList<byte> responseFrame, int count)
    {
        return IoResult<double[]>.Failure("Fastech Ezi-IO Plus-E 16-point DIO protocol does not support analog output.");
    }

    /// <inheritdoc />
    public byte[] BuildWriteAnalogOutputs(IReadOnlyDictionary<AnalogIoPoint, double> values)
    {
        throw new NotSupportedException("Fastech Ezi-IO Plus-E 16-point DIO protocol does not support analog output.");
    }

    /// <inheritdoc />
    public IoResult ParseWriteResponse(IReadOnlyList<byte> responseFrame)
    {
        var payload = ParseReply(responseFrame, SetOutputFrameType, 0);
        return payload.IsSuccess
            ? IoResult.Success()
            : IoResult.Failure(payload.Message ?? "Failed to parse Fastech write response.", payload.ErrorCode);
    }

    private byte[] BuildFrame(byte frameType, IReadOnlyList<byte> data)
    {
        if (data.Count > 252)
        {
            throw new ArgumentOutOfRangeException(nameof(data), data.Count, "Fastech frame data must be 252 bytes or less.");
        }

        var frame = new byte[5 + data.Count];
        frame[0] = Header;
        frame[1] = checked((byte)(3 + data.Count));
        frame[2] = NextSyncNo();
        frame[3] = Reserved;
        frame[4] = frameType;

        for (var i = 0; i < data.Count; i++)
        {
            frame[5 + i] = data[i];
        }

        return frame;
    }

    private IoResult<byte[]> ParseReply(IReadOnlyList<byte> responseFrame, byte expectedFrameType, int minimumPayloadLength)
    {
        ArgumentNullException.ThrowIfNull(responseFrame);

        if (responseFrame.Count < 6)
        {
            return IoResult<byte[]>.Failure("The Fastech response frame is too short.");
        }

        if (responseFrame[0] != Header)
        {
            return IoResult<byte[]>.Failure($"Invalid Fastech response header: 0x{responseFrame[0]:X2}.");
        }

        if (responseFrame[1] != responseFrame.Count - 2)
        {
            return IoResult<byte[]>.Failure($"Invalid Fastech response length: {responseFrame[1]}.");
        }

        if (responseFrame[3] != Reserved)
        {
            return IoResult<byte[]>.Failure($"Invalid Fastech response reserved byte: 0x{responseFrame[3]:X2}.");
        }

        if (responseFrame[4] != expectedFrameType)
        {
            return IoResult<byte[]>.Failure($"Unexpected Fastech response frame type: 0x{responseFrame[4]:X2}.");
        }

        var status = responseFrame[5];
        if (status != StatusOk)
        {
            return IoResult<byte[]>.Failure($"Fastech communication status error: 0x{status:X2}.", status);
        }

        var payloadLength = responseFrame.Count - 6;
        if (payloadLength < minimumPayloadLength)
        {
            return IoResult<byte[]>.Failure($"The Fastech response payload length is {payloadLength}, expected at least {minimumPayloadLength}.");
        }

        var payload = new byte[payloadLength];
        for (var i = 0; i < payload.Length; i++)
        {
            payload[i] = responseFrame[6 + i];
        }

        return IoResult<byte[]>.Success(payload);
    }

    private byte NextSyncNo()
    {
        lock (_syncLock)
        {
            _syncNo++;
            return _syncNo;
        }
    }

    private static bool[] ToInputBits(IReadOnlyList<byte> payload, int count)
    {
        count = Math.Clamp(count, 0, 16);
        var values = new bool[count];

        for (var i = 0; i < values.Length; i++)
        {
            var byteOffset = i < 8 ? 0 : 1;
            var bitIndex = i % 8;
            values[i] = (payload[byteOffset] & (1 << bitIndex)) != 0;
        }

        return values;
    }

    private static bool[] ToOutputBits(IReadOnlyList<byte> payload, int count)
    {
        count = Math.Clamp(count, 0, 16);
        var values = new bool[count];

        for (var i = 0; i < values.Length; i++)
        {
            var byteOffset = i < 8 ? 2 : 3;
            var bitIndex = i % 8;
            values[i] = (payload[byteOffset] & (1 << bitIndex)) != 0;
        }

        return values;
    }

    private static uint GetDigital16PointOutputWriteBit(int channel)
    {
        var normalizedChannel = Math.Clamp(channel, 0, 15);
        var bitIndex = normalizedChannel < 8
            ? 8 + normalizedChannel
            : normalizedChannel - 8;

        return 1u << bitIndex;
    }

    private static uint ReadUInt32BigEndian(IReadOnlyList<byte> bytes, int offset)
    {
        return ((uint)bytes[offset] << 24)
            | ((uint)bytes[offset + 1] << 16)
            | ((uint)bytes[offset + 2] << 8)
            | bytes[offset + 3];
    }

    private static void WriteUInt32BigEndian(uint value, byte[] bytes, int offset)
    {
        bytes[offset] = (byte)(value >> 24);
        bytes[offset + 1] = (byte)(value >> 16);
        bytes[offset + 2] = (byte)(value >> 8);
        bytes[offset + 3] = (byte)value;
    }
}
