using Store.ClientHttp;
using Store.Repository;

var builder = WebApplication.CreateBuilder(args);

// Aggiungi i servizi necessari per l'applicazione
builder.Services.AddHttpClient();
builder.Services.AddSingleton<DbConnection>(); // Registra DbConnection come singleton
builder.Services.AddHostedService<StoreConsumer>(); // Registra il servizio hostato

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
    var dbConnection = scope.ServiceProvider.GetRequiredService<DbConnection>();
    dbConnection.InitializeDatabase();
}

// Configura i middleware dell'applicazione
app.UseSwagger();
app.UseSwaggerUI();
app.UseRouting();
app.MapControllers();

app.Run();
