using Dreamine.IO.Abstractions.Models;
using Dreamine.IO.Abstractions.Results;

namespace Dreamine.IO.Fastech.Ethernet.Protocol;

/// <summary>
/// Provides a placeholder protocol implementation until the official Fastech Ethernet I/O frame format is supplied.
/// </summary>
public sealed class UnsupportedFastechEthernetIoProtocol : IFastechEthernetIoProtocol
{
    private const string Message = "Fastech Ethernet I/O protocol frames are not implemented. Provide an IFastechEthernetIoProtocol implementation based on the official device manual.";

    /// <inheritdoc />
    public byte[] BuildReadDigitalInputs(IReadOnlyList<IoPoint> points)
    {
        throw new NotSupportedException(Message);
    }

    /// <inheritdoc />
    public IoResult<bool[]> ParseDigitalInputs(IReadOnlyList<byte> responseFrame, int count)
    {
        return IoResult<bool[]>.Failure(Message);
    }

    /// <inheritdoc />
    public byte[] BuildReadDigitalOutputs(IReadOnlyList<IoPoint> points)
    {
        throw new NotSupportedException(Message);
    }

    /// <inheritdoc />
    public IoResult<bool[]> ParseDigitalOutputs(IReadOnlyList<byte> responseFrame, int count)
    {
        return IoResult<bool[]>.Failure(Message);
    }

    /// <inheritdoc />
    public byte[] BuildWriteDigitalOutputs(IReadOnlyDictionary<IoPoint, bool> values)
    {
        throw new NotSupportedException(Message);
    }

    /// <inheritdoc />
    public byte[] BuildReadAnalogInputs(IReadOnlyList<AnalogIoPoint> points)
    {
        throw new NotSupportedException(Message);
    }

    /// <inheritdoc />
    public IoResult<double[]> ParseAnalogInputs(IReadOnlyList<byte> responseFrame, int count)
    {
        return IoResult<double[]>.Failure(Message);
    }

    /// <inheritdoc />
    public byte[] BuildReadAnalogOutputs(IReadOnlyList<AnalogIoPoint> points)
    {
        throw new NotSupportedException(Message);
    }

    /// <inheritdoc />
    public IoResult<double[]> ParseAnalogOutputs(IReadOnlyList<byte> responseFrame, int count)
    {
        return IoResult<double[]>.Failure(Message);
    }

    /// <inheritdoc />
    public byte[] BuildWriteAnalogOutputs(IReadOnlyDictionary<AnalogIoPoint, double> values)
    {
        throw new NotSupportedException(Message);
    }

    /// <inheritdoc />
    public IoResult ParseWriteResponse(IReadOnlyList<byte> responseFrame)
    {
        return IoResult.Failure(Message);
    }
}
