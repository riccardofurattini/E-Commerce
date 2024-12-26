using Magazzino.ClientHttp;

public class KafkaConsumerService : IHostedService
{
    private readonly CatalogoConsumer _catalogoConsumer;

    public KafkaConsumerService(CatalogoConsumer catalogoConsumer)
    {
        _catalogoConsumer = catalogoConsumer;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        // Avvia il consumer in un task separato
        Task.Run(() => _catalogoConsumer.StartConsuming(), cancellationToken);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        // Fermare il consumer se necessario
        return Task.CompletedTask;
    }
}
