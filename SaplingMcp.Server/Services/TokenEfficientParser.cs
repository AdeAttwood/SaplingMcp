using System.Text;

namespace SaplingMcp.Server.Services;

/// <summary>
/// Provides token-efficient parsing and formatting for MCP server data structures.
/// </summary>
public static class TokenEfficientParser
{
    /// <summary>
    /// The delimiter character used in the token-efficient format.
    /// </summary>
    private const char Delimiter = '|';
    /// <summary>
    /// The escape sequence for newline characters in the token-efficient format.
    /// </summary>
    private const string NewlineEscape = "\\n";

    /// <summary>
    /// Formats a commit into a token-efficient line-based format.
    /// </summary>
    /// <param name="commit">The commit to format.</param>
    /// <returns>A string representation of the commit in the format: <c>sha:&lt;commit-sha&gt;|title:&lt;commit-title&gt;|pr:&lt;owner/repo#number or none&gt;</c>.</returns>
    public static string FormatCommit(Commit commit)
    {
        var sb = new StringBuilder();

        sb.Append("sha:").Append(commit.Node);
        sb.Append(Delimiter);

        // Only use the first line of the commit message to keep tokens down
        string firstLine = commit.Description.Split(new[] { '\n' }, 2)[0];
        sb.Append("title:").Append(EscapeValue(firstLine));
        sb.Append(Delimiter);

        sb.Append("pr:");
        if (commit.GitHubPullRequestNumber.HasValue &&
            !string.IsNullOrEmpty(commit.GitHubPullRequestRepoOwner) &&
            !string.IsNullOrEmpty(commit.GitHubPullRequestRepoName))
        {
            sb.Append($"{commit.GitHubPullRequestRepoOwner}/{commit.GitHubPullRequestRepoName}#{commit.GitHubPullRequestNumber}");
        }
        else
        {
            sb.Append("none");
        }

        return sb.ToString();
    }

    /// <summary>
    /// Parses a token-efficient line-based format into a commit.
    /// </summary>
    /// <param name="line">The line to parse.</param>
    /// <returns>A <c>Commit</c> object.</returns>
    public static Commit ParseCommit(string line)
    {
        var parts = SplitLine(line);
        var commit = new Commit
        {
            Node = GetValue(parts, "sha"),
            Description = UnescapeValue(GetValue(parts, "title")),
            Author = string.Empty, // Not included in compact format
            FilesAdded = new List<string>(),
            FilesRemoved = new List<string>(),
            FilesModified = new List<string>(),
            Phase = string.Empty // Not included in compact format
        };

        var prValue = GetValue(parts, "pr");
        if (prValue != "none")
        {
            // Parse PR in format owner/repo#number
            var prParts = prValue.Split('#');
            if (prParts.Length == 2)
            {
                var repoPath = prParts[0].Split('/');
                if (repoPath.Length == 2 && int.TryParse(prParts[1], out int prNumber))
                {
                    commit.GitHubPullRequestRepoOwner = repoPath[0];
                    commit.GitHubPullRequestRepoName = repoPath[1];
                    commit.GitHubPullRequestNumber = prNumber;
                }
            }
        }

        return commit;
    }

    /// <summary>
    /// Formats a pull request comment into a token-efficient line-based format.
    /// </summary>
    /// <param name="comment">The comment to format.</param>
    /// <param name="prNumber">The pull request number.</param>
    /// <param name="repoPath">The repository path in format owner/repo.</param>
    /// <returns>A string representation of the comment in the format: <c>pr:&lt;owner/repo#number&gt;|author:&lt;username&gt;|date:&lt;timestamp&gt;|id:&lt;comment-id&gt;|body:&lt;comment-text&gt;</c>.</returns>
    public static string FormatPullRequestComment(PullRequestComment comment, int prNumber, string repoPath)
    {
        var sb = new StringBuilder();

        sb.Append("pr:").Append(repoPath).Append('#').Append(prNumber);
        sb.Append(Delimiter);

        // Extract author from URL (this is a simplification, in a real implementation you'd want to get this from the API)
        var authorPlaceholder = "unknown";
        sb.Append("author:").Append(authorPlaceholder);
        sb.Append(Delimiter);

        sb.Append("date:").Append(comment.CreatedAt);
        sb.Append(Delimiter);

        // Extract ID from URL (this is a simplification, in a real implementation you'd want to get this from the API)
        var idParts = comment.Url.Split('/');
        var id = idParts.Length > 0 ? idParts[idParts.Length - 1] : "unknown";
        sb.Append("id:").Append(id);
        sb.Append(Delimiter);

        sb.Append("body:").Append(EscapeValue(comment.Body));

        return sb.ToString();
    }

