using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

using Microsoft.Extensions.Logging;

namespace SaplingMcp.Server.Services;

/// <summary>
/// Represents a GitHub pull request.
/// </summary>
public class PullRequest
{
    /// <summary>
    /// Gets or sets the pull request number.
    /// </summary>
    [JsonPropertyName("number")]
    public required int Number { get; set; }

    /// <summary>
    /// Gets or sets the title of the pull request.
    /// </summary>
    [JsonPropertyName("title")]
    public required string Title { get; set; }

    /// <summary>
    /// Gets or sets the URL of the pull request.
    /// </summary>
    [JsonPropertyName("url")]
    public required string Url { get; set; }

    /// <summary>
    /// Gets or sets the state of the pull request (open, closed, merged).
    /// </summary>
    [JsonPropertyName("state")]
    public required string State { get; set; }

    /// <summary>
    /// Gets or sets the author of the pull request.
    /// </summary>
    [JsonPropertyName("author")]
    public required string Author { get; set; }

    /// <summary>
    /// Gets or sets the date when the pull request was created.
    /// </summary>
    [JsonPropertyName("createdAt")]
    public required string CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the date when the pull request was last updated.
    /// </summary>
    [JsonPropertyName("updatedAt")]
    public required string UpdatedAt { get; set; }
}

/// <summary>
/// Represents a comment on a GitHub pull request.
/// </summary>
public class PullRequestComment
{
    /// <summary>
    /// Gets or sets the author of the comment.
    /// </summary>
    [JsonPropertyName("author")]
    public required string Author { get; set; }

    /// <summary>
    /// Gets or sets the body of the comment.
    /// </summary>
    [JsonPropertyName("body")]
    public required string Body { get; set; }

    /// <summary>
    /// Gets or sets the date when the comment was created.
    /// </summary>
    [JsonPropertyName("createdAt")]
    public required string CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the URL of the comment.
    /// </summary>
    [JsonPropertyName("url")]
    public required string Url { get; set; }
}

/// <summary>
/// Represents a review on a GitHub pull request.
/// </summary>
public class PullRequestReview
{
    /// <summary>
    /// Gets or sets the author of the review.
    /// </summary>
    [JsonPropertyName("author")]
    public required string Author { get; set; }

    /// <summary>
    /// Gets or sets the body of the review.
    /// </summary>
    [JsonPropertyName("body")]
    public required string Body { get; set; }

    /// <summary>
    /// Gets or sets the state of the review (APPROVED, CHANGES_REQUESTED, COMMENTED).
    /// </summary>
    [JsonPropertyName("state")]
    public required string State { get; set; }

    /// <summary>
    /// Gets or sets the date when the review was submitted.
    /// </summary>
    [JsonPropertyName("submittedAt")]
    public required string SubmittedAt { get; set; }

    /// <summary>
    /// Gets or sets the URL of the review.
    /// </summary>
    [JsonPropertyName("url")]
    public required string Url { get; set; }
}

/// <summary>
/// Represents a CI check status for a GitHub pull request.
/// </summary>
public class CheckStatus
{
    /// <summary>
    /// Gets or sets the name of the check.
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the state of the check (success, failure, pending, etc.).
    /// </summary>
    [JsonPropertyName("state")]
    public required string State { get; set; }

    /// <summary>
    /// Gets or sets the conclusion of the check (success, failure, skipped, etc.).
    /// </summary>
    [JsonPropertyName("conclusion")]
    public required string? Conclusion { get; set; }

    /// <summary>
    /// Gets or sets the URL of the check.
    /// </summary>
    [JsonPropertyName("link")]
    public required string Link { get; set; }
}

/// <summary>
/// Service for interacting with GitHub.
/// </summary>
public class GitHub
{
    private readonly string _repoDir;

