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
    /// <param name="repo">Optional repository in the format owner/repo. If not provided, uses the current repository.</param>
    /// <returns>A list of open pull requests.</returns>
    [McpServerTool, Description("Gets a list of open pull requests for the current repository or a specified repository")]
    public List<PullRequest> GetOpenPullRequests(
        [Description("Optional repository in the format owner/repo. If not provided, uses the current repository")] string? repo = null)
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
    /// Gets comments for a specific pull request.
    /// </summary>
    /// <param name="prNumber">The pull request number.</param>
    /// <param name="repo">Repository in the format owner/repo</param>
    /// <returns>A list of comments on the pull request.</returns>
    [McpServerTool, Description("Gets comments for a specific pull request")]
    public List<PullRequestComment> GetPullRequestComments(
        [Description("The pull request number")] int prNumber,
        [Description("Repository in the format owner/repo")] string repo)
    {
        try
        {
            return _github.GetPullRequestComments(prNumber, repo).ToList();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error getting pull request comments: {ex.Message}");
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
        [Description("Optional repository in the format owner/repo. If not provided, uses the current repository")] string? repo = null)
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
