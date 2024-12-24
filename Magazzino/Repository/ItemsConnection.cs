using Magazzino.Shared;
using Magazzino.WebApi;
using Microsoft.Data.SqlClient;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Magazzino.Repository
{
    public class ItemsConnection : IDisposable
    {
        private NpgsqlConnection connection;
        private readonly string connectionString = "Host=magazzino-db;Username=magazzino_user;Password=p4ssw0rD;Database=magazzino_db;Port=5432";

        //"Host=magazzino-db;Username=magazzino_user;Password=p4ssw0rD;Database=magazzino_db;Port=5432";


        // Costruttore
        public ItemsConnection()
        {
            connection = new NpgsqlConnection(connectionString);
        }

        // Metodo per recuperare gli articoli dal database
        public async Task<List<Item>> GetItemsAsync()
        {
            var items = new List<Item>(); // Lista da restituire
            try
            {
                if (connection.State != System.Data.ConnectionState.Open)
                {
                    await connection.OpenAsync(); // Apri la connessione solo se non è già aperta
                    Console.WriteLine("Connesso con successo al database");
                }
                

                // Eseguire una query per selezionare tutti gli articoli
                using (var cmd = new NpgsqlCommand("SELECT Id, Nome, Descrizione, Prezzo, Quantita FROM items", connection))
                {
                    // Esegui la query in modalità asincrona
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        // Leggere i dati restituiti dalla query
                        while (await reader.ReadAsync()) // Usa ReadAsync per la lettura asincrona
                        {
                            // Creare un nuovo oggetto Item per ogni riga
                            var item = new Item
                            {
                                Id = reader.GetGuid(reader.GetOrdinal("Id")),
                                Nome = reader.GetString(reader.GetOrdinal("Nome")),
                                Descrizione = reader.GetString(reader.GetOrdinal("Descrizione")),
                                Prezzo = reader.GetDouble(reader.GetOrdinal("Prezzo")),
                                Quantita = reader.GetInt32(reader.GetOrdinal("Quantita"))
                            };

                            // Aggiungere l'oggetto Item alla lista
                            items.Add(item);
                        }
                    }
                }

                return items; // Restituisci la lista di oggetti Item
            }
            catch (Exception ex)
            {
                // Gestire eventuali errori di connessione
                Console.WriteLine($"Errore di connessione: {ex.Message}");
                return null; // Puoi anche restituire una lista vuota, se preferisci
            }
        }

        public async Task<Item> GetItemById(Guid id)
        {
            Item item = null; // Inizializza item a null in caso di errore
            try
            {
                if (connection.State != System.Data.ConnectionState.Open)
                {
                    await connection.OpenAsync(); // Apri la connessione solo se non è già aperta
                    Console.WriteLine("Connesso con successo al database");
                }
                

                // Eseguire una query per selezionare un articolo in base all'ID
                using (var cmd = new NpgsqlCommand("SELECT Id, Nome, Descrizione, Prezzo, Quantita FROM items WHERE Id = @Id", connection))
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
                            item = new Item
                            {
                                Id = reader.GetGuid(reader.GetOrdinal("Id")),
                                Nome = reader.GetString(reader.GetOrdinal("Nome")),
                                Descrizione = reader.GetString(reader.GetOrdinal("Descrizione")),
                                Prezzo = reader.GetDouble(reader.GetOrdinal("Prezzo")),
                                Quantita = reader.GetInt32(reader.GetOrdinal("Quantita"))
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

        public async Task AddItemAsync(Item item)
        {
            try
            {
                if (connection.State != System.Data.ConnectionState.Open)
                {
                    await connection.OpenAsync(); // Apri la connessione solo se non è già aperta
                    Console.WriteLine("Connesso con successo al database");
                }
                using (var cmd = new NpgsqlCommand("INSERT INTO items (Id, Nome, Descrizione, Prezzo, Quantita) VALUES (@Id, @Nome, @Descrizione, @Prezzo, @Quantita)", connection))
                {
                    // Aggiungi i parametri
                    cmd.Parameters.AddWithValue("@Id", item.Id);
                    cmd.Parameters.AddWithValue("@Nome", item.Nome);
                    cmd.Parameters.AddWithValue("@Descrizione", item.Descrizione);
                    cmd.Parameters.AddWithValue("@Prezzo", item.Prezzo);
                    cmd.Parameters.AddWithValue("@Quantita", item.Quantita);

                    // Esegui la query asincrona
                    await cmd.ExecuteNonQueryAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore durante l'inserimento dell'articolo: {ex.Message}");
                throw; // Rilancia l'errore per gestirlo nel metodo chiamante
            }
        }

        public async Task UpdateItemAsync(Item item)
        {
            try
            {
                if (connection.State != System.Data.ConnectionState.Open)
                {
                    await connection.OpenAsync(); // Apri la connessione solo se non è già aperta
                    Console.WriteLine("Connesso con successo al database");
                }

                // Query per aggiornare l'item
                using (var cmd = new NpgsqlCommand("UPDATE items SET Nome = @Nome, Descrizione = @Descrizione, Prezzo = @Prezzo, Quantita = @Quantita WHERE Id = @Id", connection))
                {
                    // Aggiungi i parametri alla query
                    cmd.Parameters.AddWithValue("@Nome", item.Nome);
                    cmd.Parameters.AddWithValue("@Descrizione", item.Descrizione);
                    cmd.Parameters.AddWithValue("@Prezzo", item.Prezzo);
                    cmd.Parameters.AddWithValue("@Quantita", item.Quantita);
                    cmd.Parameters.AddWithValue("@Id", item.Id);

                    // Esegui la query in modo asincrono
                    await cmd.ExecuteNonQueryAsync();
                }
            }
            catch (Exception ex)
            {
                // Gestione dell'errore
                Console.WriteLine($"Errore durante l'aggiornamento dell'articolo: {ex.Message}");
                throw; // Rilancia l'errore per la gestione successiva
            }
        }
        public async Task DeleteItemAsync(Guid id)
        {
            try
            {
                // Assicurati che la connessione sia aperta
                if (connection.State != System.Data.ConnectionState.Open)
                {
                    await connection.OpenAsync(); // Apri la connessione solo se non è già aperta
                    Console.WriteLine("Connesso con successo al database");
                }

                // Query per rimuovere l'item dal database
                using (var cmd = new NpgsqlCommand("DELETE FROM items WHERE Id = @Id", connection))
                {
                    // Aggiungi il parametro alla query
                    cmd.Parameters.AddWithValue("@Id", id);

                    // Esegui la query in modo asincrono
                    await cmd.ExecuteNonQueryAsync();
                }
            }
            catch (Exception ex)
            {
                // Gestione dell'errore
                Console.WriteLine($"Errore durante la cancellazione dell'articolo: {ex.Message}");
                throw; // Rilancia l'errore per la gestione successiva
            }
        }


        // Dispose per chiudere correttamente la connessione
        public void Dispose()
        {
            connection?.Dispose();
        }
            
        
    }
}
