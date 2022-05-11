namespace Models;

// represents the full domain 'model' our services work on
public class GatewayRequest
{
    // simply initialize with a guid
    public string Id { get; set; } = $"{Guid.NewGuid():D}";

    // our worker will produce binary content and update this instance
    public byte[]? File { get; set; }
}
