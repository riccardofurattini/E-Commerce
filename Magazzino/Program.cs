using Magazzino.Repository;
using Magazzino.ClientHttp;
using Magazzino.Shared;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

var builder = WebApplication.CreateBuilder(args);

// servizi necessari per l'applicazione
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Registra ItemsConnection come singleton
builder.Services.AddSingleton<ItemsConnection>();

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
        var dbConnection = scope.ServiceProvider.GetRequiredService<ItemsConnection>();
        dbConnection.InitializeDatabase();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Errore: {ex.Message}");
    }


    try
    {
        MagazzinoProducer magazzino = new MagazzinoProducer();
        await magazzino.SendItemsListAsync();
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
