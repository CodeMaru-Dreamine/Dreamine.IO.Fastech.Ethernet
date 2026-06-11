using Dreamine.IO.Abstractions.Models;
using Dreamine.IO.Abstractions.Results;

namespace Dreamine.IO.Fastech.Ethernet.Protocol;

/// <summary>
/// Provides an explicit fail-fast protocol for unsupported or not-yet-verified Fastech Ethernet I/O models.
/// </summary>
/// <remarks>
/// The default controller uses <see cref="FastechPlusE16PointProtocol"/> for the real-hardware-verified
/// Ezi-IO Plus-E 16-point DIO device. Use this implementation only when an application wants a protocol
/// object that satisfies dependency injection while preventing accidental network frames for an unsupported model.
/// </remarks>
public sealed class UnsupportedFastechEthernetIoProtocol : IFastechEthernetIoProtocol
{
    private const string Message = "This Fastech Ethernet I/O model is not supported by the selected protocol. Provide an IFastechEthernetIoProtocol implementation verified against the target device manual and hardware.";

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
