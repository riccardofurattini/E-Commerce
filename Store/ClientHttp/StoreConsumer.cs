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
        private readonly IServiceProvider _serviceProvider;

        public StoreConsumer(IServiceProvider serviceProvider)
        {
            var config = new ConsumerConfig
            {
                GroupId = "store_group",
                BootstrapServers = "kafka:9092",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };
            _consumer = new ConsumerBuilder<Null, string>(config).Build();
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _consumer.Subscribe("magazzino_items");

            while (!stoppingToken.IsCancellationRequested)
            {
                var consumeResult = _consumer.Consume(stoppingToken);
                var messageValue = consumeResult.Message.Value;

                // Log del messaggio ricevuto
                Console.WriteLine($"Dati ricevuti: {messageValue}");

                // Verifica se il messaggio non è vuoto e se è un array JSON valido
                if (!string.IsNullOrWhiteSpace(messageValue))
                {
                    try
                    {
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

                        // Utilizzo del DbContext per sincronizzare gli articoli
                        using (var scope = _serviceProvider.CreateScope())
                        {
                            var dbContext = scope.ServiceProvider.GetRequiredService<StoreDbContext>();

                            foreach (var articolo in articoliList)
                            {
                                var existingArticolo = dbContext.Articoli.FirstOrDefault(a => a.Id == articolo.Id);
                                if (existingArticolo != null)
                                {
                                    // Aggiorna l'articolo esistente
                                    existingArticolo.Nome = articolo.Nome;
                                    existingArticolo.Descrizione = articolo.Descrizione;
                                    existingArticolo.Prezzo = articolo.Prezzo;
                                }
                                else
                                {
                                    // Aggiungi un nuovo articolo
                                    dbContext.Articoli.Add(articolo);
                                }
                            }

                            await dbContext.SaveChangesAsync(stoppingToken);
                        }

                        // Log degli articoli sincronizzati
                        Console.WriteLine(JsonConvert.SerializeObject(articoliList));
                    }
                    catch (JsonReaderException ex)
                    {
                        // Log di errore se il messaggio non è un JSON valido
                        Console.WriteLine($"Errore di parsing JSON: {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine("Il messaggio ricevuto è vuoto o nullo.");
                }

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
