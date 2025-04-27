using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

using Microsoft.Extensions.Logging;

namespace SaplingMcp.Server.Services;

/// <summary>
/// Represents a file status in the Sapling version control system.
/// </summary>
public class FileStatus
{
    /// <summary>
    /// Gets or sets the path of the file.
    /// </summary>
    [JsonPropertyName("path")]
    public required string Path { get; set; }

    /// <summary>
    /// Gets or sets the status code of the file.
    /// </summary>
    [JsonPropertyName("status")]
    public required string StatusCode { get; set; }

    /// <summary>
    /// Gets the human-readable status name based on the status code.
    /// </summary>
    [JsonIgnore]
    public string StatusName => GetStatusName(StatusCode);

    /// <summary>
    /// Converts a status code to a human-readable status name.
    /// </summary>
    /// <param name="code">The status code to convert.</param>
    /// <returns>A human-readable status name.</returns>
    private static string GetStatusName(string code)
    {
        return code switch
        {
            "M" => "Modified",
            "A" => "Added",
            "R" => "Removed",
            "C" => "Clean",
            "!" => "Missing",
            "?" => "Untracked",
            "I" => "Ignored",
            _ => "Unknown"
        };
    }
}

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

    /// <summary>
    /// Gets or sets the GitHub pull request number associated with this commit.
    /// </summary>
    [JsonPropertyName("github_pull_request_number")]
    public int? GitHubPullRequestNumber { get; set; }

    /// <summary>
    /// Gets or sets the GitHub repository name associated with this commit.
    /// </summary>
    [JsonPropertyName("github_pull_request_repo_name")]
    public string? GitHubPullRequestRepoName { get; set; }

    /// <summary>
    /// Gets or sets the GitHub repository owner associated with this commit.
    /// </summary>
    [JsonPropertyName("github_pull_request_repo_owner")]
    public string? GitHubPullRequestRepoOwner { get; set; }
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
      "-T{dict(node, author, desc, file_adds, file_dels, file_mods, phase, github_pull_request_number, github_pull_request_repo_name, github_pull_request_repo_owner)|json}\\n"
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
    /// Creates a new commit with the specified message and files.
    /// </summary>
    /// <param name="message">The commit message.</param>
    /// <param name="files">The list of files to include in the commit.</param>
    /// <returns>The newly created commit, or null if no files were specified.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no files are specified for the commit.</exception>
    public Commit? CreateCommit(string message, IList<string> files)
    {
        if (files.Count == 0)
        {
            throw new InvalidOperationException("No files specified for commit. Please specify at least one file to commit.");
        }

        // Create the commit with the specified files
        var commitArguments = new List<string> { "commit", "-m", message };

        // Add each file with the -I flag
        foreach (var file in files)
        {
            commitArguments.Add("-I");
            commitArguments.Add(file);
        }

        RunCommand(commitArguments);

        // Get the newly created commit
        var newCommits = Commits(".");
        return newCommits.FirstOrDefault();
    }

    /// <summary>
    /// Amends the current commit with the specified files and optionally updates the commit message.
    /// </summary>
    /// <param name="files">The list of files to include in the amendment.</param>
    /// <param name="message">The new commit message. If null or empty, the existing message is kept.</param>
    /// <returns>The amended commit, or null if no files were specified.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no files are specified for the amendment.</exception>
    public Commit? AmendCommit(IList<string> files, string? message = null)
    {
        if (files.Count == 0)
        {
            throw new InvalidOperationException("No files specified for amendment. Please specify at least one file to amend.");
        }

        // Create the amend command with the specified files
        var amendArguments = new List<string> { "amend" };

        // Add message if provided
        if (!string.IsNullOrEmpty(message))
        {
            amendArguments.Add("-m");
            amendArguments.Add(message);
        }

        // Add each file with the -I flag
        foreach (var file in files)
        {
            amendArguments.Add("-I");
            amendArguments.Add(file);
        }

        RunCommand(amendArguments);

        // Get the amended commit
        var amendedCommits = Commits(".");
        return amendedCommits.FirstOrDefault();
    }

    /// <summary>
    /// Gets the status of files in the working directory.
    /// </summary>
    /// <returns>A list of file statuses.</returns>
    public IList<FileStatus> GetStatus()
    {
        var arguments = new List<string> { "status", "-Tjson" };
        var output = RunCommand(arguments);

        var statuses = JsonSerializer.Deserialize<List<FileStatus>>(output)
            ?? throw new InvalidOperationException("Failed to deserialize file statuses");

        return statuses;
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
