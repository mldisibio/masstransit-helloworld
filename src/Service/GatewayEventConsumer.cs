using MassTransit;
using Messages;
using Models;

namespace Service;

// MassTransit's expected implementation for a consumer of a published message
public class GatewayEventConsumer : IConsumer<GatewayEventMsg>
{
    readonly ILogger<GatewayEventConsumer> _log;
    readonly IDataContext _ctx;

    public GatewayEventConsumer(ILogger<GatewayEventConsumer> logger, IDataContext ctx)
    {
        // both supplied by DI
        _log = logger;
        _ctx = ctx;
    }

    public Task Consume(ConsumeContext<GatewayEventMsg> context)
    {
        // we receive the published event containing just the 'Id' of a new 'model' request; GatewayEventMsg is the 'context.Message'
        string id = context.Message.Id;
        _log.LogInformation("Received event with {Id}", id);

        try
        {
            // retrieve the model from the underlying data store;
            GatewayRequest model = _ctx.GetModelObjectById(id);
            // the purpose of this simple listener/worker is to generate binary content for that request
            model.File = new BinaryData($"{model.Id}{Environment.NewLine}Created At {DateTime.Now:yyyy.MM.dd HH:mm:ss}").ToArray();
            // save the updated model back to the data store
            _ctx.SaveModelObject(model);
        }
        catch (Exception ex)
        {
            _log.LogError(exception: ex, message: "{ExType}: {ExMsg}", ex.GetType().Name, ex.Message);
        }

        return Task.CompletedTask;
    }
}
