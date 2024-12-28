using Confluent.Kafka;
using Newtonsoft.Json;
using Magazzino.Shared;
using Magazzino.Repository;
using System.Net.Sockets;

namespace Magazzino.ClientHttp
{
    public class MagazzinoProducer
    {
        private readonly IProducer<Null, string> _producer;

        public MagazzinoProducer()
        {
            var config = new ProducerConfig { BootstrapServers = "192.168.1.3:9092", SecurityProtocol = SecurityProtocol.Plaintext };
            _producer = new ProducerBuilder<Null, string>(config).Build();
        }

        public async Task SendItemsListAsync()
        {
            var maxRetries = 5;
            var delay = 2000; // Millisecondi

            for (int retry = 0; retry < maxRetries; retry++)
            {
                try
                {
                    ItemsConnection connection = new ItemsConnection();

                    var itemsJson = JsonConvert.SerializeObject(await connection.GetItemsAsync());
                    var deliveryResult = await _producer.ProduceAsync("magazzino_items", new Message<Null, string> { Value = itemsJson });
                    Console.WriteLine($"Messaggio consegnato a {deliveryResult.TopicPartitionOffset} {itemsJson}");
                    //_producer.Flush(TimeSpan.FromSeconds(10));
                    break; // Se il messaggio viene inviato con successo, esci dal ciclo
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
