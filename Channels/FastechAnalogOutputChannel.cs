using Dreamine.IO.Abstractions.Channels;
using Dreamine.IO.Abstractions.Models;
using Dreamine.IO.Abstractions.Results;
using Dreamine.IO.Fastech.Ethernet.Controllers;

namespace Dreamine.IO.Fastech.Ethernet.Channels;

/// <summary>
/// Provides Fastech Ethernet analog output operations.
/// </summary>
public sealed class FastechAnalogOutputChannel : IAnalogOutputChannel
{
    private readonly FastechEthernetIoController _controller;

    internal FastechAnalogOutputChannel(FastechEthernetIoController controller)
    {
        _controller = controller ?? throw new ArgumentNullException(nameof(controller));
    }

    /// <inheritdoc />
    public async Task<IoResult<double>> ReadAsync(AnalogIoPoint point, CancellationToken cancellationToken = default)
    {
        var result = await _controller.ReadAnalogOutputsAsync([point], cancellationToken).ConfigureAwait(false);

        if (!result.IsSuccess || result.Value is null)
        {
            return IoResult<double>.Failure(result.Message ?? "Failed to read the analog output.", result.ErrorCode);
        }

        return result.Value.Length == 1
            ? IoResult<double>.Success(result.Value[0])
            : IoResult<double>.Failure("The analog output response did not contain exactly one value.");
    }

    /// <inheritdoc />
    public Task<IoResult> WriteAsync(AnalogIoPoint point, double value, CancellationToken cancellationToken = default)
    {
        return WriteAsync(new Dictionary<AnalogIoPoint, double> { [point] = value }, cancellationToken);
    }

    /// <inheritdoc />
    public Task<IoResult> WriteAsync(IReadOnlyDictionary<AnalogIoPoint, double> values, CancellationToken cancellationToken = default)
    {
        return _controller.WriteAnalogOutputsAsync(values, cancellationToken);
    }
}
