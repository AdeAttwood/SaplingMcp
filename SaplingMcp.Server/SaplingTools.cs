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
    public string GetCurrentStack(string message = "")
    {
        return TokenEfficientParser.FormatCommits(_sapling.Stack().ToList());
    }

    [McpServerTool, Description("Gets all the public commits")]
    public List<Commit> GetPublicCommits(string message = "")
    {
        return _sapling.Public().ToList();
    }

    [McpServerTool, Description("Creates a new commit with the specified message and files")]
    public string CreateCommit(
        [Description("The commit message to use")] string message,
        [Description("List of files to include in the commit")] List<string> files)
    {
        try
        {
            var commit = _sapling.CreateCommit(message, files);
            if (commit == null)
            {
                throw new InvalidOperationException("Failed to create commit. Please check your repository state.");
            }

            return TokenEfficientParser.FormatCommit(commit);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error creating commit: {ex.Message}");
        }
    }

    [McpServerTool, Description("Amends the current commit with the specified files and optionally updates the commit message")]
    public string AmendCommit(
        [Description("List of files to include in the amendment")] List<string> files,
        [Description("The new commit message (optional). If not provided, the existing message is kept")] string? message = null)
    {
        try
        {
            var commit = _sapling.AmendCommit(files, message);
            if (commit == null)
            {
                throw new InvalidOperationException("Failed to amend commit. Please check your repository state.");
            }

            return TokenEfficientParser.FormatCommit(commit);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error amending commit: {ex.Message}");
        }
    }

    [McpServerTool, Description("Gets the status of files in the working directory")]
    public List<FileStatus> GetStatus(string message = "")
    {
        try
        {
            return _sapling.GetStatus().ToList();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error getting status: {ex.Message}");
        }
    }

    [McpServerTool, Description("Submits the current stack of commits to create or update pull requests")]
    public string SubmitStack(string message = "")
    {
        try
        {
            var submittedCommits = _sapling.SubmitStack().ToList();
            if (submittedCommits.Count == 0)
            {
                throw new InvalidOperationException("No commits were submitted. Please check your repository state.");
            }

            return TokenEfficientParser.FormatCommits(submittedCommits);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error submitting stack: {ex.Message}");
        }
    }
}
