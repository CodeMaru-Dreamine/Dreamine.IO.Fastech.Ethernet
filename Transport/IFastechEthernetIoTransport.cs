using Dreamine.IO.Abstractions.Results;

namespace Dreamine.IO.Fastech.Ethernet.Transport;

/// <summary>
/// Defines transport operations for Fastech Ethernet I/O communication.
/// </summary>
public interface IFastechEthernetIoTransport : IAsyncDisposable
{
    /// <summary>
    /// Gets whether the transport is connected.
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// Connects the transport.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The I/O operation result.</returns>
    Task<IoResult> ConnectAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Disconnects the transport.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The I/O operation result.</returns>
    Task<IoResult> DisconnectAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a request frame and receives a response frame.
    /// </summary>
    /// <param name="requestFrame">The request frame.</param>
    /// <param name="receiveTimeoutMs">The receive timeout in milliseconds.</param>
    /// <param name="expectedResponseLength">The expected response length. Zero reads the first available packet.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The response frame.</returns>
    Task<IoResult<byte[]>> SendAndReceiveAsync(
        IReadOnlyList<byte> requestFrame,
        int receiveTimeoutMs,
        int expectedResponseLength,
        CancellationToken cancellationToken = default);
}
