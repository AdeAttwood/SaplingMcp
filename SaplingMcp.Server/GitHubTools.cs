using System.ComponentModel;

using ModelContextProtocol.Server;

using SaplingMcp.Server.Services;

namespace SaplingMcp.Server;

/// <summary>
/// Tools for interacting with GitHub.
/// </summary>
[McpServerToolType]
public class GitHubTools
{
    private readonly GitHub _github;

    /// <summary>
    /// Initializes a new instance of the <see cref="GitHubTools"/> class.
    /// </summary>
    /// <param name="github">The GitHub service.</param>
    public GitHubTools(GitHub github)
    {
        _github = github;
    }

    /// <summary>
    /// Gets a list of open pull requests for the current repository or a specified repository.
    /// </summary>
    /// <param name="repo">The repository in the format owner/repo.</param>
    /// <returns>A list of open pull requests.</returns>
    [McpServerTool, Description("Gets a list of open pull requests for a specified repository")]
    public List<PullRequest> GetOpenPullRequests(
        [Description("The repository in the format owner/repo")] string repo)
    {
        try
        {
            return _github.GetOpenPullRequests(repo).ToList();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error getting open pull requests: {ex.Message}");
        }
    }


    /// <summary>
    /// Gets review threads for a specific pull request.
    /// </summary>
    /// <param name="prNumber">The pull request number.</param>
    /// <param name="repo">Repository in the format owner/repo.</param>
    /// <returns>A token-efficient formatted string of review threads, including their comments and resolved status.</returns>
    [McpServerTool, Description("Gets review threads for a specific pull request, including comments and resolved status")]
    public string GetPullRequestReviewThreads(
        [Description("The pull request number")] int prNumber,
        [Description("Repository in the format owner/repo")] string repo)
    {
        try
        {
            // Ensure the repo format is correct before passing it down
            if (string.IsNullOrWhiteSpace(repo) || !repo.Contains('/'))
            {
                throw new ArgumentException("Repository must be in the format 'owner/repo'.");
            }
            var threads = _github.GetPullRequestReviewThreads(prNumber, repo);
            return TokenEfficientParser.FormatReviewThreads(threads, prNumber, repo);
        }
        catch (Exception ex)
        {
            // Propagate specific argument exceptions or wrap others
            if (ex is ArgumentException) throw;
            throw new InvalidOperationException($"Error getting pull request review threads: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Gets unresolved review threads for a specific pull request.
    /// </summary>
    /// <param name="prNumber">The pull request number.</param>
    /// <param name="repo">Repository in the format owner/repo.</param>
    /// <returns>A token-efficient formatted string of unresolved review threads that need action.</returns>
    [McpServerTool, Description("Gets unresolved review threads for a specific pull request that need action")]
    public string GetUnresolvedPullRequestThreads(
        [Description("The pull request number")] int prNumber,
        [Description("Repository in the format owner/repo")] string repo)
    {
        try
        {
            // Ensure the repo format is correct before passing it down
            if (string.IsNullOrWhiteSpace(repo) || !repo.Contains('/'))
            {
                throw new ArgumentException("Repository must be in the format 'owner/repo'.");
            }

            // Get all threads and filter to only unresolved ones
            var allThreads = _github.GetPullRequestReviewThreads(prNumber, repo);
            var unresolvedThreads = allThreads.Where(thread => !thread.IsResolved).ToList();
            return TokenEfficientParser.FormatReviewThreads(unresolvedThreads, prNumber, repo);
        }
        catch (Exception ex)
        {
            // Propagate specific argument exceptions or wrap others
            if (ex is ArgumentException) throw;
            throw new InvalidOperationException($"Error getting unresolved pull request threads: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Gets CI check statuses for a specific pull request.
    /// </summary>
    /// <param name="prNumber">The pull request number.</param>
    /// <param name="repo">Optional repository in the format owner/repo. If not provided, uses the current repository.</param>
    /// <returns>A list of CI check statuses for the pull request.</returns>
    [McpServerTool, Description("Gets CI check statuses for a specific pull request")]
    public List<CheckStatus> GetPullRequestChecks(
        [Description("The pull request number")] int prNumber,
        [Description("The repository in the format owner/repo")] string repo)
    {
        try
        {
            return _github.GetPullRequestChecks(prNumber, repo).ToList();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error getting pull request checks: {ex.Message}");
        }
    }
}
