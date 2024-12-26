using Magazzino.ClientHttp;
using Magazzino.Repository;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<CatalogoConsumer>();
builder.Services.AddHostedService<KafkaConsumerService>(); // Aggiungi il servizio Kafka

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

}

ItemsConnection itemsConnection = new ItemsConnection();
itemsConnection.InitializeDatabase();

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
