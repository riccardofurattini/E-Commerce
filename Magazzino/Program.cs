using Magazzino.Repository;
using Magazzino.ClientHttp;
using Magazzino.Shared;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Aggiungi i servizi necessari per l'applicazione
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Registra MagazzinoContext come scoped, per lavorare con Entity Framework
builder.Services.AddDbContext<MagazzinoContext>(options =>
    options.UseNpgsql("Host=magazzino-db;Username=magazzino_user;Password=p4ssw0rD;Database=magazzino_db;Port=5432"));

// Registra ItemsConnection come transient, se necessario, per altre operazioni
builder.Services.AddTransient<ItemsConnection>();
builder.Services.AddTransient<MagazzinoProducer>();

var app = builder.Build();

// Configura il middleware di gestione delle richieste HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Inizializza il database e crea il topic Kafka (se necessario)
using (var scope = app.Services.CreateScope())
{
    try
    {
        // Usa il contesto per inizializzare il database
        var dbContext = scope.ServiceProvider.GetRequiredService<MagazzinoContext>();
        dbContext.Database.Migrate(); // Assicurati che le migrazioni vengano applicate
        dbContext.Popola();
        Console.WriteLine("Database inizializzato e popolato con successo.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Errore durante l'inizializzazione del database: {ex.Message}");
    }

    try
    {
        // Inizializza il producer per inviare gli articoli al Kafka
        var magazzinoProducer = scope.ServiceProvider.GetRequiredService<MagazzinoProducer>();
        await magazzinoProducer.SendItemsListAsync();
        Console.WriteLine("store_db aggiornato con successo.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Errore durante l'aggiornamento di store_db: {ex.Message}");
    }
}

// Configura i middleware dell'applicazione
app.UseAuthorization();
app.MapControllers();
app.Run();