    /// <summary>
    /// Parses a token-efficient line-based format into a pull request comment.
    /// </summary>
    /// <param name="line">The line to parse.</param>
    /// <returns>A tuple containing the pull request comment, PR number, and repository path (<c>PullRequestComment Comment, int PrNumber, string RepoPath</c>).</returns>
    public static (PullRequestComment Comment, int PrNumber, string RepoPath) ParsePullRequestComment(string line)
    {
        var parts = SplitLine(line);

        var prValue = GetValue(parts, "pr");
        var prParts = prValue.Split('#');
        var repoPath = prParts[0];
        var prNumber = int.Parse(prParts[1]);

        var comment = new PullRequestComment
        {
            Body = UnescapeValue(GetValue(parts, "body")),
            CreatedAt = GetValue(parts, "date"),
            Url = $"https://github.com/{repoPath}/pull/{prNumber}/comments/{GetValue(parts, "id")}",
            DiffHunk = string.Empty // Not included in compact format
        };

        return (comment, prNumber, repoPath);
    }

    /// <summary>
    /// Formats a list of commits into a token-efficient multi-line format.
    /// </summary>
    /// <param name="commits">The list of commits to format.</param>
    /// <returns>A string representation of the commits, one per line, using the format from <see cref="FormatCommit(Commit)"/>.</returns>
    public static string FormatCommits(IEnumerable<Commit> commits)
    {
        return string.Join(Environment.NewLine, commits.Select(FormatCommit));
    }

    /// <summary>
    /// Parses a token-efficient multi-line format into a list of commits.
    /// </summary>
    /// <param name="text">The text to parse.</param>
    /// <returns>A list of <c>Commit</c> objects.</returns>
    public static List<Commit> ParseCommits(string text)
    {
        return text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
            .Select(ParseCommit)
            .ToList();
    }

    /// <summary>
    /// Formats a list of pull request comments into a token-efficient multi-line format.
    /// </summary>
    /// <param name="comments">The list of comments to format.</param>
    /// <param name="prNumber">The pull request number.</param>
    /// <param name="repoPath">The repository path in format owner/repo.</param>
    /// <returns>A string representation of the comments, one per line, using the format from <see cref="FormatPullRequestComment(PullRequestComment, int, string)"/>.</returns>
    public static string FormatPullRequestComments(IEnumerable<PullRequestComment> comments, int prNumber, string repoPath)
    {
        return string.Join(Environment.NewLine, comments.Select(c => FormatPullRequestComment(c, prNumber, repoPath)));
    }

    /// <summary>
    /// Parses a token-efficient multi-line format into a list of pull request comments.
    /// </summary>
    /// <param name="text">The text to parse.</param>
    /// <returns>A list of tuples containing pull request comments, PR numbers, and repository paths (<c>PullRequestComment Comment, int PrNumber, string RepoPath</c>).</returns>
    public static List<(PullRequestComment Comment, int PrNumber, string RepoPath)> ParsePullRequestComments(string text)
    {
        return text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
            .Select(ParsePullRequestComment)
            .ToList();
    }

    /// <summary>
    /// Formats a review thread into a token-efficient line-based format.
    /// </summary>
    /// <param name="thread">The review thread to format.</param>
    /// <param name="prNumber">The pull request number.</param>
    /// <param name="repoPath">The repository path in format owner/repo.</param>
    /// <returns>A string representation of the thread in a token-efficient format: <c>pr:&lt;owner/repo#number&gt;|resolved:&lt;true/false&gt;|comments:&lt;count&gt;|author:&lt;username&gt;|date:&lt;timestamp&gt;|body:&lt;comment-text&gt;|diffHunk:&lt;diff-hunk&gt;</c>.</returns>
    public static string FormatReviewThread(ReviewThread thread, int prNumber, string repoPath)
    {
        var sb = new StringBuilder();

        sb.Append("pr:").Append(repoPath).Append('#').Append(prNumber);
        sb.Append(Delimiter);

        sb.Append("resolved:").Append(thread.IsResolved ? "true" : "false");
        sb.Append(Delimiter);

        sb.Append("comments:").Append(thread.Comments.Nodes.Count);
        sb.Append(Delimiter);

        // Format the first comment in the thread (usually the most important one)
        if (thread.Comments.Nodes.Count > 0)
        {
            var firstComment = thread.Comments.Nodes[0];
            sb.Append("author:").Append(firstComment.Author.Login);
            sb.Append(Delimiter);
            sb.Append("date:").Append(firstComment.CreatedAt);
            sb.Append(Delimiter);
            sb.Append("body:").Append(EscapeValue(firstComment.Body));
            sb.Append(Delimiter);
            sb.Append("diffHunk:").Append(EscapeValue(firstComment.DiffHunk));
        }

        return sb.ToString();
    }

