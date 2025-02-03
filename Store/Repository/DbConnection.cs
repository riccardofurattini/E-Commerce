using Microsoft.EntityFrameworkCore;
using Store.Shared;

namespace Store.Repository
{
    public class DbConnection : IDisposable
    {
        private readonly StoreDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly MagazzinoHttpClient _magazzinoHttpClient;

        public DbConnection(StoreDbContext context, HttpClient httpClient, MagazzinoHttpClient magazzinoHttpClient)
        {
            _context = context;
            _httpClient = httpClient;
            _magazzinoHttpClient = magazzinoHttpClient;
        }

        public async Task SincronizzaArticoli(List<Articolo> items)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                foreach (var item in items)
                {
                    var existingItem = await _context.Articoli.FindAsync(item.Id);
                    if (existingItem == null)
                    {
                        await _context.Articoli.AddAsync(item);
                    }
                    else
                    {
                        existingItem.Nome = item.Nome;
                        existingItem.Descrizione = item.Descrizione;
                        existingItem.Prezzo = item.Prezzo;
                    }
                }

                var idsToKeep = items.Select(i => i.Id).ToList();
                var articlesToDelete = _context.Articoli.Where(a => !idsToKeep.Contains(a.Id));
                _context.Articoli.RemoveRange(articlesToDelete);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore durante la sincronizzazione: {ex.Message}");
                await transaction.RollbackAsync();
                throw;
            }
        }


        public async Task<Articolo> GetArticoloById(Guid id)
        {
            try
            {
                return await _context.Articoli.FindAsync(id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore durante il recupero dell'articolo: {ex.Message}");
                return null;
            }
        }


        public async Task<List<Guid>> GetCarrelli()
        {
            try
            {
                return await _context.Carrelli.Select(c => c.IdCarrello).Distinct().ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore durante il recupero dei carrelli: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> DeleteCarrello(Guid idCarrello)
        {
            try
            {
                var items = _context.Carrelli.Where(c => c.IdCarrello == idCarrello);
                foreach (var item in items)
                {
                    var result = await DeleteArticolo(idCarrello, item.articolo.Id);
                    if (!result)
                    {
                        return false;
                    }
                }
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore durante l'eliminazione del carrello: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> AddArticoloById(Guid idCarrello, Guid idArticolo, int quantita)
        {
            try
            {
                // Verifica la disponibilità dell'articolo nel magazzino tramite la chiamata HTTP



                // Controlla se l'articolo è già nel carrello
                var existingCarrello = await _context.Carrelli
                    .FirstOrDefaultAsync(c => c.IdCarrello == idCarrello && c.articolo.Id == idArticolo);

                if (existingCarrello != null)
                {
                    // Se l'articolo è già nel carrello, aggiorna la quantità
                    existingCarrello.Quantita += quantita;
                }
                else
                {
                    // Altrimenti, aggiungi un nuovo articolo al carrello
                    var articolo = await GetArticoloById(idArticolo);
                    if (articolo == null) return false;

                    var carrello = new Carrello { IdCarrello = idCarrello, articolo = articolo, Quantita = quantita };
                    await _context.Carrelli.AddAsync(carrello);
                }

                var isAvailable = await _magazzinoHttpClient.CheckItemAvailability(idArticolo, quantita);

                if (!isAvailable)
                {
                    // Se la quantità non è disponibile, restituisce errore
                    Console.WriteLine("Errore: Quantità insufficiente nel magazzino.");
                    return false;
                }
                else
                {
                    await _context.SaveChangesAsync();// Salva i cambiamenti nel carrello
                    return true;
                }




            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore durante l'aggiunta dell'articolo al carrello: {ex.Message}");
                return false;
            }
        }


        public async Task<List<Carrello>> GetArticoliByCarrello(Guid idCarrello)
        {
            try
            {
                return await _context.Carrelli
                    .Include(c => c.articolo)
                    .Where(c => c.IdCarrello == idCarrello)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore durante il recupero degli articoli del carrello: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> EditCarrello(Guid idCarrello, Guid idArticolo, int quantita)
        {
            try
            {
                var carrello = await _context.Carrelli.FirstOrDefaultAsync(c => c.IdCarrello == idCarrello && c.articolo.Id == idArticolo);
                if (carrello == null) return false;

                await _magazzinoHttpClient.CheckItemAvailability(idArticolo, -1 * (carrello.Quantita));
                var isAvailable = await _magazzinoHttpClient.CheckItemAvailability(idArticolo, quantita);

                if (!isAvailable)
                {
                    // Se la quantità non è disponibile, restituisce errore
                    Console.WriteLine("Errore: Quantità insufficiente nel magazzino.");
                    await _magazzinoHttpClient.CheckItemAvailability(idArticolo, carrello.Quantita);
                    return false;
                }

                carrello.Quantita = quantita;





                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore durante la modifica del carrello: {ex.Message}");
                return false;
            }
        }


        public async Task<bool> DeleteArticolo(Guid idCarrello, Guid idArticolo)
        {
            try
            {
                var carrello = await _context.Carrelli.FirstOrDefaultAsync(c => c.IdCarrello == idCarrello && c.articolo.Id == idArticolo);
                if (carrello == null) return false;


                await _magazzinoHttpClient.CheckItemAvailability(idArticolo, -1 * (carrello.Quantita));
                _context.Carrelli.Remove(carrello);


                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore durante l'eliminazione dell'articolo: {ex.Message}");
                return false;
            }
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
