# SaplingMcp

A Model Context Protocol (MCP) server for Sapling SCM integration.

## Token-Efficient Parsing Format

This project implements a token-efficient line-based parsing format for the MCP server that handles stack information and PR comments for Sapling SCM. The format is designed to be compact and efficient when interacting with LLMs.

### Stack Information Format

Stack information is formatted using a pipe-delimited structure:

```
sha:<commit-sha>|title:<commit-title>|pr:<owner/repo#number or none>
```

Example:
```
sha:abc123def456|title:Fix widget rendering bug|pr:your-org/your-repo#123
```

### PR Comments Format

PR comments are formatted with a similar pipe-delimited structure:

```
pr:<owner/repo#number>|author:<username>|date:<timestamp>|id:<comment-id>|body:<comment-text>
```

Example:
```
pr:your-org/your-repo#123|author:username|date:2025-04-20T14:30:00Z|id:comment123|body:This looks good.\nJust one suggestion: let's add more tests.
```

### Special Characters

- Newlines in comments are escaped as `\\n` to maintain the line-based format
- Each item is on a separate line, making it easy to process line by line

### Usage

The MCP server provides token-efficient versions of the following endpoints:

- `GetCurrentStack`: Gets all commits in the current stack in token-efficient format
- `GetPublicCommits`: Gets all public commits in token-efficient format
- `CreateCommit`: Creates a new commit and returns it in token-efficient format
- `AmendCommit`: Amends the current commit and returns it in token-efficient format

These endpoints return strings in the token-efficient format described above, which can be parsed using the `TokenEfficientParser` class.

## Building and Running

```bash
# Build the project
dotnet build

# Run the server
dotnet run --project SaplingMcp.Server

# Run tests
dotnet test
```