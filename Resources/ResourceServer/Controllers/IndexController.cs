using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Mvc;
using ModelContextProtocol;
using ModelContextProtocol.Server;

namespace ResourceServer.Controllers;

[ApiController]
[Route("[controller]")]
public class IndexController : ControllerBase
{
    private readonly IMcpServer _mcpServer;

    public IndexController(IMcpServer mcpServer)
    {
        _mcpServer = mcpServer;
    }

    [HttpGet("notification")]
    public async Task<string> NotificationReadme(string message)
    {
        await _mcpServer.SendNotificationAsync("notifications/resource/updated",
            new
            {
                Uri = "test://static/resource/README.txt",
            });

        return "已通知";
    }
}
