using Dreamine.IO.Abstractions.Channels;
using Dreamine.IO.Abstractions.Models;
using Dreamine.IO.Abstractions.Results;
using Dreamine.IO.Fastech.Ethernet.Controllers;

namespace Dreamine.IO.Fastech.Ethernet.Channels;

/// <summary>
/// Provides Fastech Ethernet analog input operations.
/// </summary>
public sealed class FastechAnalogInputChannel : IAnalogInputChannel
{
    private readonly FastechEthernetIoController _controller;

    internal FastechAnalogInputChannel(FastechEthernetIoController controller)
    {
        _controller = controller ?? throw new ArgumentNullException(nameof(controller));
    }

    /// <inheritdoc />
    public async Task<IoResult<double>> ReadAsync(AnalogIoPoint point, CancellationToken cancellationToken = default)
    {
        var result = await ReadAsync([point], cancellationToken).ConfigureAwait(false);

        if (!result.IsSuccess || result.Value is null)
        {
            return IoResult<double>.Failure(result.Message ?? "Failed to read the analog input.", result.ErrorCode);
        }

        return result.Value.Length == 1
            ? IoResult<double>.Success(result.Value[0])
            : IoResult<double>.Failure("The analog input response did not contain exactly one value.");
    }

    /// <inheritdoc />
    public Task<IoResult<double[]>> ReadAsync(IReadOnlyList<AnalogIoPoint> points, CancellationToken cancellationToken = default)
    {
        return _controller.ReadAnalogInputsAsync(points, cancellationToken);
    }
}
