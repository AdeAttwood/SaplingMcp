using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

using Microsoft.Extensions.Logging;

namespace SaplingMcp.Server.Services;

/// <summary>
/// Represents a commit in the Sapling version control system.
/// </summary>
public class Commit
{
    /// <summary>
    /// Gets or sets the unique identifier (hash) of the commit.
    /// </summary>
    [JsonPropertyName("node")]
    public required string Node { get; set; }

    /// <summary>
    /// Gets or sets the commit message description.
    /// </summary>
    [JsonPropertyName("desc")]
    public required string Description { get; set; }

    /// <summary>
    /// Gets or sets the author of the commit.
    /// </summary>
    [JsonPropertyName("author")]
    public required string Author { get; set; }

    /// <summary>
    /// Gets or sets the list of files added in this commit.
    /// </summary>
    [JsonPropertyName("file_adds")]
    public required List<string> FilesAdded { get; set; }

    /// <summary>
    /// Gets or sets the list of files removed in this commit.
    /// </summary>
    [JsonPropertyName("file_dels")]
    public required List<string> FilesRemoved { get; set; }

    /// <summary>
    /// Gets or sets the list of files modified in this commit.
    /// </summary>
    [JsonPropertyName("file_mods")]
    public required List<string> FilesModified { get; set; }

    /// <summary>
    /// Gets or sets the phase of the commit (e.g., public, draft).
    /// </summary>
    [JsonPropertyName("phase")]
    public required string Phase { get; set; }
}

/// <summary>
/// Service for interacting with the Sapling version control system.
/// </summary>
public class Sapling
{
    private readonly string _repoDir;

    /// <summary>
    /// Initializes a new instance of the <see cref="Sapling"/> class with a specified repository directory.
    /// </summary>
    /// <param name="repoDir">The directory path of the Sapling repository.</param>
    public Sapling(string repoDir)
    {
        _repoDir = repoDir;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Sapling"/> class using the current directory as the repository.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public Sapling(ILogger<Sapling> logger)
    {
        _repoDir = Directory.GetCurrentDirectory();
    }

    /// <summary>
    /// Gets the current stack of commits from bottom to top.
    /// </summary>
    /// <returns>A list of commits in the current stack.</returns>
    public IList<Commit> Stack() => this.Commits("bottom::top");

    /// <summary>
    /// Gets all public commits in the repository.
    /// </summary>
    /// <returns>A list of public commits.</returns>
    public IList<Commit> Public() => this.Commits("public()");

    /// <summary>
    /// Gets commits matching the specified revision specifier.
    /// </summary>
    /// <param name="rev">The revision specifier to match commits.</param>
    /// <returns>A list of matching commits.</returns>
    public IList<Commit> Commits(string rev)
    {
        var arguments = new List<string>
    {
      "log",
      "-r",
      rev,
      "-T{dict(node, author, desc, file_adds, file_dels, file_mods, phase)|json}\\n"
    };

        var output = this.RunCommand(arguments);

        var commits = output
            .Split('\n')
            .Where(reference => !string.IsNullOrWhiteSpace(reference))
            .Select(reference =>
            {
                return JsonSerializer.Deserialize<Commit>(reference)
                ?? throw new InvalidOperationException($"Failed to deserialize commit: {reference}");
            })
            .ToList();

        return commits;
    }

    /// <summary>
    /// Runs a Sapling command with the specified arguments.
    /// </summary>
    /// <param name="arguments">The command arguments to pass to Sapling.</param>
    /// <returns>The standard output of the command.</returns>
    private string RunCommand(IList<string> arguments)
    {
        var process = new Process();

        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.FileName = "sl";
        process.StartInfo.WorkingDirectory = _repoDir;

        // Use ProcessStartInfo.ArgumentList instead of Arguments to avoid shell injection
        foreach (var arg in arguments)
        {
            process.StartInfo.ArgumentList.Add(arg);
        }

        Console.Error.WriteLine($"Command: sl {string.Join(" ", arguments)}");

        process.Start();

        string output = process.StandardOutput.ReadToEnd();
        string error = process.StandardError.ReadToEnd();

        process.WaitForExit();

        Console.Error.WriteLine($"The out is {output}");
        Console.Error.WriteLine($"The err is {error}");

        return output;
    }
}
