﻿using Store.Shared;
using Npgsql;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

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
            if (connection.State != System.Data.ConnectionState.Open)
            {
                await connection.OpenAsync(); // Apri la connessione solo se non è già aperta
                Console.WriteLine("Connesso con successo al database");
            }

            // Inizia una transazione per garantire la consistenza
            using var transaction = await connection.BeginTransactionAsync();

            try
            {
                // Step 1: Aggiorna o inserisce gli articoli
                foreach (var item in items)
                {
                    var query = @"
                        INSERT INTO articoli (Id, Nome, Descrizione, Prezzo)
                        VALUES (@Id, @Nome, @Descrizione, @Prezzo)
                        ON CONFLICT (Id) DO UPDATE
                        SET Nome = EXCLUDED.Nome,
                            Descrizione = EXCLUDED.Descrizione,
                            Prezzo = EXCLUDED.Prezzo;";

                    using var command = new NpgsqlCommand(query, connection, transaction);
                    command.Parameters.AddWithValue("Id", item.Id);
                    command.Parameters.AddWithValue("Nome", item.Nome);
                    command.Parameters.AddWithValue("Descrizione", item.Descrizione ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("Prezzo", item.Prezzo);

                    await command.ExecuteNonQueryAsync();
                }

                // Step 2: Elimina articoli dal carrello che non sono più presenti nella lista
                var idsToKeep = string.Join(",", items.Select(i => $"'{i.Id}'"));
                var deleteFromCartQuery = $@"
                    DELETE FROM carrello
                    WHERE IdArticolo NOT IN ({idsToKeep});";

                using var deleteCartCommand = new NpgsqlCommand(deleteFromCartQuery, connection, transaction);
                await deleteCartCommand.ExecuteNonQueryAsync();

                // Step 3: Elimina articoli dalla tabella articoli che non sono più nella lista
                var deleteFromArticlesQuery = $@"
                    DELETE FROM articoli
                    WHERE Id NOT IN ({idsToKeep});";

                using var deleteArticlesCommand = new NpgsqlCommand(deleteFromArticlesQuery, connection, transaction);
                await deleteArticlesCommand.ExecuteNonQueryAsync();

                // Conferma la transazione
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore durante la sincronizzazione: {ex.Message}");
                await transaction.RollbackAsync();
                throw;
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

        public async Task<Articolo> GetArticoloById(Guid id)
        {
            Articolo item = null; // Inizializza item a null in caso di errore
            try
            {
                if (connection.State != System.Data.ConnectionState.Open)
                {
                    await connection.OpenAsync(); // Apri la connessione solo se non è già aperta
                    Console.WriteLine("Connesso con successo al database");
                }


                // Eseguire una query per selezionare un articolo in base all'ID
                using (var cmd = new NpgsqlCommand("SELECT Id, Nome, Descrizione, Prezzo FROM articoli WHERE Id = @Id", connection))
                {
                    // Aggiungi il parametro per evitare SQL Injection
                    cmd.Parameters.AddWithValue("@Id", id);

                    // Esegui la query in modalità asincrona
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        // Verifica se la query ha restituito almeno una riga
                        if (await reader.ReadAsync())
                        {
                            // Leggi i dati restituiti dalla query
                            item = new Articolo
                            {
                                Id = reader.GetGuid(reader.GetOrdinal("Id")),
                                Nome = reader.GetString(reader.GetOrdinal("Nome")),
                                Descrizione = reader.GetString(reader.GetOrdinal("Descrizione")),
                                Prezzo = reader.GetDouble(reader.GetOrdinal("Prezzo"))
                            };
                        }
                    }
                }

                return item;
            }
            catch (Exception ex)
            {
                // Gestire eventuali errori di connessione
                Console.WriteLine($"Errore di connessione: {ex.Message}");
                return null; // Restituisci null se c'è un errore
            }
        }

        public void InitializeDatabase()
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                string createTablesQuery = @"
                CREATE TABLE IF NOT EXISTS articoli (
                    Id UUID PRIMARY KEY,  
                    Nome VARCHAR(100) NOT NULL,    
                    Descrizione VARCHAR(255),  
                    Prezzo NUMERIC(10, 2) NOT NULL  -- Precisione per rappresentare importi monetari
                );

                CREATE TABLE IF NOT EXISTS carrello (
                    IdCarrello UUID NOT NULL,
                    IdArticolo UUID NOT NULL REFERENCES articoli(Id),   
                    Quantita INT NOT NULL
                );";

                using (var command = new NpgsqlCommand(createTablesQuery, connection))
                {
                    command.ExecuteNonQuery(); // Esegui la query
                }
            }

        }

        // Dispose per chiudere correttamente la connessione
        public void Dispose()
        {
            connection?.Dispose();
        }

        //metodi per gestire il carrello

        public async Task<List<Guid>> GetCarrelli()
        {
            var carrelli = new List<Guid>(); // Lista da restituire
            try
            {
                if (connection.State != System.Data.ConnectionState.Open)
                {
                    await connection.OpenAsync(); // Apri la connessione solo se non è già aperta
                    Console.WriteLine("Connesso con successo al database");
                }


                // Eseguire una query per selezionare tutti gli articoli
                using (var cmd = new NpgsqlCommand("SELECT IdCarrello FROM carrello GROUP BY IdCarrello", connection))
                {
                    // Esegui la query in modalità asincrona
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        // Leggere i dati restituiti dalla query
                        while (await reader.ReadAsync()) // Usa ReadAsync per la lettura asincrona
                        {

                            Guid carrello = reader.GetGuid(reader.GetOrdinal("IdCarrello"));

                            carrelli.Add(carrello);
                        }
                    }
                }

                return carrelli; // Restituisci la lista di oggetti Item
            }
            catch (Exception ex)
            {
                // Gestire eventuali errori di connessione
                Console.WriteLine($"Errore di connessione: {ex.Message}");
                return null;
            }
        }



        public async Task<bool> EsisteCarrello(Guid idCarrello)
        {
            try
            {
                if (connection.State != System.Data.ConnectionState.Open)
                {
                    await connection.OpenAsync(); // Apri la connessione solo se non è già aperta
                    Console.WriteLine("Connesso con successo al database");
                }

                // Verifica se il carrello esiste
                var query = "SELECT COUNT(*) FROM carrello WHERE IdCarrello = @IdCarrello";
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("IdCarrello", idCarrello);
                    var result = (long)await command.ExecuteScalarAsync(); // Conta le righe che corrispondono
                    return result > 0; // Se il risultato è maggiore di 0, il carrello esiste
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore di connessione: {ex.Message}");
                return false;
            }
        }



        public async Task<bool> DeleteCarrello(Guid idCarrello)
        {
            try
            {
                if (connection.State != System.Data.ConnectionState.Open)
                {
                    await connection.OpenAsync(); // Apri la connessione solo se non è già aperta
                    Console.WriteLine("Connesso con successo al database");
                }

                // Elimina il carrello dal database
                var query = "DELETE FROM carrello WHERE IdCarrello = @IdCarrello";
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("IdCarrello", idCarrello);
                    var rowsAffected = await command.ExecuteNonQueryAsync();
                    return rowsAffected > 0; // Restituisce true se il carrello è stato eliminato
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore di connessione: {ex.Message}");
                return false;
            }
        }


        public async Task<bool> AddArticoloById(Guid idCarrello, Guid idArticolo, int quantita)
        {
            try
            {


                // Aggiungi l'articolo al carrello
                if (connection.State != System.Data.ConnectionState.Open)
                {
                    await connection.OpenAsync(); // Apri la connessione solo se non è già aperta
                    Console.WriteLine("Connesso con successo al database");
                }

                var query = "INSERT INTO carrello (IdCarrello, IdArticolo, Quantita) VALUES (@IdCarrello, @IdArticolo, @Quantita)";
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("IdCarrello", idCarrello);
                    command.Parameters.AddWithValue("IdArticolo", idArticolo);
                    command.Parameters.AddWithValue("Quantita", quantita);
                    var rowsAffected = await command.ExecuteNonQueryAsync();
                    return rowsAffected > 0; // Restituisce true se l'articolo è stato aggiunto
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore di connessione: {ex.Message}");
                return false;
            }
        }

        public async Task<List<Articolo>> GetArticoliByCarrello(Guid idCarrello)
        {
            var articoli = new List<Articolo>(); // Lista da restituire
            try
            {
                if (connection.State != System.Data.ConnectionState.Open)
                {
                    await connection.OpenAsync(); // Apri la connessione solo se non è già aperta
                    Console.WriteLine("Connesso con successo al database");
                }

                // Esegui una query per ottenere gli articoli del carrello
                using (var cmd = new NpgsqlCommand(@"
                    SELECT a.Id, a.Nome, a.Descrizione, a.Prezzo
                    FROM articoli a
                    JOIN carrello c ON c.IdArticolo = a.Id
                    WHERE c.IdCarrello = @IdCarrello", connection))
                {
                    // Aggiungi il parametro per evitare SQL Injection
                    cmd.Parameters.AddWithValue("@IdCarrello", idCarrello);

                    // Esegui la query in modalità asincrona
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        // Leggere i dati restituiti dalla query
                        while (await reader.ReadAsync()) // Usa ReadAsync per la lettura asincrona
                        {
                            // Creare un nuovo oggetto Articolo per ogni riga
                            var articolo = new Articolo
                            {
                                Id = reader.GetGuid(reader.GetOrdinal("Id")),
                                Nome = reader.GetString(reader.GetOrdinal("Nome")),
                                Descrizione = reader.IsDBNull(reader.GetOrdinal("Descrizione")) ? null : reader.GetString(reader.GetOrdinal("Descrizione")),
                                Prezzo = reader.GetDouble(reader.GetOrdinal("Prezzo"))
                            };

                            // Aggiungere l'oggetto Articolo alla lista
                            articoli.Add(articolo);
                        }
                    }
                }

                return articoli; // Restituisci la lista di articoli
            }
            catch (Exception ex)
            {
                // Gestire eventuali errori di connessione
                Console.WriteLine($"Errore di connessione: {ex.Message}");
                return null;
            }
        }


        public async Task<bool> EditCarrello(Guid idCarrello, Guid idArticolo, int quantita)
        {
            try
            {
                if (connection.State != System.Data.ConnectionState.Open)
                {
                    await connection.OpenAsync(); // Apri la connessione solo se non è già aperta
                    Console.WriteLine("Connesso con successo al database");
                }

                // Modifica la quantità dell'articolo nel carrello
                var query = "UPDATE carrello SET Quantita = @Quantita WHERE IdCarrello = @IdCarrello AND IdArticolo = @IdArticolo";
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("IdCarrello", idCarrello);
                    command.Parameters.AddWithValue("IdArticolo", idArticolo);
                    command.Parameters.AddWithValue("Quantita", quantita);
                    var rowsAffected = await command.ExecuteNonQueryAsync();
                    return rowsAffected > 0; // Restituisce true se la quantità è stata aggiornata
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore di connessione: {ex.Message}");
                return false;
            }
        }


        public async Task<bool> DeleteArticolo(Guid idCarrello, Guid idArticolo)
        {
            try
            {
                if (connection.State != System.Data.ConnectionState.Open)
                {
                    await connection.OpenAsync(); // Apri la connessione solo se non è già aperta
                    Console.WriteLine("Connesso con successo al database");
                }

                // Elimina l'articolo dal carrello
                var query = "DELETE FROM carrello WHERE IdCarrello = @IdCarrello AND IdArticolo = @IdArticolo";
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("IdCarrello", idCarrello);
                    command.Parameters.AddWithValue("IdArticolo", idArticolo);
                    var rowsAffected = await command.ExecuteNonQueryAsync();
                    return rowsAffected > 0; // Restituisce true se l'articolo è stato eliminato
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore di connessione: {ex.Message}");
                return false;
            }
        }


    }
}
