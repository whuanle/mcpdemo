using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging.Abstractions;
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

var resourceTemplates = await client.ListResourceTemplatesAsync();
var resources = await client.ListResourcesAsync();

foreach (var template in resourceTemplates)
{
    Console.WriteLine($"Connected to server with resource templates: {template.Name}");
}

foreach (var resource in resources)
{
    Console.WriteLine($"Connected to server with resources: {resource.Name}");
}

var readmeResource = await client.ReadResourceAsync(resources.First().Uri);
var textContent = readmeResource.Contents.First() as TextResourceContents;
Console.WriteLine(textContent.Text);

client.RegisterNotificationHandler("notifications/resource/updated", async (message, ctx) =>
{
    await Task.CompletedTask;

});
await client.SubscribeToResourceAsync("test://static/resource/README.txt");

Console.ReadKey();