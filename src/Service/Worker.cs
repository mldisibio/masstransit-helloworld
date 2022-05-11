using MassTransit;

namespace Service;

// one way to host an event listener service wrapped by MassTransit;
// original walkthru uses IHostedService directly
public class Worker : BackgroundService
{
    readonly ILogger<Worker> _log;
    readonly IBusControl _busControl;

    public Worker(ILogger<Worker> logger, IBusControl busControl)
    {
        _log = logger;
        _busControl = busControl;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // since we are using a third party queue manager, we do not need a loop to keep listening to the queue;
        // however, we will use the BackgroundService lifetime semantics to start and stop the queue subscription

        // register cleanup of queue resources when cancellation is requested
        stoppingToken.Register(async () => await ShutDownListener());
        // start the listener
        await _busControl.StartAsync(TimeSpan.FromSeconds(10)).ConfigureAwait(false);
        // the listener will be (have been) configured during startup to consume incoming messages with the GatewayEventConsumer
    }

    async Task ShutDownListener()
    {
        _log.LogInformation(message: $"Closing Message Queue Resources...");

        try { await _busControl.StopAsync(TimeSpan.FromSeconds(10)).ConfigureAwait(false); }
        catch (Exception ex) { _log.LogError(message: $"{ex.GetType().Name}: {ex.Message}"); }
    }

}
