using Dreamine.IO.Abstractions.Models;
using Dreamine.IO.Abstractions.Results;

namespace Dreamine.IO.Fastech.Ethernet.Protocol;

/// <summary>
/// \if KO
/// <para>Fastech Ezi-IO Plus-E 16점 디지털 I/O UDP 프레임을 생성하고 파싱합니다.</para>
/// \endif
/// \if EN
/// <para>Builds and parses Fastech Ezi-IO Plus-E 16-point digital-I/O UDP frames.</para>
/// \endif
/// </summary>
public sealed class FastechPlusE16PointProtocol : IFastechEthernetIoProtocol
{
    /// <summary>
    /// \if KO
    /// <para>Header 값을 보관합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Stores the header value.</para>
    /// \endif
    /// </summary>
    private const byte Header = 0xAA;
    /// <summary>
    /// \if KO
    /// <para>Reserved 값을 보관합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Stores the reserved value.</para>
    /// \endif
    /// </summary>
    private const byte Reserved = 0x00;
    /// <summary>
    /// \if KO
    /// <para>Status Ok 값을 보관합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Stores the status ok value.</para>
    /// \endif
    /// </summary>
    private const byte StatusOk = 0x00;
    /// <summary>
    /// \if KO
    /// <para>Get Input Frame Type 값을 보관합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Stores the get input frame type value.</para>
    /// \endif
    /// </summary>
    private const byte GetInputFrameType = 0xC0;
    /// <summary>
    /// \if KO
    /// <para>Get Output Frame Type 값을 보관합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Stores the get output frame type value.</para>
    /// \endif
    /// </summary>
    private const byte GetOutputFrameType = 0xC5;
    /// <summary>
    /// \if KO
    /// <para>Set Output Frame Type 값을 보관합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Stores the set output frame type value.</para>
    /// \endif
    /// </summary>
    private const byte SetOutputFrameType = 0xC6;
    /// <summary>
    /// \if KO
    /// <para>Get Slave Info Frame Type 값을 보관합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Stores the get slave info frame type value.</para>
    /// \endif
    /// </summary>
    private const byte GetSlaveInfoFrameType = 0x01;
    /// <summary>
    /// \if KO
    /// <para>Digital16 Point Reset Mask 값을 보관합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Stores the digital16 point reset mask value.</para>
    /// \endif
    /// </summary>
    private const uint Digital16PointResetMask = 0xFFFF_FFFF;
    /// <summary>
    /// \if KO
    /// <para>Analog Input Not Supported Message 값을 보관합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Stores the analog input not supported message value.</para>
    /// \endif
    /// </summary>
    private const string AnalogInputNotSupportedMessage = "Fastech Ezi-IO Plus-E 16-point DIO protocol does not support analog input.";
    /// <summary>
    /// \if KO
    /// <para>Analog Output Not Supported Message 값을 보관합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Stores the analog output not supported message value.</para>
    /// \endif
    /// </summary>
    private const string AnalogOutputNotSupportedMessage = "Fastech Ezi-IO Plus-E 16-point DIO protocol does not support analog output.";
    /// <summary>
    /// \if KO
    /// <para>sync Lock 값을 보관합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Stores the sync lock value.</para>
    /// \endif
    /// </summary>
    private readonly object _syncLock = new();
    /// <summary>
    /// \if KO
    /// <para>sync No 값을 보관합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Stores the sync no value.</para>
    /// \endif
    /// </summary>
    private byte _syncNo;

    /// <summary>
    /// \if KO
    /// <para>슬레이브 정보 조회 요청 프레임을 만듭니다.</para>
    /// \endif
    /// \if EN
    /// <para>Builds a slave-information request frame.</para>
    /// \endif
    /// </summary>
    /// <returns>
    /// \if KO
    /// <para>슬레이브 정보 요청 프레임입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The slave-information request frame.</para>
    /// \endif
    /// </returns>
    public byte[] BuildGetSlaveInfo()
    {
        return BuildFrame(GetSlaveInfoFrameType, []);
    }

