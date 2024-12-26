using Confluent.Kafka;
using Store.Shared;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace Store.ClientHttp
{
    public class RichiestaCatalogo
    {
        private readonly IProducer<string, string> _producer;
        private readonly IConsumer<string, string> _consumer;

        // Costruttore
        public RichiestaCatalogo(IHttpClientFactory httpClientFactory)
        {
            // Configurazione Kafka per il Producer
            var configProducer = new ProducerConfig
            {
                BootstrapServers = "kafka:9092" // Indirizzo del server Kafka
            };
            _producer = new ProducerBuilder<string, string>(configProducer).Build();

            // Configurazione Kafka per il Consumer
            var configConsumer = new ConsumerConfig
            {
                GroupId = "store-group",
                BootstrapServers = "kafka:9092",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };
            _consumer = new ConsumerBuilder<string, string>(configConsumer).Build();
        }

        public async Task<List<ArticoloDto>> GetArticoliAsync()
        {
            // Invia la richiesta a Kafka
            var requestMessage = new Message<string, string>
            {
                Key = "store-request",
                Value = JsonConvert.SerializeObject(new { Request = "get-articoli" })
            };

            await _producer.ProduceAsync("richiesta-articoli", requestMessage); // Topic richiesta-articoli

            // Consuma la risposta da Kafka
            _consumer.Subscribe("risposta-articoli"); // Topic risposta-articoli

            try
            {
                var consumeResult = _consumer.Consume();
                var articoliDto = JsonConvert.DeserializeObject<List<ArticoloDto>>(consumeResult.Message.Value);
                return articoliDto;
            }
            catch (ConsumeException e)
            {
                Console.WriteLine($"Errore durante la lettura del messaggio: {e.Error.Reason}");
                return new List<ArticoloDto>();
            }
        }
    }
}
