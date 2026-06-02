using Dreamine.IO.Abstractions.Channels;
using Dreamine.IO.Abstractions.Models;
using Dreamine.IO.Abstractions.Results;
using Dreamine.IO.Fastech.Ethernet.Controllers;

namespace Dreamine.IO.Fastech.Ethernet.Channels;

/// <summary>
/// Provides Fastech Ethernet digital output operations.
/// </summary>
public sealed class FastechDigitalOutputChannel : IDigitalOutputChannel
{
    private readonly FastechEthernetIoController _controller;

    internal FastechDigitalOutputChannel(FastechEthernetIoController controller)
    {
        _controller = controller ?? throw new ArgumentNullException(nameof(controller));
    }

    /// <inheritdoc />
    public async Task<IoResult<bool>> ReadAsync(IoPoint point, CancellationToken cancellationToken = default)
    {
        var modulePoints = Enumerable
            .Range(0, 16)
            .Select(channel => new IoPoint(point.Module, channel, $"DO{channel:00}"))
            .ToArray();

        var result = await ReadAsync(modulePoints, cancellationToken).ConfigureAwait(false);

        if (!result.IsSuccess || result.Value is null)
        {
            return IoResult<bool>.Failure(result.Message ?? "Failed to read the digital output.", result.ErrorCode);
        }

        return point.Channel >= 0 && point.Channel < result.Value.Length
            ? IoResult<bool>.Success(result.Value[point.Channel])
            : IoResult<bool>.Failure("The digital output response did not contain the requested channel.");
    }

    /// <inheritdoc />
    public Task<IoResult<bool[]>> ReadAsync(IReadOnlyList<IoPoint> points, CancellationToken cancellationToken = default)
    {
        return _controller.ReadDigitalOutputsAsync(points, cancellationToken);
    }

    /// <inheritdoc />
    public Task<IoResult> WriteAsync(IoPoint point, bool value, CancellationToken cancellationToken = default)
    {
        return WriteAsync(new Dictionary<IoPoint, bool> { [point] = value }, cancellationToken);
    }

    /// <inheritdoc />
    public Task<IoResult> WriteAsync(IReadOnlyDictionary<IoPoint, bool> values, CancellationToken cancellationToken = default)
    {
        return _controller.WriteDigitalOutputsAsync(values, cancellationToken);
    }
}
