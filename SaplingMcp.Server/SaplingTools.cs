using System.ComponentModel;
using ModelContextProtocol.Server;
using SaplingMcp.Server.Services;

namespace SaplingMcp.Server;


[McpServerToolType]
public static class EchoTools
{
  [McpServerTool, Description("Echoes the message back to the client.")]
  public static string Echo(string message) => $"Hello from sapling scm mcp: {message}";

  [McpServerTool, Description("Echoes in reverse the message sent by the client.")]
  public static string ReverseEcho(string message) => new string(message.Reverse().ToArray());
}

[McpServerToolType]
public class SaplingTools
{

  private readonly Sapling _sapling;

  public SaplingTools(Sapling sapling)
  {
    _sapling = sapling;
  }

  [McpServerTool, Description("Gets all the commits in the current stack")]
  public List<Commit> GetCurrentStack(string message = "")
  {
      return _sapling.Stack().ToList();
  }

  [McpServerTool, Description("Gets all the public commits")]
  public List<Commit> GetPublicCommits(string message = "")
  {
      return _sapling.Public().ToList();
  }
}
