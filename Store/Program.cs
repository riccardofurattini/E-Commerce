using Microsoft.EntityFrameworkCore;
using Store.ClientHttp;
using Store.Repository;

var builder = WebApplication.CreateBuilder(args);

// Aggiungi i servizi necessari per l'applicazione
builder.Services.AddHttpClient();
builder.Services.AddScoped<DbConnection>(); // Cambia DbConnection a Scoped
builder.Services.AddHostedService<StoreConsumer>(); // Registra il servizio hostato
builder.Services.AddDbContext<StoreDbContext>(options =>
    options.UseNpgsql("Host=store-db;Username=store_user;Password=p4ssw0rD;Database=store_db;Port=5432"));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configura il middleware di gestione delle richieste HTTP
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}


// Inizializza il database
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<StoreDbContext>();
    try
    {
        dbContext.Database.Migrate(); // Applica automaticamente le migrazioni
        Console.WriteLine("Migrazioni applicate con successo.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Errore nell'applicare le migrazioni: {ex.Message}");
    }
}


// Configura i middleware dell'applicazione
app.UseSwagger();
app.UseSwaggerUI();
app.UseRouting();
app.MapControllers();

app.Run();
