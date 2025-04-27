using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using SaplingMcp.Server;
using SaplingMcp.Server.Services;
using System.ComponentModel;

var builder = Host.CreateApplicationBuilder(args);
builder.Logging.AddConsole(consoleLogOptions =>
{
    // Configure all logs to go to stderr
    consoleLogOptions.LogToStandardErrorThreshold = LogLevel.Trace;
});

builder.Services.AddSingleton<Sapling>();

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithTools<SaplingTools>();


await builder.Build().RunAsync();
