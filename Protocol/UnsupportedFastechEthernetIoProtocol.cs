using Dreamine.IO.Abstractions.Models;
using Dreamine.IO.Abstractions.Results;

namespace Dreamine.IO.Fastech.Ethernet.Protocol;

/// <summary>
/// \if KO
/// <para>지원되지 않거나 검증되지 않은 Fastech Ethernet I/O 모델에 대해 명시적으로 실패하는 프로토콜입니다.</para>
/// \endif
/// \if EN
/// <para>Provides an explicit fail-fast protocol for unsupported or not-yet-verified Fastech Ethernet I/O models.</para>
/// \endif
/// </summary>
/// <remarks>
/// \if KO
/// <para>지원되지 않거나 검증되지 않은 Fastech Ethernet I/O 모델에 대해 명시적으로 실패하는 프로토콜입니다.</para>
/// \endif
/// \if EN
/// <para>Provides an explicit fail-fast protocol for unsupported or not-yet-verified Fastech Ethernet I/O models.</para>
/// \endif
/// </remarks>
public sealed class UnsupportedFastechEthernetIoProtocol : IFastechEthernetIoProtocol
{
    /// <summary>
    /// \if KO
    /// <para>Message 값을 보관합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Stores the message value.</para>
    /// \endif
    /// </summary>
    private const string Message = "This Fastech Ethernet I/O model is not supported by the selected protocol. Provide an IFastechEthernetIoProtocol implementation verified against the target device manual and hardware.";

    /// <summary>
    /// \if KO
    /// <para>미지원 디지털 입력 읽기 요청 생성을 거부합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Rejects building an unsupported digital-input read request.</para>
    /// \endif
    /// </summary>
    /// <param name="points">
    /// \if KO
    /// <para>요청된 입력 지점입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The requested input points.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>이 메서드는 반환하지 않습니다.</para>
    /// \endif
    /// \if EN
    /// <para>This method does not return.</para>
    /// \endif
    /// </returns>
    /// <exception cref="NotSupportedException">
    /// \if KO
    /// <para>항상 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Always thrown.</para>
    /// \endif
    /// </exception>
    public byte[] BuildReadDigitalInputs(IReadOnlyList<IoPoint> points)
    {
        throw new NotSupportedException(Message);
    }

    /// <summary>
    /// \if KO
    /// <para>미지원 디지털 입력 응답에 실패 결과를 반환합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Returns a failure result for an unsupported digital-input response.</para>
    /// \endif
    /// </summary>
    /// <param name="responseFrame">
    /// \if KO
    /// <para>무시되는 응답 프레임입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The ignored response frame.</para>
    /// \endif
    /// </param>
    /// <param name="count">
    /// \if KO
    /// <para>무시되는 예상 수입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The ignored expected count.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>미지원 실패 결과입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The unsupported-operation failure result.</para>
    /// \endif
    /// </returns>
    public IoResult<bool[]> ParseDigitalInputs(IReadOnlyList<byte> responseFrame, int count)
    {
        return IoResult<bool[]>.Failure(Message);
    }

    /// <summary>
    /// \if KO
    /// <para>미지원 디지털 출력 읽기 요청 생성을 거부합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Rejects building an unsupported digital-output read request.</para>
    /// \endif
    /// </summary>
    /// <param name="points">
    /// \if KO
    /// <para>요청된 출력 지점입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The requested output points.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>이 메서드는 반환하지 않습니다.</para>
    /// \endif
    /// \if EN
    /// <para>This method does not return.</para>
    /// \endif
    /// </returns>
    /// <exception cref="NotSupportedException">
    /// \if KO
    /// <para>항상 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Always thrown.</para>
    /// \endif
    /// </exception>
    public byte[] BuildReadDigitalOutputs(IReadOnlyList<IoPoint> points)
    {
        throw new NotSupportedException(Message);
    }

    /// <summary>
    /// \if KO
    /// <para>미지원 디지털 출력 응답에 실패 결과를 반환합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Returns a failure result for an unsupported digital-output response.</para>
    /// \endif
    /// </summary>
    /// <param name="responseFrame">
    /// \if KO
    /// <para>무시되는 응답 프레임입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The ignored response frame.</para>
    /// \endif
    /// </param>
    /// <param name="count">
    /// \if KO
    /// <para>무시되는 예상 수입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The ignored expected count.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>미지원 실패 결과입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The unsupported-operation failure result.</para>
    /// \endif
    /// </returns>
    public IoResult<bool[]> ParseDigitalOutputs(IReadOnlyList<byte> responseFrame, int count)
    {
        return IoResult<bool[]>.Failure(Message);
    }

