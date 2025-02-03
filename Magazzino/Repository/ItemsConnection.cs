using Magazzino.ClientHttp;
using Magazzino.Shared;
using Microsoft.EntityFrameworkCore;

namespace Magazzino.Repository
{
    public class ItemsConnection : IDisposable
    {
        private readonly MagazzinoContext _context;
        private readonly MagazzinoProducer _producer;

        public ItemsConnection(MagazzinoContext context, MagazzinoProducer producer)
        {
            _context = context;
            _producer = producer;
        }

        // Metodo per recuperare tutti gli articoli
        public async Task<List<Item>> GetItemsAsync()
        {
            try
            {
                var items = await _context.Items.ToListAsync();
                await _producer.SendItemsListAsync(); // Invoca il producer dopo aver recuperato gli articoli
                return items;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore: {ex.Message}");
                return null;
            }
        }

        // Metodo per recuperare un articolo per ID
        public async Task<Item> GetItemByIdAsync(Guid id)
        {
            try
            {
                var item = await _context.Items.FindAsync(id);
                await _producer.SendItemsListAsync(); // Invoca il producer dopo aver recuperato l'articolo
                return item;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore: {ex.Message}");
                return null;
            }
        }

        // Metodo per aggiungere un articolo
        public async Task AddItemAsync(Item item)
        {
            try
            {
                await _context.AddAsync(item);
                await _context.SaveChangesAsync(); // Salva l'articolo nel database
                await _producer.SendItemsListAsync(); // Invoca il producer dopo aver aggiunto l'articolo
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore durante l'inserimento dell'articolo: {ex.Message}");
                throw;
            }
        }

        // Metodo per aggiornare un articolo
        public async Task UpdateItemAsync(Item item)
        {
            try
            {
                _context.Items.Update(item); // Aggiorna l'articolo nel database
                await _context.SaveChangesAsync(); // Salva le modifiche
                await _producer.SendItemsListAsync(); // Invoca il producer dopo l'aggiornamento
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore durante l'aggiornamento dell'articolo: {ex.Message}");
                throw;
            }
        }

        // Metodo per eliminare un articolo
        public async Task DeleteItemAsync(Guid id)
        {
            try
            {
                var item = await _context.Items.FindAsync(id);
                if (item != null)
                {
                    _context.Items.Remove(item); // Rimuove l'articolo dal database
                    await _context.SaveChangesAsync(); // Salva le modifiche
                    await _producer.SendItemsListAsync(); // Invoca il producer dopo la rimozione
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore durante la cancellazione dell'articolo: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> ControlloQuantitaAsync(Guid id, int quantita)
        {
            try
            {
                if (quantita >= 0)
                {
                    // Recupera l'articolo dal database
                    var item = await _context.Items.FindAsync(id);

                    if (item == null)
                    {
                        Console.WriteLine($"Articolo con ID {id} non trovato.");
                        return false;
                    }

                    // Controlla se la quantità disponibile è sufficiente
                    if (item.Quantita >= quantita)
                    {
                        // Sottrai la quantità richiesta
                        item.Quantita -= quantita;
                        await _context.SaveChangesAsync();
                        await _producer.SendItemsListAsync();
                        return true;
                    }
                    else
                    {
                        // La quantità richiesta è maggiore della disponibilità
                        Console.WriteLine($"Quantità insufficiente per l'articolo {item.Nome}. Disponibile: {item.Quantita}, Richiesta: {quantita}");
                        return false;
                    }
                }
                else//in caso di cancellazione dell'articolo dal carrello
                {
                    // Recupera l'articolo dal database
                    var item = await _context.Items.FindAsync(id);

                    if (item == null)
                    {
                        Console.WriteLine($"Articolo con ID {id} non trovato.");
                        return false;
                    }
                    quantita *= -1;
                    // aggiungi la quantità richiesta
                    item.Quantita += quantita;
                    await _context.SaveChangesAsync();
                    await _producer.SendItemsListAsync();
                    return true;

                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore durante l'aggiornamento della quantità per l'articolo {id}: {ex.Message}");
                return false;
            }
        }


        // Dispose per liberare le risorse
        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}