    /// <summary>
    /// \if KO
    /// <para>슬레이브 정보 응답에서 장치 형식과 ASCII 이름을 파싱합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Parses the device type and ASCII name from a slave-information response.</para>
    /// \endif
    /// </summary>
    /// <param name="responseFrame">
    /// \if KO
    /// <para>수신 프레임입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The received frame.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>슬레이브 형식과 이름 텍스트를 포함하는 결과입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A result containing slave-type and name text.</para>
    /// \endif
    /// </returns>
    public IoResult<string> ParseSlaveInfo(IReadOnlyList<byte> responseFrame)
    {
        var payload = ParseReply(responseFrame, GetSlaveInfoFrameType, 1);
        if (!payload.IsSuccess || payload.Value is null)
        {
            return IoResult<string>.Failure(payload.Message ?? "Failed to parse Fastech slave information response.", payload.ErrorCode);
        }

        var slaveType = payload.Value[0];
        var name = payload.Value.Length > 1
            ? System.Text.Encoding.ASCII.GetString(payload.Value, 1, payload.Value.Length - 1).TrimEnd('\0')
            : string.Empty;

        return IoResult<string>.Success($"SlaveType={slaveType}, Name={name}");
    }

    /// <summary>
    /// \if KO
    /// <para>16점 디지털 입력 전체 조회 프레임을 만듭니다.</para>
    /// \endif
    /// \if EN
    /// <para>Builds a full 16-point digital-input query frame.</para>
    /// \endif
    /// </summary>
    /// <param name="points">
    /// \if KO
    /// <para>결과 매핑에 사용되는 요청 지점입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The requested points used for result mapping.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>입력 조회 프레임입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The input-query frame.</para>
    /// \endif
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// \if KO
    /// <para><paramref name="points"/>가 <see langword="null"/>인 경우 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when <paramref name="points"/> is <see langword="null"/>.</para>
    /// \endif
    /// </exception>
    public byte[] BuildReadDigitalInputs(IReadOnlyList<IoPoint> points)
    {
        ArgumentNullException.ThrowIfNull(points);

        return BuildFrame(GetInputFrameType, []);
    }

    /// <summary>
    /// \if KO
    /// <para>입력 응답을 검증하고 요청 개수만큼 부울 상태로 변환합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Validates an input response and converts it to the requested number of Boolean states.</para>
    /// \endif
    /// </summary>
    /// <param name="responseFrame">
    /// \if KO
    /// <para>입력 응답 프레임입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The input-response frame.</para>
    /// \endif
    /// </param>
    /// <param name="count">
    /// \if KO
    /// <para>반환할 지점 수입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The number of points to return.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>입력 상태 배열 결과입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The input-state array result.</para>
    /// \endif
    /// </returns>
    public IoResult<bool[]> ParseDigitalInputs(IReadOnlyList<byte> responseFrame, int count)
    {
        var payload = ParseReply(responseFrame, GetInputFrameType, 8);
        if (!payload.IsSuccess || payload.Value is null)
        {
            return IoResult<bool[]>.Failure(payload.Message ?? "Failed to parse Fastech input response.", payload.ErrorCode);
        }

        return IoResult<bool[]>.Success(ToInputBits(payload.Value, count));
    }

    /// <summary>
    /// \if KO
    /// <para>16점 디지털 출력 전체 조회 프레임을 만듭니다.</para>
    /// \endif
    /// \if EN
    /// <para>Builds a full 16-point digital-output query frame.</para>
    /// \endif
    /// </summary>
    /// <param name="points">
    /// \if KO
    /// <para>결과 매핑에 사용되는 요청 지점입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The requested points used for result mapping.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>출력 조회 프레임입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The output-query frame.</para>
    /// \endif
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// \if KO
    /// <para><paramref name="points"/>가 <see langword="null"/>인 경우 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when <paramref name="points"/> is <see langword="null"/>.</para>
    /// \endif
    /// </exception>
    public byte[] BuildReadDigitalOutputs(IReadOnlyList<IoPoint> points)
    {
        ArgumentNullException.ThrowIfNull(points);

        return BuildFrame(GetOutputFrameType, []);
    }

