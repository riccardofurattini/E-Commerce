using Confluent.Kafka;
using Newtonsoft.Json;
using Magazzino.Shared;
using Magazzino.Repository;
using System.Net.Sockets;
using Microsoft.EntityFrameworkCore;

namespace Magazzino.ClientHttp
{
    public class MagazzinoProducer
    {
        private readonly IProducer<Null, string> _producer;
        private readonly MagazzinoContext _dbContext;  // Aggiungi il DbContext

        // Costruttore che ora riceve il MagazzinoContext tramite DI
        public MagazzinoProducer(MagazzinoContext dbContext)
        {
            var config = new ProducerConfig { BootstrapServers = "192.168.1.3:9092", SecurityProtocol = SecurityProtocol.Plaintext };
            _producer = new ProducerBuilder<Null, string>(config).Build();
            _dbContext = dbContext;  // Inizializza il DbContext
        }

        public async Task SendItemsListAsync()
        {
            var maxRetries = 5;
            var delay = 2000; // Millisecondi

            for (int retry = 0; retry < maxRetries; retry++)
            {
                try
                {
                    // Usa Entity Framework per recuperare gli articoli dal database
                    var items = await _dbContext.Items.ToListAsync();

                    // Serializza la lista degli articoli in formato JSON
                    var itemsJson = JsonConvert.SerializeObject(items);

                    // Invia il messaggio a Kafka
                    var deliveryResult = await _producer.ProduceAsync("magazzino_items", new Message<Null, string> { Value = itemsJson });
                    Console.WriteLine($"Messaggio consegnato a {deliveryResult.TopicPartitionOffset} {itemsJson}");

                    break; // Esci dal ciclo se il messaggio è stato inviato con successo
                }
                catch (ProduceException<Null, string> e) when (e.Error.IsFatal)
                {
                    Console.WriteLine($"Errore nella produzione: {e.Error.Reason}");
                    throw; // Rilancia l'eccezione se è fatale
                }
                catch (SocketException)
                {
                    Console.WriteLine("Errore di connessione. Riprovo...");
                    await Task.Delay(delay);
                    delay *= 2; // Aumenta il ritardo con attesa esponenziale
                }
            }
        }
    }
}
