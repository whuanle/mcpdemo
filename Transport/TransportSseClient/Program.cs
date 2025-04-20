using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol.Transport;

var defaultOptions = new McpClientOptions
{
    ClientInfo = new() { Name = "IntegrationTestClient", Version = "1.0.0" }
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

var tools = await client.ListToolsAsync();

foreach (var tool in tools)
{
    Console.WriteLine($"Connected to server with tools: {tool.Name}");
}

var result = await client.CallToolAsync("Echo", new Dictionary<string, object?>
{
    { "message","痴者工良"}
});


foreach (var item in result.Content)
{
    Console.WriteLine($"type: {item.Type},text: {item.Text}");
}