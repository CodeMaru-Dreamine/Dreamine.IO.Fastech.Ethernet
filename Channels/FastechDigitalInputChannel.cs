using Dreamine.IO.Abstractions.Channels;
using Dreamine.IO.Abstractions.Models;
using Dreamine.IO.Abstractions.Results;
using Dreamine.IO.Fastech.Ethernet.Controllers;

namespace Dreamine.IO.Fastech.Ethernet.Channels;

/// <summary>
/// Provides Fastech Ethernet digital input operations.
/// </summary>
public sealed class FastechDigitalInputChannel : IDigitalInputChannel
{
    private readonly FastechEthernetIoController _controller;

    internal FastechDigitalInputChannel(FastechEthernetIoController controller)
    {
        _controller = controller ?? throw new ArgumentNullException(nameof(controller));
    }

    /// <inheritdoc />
    public async Task<IoResult<bool>> ReadAsync(IoPoint point, CancellationToken cancellationToken = default)
    {
        var result = await ReadAsync([point], cancellationToken).ConfigureAwait(false);

        if (!result.IsSuccess || result.Value is null)
        {
            return IoResult<bool>.Failure(result.Message ?? "Failed to read the digital input.", result.ErrorCode);
        }

        return result.Value.Length == 1
            ? IoResult<bool>.Success(result.Value[0])
            : IoResult<bool>.Failure("The digital input response did not contain exactly one value.");
    }

    /// <inheritdoc />
    public Task<IoResult<bool[]>> ReadAsync(IReadOnlyList<IoPoint> points, CancellationToken cancellationToken = default)
    {
        return _controller.ReadDigitalInputsAsync(points, cancellationToken);
    }
}
