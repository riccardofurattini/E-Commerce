using Microsoft.EntityFrameworkCore;
using Magazzino.Shared;

namespace Magazzino.Repository
{
    public class MagazzinoContext : DbContext
    {
        public DbSet<Item> Items { get; set; }

        public MagazzinoContext(DbContextOptions<MagazzinoContext> options) : base(options) { }


        public void Popola()
        {
            if (!Items.Any()) // Verifica se la tabella è vuota
            {
                // Dati di esempio da inserire
                Items.AddRange(
                    new Item
                    {
                        Id = Guid.Parse("d4d1f8b7-8f2f-4c42-bb4d-f0d028cf9b3f"),
                        Nome = "Mele",
                        Descrizione = "Frutta",
                        Prezzo = 0.99,
                        Quantita = 500
                    },
                    new Item
                    {
                        Id = Guid.Parse("cb1a7c1e-559a-4de7-9a42-b388db30a4a1"),
                        Nome = "Pere",
                        Descrizione = "Frutta",
                        Prezzo = 1.50,
                        Quantita = 400
                    },
                    new Item
                    {
                        Id = Guid.Parse("e4c87b13-b19b-4ff2-95b8-d8306f16c0a7"),
                        Nome = "Banane",
                        Descrizione = "Frutta",
                        Prezzo = 2.50,
                        Quantita = 700
                    }
                );

                SaveChanges(); // Salva i dati nel database
            }
        }
    }
}
