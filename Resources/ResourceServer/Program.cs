using Microsoft.Extensions.AI;
using ModelContextProtocol.Protocol.Types;
using ModelContextProtocol.Server;
using ResourceServer;
using System;
using System.Text;

HashSet<string> subscriptions = [];

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddMcpServer()
    .WithHttpTransport()
    .WithStdioServerTransport()
    .WithToolsFromAssembly()
    // 通过模板方式提供一类 Resource
        .WithListResourceTemplatesHandler(async (ctx, ct) =>
        {
            return new ListResourceTemplatesResult
            {
                ResourceTemplates =
                [
                    new ResourceTemplate { Name = "Static Resource", Description = "A static resource with a numeric ID", UriTemplate = "test://static/resource/{id}" }
                ]
            };
        })
        // 固定 Resource Uri
        .WithListResourcesHandler(async (ctx, ct) =>
        {
            await Task.CompletedTask;
            var readmeResource = new Resource
            {
                Uri = "test://static/resource/README.txt",
                Name = "Resource README.txt",
                MimeType = "application/octet-stream",
                Description = Convert.ToBase64String(Encoding.UTF8.GetBytes("这是一个必读文件"))
            };

            return new ListResourcesResult
            {
                Resources = new List<Resource>
                {
                    readmeResource
                }
            };
        })
    .WithReadResourceHandler(async (ctx, ct) =>
    {
        var uri = ctx.Params?.Uri;

        if (uri is null || !uri.StartsWith("test://static/resource/"))
        {
            throw new NotSupportedException($"Unknown resource: {uri}");
        }

        if(uri == "test://static/resource/README.txt")
        {
            var readmeResource = new Resource
            {
                Uri = "test://static/resource/README.txt",
                Name = "Resource README.txt",
                MimeType = "application/octet-stream",
                Description = "这是一个必读文件"
            };
            return new ReadResourceResult
            {
                Contents = [new TextResourceContents
                {
                    Text = File.ReadAllText("README.txt"),
                    MimeType = readmeResource.MimeType,
                    Uri = readmeResource.Uri,
                }]
            };
        }

        int index = int.Parse(uri["test://static/resource/".Length..]) - 1;

        if (index < 0 || index >= ResourceGenerator.Resources.Count)
        {
            throw new NotSupportedException($"Unknown resource: {uri}");
        }

        var resource = ResourceGenerator.Resources[index];
        return new ReadResourceResult
        {
            Contents = [new TextResourceContents
                {
                    Text = resource.Description!,
                    MimeType = resource.MimeType,
                    Uri = resource.Uri,
                }]
        };
    })
    .WithSubscribeToResourcesHandler(async (ctx, ct) =>
    {
        var uri = ctx.Params?.Uri;

        if (uri is not null)
        {
            subscriptions.Add(uri);
        }

        return new EmptyResult();
    })
    .WithUnsubscribeFromResourcesHandler(async (ctx, ct) =>
    {
        var uri = ctx.Params?.Uri;
        if (uri is not null)
        {
            subscriptions.Remove(uri);
        }
        return new EmptyResult();
    });

builder.Services.AddControllers();

var app = builder.Build();

app.MapMcp();

app.MapControllers();
app.Run("http://0.0.0.0:5000");
