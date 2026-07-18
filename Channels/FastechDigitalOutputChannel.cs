using Dreamine.IO.Abstractions.Channels;
using Dreamine.IO.Abstractions.Models;
using Dreamine.IO.Abstractions.Results;
using Dreamine.IO.Fastech.Ethernet.Controllers;

namespace Dreamine.IO.Fastech.Ethernet.Channels;

/// <summary>
/// \if KO
/// <para>Fastech Ethernet 컨트롤러의 디지털 출력 채널 작업을 제공합니다.</para>
/// \endif
/// \if EN
/// <para>Provides digital-output channel operations for a Fastech Ethernet controller.</para>
/// \endif
/// </summary>
public sealed class FastechDigitalOutputChannel : IDigitalOutputChannel
{
    /// <summary>
    /// \if KO
    /// <para>controller 값을 보관합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Stores the controller value.</para>
    /// \endif
    /// </summary>
    private readonly FastechEthernetIoController _controller;

    /// <summary>
    /// \if KO
    /// <para>소유 컨트롤러로 디지털 출력 채널을 초기화합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Initializes the digital-output channel with its owning controller.</para>
    /// \endif
    /// </summary>
    /// <param name="controller">
    /// \if KO
    /// <para>요청을 실행할 컨트롤러입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The controller that executes requests.</para>
    /// \endif
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// \if KO
    /// <para><paramref name="controller"/>가 <see langword="null"/>인 경우 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when <paramref name="controller"/> is <see langword="null"/>.</para>
    /// \endif
    /// </exception>
    internal FastechDigitalOutputChannel(FastechEthernetIoController controller)
    {
        _controller = controller ?? throw new ArgumentNullException(nameof(controller));
    }

    /// <summary>
    /// \if KO
    /// <para>장치의 16점 출력 상태를 읽어 요청 채널의 상태를 반환합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Reads the device's 16-point output state and returns the requested channel state.</para>
    /// \endif
    /// </summary>
    /// <param name="point">
    /// \if KO
    /// <para>읽을 출력 지점입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The output point to read.</para>
    /// \endif
    /// </param>
    /// <param name="cancellationToken">
    /// \if KO
    /// <para>읽기 취소 토큰입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A token used to cancel the read.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>요청 출력 상태를 포함하는 결과입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A result containing the requested output state.</para>
    /// \endif
    /// </returns>
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

    /// <summary>
    /// \if KO
    /// <para>여러 디지털 출력의 현재 상태를 읽습니다.</para>
    /// \endif
    /// \if EN
    /// <para>Reads the current states of multiple digital outputs.</para>
    /// \endif
    /// </summary>
    /// <param name="points">
    /// \if KO
    /// <para>읽을 지점 목록입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The points to read.</para>
    /// \endif
    /// </param>
    /// <param name="cancellationToken">
    /// \if KO
    /// <para>읽기 취소 토큰입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A token used to cancel the read.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>출력 상태 배열을 포함하는 결과입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A result containing the output-state array.</para>
    /// \endif
    /// </returns>
    public Task<IoResult<bool[]>> ReadAsync(IReadOnlyList<IoPoint> points, CancellationToken cancellationToken = default)
    {
        return _controller.ReadDigitalOutputsAsync(points, cancellationToken);
    }

    /// <summary>
    /// \if KO
    /// <para>단일 디지털 출력 상태를 씁니다.</para>
    /// \endif
    /// \if EN
    /// <para>Writes one digital-output state.</para>
    /// \endif
    /// </summary>
    /// <param name="point">
    /// \if KO
    /// <para>쓸 지점입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The point to write.</para>
    /// \endif
    /// </param>
    /// <param name="value">
    /// \if KO
    /// <para>출력 상태입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The output state.</para>
    /// \endif
    /// </param>
    /// <param name="cancellationToken">
    /// \if KO
    /// <para>쓰기 취소 토큰입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A token used to cancel the write.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>쓰기 결과입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The write result.</para>
    /// \endif
    /// </returns>
    public Task<IoResult> WriteAsync(IoPoint point, bool value, CancellationToken cancellationToken = default)
    {
        return WriteAsync(new Dictionary<IoPoint, bool> { [point] = value }, cancellationToken);
    }

    /// <summary>
    /// \if KO
    /// <para>여러 디지털 출력 상태를 씁니다.</para>
    /// \endif
    /// \if EN
    /// <para>Writes multiple digital-output states.</para>
    /// \endif
    /// </summary>
    /// <param name="values">
    /// \if KO
    /// <para>지점별 출력 상태입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The output states keyed by point.</para>
    /// \endif
    /// </param>
    /// <param name="cancellationToken">
    /// \if KO
    /// <para>쓰기 취소 토큰입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A token used to cancel the write.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>일괄 쓰기 결과입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The batch-write result.</para>
    /// \endif
    /// </returns>
    public Task<IoResult> WriteAsync(IReadOnlyDictionary<IoPoint, bool> values, CancellationToken cancellationToken = default)
    {
        return _controller.WriteDigitalOutputsAsync(values, cancellationToken);
    }
}