    /// <summary>
    /// \if KO
    /// <para>출력 응답을 검증하고 요청 개수만큼 부울 상태로 변환합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Validates an output response and converts it to the requested number of Boolean states.</para>
    /// \endif
    /// </summary>
    /// <param name="responseFrame">
    /// \if KO
    /// <para>출력 응답 프레임입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The output-response frame.</para>
    /// \endif
    /// </param>
    /// <param name="count">
    /// \if KO
    /// <para>반환할 지점 수입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The number of points to return.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>출력 상태 배열 결과입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The output-state array result.</para>
    /// \endif
    /// </returns>
    public IoResult<bool[]> ParseDigitalOutputs(IReadOnlyList<byte> responseFrame, int count)
    {
        var payload = ParseReply(responseFrame, GetOutputFrameType, 8);
        if (!payload.IsSuccess || payload.Value is null)
        {
            return IoResult<bool[]>.Failure(payload.Message ?? "Failed to parse Fastech output response.", payload.ErrorCode);
        }

        return IoResult<bool[]>.Success(ToOutputBits(payload.Value, count));
    }

    /// <summary>
    /// \if KO
    /// <para>0~15 채널 변경 값을 설정/해제 마스크로 인코딩한 출력 쓰기 프레임을 만듭니다.</para>
    /// \endif
    /// \if EN
    /// <para>Builds an output-write frame encoding channel 0-15 changes as set and clear masks.</para>
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
    /// <para>출력 쓰기 프레임입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The output-write frame.</para>
    /// \endif
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// \if KO
    /// <para><paramref name="values"/>가 <see langword="null"/>인 경우 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when <paramref name="values"/> is <see langword="null"/>.</para>
    /// \endif
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// \if KO
    /// <para>채널이 0~15 범위를 벗어난 경우 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when a channel is outside the 0-15 range.</para>
    /// \endif
    /// </exception>
    public byte[] BuildWriteDigitalOutputs(IReadOnlyDictionary<IoPoint, bool> values)
    {
        ArgumentNullException.ThrowIfNull(values);

        uint setMask = 0;
        uint clearMask = Digital16PointResetMask;

        foreach (var value in values)
        {
            if (value.Key.Channel is < 0 or > 15)
            {
                throw new ArgumentOutOfRangeException(nameof(values), value.Key.Channel, "Channel must be between 0 and 15.");
            }

            var bit = GetDigital16PointOutputWriteBit(value.Key.Channel);
            if (value.Value)
            {
                setMask |= bit;
                clearMask &= ~bit;
            }
        }

        var data = new byte[8];
        WriteUInt32BigEndian(setMask, data, 0);
        WriteUInt32BigEndian(clearMask, data, 4);

        return BuildFrame(SetOutputFrameType, data);
    }

