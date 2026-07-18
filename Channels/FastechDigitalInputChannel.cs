using Dreamine.IO.Abstractions.Channels;
using Dreamine.IO.Abstractions.Models;
using Dreamine.IO.Abstractions.Results;
using Dreamine.IO.Fastech.Ethernet.Controllers;

namespace Dreamine.IO.Fastech.Ethernet.Channels;

/// <summary>
/// \if KO
/// <para>Fastech Ethernet 컨트롤러의 디지털 입력 채널 작업을 제공합니다.</para>
/// \endif
/// \if EN
/// <para>Provides digital-input channel operations for a Fastech Ethernet controller.</para>
/// \endif
/// </summary>
public sealed class FastechDigitalInputChannel : IDigitalInputChannel
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
    /// <para>소유 컨트롤러로 디지털 입력 채널을 초기화합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Initializes the digital-input channel with its owning controller.</para>
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
    internal FastechDigitalInputChannel(FastechEthernetIoController controller)
    {
        _controller = controller ?? throw new ArgumentNullException(nameof(controller));
    }

    /// <summary>
    /// \if KO
    /// <para>단일 디지털 입력을 읽고 응답 값 수를 검증합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Reads one digital input and validates the response value count.</para>
    /// \endif
    /// </summary>
    /// <param name="point">
    /// \if KO
    /// <para>읽을 지점입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The point to read.</para>
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
    /// <para>단일 입력 상태를 포함하는 결과입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A result containing the single input state.</para>
    /// \endif
    /// </returns>
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

    /// <summary>
    /// \if KO
    /// <para>컨트롤러를 통해 여러 디지털 입력을 읽습니다.</para>
    /// \endif
    /// \if EN
    /// <para>Reads multiple digital inputs through the controller.</para>
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
    /// <para>입력 상태 배열을 포함하는 결과입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A result containing the input-state array.</para>
    /// \endif
    /// </returns>
    public Task<IoResult<bool[]>> ReadAsync(IReadOnlyList<IoPoint> points, CancellationToken cancellationToken = default)
    {
        return _controller.ReadDigitalInputsAsync(points, cancellationToken);
    }
}
