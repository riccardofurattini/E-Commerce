using Microsoft.EntityFrameworkCore;
using Store.Shared;

namespace Store.Repository
{
    public class StoreDbContext : DbContext
    {
        public StoreDbContext(DbContextOptions<StoreDbContext> options) : base(options) { }

        public DbSet<Articolo> Articoli { get; set; }
        public DbSet<Carrello> Carrelli { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configurazione della tabella articoli
            modelBuilder.Entity<Articolo>(entity =>
            {
                entity.HasKey(a => a.Id);
                entity.Property(a => a.Nome).IsRequired().HasMaxLength(100);
                entity.Property(a => a.Descrizione).HasMaxLength(255);
                entity.Property(a => a.Prezzo).HasColumnType("decimal(10,2)");
            });

            // Configurazione della tabella carrello
            modelBuilder.Entity<Carrello>(entity =>
            {
                entity.HasKey(c => c.IdCarrello);  // Imposta la chiave primaria come IdCarrello
                entity.Property(c => c.Quantita).IsRequired();

                // La relazione tra Carrello e Articolo
                entity.HasOne(c => c.articolo)
                      .WithMany()  // Articolo non ha una navigazione inversa
                      .HasForeignKey(c => c.ArticoloId)
                      .OnDelete(DeleteBehavior.Cascade);  // Comportamento alla cancellazione
            });
        }
    }
}
