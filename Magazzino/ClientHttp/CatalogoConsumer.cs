using Confluent.Kafka;
using Magazzino.Repository;
using Magazzino.Shared;
using Newtonsoft.Json;

namespace Magazzino.ClientHttp
{
    public class CatalogoConsumer
    {
        private readonly IConsumer<string, string> _consumer;
        private readonly IProducer<string, string> _producer;

        // Costruttore
        public CatalogoConsumer()
        {
            // Configurazione Kafka per il Consumer
            var configConsumer = new ConsumerConfig
            {
                GroupId = "magazzino-group",
                BootstrapServers = "kafka:9092",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };
            _consumer = new ConsumerBuilder<string, string>(configConsumer).Build();

            // Configurazione Kafka per il Producer (per inviare la risposta)
            var configProducer = new ProducerConfig
            {
                BootstrapServers = "kafka:9092" // Indirizzo del server Kafka
            };
            _producer = new ProducerBuilder<string, string>(configProducer).Build();
        }

        // Metodo per avviare il consumo dei messaggi di richiesta e inviare la risposta
        public async Task StartConsuming()
        {
            _consumer.Subscribe("richiesta-articoli"); // Topic richiesta-articoli

            while (true)
            {
                try
                {
                    // Consuma il messaggio di richiesta da Kafka
                    var consumeResult = _consumer.Consume();
                    var request = JsonConvert.DeserializeObject<dynamic>(consumeResult.Message.Value);

                    // Verifica la richiesta (in questo caso: "get-articoli")
                    if (request.Request == "get-articoli")
                    {
                        ItemsConnection c = new ItemsConnection();
                        // Ottieni gli articoli dal metodo esistente GetItemsAsync
                        var articoli = await c.GetItemsAsync();

                        // Mappa gli articoli dal modello Item a ItemDto
                        var articoliDto = MapToItemDto(articoli);

                        // Serializza la risposta in JSON
                        var responseMessage = new Message<string, string>
                        {
                            Key = "magazzino-response",
                            Value = JsonConvert.SerializeObject(articoliDto)
                        };

                        // Invia la risposta tramite Kafka
                        _producer.Produce("risposta-articoli", responseMessage); // Topic risposta-articoli
                    }
                }
                catch (ConsumeException e)
                {
                    Console.WriteLine($"Errore durante la lettura del messaggio: {e.Error.Reason}");
                }
            }
        }


        // Metodo per mappare da Item a ItemDto
        private List<ItemDto> MapToItemDto(List<Item> articoli)
        {
            var articoliDto = new List<ItemDto>();
            foreach (var item in articoli)
            {
                articoliDto.Add(new ItemDto(
                        Id: item.Id,
                        Nome: item.Nome,
                        Descrizione: item.Descrizione,
                        Prezzo: item.Prezzo,
                        Quantita: item.Quantita
                    ));
            }
            return articoliDto;
        }
    }
}
