using Dreamine.IO.Abstractions.Enums;
using Dreamine.IO.Abstractions.Options;

namespace Dreamine.IO.Fastech.Ethernet.Options;

/// <summary>
/// Represents Fastech Ethernet I/O connection options.
/// </summary>
public sealed class FastechEthernetIoOptions
{
    /// <summary>
    /// Gets or sets the target host.
    /// </summary>
    public string Host { get; set; } = "127.0.0.1";

    /// <summary>
    /// Gets or sets the target port.
    /// </summary>
    public int Port { get; set; } = 3001;

    /// <summary>
    /// Gets or sets the local UDP port. Zero lets Windows choose an ephemeral source port.
    /// </summary>
    public int LocalPort { get; set; }

    /// <summary>
    /// Gets or sets the transport type.
    /// </summary>
    public FastechEthernetIoTransportType TransportType { get; set; } = FastechEthernetIoTransportType.Udp;

    /// <summary>
    /// Gets or sets the board, device, or controller index.
    /// </summary>
    public int DeviceIndex { get; set; }

    /// <summary>
    /// Gets or sets the connect timeout in milliseconds.
    /// </summary>
    public int ConnectTimeoutMs { get; set; } = 3000;

    /// <summary>
    /// Gets or sets the receive timeout in milliseconds.
    /// </summary>
    public int ReceiveTimeoutMs { get; set; } = 3000;

    /// <summary>
    /// Gets or sets the retry count.
    /// </summary>
    public int RetryCount { get; set; } = 1;

    /// <summary>
    /// Gets or sets the expected response length. Zero means the transport reads until the first available packet ends.
    /// </summary>
    public int ExpectedResponseLength { get; set; }

    /// <summary>
    /// Gets provider-specific properties that should be interpreted only by the concrete protocol implementation.
    /// </summary>
    public IDictionary<string, string> Properties { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Converts these options to provider-neutral I/O connection options.
    /// </summary>
    /// <returns>The provider-neutral options.</returns>
    public IoConnectionOptions ToIoConnectionOptions()
    {
        var options = new IoConnectionOptions
        {
            Provider = IoProvider.Fastech,
            DeviceIndex = DeviceIndex,
            Name = Host
        };

        options.Properties["Host"] = Host;
        options.Properties["Port"] = Port.ToString();
        options.Properties["LocalPort"] = LocalPort.ToString();
        options.Properties["ConnectTimeoutMs"] = ConnectTimeoutMs.ToString();
        options.Properties["ReceiveTimeoutMs"] = ReceiveTimeoutMs.ToString();
        options.Properties["RetryCount"] = RetryCount.ToString();

        foreach (var property in Properties)
        {
            options.Properties[property.Key] = property.Value;
        }

        return options;
    }
}
