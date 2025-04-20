using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel;
using ModelContextProtocol;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol.Transport;
using ModelContextProtocol.Protocol.Types;
using System.Text;

var defaultOptions = new McpClientOptions
{
    ClientInfo = new() { Name = "ResourceClient", Version = "1.0.0" }
};

var defaultConfig = new SseClientTransportOptions
{
    Endpoint = new Uri($"http://localhost:5000/sse"),
    Name = "Everything",
};

// Create client and run tests
await using var client = await McpClientFactory.CreateAsync(
    new SseClientTransport(defaultConfig),
    defaultOptions,
    loggerFactory: NullLoggerFactory.Instance);

var prompts = await client.ListPromptsAsync();
foreach (var item in prompts)
{
    Console.WriteLine($"prompt name :{item.Name}");
}

Console.ReadKey();