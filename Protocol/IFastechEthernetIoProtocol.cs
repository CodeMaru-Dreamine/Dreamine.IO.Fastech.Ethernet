using Dreamine.IO.Abstractions.Models;
using Dreamine.IO.Abstractions.Results;

namespace Dreamine.IO.Fastech.Ethernet.Protocol;

/// <summary>
/// Defines protocol frame building and parsing for Fastech Ethernet I/O.
/// </summary>
public interface IFastechEthernetIoProtocol
{
    /// <summary>
    /// Builds a digital input read request.
    /// </summary>
    /// <param name="points">The digital input points.</param>
    /// <returns>The request frame.</returns>
    byte[] BuildReadDigitalInputs(IReadOnlyList<IoPoint> points);

    /// <summary>
    /// Parses a digital input read response.
    /// </summary>
    /// <param name="responseFrame">The response frame.</param>
    /// <param name="count">The expected point count.</param>
    /// <returns>The digital input values.</returns>
    IoResult<bool[]> ParseDigitalInputs(IReadOnlyList<byte> responseFrame, int count);

    /// <summary>
    /// Builds a digital output read request.
    /// </summary>
    /// <param name="points">The digital output points.</param>
    /// <returns>The request frame.</returns>
    byte[] BuildReadDigitalOutputs(IReadOnlyList<IoPoint> points);

    /// <summary>
    /// Parses a digital output read response.
    /// </summary>
    /// <param name="responseFrame">The response frame.</param>
    /// <param name="count">The expected point count.</param>
    /// <returns>The digital output values.</returns>
    IoResult<bool[]> ParseDigitalOutputs(IReadOnlyList<byte> responseFrame, int count);

    /// <summary>
    /// Builds a digital output write request.
    /// </summary>
    /// <param name="values">The digital output values keyed by point.</param>
    /// <returns>The request frame.</returns>
    byte[] BuildWriteDigitalOutputs(IReadOnlyDictionary<IoPoint, bool> values);

    /// <summary>
    /// Builds an analog input read request.
    /// </summary>
    /// <param name="points">The analog input points.</param>
    /// <returns>The request frame.</returns>
    byte[] BuildReadAnalogInputs(IReadOnlyList<AnalogIoPoint> points);

    /// <summary>
    /// Parses an analog input read response.
    /// </summary>
    /// <param name="responseFrame">The response frame.</param>
    /// <param name="count">The expected point count.</param>
    /// <returns>The analog input values.</returns>
    IoResult<double[]> ParseAnalogInputs(IReadOnlyList<byte> responseFrame, int count);

    /// <summary>
    /// Builds an analog output read request.
    /// </summary>
    /// <param name="points">The analog output points.</param>
    /// <returns>The request frame.</returns>
    byte[] BuildReadAnalogOutputs(IReadOnlyList<AnalogIoPoint> points);

    /// <summary>
    /// Parses an analog output read response.
    /// </summary>
    /// <param name="responseFrame">The response frame.</param>
    /// <param name="count">The expected point count.</param>
    /// <returns>The analog output values.</returns>
    IoResult<double[]> ParseAnalogOutputs(IReadOnlyList<byte> responseFrame, int count);

    /// <summary>
    /// Builds an analog output write request.
    /// </summary>
    /// <param name="values">The analog output values keyed by point.</param>
    /// <returns>The request frame.</returns>
    byte[] BuildWriteAnalogOutputs(IReadOnlyDictionary<AnalogIoPoint, double> values);

    /// <summary>
    /// Parses a write response.
    /// </summary>
    /// <param name="responseFrame">The response frame.</param>
    /// <returns>The I/O operation result.</returns>
    IoResult ParseWriteResponse(IReadOnlyList<byte> responseFrame);
}
