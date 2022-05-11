using Models;
using Service;
using MassTransit;

var builder = Host.CreateDefaultBuilder(args);

IHost host = Host.CreateDefaultBuilder(args)
                 .ConfigureServices((ctx, services) =>
                 {
                     
                     // register our data store context
                     // original: services.AddDbContext<SomeEFCoreContext>();
                     services.RegisterDataContext(ctx.Configuration.GetConnectionString("RedisConnection"));
                     // tell MassTransit to consume messages from RabbbitMQ
                     services.AddMassTransit(bus =>
                     {
                         // look for all consumers in the same assembly as our GatewayEventConsumer
                         bus.AddConsumers(typeof(GatewayEventConsumer).Assembly);
                         // wire up MassTransit and RabbitMQ
                         bus.UsingRabbitMq((mt, mq) =>
                         {
                             // if our BackgroundService is also hosted in docker, then use the service name defined in docker-compose file;
                             // otherwise, configuration will figure out RabbitMQ is listening at localhost:5672
                             if (EnvHelper.IsRunningInContainer)
                                 mq.Host("rabbitmq.hw");
                             //else
                             //    as long as docker port mapping is 5672, it figures out localhost
                             //    mq.Host("127.0.0.1");

                             // configure a named Queue; any name;
                             mq.ReceiveEndpoint(queueName:"HelloWorldQueue", configureEndpoint: endPt =>
                             {
                                 // have the GatewayEventConsumer subscribe to the messages published to the given queue
                                 endPt.ConfigureConsumer<GatewayEventConsumer>(mt);
                             });
                         });
                     });
                 
                     // register the hosted listener
                     services.AddHostedService<Worker>();
                 })
                 .Build();

await host.RunAsync();

internal static class EnvHelper
{
    static bool? _isRunningInContainer;

    // for this sample app, Redis and RabbitMQ are hosted in Docker
    // this method checks if our 'Service' (BackgroundService) is also hosted in Docker
    // my walthru runs it locally
    internal static bool IsRunningInContainer =>
        _isRunningInContainer ??= bool.TryParse(Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"), out var inContainer) && inContainer;

}
