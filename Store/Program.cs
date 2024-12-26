using Store.ClientHttp;


var builder = WebApplication.CreateBuilder(args);

// Aggiungi i servizi necessari per l'applicazione
builder.Services.AddHttpClient();
builder.Services.AddSingleton<RichiestaCatalogo>(); // Aggiungi RichiestaCatalogo come singleton
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

//app.UseHttpsRedirection();
app.UseSwagger();
app.UseSwaggerUI();
app.UseRouting();
app.MapControllers();

app.Run();