    /// <summary>
    /// Formats a list of review threads into a token-efficient multi-line format.
    /// </summary>
    /// <param name="threads">The list of threads to format.</param>
    /// <param name="prNumber">The pull request number.</param>
    /// <param name="repoPath">The repository path in format owner/repo.</param>
    /// <returns>A string representation of the threads, one per line, using the format from <see cref="FormatReviewThread(ReviewThread, int, string)"/>.</returns>
    public static string FormatReviewThreads(IEnumerable<ReviewThread> threads, int prNumber, string repoPath)
    {
        return string.Join(Environment.NewLine, threads.Select(t => FormatReviewThread(t, prNumber, repoPath)));
    }

    /// <summary>
    /// Formats a review comment into a token-efficient line-based format.
    /// </summary>
    /// <param name="comment">The review comment to format.</param>
    /// <returns>A string representation of the comment in a token-efficient format: <c>id:&lt;comment-id&gt;|author:&lt;username&gt;|date:&lt;timestamp&gt;|body:&lt;comment-text&gt;</c>.</returns>
    public static string FormatReviewComment(PullRequestReviewComment comment)
    {
        var sb = new StringBuilder();

        sb.Append("id:").Append(comment.Id);
        sb.Append(Delimiter);

        sb.Append("author:").Append(comment.Author.Login);
        sb.Append(Delimiter);

        sb.Append("date:").Append(comment.CreatedAt);
        sb.Append(Delimiter);

        sb.Append("body:").Append(EscapeValue(comment.Body));

        return sb.ToString();
    }

    /// <summary>
    /// Formats a list of review comments into a token-efficient multi-line format.
    /// </summary>
    /// <param name="comments">The list of comments to format.</param>
    /// <returns>A string representation of the comments, one per line, using the format from <see cref="FormatReviewComment(PullRequestReviewComment)"/>.</returns>
    public static string FormatReviewComments(IEnumerable<PullRequestReviewComment> comments)
    {
        return string.Join(Environment.NewLine, comments.Select(FormatReviewComment));
    }

    /// <summary>
    /// Escapes special characters in a value for the token-efficient format.
    /// </summary>
    /// <param name="value">The value to escape.</param>
    /// <returns>The escaped value.</returns>
    private static string EscapeValue(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        return value.Replace("\n", NewlineEscape);
    }

    /// <summary>
    /// Unescapes special characters in a value from the token-efficient format.
    /// </summary>
    /// <param name="value">The value to unescape.</param>
    /// <returns>The unescaped value.</returns>
    private static string UnescapeValue(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        return value.Replace(NewlineEscape, "\n");
    }

    /// <summary>
    /// Splits a line into key-value pairs.
    /// </summary>
    /// <param name="line">The line to split.</param>
    /// <returns>A dictionary of key-value pairs.</returns>
    private static Dictionary<string, string> SplitLine(string line)
    {
        var result = new Dictionary<string, string>();
        var parts = line.Split(Delimiter);

        foreach (var part in parts)
        {
            var keyValue = part.Split(new[] { ':' }, 2);
            if (keyValue.Length == 2)
            {
                result[keyValue[0]] = keyValue[1];
            }
        }

        return result;
    }

    /// <summary>
    /// Gets a value from a dictionary of key-value pairs.
    /// </summary>
    /// <param name="parts">The dictionary of key-value pairs.</param>
    /// <param name="key">The key to get the value for.</param>
    /// <returns>The value, or an empty string if the key is not found.</returns>
    private static string GetValue(Dictionary<string, string> parts, string key)
    {
        return parts.TryGetValue(key, out var value) ? value : string.Empty;
    }
}
