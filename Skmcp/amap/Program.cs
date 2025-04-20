#pragma warning disable SKEXP0001 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol.Transport;

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddJsonFile("appsettings.Development.json")
    .Build();

using ILoggerFactory factory = LoggerFactory.Create(builder => builder.AddConsole());

// 第一步：创建 mcp 客户端
var defaultOptions = new McpClientOptions
{
    ClientInfo = new() { Name = "地图规划", Version = "1.0.0" }
};

var defaultConfig = new SseClientTransportOptions
{
    Endpoint = new Uri(configuration["McpServers:amap-amap-sse:url"]!),
    Name = "amap-amap-sse",
};

await using var client = await McpClientFactory.CreateAsync(
    new SseClientTransport(defaultConfig),
    defaultOptions,
    loggerFactory: factory);

var tools = await client.ListToolsAsync();

foreach (var tool in tools)
{
    Console.WriteLine($"Connected to server with tools: {tool.Name}");
}

// 第二步：连接 AI 模型
var aiModel = configuration.GetSection("AIModel");
var builder = Kernel.CreateBuilder()
    .AddAzureOpenAIChatCompletion(
    deploymentName: aiModel["ModelId"],
    endpoint: aiModel["Endpoint"],
    apiKey: aiModel["Key"]);

builder.Services.AddLogging(s =>
{
    s.AddConsole();
});

Kernel kernel = builder.Build();

// 这里将 mcp 转换为 functaion call
kernel.Plugins.AddFromFunctions("amap", tools.Select(aiFunction => aiFunction.AsKernelFunction()));

var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
{
    Temperature = 0,
    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(options: new() { RetainArgumentTypes = true })
};

// 第三步：对话交互
var history = new ChatHistory();

string? userInput;
do
{
    Console.Write("用户提问 > ");
    userInput = Console.ReadLine();

    history.AddUserMessage(userInput!);

    var result = await chatCompletionService.GetChatMessageContentAsync(
        history,
        executionSettings: openAIPromptExecutionSettings,
        kernel: kernel);

    Console.WriteLine("AI 回答 > " + result);

    history.AddMessage(result.Role, result.Content ?? string.Empty);
} while (userInput is not null);

