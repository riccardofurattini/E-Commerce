using Confluent.Kafka;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Store.Shared;
using Store.Repository;

namespace Store.ClientHttp
{
    public class StoreConsumer : BackgroundService
    {
        private readonly IConsumer<Null, string> _consumer;

        public StoreConsumer()
        {
            var config = new ConsumerConfig
            {
                GroupId = "store_group",
                BootstrapServers = "kafka:9092",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };
            _consumer = new ConsumerBuilder<Null, string>(config).Build();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _consumer.Subscribe("magazzino_items");
            DbConnection connection = new DbConnection();

            while (!stoppingToken.IsCancellationRequested)
            {
                var consumeResult = _consumer.Consume(stoppingToken);
                var messageValue = consumeResult.Message.Value;

                //Console.WriteLine($"Dati ricevuti: {messageValue}");

                var items = JArray.Parse(messageValue);

                var articoliList = items.Select(item =>
                {
                    var articolo = new Articolo();

                    // Controllo Id
                    articolo.Id = item["Id"] != null && Guid.TryParse(item["Id"].ToString(), out var guidId) ? guidId : Guid.Empty;

                    // Controllo Nome
                    articolo.Nome = item["Nome"] != null ? item["Nome"].ToString() : "NomeSconosciuto";

                    // Controllo Descrizione
                    articolo.Descrizione = item["Descrizione"] != null ? item["Descrizione"].ToString() : "DescrizioneSconosciuta";

                    // Controllo Prezzo
                    articolo.Prezzo = item["Prezzo"] != null && double.TryParse(item["Prezzo"].ToString(), out var prezzo) ? prezzo : 0.0;

                    return articolo;
                }).ToList();



                await connection.SincronizzaArticoli(articoliList);

                // Puoi elaborare la lista di articoli come desideri
                Console.WriteLine(JsonConvert.SerializeObject(articoliList));

                await Task.Delay(1000, stoppingToken);
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _consumer.Close();
            await base.StopAsync(stoppingToken);
        }
    }
}