    /// <summary>
    /// \if KO
    /// <para>이 16점 DIO 프로토콜에서 지원하지 않는 아날로그 입력 요청을 거부합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Rejects an analog-input request unsupported by this 16-point DIO protocol.</para>
    /// \endif
    /// </summary>
    /// <param name="points">
    /// \if KO
    /// <para>요청된 아날로그 입력 지점입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The requested analog-input points.</para>
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
        throw CreateNotSupportedException(AnalogInputNotSupportedMessage);
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
    /// <para>무시되는 응답입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The ignored response.</para>
    /// \endif
    /// </param>
    /// <param name="count">
    /// \if KO
    /// <para>무시되는 지점 수입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The ignored point count.</para>
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
        return CreateUnsupportedAnalogResult(AnalogInputNotSupportedMessage);
    }

    /// <summary>
    /// \if KO
    /// <para>이 프로토콜에서 지원하지 않는 아날로그 출력 읽기 요청을 거부합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Rejects an analog-output read request unsupported by this protocol.</para>
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
        throw CreateNotSupportedException(AnalogOutputNotSupportedMessage);
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
    /// <para>무시되는 응답입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The ignored response.</para>
    /// \endif
    /// </param>
    /// <param name="count">
    /// \if KO
    /// <para>무시되는 지점 수입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The ignored point count.</para>
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
        return CreateUnsupportedAnalogResult(AnalogOutputNotSupportedMessage);
    }

    /// <summary>
    /// \if KO
    /// <para>이 프로토콜에서 지원하지 않는 아날로그 출력 쓰기 요청을 거부합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Rejects an analog-output write request unsupported by this protocol.</para>
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
        throw CreateNotSupportedException(AnalogOutputNotSupportedMessage);
    }

    /// <summary>
    /// \if KO
    /// <para>디지털 출력 쓰기 응답을 검증하여 성공 또는 실패 결과로 변환합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Validates a digital-output write response and converts it to success or failure.</para>
    /// \endif
    /// </summary>
    /// <param name="responseFrame">
    /// \if KO
    /// <para>쓰기 응답 프레임입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The write-response frame.</para>
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
    public IoResult ParseWriteResponse(IReadOnlyList<byte> responseFrame)
    {
        var payload = ParseReply(responseFrame, SetOutputFrameType, 0);
        return payload.IsSuccess
            ? IoResult.Success()
            : IoResult.Failure(payload.Message ?? "Failed to parse Fastech write response.", payload.ErrorCode);
    }

    /// <summary>
    /// \if KO
    /// <para>헤더, 길이, 동기 번호, 예약 바이트, 프레임 형식 및 데이터로 요청 프레임을 조립합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Assembles a request frame from header, length, synchronization number, reserved byte, frame type, and data.</para>
    /// \endif
    /// </summary>
    /// <param name="frameType">
    /// \if KO
    /// <para>Fastech 프레임 형식 바이트입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The Fastech frame-type byte.</para>
    /// \endif
    /// </param>
    /// <param name="data">
    /// \if KO
    /// <para>최대 252바이트의 프레임 데이터입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The frame data, limited to 252 bytes.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>조립된 프레임입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The assembled frame.</para>
    /// \endif
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// \if KO
    /// <para>데이터가 252바이트를 초과한 경우 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when data exceeds 252 bytes.</para>
    /// \endif
    /// </exception>
    private byte[] BuildFrame(byte frameType, IReadOnlyList<byte> data)
    {
        if (data.Count > 252)
        {
            throw new ArgumentOutOfRangeException(nameof(data), data.Count, "Fastech frame data must be 252 bytes or less.");
        }

        var frame = new byte[5 + data.Count];
        frame[0] = Header;
        frame[1] = checked((byte)(3 + data.Count));
        frame[2] = NextSyncNo();
        frame[3] = Reserved;
        frame[4] = frameType;

        for (var i = 0; i < data.Count; i++)
        {
            frame[5 + i] = data[i];
        }

        return frame;
    }

    /// <summary>
    /// \if KO
    /// <para>지정한 설명의 미지원 예외를 만듭니다.</para>
    /// \endif
    /// \if EN
    /// <para>Creates an unsupported-operation exception with the specified message.</para>
    /// \endif
    /// </summary>
    /// <param name="message">
    /// \if KO
    /// <para>예외 설명입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The exception message.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>새 예외입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The new exception.</para>
    /// \endif
    /// </returns>
    private static NotSupportedException CreateNotSupportedException(string message)
    {
        return new NotSupportedException(message);
    }

    /// <summary>
    /// \if KO
    /// <para>아날로그 작업 미지원 실패 결과를 만듭니다.</para>
    /// \endif
    /// \if EN
    /// <para>Creates an unsupported analog-operation failure result.</para>
    /// \endif
    /// </summary>
    /// <param name="message">
    /// \if KO
    /// <para>실패 설명입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The failure message.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>아날로그 배열 실패 결과입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The analog-array failure result.</para>
    /// \endif
    /// </returns>
    private static IoResult<double[]> CreateUnsupportedAnalogResult(string message)
    {
        return IoResult<double[]>.Failure(message);
    }

    /// <summary>
    /// \if KO
    /// <para>응답 헤더, 길이, 예약 바이트, 프레임 형식, 상태 및 최소 페이로드 길이를 검증합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Validates response header, length, reserved byte, frame type, status, and minimum payload length.</para>
    /// \endif
    /// </summary>
    /// <param name="responseFrame">
    /// \if KO
    /// <para>검증할 응답 프레임입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The response frame to validate.</para>
    /// \endif
    /// </param>
    /// <param name="expectedFrameType">
    /// \if KO
    /// <para>예상 프레임 형식입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The expected frame type.</para>
    /// \endif
    /// </param>
    /// <param name="minimumPayloadLength">
    /// \if KO
    /// <para>필요한 최소 페이로드 바이트 수입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The required minimum payload byte count.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>검증된 페이로드 또는 설명이 있는 실패 결과입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The validated payload or a descriptive failure result.</para>
    /// \endif
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// \if KO
    /// <para><paramref name="responseFrame"/>이 <see langword="null"/>인 경우 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when <paramref name="responseFrame"/> is <see langword="null"/>.</para>
    /// \endif
    /// </exception>
    private IoResult<byte[]> ParseReply(IReadOnlyList<byte> responseFrame, byte expectedFrameType, int minimumPayloadLength)
    {
        ArgumentNullException.ThrowIfNull(responseFrame);

        if (responseFrame.Count < 6)
        {
            return IoResult<byte[]>.Failure("The Fastech response frame is too short.");
        }

        if (responseFrame[0] != Header)
        {
            return IoResult<byte[]>.Failure($"Invalid Fastech response header: 0x{responseFrame[0]:X2}.");
        }

        if (responseFrame[1] != responseFrame.Count - 2)
        {
            return IoResult<byte[]>.Failure($"Invalid Fastech response length: {responseFrame[1]}.");
        }

        if (responseFrame[3] != Reserved)
        {
            return IoResult<byte[]>.Failure($"Invalid Fastech response reserved byte: 0x{responseFrame[3]:X2}.");
        }

        if (responseFrame[4] != expectedFrameType)
        {
            return IoResult<byte[]>.Failure($"Unexpected Fastech response frame type: 0x{responseFrame[4]:X2}.");
        }

        var status = responseFrame[5];
        if (status != StatusOk)
        {
            return IoResult<byte[]>.Failure($"Fastech communication status error: 0x{status:X2}.", status);
        }

        var payloadLength = responseFrame.Count - 6;
        if (payloadLength < minimumPayloadLength)
        {
            return IoResult<byte[]>.Failure($"The Fastech response payload length is {payloadLength}, expected at least {minimumPayloadLength}.");
        }

        var payload = new byte[payloadLength];
        for (var i = 0; i < payload.Length; i++)
        {
            payload[i] = responseFrame[6 + i];
        }

        return IoResult<byte[]>.Success(payload);
    }

    /// <summary>
    /// \if KO
    /// <para>잠금으로 보호된 다음 8비트 동기 번호를 반환합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Returns the next lock-protected 8-bit synchronization number.</para>
    /// \endif
    /// </summary>
    /// <returns>
    /// \if KO
    /// <para>증가된 동기 번호입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The incremented synchronization number.</para>
    /// \endif
    /// </returns>
    private byte NextSyncNo()
    {
        lock (_syncLock)
        {
            _syncNo++;
            return _syncNo;
        }
    }

    /// <summary>
    /// \if KO
    /// <para>입력 페이로드의 첫 두 바이트를 최대 16개 부울 상태로 변환합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Converts the first two input-payload bytes into at most 16 Boolean states.</para>
    /// \endif
    /// </summary>
    /// <param name="payload">
    /// \if KO
    /// <para>입력 상태 페이로드입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The input-state payload.</para>
    /// \endif
    /// </param>
    /// <param name="count">
    /// \if KO
    /// <para>반환할 상태 수입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The number of states to return.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>입력 상태 배열입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The input-state array.</para>
    /// \endif
    /// </returns>
    private static bool[] ToInputBits(IReadOnlyList<byte> payload, int count)
    {
        count = Math.Clamp(count, 0, 16);
        var values = new bool[count];

        for (var i = 0; i < values.Length; i++)
        {
            var byteOffset = i < 8 ? 0 : 1;
            var bitIndex = i % 8;
            values[i] = (payload[byteOffset] & (1 << bitIndex)) != 0;
        }

        return values;
    }

    /// <summary>
    /// \if KO
    /// <para>출력 페이로드의 상태 바이트를 최대 16개 부울 상태로 변환합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Converts output-payload state bytes into at most 16 Boolean states.</para>
    /// \endif
    /// </summary>
    /// <param name="payload">
    /// \if KO
    /// <para>출력 상태 페이로드입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The output-state payload.</para>
    /// \endif
    /// </param>
    /// <param name="count">
    /// \if KO
    /// <para>반환할 상태 수입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The number of states to return.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>출력 상태 배열입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The output-state array.</para>
    /// \endif
    /// </returns>
    private static bool[] ToOutputBits(IReadOnlyList<byte> payload, int count)
    {
        count = Math.Clamp(count, 0, 16);
        var values = new bool[count];

        for (var i = 0; i < values.Length; i++)
        {
            var byteOffset = i < 8 ? 2 : 3;
            var bitIndex = i % 8;
            values[i] = (payload[byteOffset] & (1 << bitIndex)) != 0;
        }

        return values;
    }

    /// <summary>
    /// \if KO
    /// <para>논리 채널을 Plus-E 16점 출력 쓰기 마스크 비트 위치로 변환합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Converts a logical channel to its Plus-E 16-point output-write mask bit.</para>
    /// \endif
    /// </summary>
    /// <param name="channel">
    /// \if KO
    /// <para>0~15 논리 채널입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The logical channel from 0 through 15.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>해당 단일 비트 마스크입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The corresponding single-bit mask.</para>
    /// \endif
    /// </returns>
    private static uint GetDigital16PointOutputWriteBit(int channel)
    {
        var normalizedChannel = Math.Clamp(channel, 0, 15);
        var bitIndex = normalizedChannel < 8
            ? 8 + normalizedChannel
            : normalizedChannel - 8;

        return 1u << bitIndex;
    }

    /// <summary>
    /// \if KO
    /// <para>지정 오프셋에서 big-endian 32비트 부호 없는 정수를 읽습니다.</para>
    /// \endif
    /// \if EN
    /// <para>Reads a big-endian 32-bit unsigned integer at the specified offset.</para>
    /// \endif
    /// </summary>
    /// <param name="bytes">
    /// \if KO
    /// <para>원본 바이트 목록입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The source byte list.</para>
    /// \endif
    /// </param>
    /// <param name="offset">
    /// \if KO
    /// <para>첫 바이트 오프셋입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The first-byte offset.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>디코딩된 값입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The decoded value.</para>
    /// \endif
    /// </returns>
    private static uint ReadUInt32BigEndian(IReadOnlyList<byte> bytes, int offset)
    {
        return ((uint)bytes[offset] << 24)
            | ((uint)bytes[offset + 1] << 16)
            | ((uint)bytes[offset + 2] << 8)
            | bytes[offset + 3];
    }

    /// <summary>
    /// \if KO
    /// <para>32비트 부호 없는 정수를 지정 오프셋에 big-endian으로 씁니다.</para>
    /// \endif
    /// \if EN
    /// <para>Writes a 32-bit unsigned integer at the specified offset in big-endian order.</para>
    /// \endif
    /// </summary>
    /// <param name="value">
    /// \if KO
    /// <para>쓸 값입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The value to write.</para>
    /// \endif
    /// </param>
    /// <param name="bytes">
    /// \if KO
    /// <para>대상 바이트 배열입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The destination byte array.</para>
    /// \endif
    /// </param>
    /// <param name="offset">
    /// \if KO
    /// <para>첫 바이트 오프셋입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The first-byte offset.</para>
    /// \endif
    /// </param>
    private static void WriteUInt32BigEndian(uint value, byte[] bytes, int offset)
    {
        bytes[offset] = (byte)(value >> 24);
        bytes[offset + 1] = (byte)(value >> 16);
        bytes[offset + 2] = (byte)(value >> 8);
        bytes[offset + 3] = (byte)value;
    }
}
