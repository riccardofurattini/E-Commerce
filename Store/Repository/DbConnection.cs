using Store.Shared;
using Npgsql;

namespace Store.Repository
{
    public class DbConnection : IDisposable
    {
        private NpgsqlConnection connection;
        private readonly string connectionString = "Host=store-db;Username=store_user;Password=p4ssw0rD;Database=store_db;Port=5432";

        //"Host=store-db;Username=store_user;Password=p4ssw0rD;Database=store_db;Port=5432";


        // Costruttore
        public DbConnection()
        {
            connection = new NpgsqlConnection(connectionString);
        }

        // Metodo per recuperare gli articoli dal database
        public async Task SincronizzaArticoli(List<Articolo> items)
        {
            await connection.OpenAsync(); // Apri la connessione

            // Salva gli articoli nel database
            foreach (var item in items)
            {
                var query = @"
                    INSERT INTO articoli (Id, Nome, Descrizione, Prezzo)
                    VALUES (@Id, @Nome, @Descrizione, @Prezzo)
                    ON CONFLICT (Id) DO NOTHING;"; // Evita duplicati

                using var command = new NpgsqlCommand(query, connection);
                command.Parameters.AddWithValue("Id", item.Id);
                command.Parameters.AddWithValue("Nome", item.Nome);
                command.Parameters.AddWithValue("Descrizione", item.Descrizione ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("Prezzo", item.Prezzo);

                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task<List<Articolo>> GetArticoli()
        {
            var articoli = new List<Articolo>(); // Lista da restituire
            try
            {
                if (connection.State != System.Data.ConnectionState.Open)
                {
                    await connection.OpenAsync(); // Apri la connessione solo se non è già aperta
                    Console.WriteLine("Connesso con successo al database");
                }


                // Eseguire una query per selezionare tutti gli articoli
                using (var cmd = new NpgsqlCommand("SELECT Id, Nome, Descrizione, Prezzo FROM articoli", connection))
                {
                    // Esegui la query in modalità asincrona
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        // Leggere i dati restituiti dalla query
                        while (await reader.ReadAsync()) // Usa ReadAsync per la lettura asincrona
                        {
                            // Creare un nuovo oggetto Item per ogni riga
                            var articolo = new Articolo
                            {
                                Id = reader.GetGuid(reader.GetOrdinal("Id")),
                                Nome = reader.GetString(reader.GetOrdinal("Nome")),
                                Descrizione = reader.GetString(reader.GetOrdinal("Descrizione")),
                                Prezzo = reader.GetDouble(reader.GetOrdinal("Prezzo")),
                            };

                            // Aggiungere l'oggetto Item alla lista
                            articoli.Add(articolo);
                        }
                    }
                }

                return articoli; // Restituisci la lista di oggetti Item
            }
            catch (Exception ex)
            {
                // Gestire eventuali errori di connessione
                Console.WriteLine($"Errore di connessione: {ex.Message}");
                return null; 
            }
        }


        // Dispose per chiudere correttamente la connessione
        public void Dispose()
        {
            connection?.Dispose();
        }
    }
}
