# Dreamine.IO.Fastech.Ethernet

[![CI](https://github.com/CodeMaru-Dreamine/Dreamine.IO.Fastech.Ethernet/actions/workflows/ci.yml/badge.svg?branch=main)](https://github.com/CodeMaru-Dreamine/Dreamine.IO.Fastech.Ethernet/actions/workflows/ci.yml)
[![Quality Gate](https://sonarcloud.io/api/project_badges/measure?project=CodeMaru-Dreamine_Dreamine.IO.Fastech.Ethernet&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=CodeMaru-Dreamine_Dreamine.IO.Fastech.Ethernet)
[![Security Rating](https://sonarcloud.io/api/project_badges/measure?project=CodeMaru-Dreamine_Dreamine.IO.Fastech.Ethernet&metric=security_rating)](https://sonarcloud.io/summary/new_code?id=CodeMaru-Dreamine_Dreamine.IO.Fastech.Ethernet)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=CodeMaru-Dreamine_Dreamine.IO.Fastech.Ethernet&metric=coverage)](https://sonarcloud.io/summary/new_code?id=CodeMaru-Dreamine_Dreamine.IO.Fastech.Ethernet)

[![License](https://img.shields.io/badge/license-MIT-2496ED.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-8-512BD4.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![NuGet](https://img.shields.io/nuget/v/Dreamine.IO.Fastech.Ethernet.svg)](https://www.nuget.org/packages/Dreamine.IO.Fastech.Ethernet)
[![Downloads](https://img.shields.io/nuget/dt/Dreamine.IO.Fastech.Ethernet.svg)](https://www.nuget.org/packages/Dreamine.IO.Fastech.Ethernet)

[![Docs](https://img.shields.io/badge/%F0%9F%93%98%20Docs-dreamine.kr-2496ED)](https://dreamine.kr/libraries?lang=en)
[![Guide](https://img.shields.io/badge/%F0%9F%93%98%20Guide-dreamine.kr-2496ED)](https://dreamine.kr/guide?lang=en)
[![Playground](https://img.shields.io/badge/%F0%9F%8E%AE%20Playground-dreamine.kr-7B2CBF)](https://dreamine.kr/playground?lang=en)
[![Book](https://img.shields.io/badge/%F0%9F%93%96%20Book-Practical%20MVVM%20Architecture-black)](https://bookk.co.kr/bookStore/69c0f1b41461ec1ae849a0f6)

[Korean documentation](./README_KO.md)

Fastech Ethernet I/O adapter package for the Dreamine IO stack.

This package references only `Dreamine.IO.Abstractions` and .NET networking APIs. It does not redistribute Fastech SDK DLLs, vendor runtime DLLs, or vendor source code.

## Current Scope

- Fastech Ethernet connection options
- UDP transport for Fastech Ezi-IO Plus-E communication
- TCP transport scaffold for future protocol variants
- `IIoController` implementation
- Digital input/output channel wrappers
- Analog input/output channel wrappers that currently report not-supported for the 16-point DIO protocol
- `IFastechEthernetIoProtocol` injection point
- Built-in `FastechPlusE16PointProtocol` for Ezi-IO Plus-E 16 DI / 16 DO

## Implemented Hardware Protocol

The current real-hardware implementation targets Fastech Ezi-IO Plus-E 16-point digital I/O over UDP.

- Transport: UDP
- Default port: `3001`
- Header: `0xAA`
- Implemented commands:
  - `0x01` GetSlaveInfo probe
  - `0xC0` Read digital inputs
  - `0xC5` Read digital outputs
  - `0xC6` Write digital outputs
- Channel numbering is zero-based: `DI00`/`DO00` is channel `0`.

The implementation was verified against physical hardware through `SampleSmart`. The device frame layout is handled inside `FastechPlusE16PointProtocol`; application code should use `IoPoint.Channel` values `0` through `15`.

## SampleSmart Verification

`SampleSmart` includes a Dreamine I/O sample page for a 16 DI / 16 DO Fastech device.

Recommended test flow:

1. Set the PC wired NIC and the Fastech device to the same subnet.
2. Enter the device IP and port `3001`.
3. Select `Use Real UDP`.
4. Click `Connect`.
5. Use `Probe` to confirm a raw UDP response.
6. Use `Read DI`, `Write DO`, and `Read DO` to verify channel mapping.

The sample status line prints raw TX/RX frames. Keep this visible when adding a new Fastech model because model-specific byte mapping should be confirmed with real hardware.

## Current Limitations

- The built-in protocol currently covers the verified Ezi-IO Plus-E 16 DI / 16 DO model only.
- Analog I/O is not implemented in this protocol.
- A-contact/B-contact inversion, debounce, on-delay, off-delay, pulse output, and interlock logic are not implemented yet.
- Signal conditioning should be added as a vendor-neutral layer above raw I/O channels, not inside this Fastech protocol class.
- Additional Fastech models should be added as separate protocol implementations and verified with physical hardware.

## Example

```csharp
var options = new FastechEthernetIoOptions
{
    Host = "192.168.0.2",
    Port = 3001,
    TransportType = FastechEthernetIoTransportType.Udp,
    ReceiveTimeoutMs = 1000
};

await using var controller = new FastechEthernetIoController(options);
await controller.ConnectAsync();

var inputs = Enumerable
    .Range(0, 16)
    .Select(channel => new IoPoint(0, channel, $"DI{channel:00}"))
    .ToArray();

var readResult = await controller.DigitalInputs.ReadAsync(inputs);
```

## Vendor Runtime Policy

This package does not include Fastech runtime DLLs. The current Ezi-IO Plus-E implementation uses direct UDP frames only. If a future adapter needs vendor-provided runtime files, keep that adapter in a separate package and require users to install the vendor software with the proper license.

## License

MIT License.