    /// <summary>
    /// Initializes a new instance of the <see cref="GitHub"/> class.
    /// </summary>
    /// <param name="repoDir">The directory path of the repository.</param>
    public GitHub(string repoDir)
    {
        _repoDir = repoDir;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GitHub"/> class using the current directory as the repository.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public GitHub(ILogger<GitHub> logger)
    {
        _repoDir = Directory.GetCurrentDirectory();
    }

    /// <summary>
    /// Gets a list of open pull requests for the current repository or a specified repository.
    /// </summary>
    /// <param name="repo">Optional repository in the format owner/repo. If not provided, uses the current repository.</param>
    /// <returns>A list of open pull requests.</returns>
    public IList<PullRequest> GetOpenPullRequests(string? repo = null)
    {
        var args = new List<string> { "pr", "list", "--json", "number,title,url,state,author,createdAt,updatedAt" };

        if (!string.IsNullOrEmpty(repo))
        {
            args.Add("--repo");
            args.Add(repo);
        }

        var output = RunCommand(args);
        var pullRequests = JsonSerializer.Deserialize<List<PullRequest>>(output)
            ?? throw new InvalidOperationException("Failed to deserialize pull requests");

        return pullRequests;
    }

    /// <summary>
    /// Gets comments for a specific pull request.
    /// </summary>
    /// <param name="prNumber">The pull request number.</param>
    /// <param name="repo">Optional repository in the format owner/repo. If not provided, uses the current repository.</param>
    /// <returns>A list of comments on the pull request.</returns>
    public IList<PullRequestComment> GetPullRequestComments(int prNumber, string? repo = null)
    {
        var repoPath = string.IsNullOrEmpty(repo) ? "." : repo;
        var args = new List<string> { "api", $"repos/{repoPath}/issues/{prNumber}/comments" };

        var output = RunCommand(args);
        var comments = JsonSerializer.Deserialize<List<PullRequestComment>>(output)
            ?? throw new InvalidOperationException("Failed to deserialize pull request comments");

        return comments;
    }

    /// <summary>
    /// Gets reviews for a specific pull request.
    /// </summary>
    /// <param name="prNumber">The pull request number.</param>
    /// <param name="repo">Optional repository in the format owner/repo. If not provided, uses the current repository.</param>
    /// <returns>A list of reviews on the pull request.</returns>
    public IList<PullRequestReview> GetPullRequestReviews(int prNumber, string? repo = null)
    {
        var repoPath = string.IsNullOrEmpty(repo) ? "." : repo;
        var args = new List<string> { "api", $"repos/{repoPath}/pulls/{prNumber}/reviews" };

        var output = RunCommand(args);
        var reviews = JsonSerializer.Deserialize<List<PullRequestReview>>(output)
            ?? throw new InvalidOperationException("Failed to deserialize pull request reviews");

        return reviews;
    }

    /// <summary>
    /// Gets CI check statuses for a specific pull request.
    /// </summary>
    /// <param name="prNumber">The pull request number.</param>
    /// <param name="repo">Optional repository in the format owner/repo. If not provided, uses the current repository.</param>
    /// <returns>A list of CI check statuses for the pull request.</returns>
    public IList<CheckStatus> GetPullRequestChecks(int prNumber, string? repo = null)
    {
        var args = new List<string> { "pr", "checks", prNumber.ToString(), "--json", "name,state,conclusion,link" };

        if (!string.IsNullOrEmpty(repo))
        {
            args.Add("--repo");
            args.Add(repo);
        }

        var output = RunCommand(args);
        var checks = JsonSerializer.Deserialize<List<CheckStatus>>(output)
            ?? throw new InvalidOperationException("Failed to deserialize check statuses");

        return checks;
    }

    /// <summary>
    /// Gets review comments for a specific pull request.
    /// </summary>
    /// <param name="prNumber">The pull request number.</param>
    /// <param name="repo">Optional repository in the format owner/repo. If not provided, uses the current repository.</param>
    /// <returns>A list of review comments on the pull request.</returns>
    public IList<PullRequestComment> GetPullRequestReviewComments(int prNumber, string? repo = null)
    {
        var repoPath = string.IsNullOrEmpty(repo) ? "." : repo;
        var args = new List<string> { "api", $"repos/{repoPath}/pulls/{prNumber}/comments" };

        var output = RunCommand(args);
        var comments = JsonSerializer.Deserialize<List<PullRequestComment>>(output)
            ?? throw new InvalidOperationException("Failed to deserialize pull request review comments");

        return comments;
    }

    /// <summary>
    /// Runs a GitHub CLI command with the specified arguments.
    /// </summary>
    /// <param name="arguments">The command arguments to pass to the GitHub CLI.</param>
    /// <returns>The standard output of the command.</returns>
    private string RunCommand(IEnumerable<string> arguments)
    {
        var process = new Process();

        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.FileName = "gh";
        process.StartInfo.WorkingDirectory = _repoDir;

        // Use ProcessStartInfo.ArgumentList instead of Arguments to avoid shell injection
        foreach (var arg in arguments)
        {
            process.StartInfo.ArgumentList.Add(arg);
        }

        Console.Error.WriteLine($"Command: gh {string.Join(" ", arguments)}");

        process.Start();

        string output = process.StandardOutput.ReadToEnd();
        string error = process.StandardError.ReadToEnd();

        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"GitHub CLI command failed: {error}");
        }

        Console.Error.WriteLine($"The out is {output}");
        Console.Error.WriteLine($"The err is {error}");

        return output;
    }
}