    /// <summary>
    /// \if KO
    /// <para>미지원 디지털 출력 쓰기 요청 생성을 거부합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Rejects building an unsupported digital-output write request.</para>
    /// \endif
    /// </summary>
    /// <param name="values">
    /// \if KO
    /// <para>요청된 출력 상태입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The requested output states.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>이 메서드는 반환하지 않습니다.</para>
    /// \endif
    /// \if EN
    /// <para>This method does not return.</para>
    /// \endif
    /// </returns>
    /// <exception cref="NotSupportedException">
    /// \if KO
    /// <para>항상 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Always thrown.</para>
    /// \endif
    /// </exception>
    public byte[] BuildWriteDigitalOutputs(IReadOnlyDictionary<IoPoint, bool> values)
    {
        throw new NotSupportedException(Message);
    }

    /// <summary>
    /// \if KO
    /// <para>미지원 아날로그 입력 읽기 요청 생성을 거부합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Rejects building an unsupported analog-input read request.</para>
    /// \endif
    /// </summary>
    /// <param name="points">
    /// \if KO
    /// <para>요청된 입력 지점입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The requested input points.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>이 메서드는 반환하지 않습니다.</para>
    /// \endif
    /// \if EN
    /// <para>This method does not return.</para>
    /// \endif
    /// </returns>
    /// <exception cref="NotSupportedException">
    /// \if KO
    /// <para>항상 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Always thrown.</para>
    /// \endif
    /// </exception>
    public byte[] BuildReadAnalogInputs(IReadOnlyList<AnalogIoPoint> points)
    {
        throw new NotSupportedException(Message);
    }

    /// <summary>
    /// \if KO
    /// <para>미지원 아날로그 입력 응답에 실패 결과를 반환합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Returns a failure result for an unsupported analog-input response.</para>
    /// \endif
    /// </summary>
    /// <param name="responseFrame">
    /// \if KO
    /// <para>무시되는 응답 프레임입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The ignored response frame.</para>
    /// \endif
    /// </param>
    /// <param name="count">
    /// \if KO
    /// <para>무시되는 예상 수입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The ignored expected count.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>미지원 실패 결과입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The unsupported-operation failure result.</para>
    /// \endif
    /// </returns>
    public IoResult<double[]> ParseAnalogInputs(IReadOnlyList<byte> responseFrame, int count)
    {
        return IoResult<double[]>.Failure(Message);
    }

    /// <summary>
    /// \if KO
    /// <para>미지원 아날로그 출력 읽기 요청 생성을 거부합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Rejects building an unsupported analog-output read request.</para>
    /// \endif
    /// </summary>
    /// <param name="points">
    /// \if KO
    /// <para>요청된 출력 지점입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The requested output points.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>이 메서드는 반환하지 않습니다.</para>
    /// \endif
    /// \if EN
    /// <para>This method does not return.</para>
    /// \endif
    /// </returns>
    /// <exception cref="NotSupportedException">
    /// \if KO
    /// <para>항상 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Always thrown.</para>
    /// \endif
    /// </exception>
    public byte[] BuildReadAnalogOutputs(IReadOnlyList<AnalogIoPoint> points)
    {
        throw new NotSupportedException(Message);
    }

    /// <summary>
    /// \if KO
    /// <para>미지원 아날로그 출력 응답에 실패 결과를 반환합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Returns a failure result for an unsupported analog-output response.</para>
    /// \endif
    /// </summary>
    /// <param name="responseFrame">
    /// \if KO
    /// <para>무시되는 응답 프레임입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The ignored response frame.</para>
    /// \endif
    /// </param>
    /// <param name="count">
    /// \if KO
    /// <para>무시되는 예상 수입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The ignored expected count.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>미지원 실패 결과입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The unsupported-operation failure result.</para>
    /// \endif
    /// </returns>
    public IoResult<double[]> ParseAnalogOutputs(IReadOnlyList<byte> responseFrame, int count)
    {
        return IoResult<double[]>.Failure(Message);
    }

    /// <summary>
    /// \if KO
    /// <para>미지원 아날로그 출력 쓰기 요청 생성을 거부합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Rejects building an unsupported analog-output write request.</para>
    /// \endif
    /// </summary>
    /// <param name="values">
    /// \if KO
    /// <para>요청된 출력 값입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The requested output values.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>이 메서드는 반환하지 않습니다.</para>
    /// \endif
    /// \if EN
    /// <para>This method does not return.</para>
    /// \endif
    /// </returns>
    /// <exception cref="NotSupportedException">
    /// \if KO
    /// <para>항상 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Always thrown.</para>
    /// \endif
    /// </exception>
    public byte[] BuildWriteAnalogOutputs(IReadOnlyDictionary<AnalogIoPoint, double> values)
    {
        throw new NotSupportedException(Message);
    }

    /// <summary>
    /// \if KO
    /// <para>미지원 쓰기 응답에 실패 결과를 반환합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Returns a failure result for an unsupported write response.</para>
    /// \endif
    /// </summary>
    /// <param name="responseFrame">
    /// \if KO
    /// <para>무시되는 응답 프레임입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The ignored response frame.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>미지원 실패 결과입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The unsupported-operation failure result.</para>
    /// \endif
    /// </returns>
    public IoResult ParseWriteResponse(IReadOnlyList<byte> responseFrame)
    {
        return IoResult.Failure(Message);
    }
}
