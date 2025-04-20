using TransportSseServer.Tools;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly();

builder.Services.AddScoped<MyService>();

var app = builder.Build();

app.MapMcp();

app.Run("http://0.0.0.0:5000");
