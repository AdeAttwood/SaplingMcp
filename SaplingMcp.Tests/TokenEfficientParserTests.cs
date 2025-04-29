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
    public void FormatReviewThread_ShouldReturnCorrectFormat()
    {
        // Arrange
        var thread = new ReviewThread
        {
            IsResolved = false,
            Comments = new ReviewThreadComments
            {
                Nodes = new List<PullRequestReviewComment>
                {
                    new PullRequestReviewComment
                    {
                        Author = new Author { Login = "testuser" },
                        CreatedAt = "2023-10-27T10:00:00Z",
                        Body = "Initial comment body.",
                        DiffHunk = "@@ -1 +1 @@\n-old\n+new"
                    }
                }
            }
        };
        var prNumber = 123;
        var repoPath = "testorg/testrepo";

        // Act
        var result = TokenEfficientParser.FormatReviewThread(thread, prNumber, repoPath);

        // Assert
        Assert.Equal("pr:testorg/testrepo#123|resolved:false|comments:1|author:testuser|date:2023-10-27T10:00:00Z|body:Initial comment body.|diffHunk:@@ -1 +1 @@\\n-old\\n+new", result);
    }

    [Fact]
    public void FormatReviewThreads_ShouldReturnMultilineFormat()
    {
        // Arrange
        var threads = new List<ReviewThread>
        {
            new ReviewThread
            {
                IsResolved = true,
                Comments = new ReviewThreadComments
                {
                    Nodes = new List<PullRequestReviewComment>
                    {
                        new PullRequestReviewComment
                        {
                            Author = new Author { Login = "user1" },
                            CreatedAt = "2023-10-27T10:00:00Z",
                            Body = "Comment 1.",
                            DiffHunk = "diff1"
                        }
                    }
                }
            },
            new ReviewThread
            {
                IsResolved = false,
                Comments = new ReviewThreadComments
                {
                    Nodes = new List<PullRequestReviewComment>
                    {
                        new PullRequestReviewComment
                        {
                            Author = new Author { Login = "user2" },
                            CreatedAt = "2023-10-27T11:00:00Z",
                            Body = "Comment 2.\nNew line.",
                            DiffHunk = "diff2\\nwith newline"
                        }
                    }
                }
            }
        };
        var prNumber = 456;
        var repoPath = "anotherorg/anotherrepo";

        // Act
        var result = TokenEfficientParser.FormatReviewThreads(threads, prNumber, repoPath);
        var lines = result.Split(Environment.NewLine);

        // Assert
        Assert.Equal(2, lines.Length);
        Assert.Equal("pr:anotherorg/anotherrepo#456|resolved:true|comments:1|author:user1|date:2023-10-27T10:00:00Z|body:Comment 1.|diffHunk:diff1", lines[0]);
        Assert.Equal("pr:anotherorg/anotherrepo#456|resolved:false|comments:1|author:user2|date:2023-10-27T11:00:00Z|body:Comment 2.\\nNew line.|diffHunk:diff2\\\\nwith newline", lines[1]);
    }

    [Fact]
    public void FormatReviewComment_ShouldReturnCorrectFormat()
    {
        // Arrange
        var comment = new PullRequestReviewComment
        {
            Id = "comment789",
            Author = new Author { Login = "reviewer" },
            CreatedAt = "2023-10-28T09:00:00Z",
            Body = "Looks good overall."
        };

        // Act
        var result = TokenEfficientParser.FormatReviewComment(comment);

        // Assert
        Assert.Equal("id:comment789|author:reviewer|date:2023-10-28T09:00:00Z|body:Looks good overall.", result);
    }

    [Fact]
    public void FormatReviewComments_ShouldReturnMultilineFormat()
    {
        // Arrange
        var comments = new List<PullRequestReviewComment>
        {
            new PullRequestReviewComment
            {
                Id = "comment1",
                Author = new Author { Login = "userA" },
                CreatedAt = "2023-10-28T09:00:00Z",
                Body = "Comment A."
            },
            new PullRequestReviewComment
            {
                Id = "comment2",
                Author = new Author { Login = "userB" },
                CreatedAt = "2023-10-28T10:00:00Z",
                Body = "Comment B.\nWith newline."
            }
        };

        // Act
        var result = TokenEfficientParser.FormatReviewComments(comments);
        var lines = result.Split(Environment.NewLine);

        // Assert
        Assert.Equal(2, lines.Length);
        Assert.Equal("id:comment1|author:userA|date:2023-10-28T09:00:00Z|body:Comment A.", lines[0]);
        Assert.Equal("id:comment2|author:userB|date:2023-10-28T10:00:00Z|body:Comment B.\\nWith newline.", lines[1]);
    }

    [Fact]
    public void FormatReviewThread_ShouldReturnCorrectFormat()
    {
        // Arrange
        var thread = new ReviewThread
        {
            IsResolved = false,
            Comments = new ReviewThreadComments
            {
                Nodes = new List<PullRequestReviewComment>
                {
                    new PullRequestReviewComment
                    {
                        Author = new Author { Login = "testuser" },
                        CreatedAt = "2023-10-27T10:00:00Z",
                        Body = "Initial comment body.",
                        DiffHunk = "@@ -1 +1 @@\n-old\n+new"
                    }
                }
            }
        };
        var prNumber = 123;
        var repoPath = "testorg/testrepo";

        // Act
        var result = TokenEfficientParser.FormatReviewThread(thread, prNumber, repoPath);

        // Assert
        Assert.Equal("pr:testorg/testrepo#123|resolved:false|comments:1|author:testuser|date:2023-10-27T10:00:00Z|body:Initial comment body.|diffHunk:@@ -1 +1 @@\\n-old\\n+new", result);
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