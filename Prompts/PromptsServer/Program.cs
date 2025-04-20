HashSet<string> subscriptions = [];

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddMcpServer()
    .WithHttpTransport()
    .WithStdioServerTransport()
    .WithPromptsFromAssembly();

builder.Services.AddControllers();

var app = builder.Build();

app.MapMcp();

app.MapControllers();
app.Run("http://0.0.0.0:5000");
