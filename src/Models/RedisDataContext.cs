using Microsoft.Extensions.Logging;
using StackExchange.Redis;
namespace Models;

// no frills implementation of the IDataContext using Redis;
// could be any db, EF implementation, or any distributed store; 
public class RedisDataContext : IDataContext
{
    readonly IConnectionMultiplexer _redis;
    readonly ILogger<RedisDataContext> _log;

    public RedisDataContext(IConnectionMultiplexer redisConn, ILogger<RedisDataContext> logger)
    {
        // both supplied by DI
        _redis = redisConn;
        _log = logger;
    }

    public GatewayRequest GetModelObjectById(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentNullException(paramName: id);

        var store = _redis.GetDatabase();
        string? modelAsJson = store.StringGet(id);
        if (modelAsJson != null)
            return modelAsJson.FromJson<GatewayRequest>() ?? throw new InvalidCastException($"Could not deserialize request {id}");
        else
            throw new InvalidOperationException($"Model {id} not found");
    }

    // we are simply saving the 'model' as a serialized json string, using its Id as the key;
    // could be done other ways, such as a db table with columns, or even explicit Hash type in Redis
    public void SaveModelObject(GatewayRequest model)
    {
        if (model != null)
        {
            IDatabase store = _redis.GetDatabase();
            string modelAsJson = model.ToFlatJson();
            store.StringSet(model.Id, modelAsJson);
            _log.LogInformation("Saved model {ModelId}", model.Id);
        }
        else
            _log.LogError("Save invoked on null model");
    }

    public byte[] GetModelContentById(string id)
    {
        GatewayRequest model = GetModelObjectById(id);
        return model.File ?? Array.Empty<byte>();
    }
}
