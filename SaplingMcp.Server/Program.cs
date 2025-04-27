using System.ComponentModel;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using ModelContextProtocol.Server;

using SaplingMcp.Server;
using SaplingMcp.Server.Services;

var builder = Host.CreateApplicationBuilder(args);
builder.Logging.AddConsole(consoleLogOptions =>
{
    // Configure all logs to go to stderr
    consoleLogOptions.LogToStandardErrorThreshold = LogLevel.Trace;
});

builder.Services.AddSingleton<Sapling>();
builder.Services.AddSingleton<GitHub>(provider =>
{
    // Use the same directory as the Sapling service
    var repoDir = Directory.GetCurrentDirectory();
    return new GitHub(repoDir);
});

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithTools<SaplingTools>()
    .WithTools<GitHubTools>();

await builder.Build().RunAsync();
