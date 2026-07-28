using System.Net;
using System.Net.Sockets;
using Dreamine.IO.Fastech.Ethernet.Options;
using Dreamine.IO.Fastech.Ethernet.Transport;

namespace Dreamine.IO.Fastech.Ethernet.Tests;

public sealed class FastechTransportTests
{
    [Theory]
    [InlineData("", 3001, 0)]
    [InlineData("127.0.0.1", 0, 0)]
    [InlineData("127.0.0.1", 3001, -1)]
    public async Task UdpConnect_RejectsInvalidOptions(string host, int port, int localPort)
    {
        await using var transport = new UdpFastechEthernetIoTransport(
            new FastechEthernetIoOptions { Host = host, Port = port, LocalPort = localPort });

        Assert.False((await transport.ConnectAsync()).IsSuccess);
    }

    [Fact]
    public async Task UdpTransport_RoundTripsDatagramAndTracksFrames()
    {
        using var server = new UdpClient(new IPEndPoint(IPAddress.Loopback, 0));
        var endpoint = (IPEndPoint)server.Client.LocalEndPoint!;
        await using var transport = new UdpFastechEthernetIoTransport(
            new FastechEthernetIoOptions { Host = "127.0.0.1", Port = endpoint.Port });
        var serverTask = Task.Run(async () =>
        {
            var request = await server.ReceiveAsync();
            await server.SendAsync(new byte[] { 9, 8, 7 }, 3, request.RemoteEndPoint);
        });

        Assert.True((await transport.ConnectAsync()).IsSuccess);
        var response = await transport.SendAndReceiveAsync([1, 2, 3], 2000, 0);
        await serverTask;

        Assert.True(response.IsSuccess);
        Assert.Equal([9, 8, 7], response.Value);
        Assert.Equal([1, 2, 3], transport.LastRequestFrame);
        Assert.Equal([9, 8, 7], transport.LastResponseFrame);
        Assert.True((await transport.DisconnectAsync()).IsSuccess);
        Assert.False(transport.IsConnected);
    }

    [Fact]
    public async Task UdpSend_ValidatesStateRequestAndTimeout()
    {
        await using var transport = new UdpFastechEthernetIoTransport(new FastechEthernetIoOptions());

        Assert.False((await transport.SendAndReceiveAsync([], 100, 0)).IsSuccess);
        Assert.False((await transport.SendAndReceiveAsync([1], 0, 0)).IsSuccess);
        Assert.False((await transport.SendAndReceiveAsync([1], 100, 0)).IsSuccess);
    }

    [Theory]
    [InlineData("", 3001, 100)]
    [InlineData("127.0.0.1", 0, 100)]
    [InlineData("127.0.0.1", 3001, 0)]
    public async Task TcpConnect_RejectsInvalidOptions(string host, int port, int timeout)
    {
        await using var transport = new TcpFastechEthernetIoTransport(
            new FastechEthernetIoOptions { Host = host, Port = port, ConnectTimeoutMs = timeout });

        Assert.False((await transport.ConnectAsync()).IsSuccess);
    }

    [Theory]
    [InlineData(4)]
    [InlineData(0)]
    public async Task TcpTransport_RoundTripsExpectedAndAvailableResponse(int expectedLength)
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var endpoint = (IPEndPoint)listener.LocalEndpoint;
        await using var transport = new TcpFastechEthernetIoTransport(
            new FastechEthernetIoOptions
            {
                Host = "127.0.0.1",
                Port = endpoint.Port,
                ConnectTimeoutMs = 2000
            });
        var serverTask = Task.Run(async () =>
        {
            using var client = await listener.AcceptTcpClientAsync();
            var stream = client.GetStream();
            var request = new byte[3];
            await stream.ReadExactlyAsync(request);
            await stream.WriteAsync(new byte[] { 4, 5, 6, 7 });
            await stream.FlushAsync();
        });

        Assert.True((await transport.ConnectAsync()).IsSuccess);
        var response = await transport.SendAndReceiveAsync([1, 2, 3], 2000, expectedLength);
        await serverTask;
        listener.Stop();

        Assert.True(response.IsSuccess);
        Assert.Equal([4, 5, 6, 7], response.Value);
        Assert.True((await transport.DisconnectAsync()).IsSuccess);
    }

    [Fact]
    public async Task TcpSend_ValidatesStateRequestTimeoutAndLength()
    {
        await using var transport = new TcpFastechEthernetIoTransport(new FastechEthernetIoOptions());

        Assert.False((await transport.SendAndReceiveAsync([], 100, 0)).IsSuccess);
        Assert.False((await transport.SendAndReceiveAsync([1], 0, 0)).IsSuccess);
        Assert.False((await transport.SendAndReceiveAsync([1], 100, -1)).IsSuccess);
        Assert.False((await transport.SendAndReceiveAsync([1], 100, 0)).IsSuccess);
    }
}
