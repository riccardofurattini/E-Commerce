using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Store.Shared;

namespace Store.Repository
{
    public class DbConnection : IDisposable
    {
        private readonly StoreDbContext _context;

        public DbConnection(StoreDbContext context)
        {
            _context = context;
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

        public async Task<List<Articolo>> GetArticoli()
        {
            try
            {
                return await _context.Articoli.ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore durante il recupero degli articoli: {ex.Message}");
                return null;
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

        public async Task InitializeDatabase()
        {
            try
            {
                await _context.Database.EnsureCreatedAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore durante l'inizializzazione del database: {ex.Message}");
                throw;
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

        public async Task<bool> EsisteCarrello(Guid idCarrello)
        {
            try
            {
                return await _context.Carrelli.AnyAsync(c => c.IdCarrello == idCarrello);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore durante la verifica del carrello: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteCarrello(Guid idCarrello)
        {
            try
            {
                var items = _context.Carrelli.Where(c => c.IdCarrello == idCarrello);
                _context.Carrelli.RemoveRange(items);
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

                await _context.SaveChangesAsync();
                return true;
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

        public async Task<bool> EsisteArticoloNelCarrello(Guid idCarrello, Guid idArticolo)
        {
            try
            {
                return await _context.Carrelli.AnyAsync(c => c.IdCarrello == idCarrello && c.articolo.Id == idArticolo);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Errore durante la verifica dell'articolo nel carrello: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteArticolo(Guid idCarrello, Guid idArticolo)
        {
            try
            {
                var carrello = await _context.Carrelli.FirstOrDefaultAsync(c => c.IdCarrello == idCarrello && c.articolo.Id == idArticolo);
                if (carrello == null) return false;

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
