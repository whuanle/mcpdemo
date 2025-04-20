using ModelContextProtocol.Server;
using System.ComponentModel;

namespace TransportSseServer.Tools;

[McpServerToolType]
public sealed class MyTool
{
    [McpServerTool, Description("Echoes the input back to the client.")]
    public static string Echo(MyService myService, string message)
    {
        return myService.Echo(message);
    }
}

public class MyService
{
    public string Echo(string message)
    {
        return "hello " + message;
    }
}