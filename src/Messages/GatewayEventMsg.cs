namespace Messages;

// a simple dto which passes the request id between services when the next processing step is queued;
// since each component has access to the data store, we do not need to pass the full model around;
public class GatewayEventMsg
{
    public string Id { get; set; } = string.Empty;
}
