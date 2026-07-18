using Dreamine.IO.Abstractions.Channels;
using Dreamine.IO.Abstractions.Models;
using Dreamine.IO.Abstractions.Results;
using Dreamine.IO.Fastech.Ethernet.Controllers;

namespace Dreamine.IO.Fastech.Ethernet.Channels;

/// <summary>
/// \if KO
/// <para>Fastech Ethernet 컨트롤러의 아날로그 출력 채널 작업을 제공합니다.</para>
/// \endif
/// \if EN
/// <para>Provides analog-output channel operations for a Fastech Ethernet controller.</para>
/// \endif
/// </summary>
public sealed class FastechAnalogOutputChannel : IAnalogOutputChannel
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
    /// <para>소유 컨트롤러로 아날로그 출력 채널을 초기화합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Initializes the analog-output channel with its owning controller.</para>
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
    internal FastechAnalogOutputChannel(FastechEthernetIoController controller)
    {
        _controller = controller ?? throw new ArgumentNullException(nameof(controller));
    }

    /// <summary>
    /// \if KO
    /// <para>단일 아날로그 출력의 현재 값을 읽고 응답 값 수를 검증합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Reads one analog output's current value and validates the response count.</para>
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
    /// <para>현재 출력 값을 포함하는 결과입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A result containing the current output value.</para>
    /// \endif
    /// </returns>
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

    /// <summary>
    /// \if KO
    /// <para>단일 아날로그 출력 값을 씁니다.</para>
    /// \endif
    /// \if EN
    /// <para>Writes one analog-output value.</para>
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
    /// <para>출력 값입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The output value.</para>
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
    public Task<IoResult> WriteAsync(AnalogIoPoint point, double value, CancellationToken cancellationToken = default)
    {
        return WriteAsync(new Dictionary<AnalogIoPoint, double> { [point] = value }, cancellationToken);
    }

    /// <summary>
    /// \if KO
    /// <para>컨트롤러를 통해 여러 아날로그 출력 값을 씁니다.</para>
    /// \endif
    /// \if EN
    /// <para>Writes multiple analog-output values through the controller.</para>
    /// \endif
    /// </summary>
    /// <param name="values">
    /// \if KO
    /// <para>지점별 출력 값입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The output values keyed by point.</para>
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
    public Task<IoResult> WriteAsync(IReadOnlyDictionary<AnalogIoPoint, double> values, CancellationToken cancellationToken = default)
    {
        return _controller.WriteAnalogOutputsAsync(values, cancellationToken);
    }
}
