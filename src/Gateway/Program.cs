using Models;
using MassTransit;
using Messages;

var builder = WebApplication.CreateBuilder(args);

// register the DataContext; 
// original: builder.Services.AddDbContext<SomeEFCoreContext>();
builder.Services.RegisterDataContext(builder.Configuration.GetConnectionString("RedisConnection"));

// register MassTransit
builder.Services.AddMassTransit(bus =>
{
    bus.UsingRabbitMq((mt, mq) =>
    {
        // if our gateway is also hosted in docker, then use the service name defined in docker-compose file;
        // otherwise, configuration will figure out RabbitMQ is listening at localhost:5672
        if (EnvHelper.IsRunningInContainer)
            mq.Host("rabbitmq.hw");
    });
});

var app = builder.Build();

// MinWebApi controller to create a request
// GET https://localhost:7000/generate
// returns the request id, which is a guid string
app.MapGet("generate", async (IDataContext ctx, IBus transit) =>
{
    var request = new GatewayRequest();
    try
    {
        // save the request to the underlying data store
        // IDataContext is found in DI services by convention
        ctx.SaveModelObject(request);
        // publish the event announcing that a file has been requested;
        // simulates a long-running worker task to be done on the request;
        // IBus is also implicitly found in DI container;
        await transit.Publish(new GatewayEventMsg { Id = request.Id });
        // return the id of the request back to the caller
        return Results.Ok(request.Id);
    }
    catch (Exception ex)
    {
        return Results.Problem(detail: $"{ex.GetType().Name}: {ex.Message}");
    }
});

// MinWebApi controller to download the generated binary content as a file
// GET https://localhost:7000/download/65b74fb1-1e7a-4117-b0de-0aeec8c4f6a4
// returns the some arbitrary binary content as a file
app.MapGet("download/{id}", (string id, IDataContext ctx) =>
{
    app.Logger.LogInformation("Received download request for {Id}", id);
    try
    {
        byte[] content = ctx.GetModelContentById(id);
        return Results.File(fileContents: content, fileDownloadName: $"{id}.txt");
    }
    catch (Exception ex)
    {
        return Results.Problem(detail: $"{ex.GetType().Name}: {ex.Message}");
    }
});

app.Run();


internal static class EnvHelper
{
    static bool? _isRunningInContainer;

    // for this sample app, Redis and RabbitMQ are hosted in Docker
    // this method checks if our 'Gateway' (web api) is also hosted in Docker
    // my walthru runs it locally
    internal static bool IsRunningInContainer =>
        _isRunningInContainer ??= bool.TryParse(Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"), out var inContainer) && inContainer;

}
