using SaplingMcp.Server.Services;
using Xunit;

namespace SaplingMcp.Tests;

public class TokenEfficientParserTests
{
    [Fact]
    public void FormatCommit_ShouldReturnCorrectFormat()
    {
        // Arrange
        var commit = new Commit
        {
            Node = "abc123def456",
            Description = "Fix widget rendering bug",
            Author = "John Doe",
            FilesAdded = new List<string>(),
            FilesRemoved = new List<string>(),
            FilesModified = new List<string>(),
            Phase = "draft",
            GitHubPullRequestNumber = 123,
            GitHubPullRequestRepoOwner = "your-org",
            GitHubPullRequestRepoName = "your-repo"
        };

        // Act
        var result = TokenEfficientParser.FormatCommit(commit);

        // Assert
        Assert.Equal("sha:abc123def456|title:Fix widget rendering bug|pr:your-org/your-repo#123", result);
    }

    [Fact]
    public void FormatCommit_WithoutPR_ShouldReturnNoneForPR()
    {
        // Arrange
        var commit = new Commit
        {
            Node = "abc123def456",
            Description = "Fix widget rendering bug",
            Author = "John Doe",
            FilesAdded = new List<string>(),
            FilesRemoved = new List<string>(),
            FilesModified = new List<string>(),
            Phase = "draft"
        };

        // Act
        var result = TokenEfficientParser.FormatCommit(commit);

        // Assert
        Assert.Equal("sha:abc123def456|title:Fix widget rendering bug|pr:none", result);
    }

    [Fact]
    public void ParseCommit_ShouldReturnCorrectObject()
    {
        // Arrange
        var line = "sha:abc123def456|title:Fix widget rendering bug|pr:your-org/your-repo#123";

        // Act
        var result = TokenEfficientParser.ParseCommit(line);

        // Assert
        Assert.Equal("abc123def456", result.Node);
        Assert.Equal("Fix widget rendering bug", result.Description);
        Assert.Equal(123, result.GitHubPullRequestNumber);
        Assert.Equal("your-org", result.GitHubPullRequestRepoOwner);
        Assert.Equal("your-repo", result.GitHubPullRequestRepoName);
    }

    [Fact]
    public void ParseCommit_WithoutPR_ShouldReturnNullPRNumber()
    {
        // Arrange
        var line = "sha:abc123def456|title:Fix widget rendering bug|pr:none";

        // Act
        var result = TokenEfficientParser.ParseCommit(line);

        // Assert
        Assert.Equal("abc123def456", result.Node);
        Assert.Equal("Fix widget rendering bug", result.Description);
        Assert.Null(result.GitHubPullRequestNumber);
        Assert.Null(result.GitHubPullRequestRepoOwner);
        Assert.Null(result.GitHubPullRequestRepoName);
    }

    [Fact]
    public void FormatPullRequestComment_ShouldReturnCorrectFormat()
    {
        // Arrange
        var comment = new PullRequestComment
        {
            Body = "This looks good.\nJust one suggestion: let's add more tests.",
            CreatedAt = "2025-04-20T14:30:00Z",
            Url = "https://github.com/your-org/your-repo/pull/123/comments/comment123",
            DiffHunk = "diff --git a/file.txt b/file.txt"
        };

        // Act
        var result = TokenEfficientParser.FormatPullRequestComment(comment, 123, "your-org/your-repo");

        // Assert
        Assert.Equal("pr:your-org/your-repo#123|author:unknown|date:2025-04-20T14:30:00Z|id:comment123|body:This looks good.\\nJust one suggestion: let's add more tests.", result);
    }

    [Fact]
    public void ParsePullRequestComment_ShouldReturnCorrectObject()
    {
        // Arrange
        var line = "pr:your-org/your-repo#123|author:username|date:2025-04-20T14:30:00Z|id:comment123|body:This looks good.\\nJust one suggestion: let's add more tests.";

        // Act
        var (comment, prNumber, repoPath) = TokenEfficientParser.ParsePullRequestComment(line);

        // Assert
        Assert.Equal("This looks good.\nJust one suggestion: let's add more tests.", comment.Body);
        Assert.Equal("2025-04-20T14:30:00Z", comment.CreatedAt);
        Assert.Contains("comment123", comment.Url);
        Assert.Equal(123, prNumber);
        Assert.Equal("your-org/your-repo", repoPath);
    }

    [Fact]
    public void FormatCommits_ShouldReturnMultilineFormat()
    {
        // Arrange
        var commits = new List<Commit>
        {
            new Commit
            {
                Node = "abc123",
                Description = "Fix bug",
                Author = "John",
                FilesAdded = new List<string>(),
                FilesRemoved = new List<string>(),
                FilesModified = new List<string>(),
                Phase = "draft",
                GitHubPullRequestNumber = 123,
                GitHubPullRequestRepoOwner = "org1",
                GitHubPullRequestRepoName = "repo1"
            },
            new Commit
            {
                Node = "def456",
                Description = "Add feature",
                Author = "Jane",
                FilesAdded = new List<string>(),
                FilesRemoved = new List<string>(),
                FilesModified = new List<string>(),
                Phase = "draft"
            }
        };

        // Act
        var result = TokenEfficientParser.FormatCommits(commits);
        var lines = result.Split(Environment.NewLine);

        // Assert
        Assert.Equal(2, lines.Length);
        Assert.Equal("sha:abc123|title:Fix bug|pr:org1/repo1#123", lines[0]);
        Assert.Equal("sha:def456|title:Add feature|pr:none", lines[1]);
    }

    [Fact]
    public void ParseCommits_ShouldReturnListOfCommits()
    {
        // Arrange
        var text = "sha:abc123|title:Fix bug|pr:org1/repo1#123" + Environment.NewLine +
                   "sha:def456|title:Add feature|pr:none";

        // Act
        var result = TokenEfficientParser.ParseCommits(text);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("abc123", result[0].Node);
        Assert.Equal("Fix bug", result[0].Description);
        Assert.Equal(123, result[0].GitHubPullRequestNumber);
        Assert.Equal("org1", result[0].GitHubPullRequestRepoOwner);
        Assert.Equal("repo1", result[0].GitHubPullRequestRepoName);
        
        Assert.Equal("def456", result[1].Node);
        Assert.Equal("Add feature", result[1].Description);
        Assert.Null(result[1].GitHubPullRequestNumber);
    }

    [Fact]
    public void FormatCommit_ShouldOnlyUseFirstLine()
    {
        // Arrange
        var commit = new Commit
        {
            Node = "abc123",
            Description = "Line 1\nLine 2\nLine 3",
            Author = "John",
            FilesAdded = new List<string>(),
            FilesRemoved = new List<string>(),
            FilesModified = new List<string>(),
            Phase = "draft"
        };

        // Act
        var result = TokenEfficientParser.FormatCommit(commit);

        // Assert
        Assert.Equal("sha:abc123|title:Line 1|pr:none", result);
    }

    [Fact]
    public void UnescapeValue_ShouldHandleEscapedNewlines()
    {
        // Arrange
        var line = "sha:abc123|title:Line 1\\nLine 2\\nLine 3|pr:none";

        // Act
        var result = TokenEfficientParser.ParseCommit(line);

        // Assert
        Assert.Equal("Line 1\nLine 2\nLine 3", result.Description);
    }
}