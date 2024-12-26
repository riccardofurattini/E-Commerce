using Store.ClientHttp;
using Store.Repository;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Aggiungi DbConnection al contenitore DI
builder.Services.AddScoped<DbConnection>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configurazione di HttpClient per Magazzino
builder.Services.AddHttpClient("RichiestaCatalogo", client =>
{
    client.BaseAddress = new Uri("http://magazzino:8080"); // URL del servizio Magazzino
});

// Registra il servizio MagazzinoClient
builder.Services.AddTransient<RichiestaCatalogo>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
