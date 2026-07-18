using Dreamine.IO.Abstractions.Models;
using Dreamine.IO.Abstractions.Results;

namespace Dreamine.IO.Fastech.Ethernet.Protocol;

/// <summary>
/// \if KO
/// <para>Fastech Ethernet I/O 요청 프레임 생성과 응답 파싱 계약을 정의합니다.</para>
/// \endif
/// \if EN
/// <para>Defines request-frame building and response parsing for Fastech Ethernet I/O.</para>
/// \endif
/// </summary>
public interface IFastechEthernetIoProtocol
{
    /// <summary>
    /// \if KO
    /// <para>디지털 입력 읽기 요청을 만듭니다.</para>
    /// \endif
    /// \if EN
    /// <para>Builds a digital-input read request.</para>
    /// \endif
    /// </summary>
    /// <param name="points">
    /// \if KO
    /// <para>읽을 디지털 입력 지점입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The digital-input points to read.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>전송할 요청 프레임입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The request frame to transmit.</para>
    /// \endif
    /// </returns>
    byte[] BuildReadDigitalInputs(IReadOnlyList<IoPoint> points);

    /// <summary>
    /// \if KO
    /// <para>디지털 입력 읽기 응답을 파싱합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Parses a digital-input read response.</para>
    /// \endif
    /// </summary>
    /// <param name="responseFrame">
    /// \if KO
    /// <para>수신 응답 프레임입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The received response frame.</para>
    /// \endif
    /// </param>
    /// <param name="count">
    /// \if KO
    /// <para>예상 지점 수입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The expected point count.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>디지털 입력 상태를 포함하는 결과입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A result containing digital-input states.</para>
    /// \endif
    /// </returns>
    IoResult<bool[]> ParseDigitalInputs(IReadOnlyList<byte> responseFrame, int count);

    /// <summary>
    /// \if KO
    /// <para>디지털 출력 읽기 요청을 만듭니다.</para>
    /// \endif
    /// \if EN
    /// <para>Builds a digital-output read request.</para>
    /// \endif
    /// </summary>
    /// <param name="points">
    /// \if KO
    /// <para>읽을 디지털 출력 지점입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The digital-output points to read.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>전송할 요청 프레임입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The request frame to transmit.</para>
    /// \endif
    /// </returns>
    byte[] BuildReadDigitalOutputs(IReadOnlyList<IoPoint> points);

    /// <summary>
    /// \if KO
    /// <para>디지털 출력 읽기 응답을 파싱합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Parses a digital-output read response.</para>
    /// \endif
    /// </summary>
    /// <param name="responseFrame">
    /// \if KO
    /// <para>수신 응답 프레임입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The received response frame.</para>
    /// \endif
    /// </param>
    /// <param name="count">
    /// \if KO
    /// <para>예상 지점 수입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The expected point count.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>디지털 출력 상태를 포함하는 결과입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A result containing digital-output states.</para>
    /// \endif
    /// </returns>
    IoResult<bool[]> ParseDigitalOutputs(IReadOnlyList<byte> responseFrame, int count);

    /// <summary>
    /// \if KO
    /// <para>디지털 출력 쓰기 요청을 만듭니다.</para>
    /// \endif
    /// \if EN
    /// <para>Builds a digital-output write request.</para>
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
    /// <returns>
    /// \if KO
    /// <para>전송할 요청 프레임입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The request frame to transmit.</para>
    /// \endif
    /// </returns>
    byte[] BuildWriteDigitalOutputs(IReadOnlyDictionary<IoPoint, bool> values);

    /// <summary>
    /// \if KO
    /// <para>아날로그 입력 읽기 요청을 만듭니다.</para>
    /// \endif
    /// \if EN
    /// <para>Builds an analog-input read request.</para>
    /// \endif
    /// </summary>
    /// <param name="points">
    /// \if KO
    /// <para>읽을 아날로그 입력 지점입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The analog-input points to read.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>전송할 요청 프레임입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The request frame to transmit.</para>
    /// \endif
    /// </returns>
    byte[] BuildReadAnalogInputs(IReadOnlyList<AnalogIoPoint> points);

    /// <summary>
    /// \if KO
    /// <para>아날로그 입력 읽기 응답을 파싱합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Parses an analog-input read response.</para>
    /// \endif
    /// </summary>
    /// <param name="responseFrame">
    /// \if KO
    /// <para>수신 응답 프레임입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The received response frame.</para>
    /// \endif
    /// </param>
    /// <param name="count">
    /// \if KO
    /// <para>예상 지점 수입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The expected point count.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>아날로그 입력 값을 포함하는 결과입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A result containing analog-input values.</para>
    /// \endif
    /// </returns>
    IoResult<double[]> ParseAnalogInputs(IReadOnlyList<byte> responseFrame, int count);

    /// <summary>
    /// \if KO
    /// <para>아날로그 출력 읽기 요청을 만듭니다.</para>
    /// \endif
    /// \if EN
    /// <para>Builds an analog-output read request.</para>
    /// \endif
    /// </summary>
    /// <param name="points">
    /// \if KO
    /// <para>읽을 아날로그 출력 지점입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The analog-output points to read.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>전송할 요청 프레임입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The request frame to transmit.</para>
    /// \endif
    /// </returns>
    byte[] BuildReadAnalogOutputs(IReadOnlyList<AnalogIoPoint> points);

    /// <summary>
    /// \if KO
    /// <para>아날로그 출력 읽기 응답을 파싱합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Parses an analog-output read response.</para>
    /// \endif
    /// </summary>
    /// <param name="responseFrame">
    /// \if KO
    /// <para>수신 응답 프레임입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The received response frame.</para>
    /// \endif
    /// </param>
    /// <param name="count">
    /// \if KO
    /// <para>예상 지점 수입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The expected point count.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>아날로그 출력 값을 포함하는 결과입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A result containing analog-output values.</para>
    /// \endif
    /// </returns>
    IoResult<double[]> ParseAnalogOutputs(IReadOnlyList<byte> responseFrame, int count);

    /// <summary>
    /// \if KO
    /// <para>아날로그 출력 쓰기 요청을 만듭니다.</para>
    /// \endif
    /// \if EN
    /// <para>Builds an analog-output write request.</para>
    /// \endif
    /// </summary>
    /// <param name="values">
    /// \if KO
    /// <para>지점별 아날로그 출력 값입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The analog-output values keyed by point.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>전송할 요청 프레임입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The request frame to transmit.</para>
    /// \endif
    /// </returns>
    byte[] BuildWriteAnalogOutputs(IReadOnlyDictionary<AnalogIoPoint, double> values);

    /// <summary>
    /// \if KO
    /// <para>출력 쓰기 응답을 파싱합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Parses an output-write response.</para>
    /// \endif
    /// </summary>
    /// <param name="responseFrame">
    /// \if KO
    /// <para>수신 응답 프레임입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The received response frame.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>장치 쓰기 결과입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The device-write result.</para>
    /// \endif
    /// </returns>
    IoResult ParseWriteResponse(IReadOnlyList<byte> responseFrame);
}
