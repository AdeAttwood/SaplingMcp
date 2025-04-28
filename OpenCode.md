# SaplingMcp Development Guidelines

## Build Commands

- Build: `dotnet build`
- Run: `dotnet run --project SaplingMcp.Server`
- Build specific project: `dotnet build SaplingMcp.Server`
- Test: `dotnet test`
- Test single: `dotnet test --filter "FullyQualifiedName=Namespace.TestClass.TestMethod"`
- Format code: `dotnet format`

## Code Style Guidelines

- **Imports**: Group by namespace, order alphabetically
- **Formatting**: Use 4 spaces for indentation, braces on new lines
- **Types**: Enable nullable reference types, use explicit types for public APIs
- **Naming**:
  - PascalCase for classes, methods, properties
  - camelCase for local variables and parameters
  - Prefix interfaces with 'I'
- **Error Handling**: Use exceptions for exceptional cases, nullable for expected failures
- **Documentation**: XML comments for public APIs
- **Async**: Use async/await pattern consistently, avoid mixing with Task.Result

## Project Structure

- Follow standard .NET project structure
- Use feature folders for organizing related functionality
- Keep services stateless when possible

## Pre-Commit Checks

Before committing changes, ensure:
1. The application builds successfully: `dotnet build`
2. Code is properly formatted: `dotnet format`
3. All tests pass: `dotnet test`

This helps maintain code quality and prevents broken builds in the repository.

## Committing

All commit messages should follow the conventional format for the title, then
there is a "Summary" and a "Test Plan". For example:

```txt
<Type>: <Title>

Summary:

<Summary>

Test Plan:

<TestPlan>
```

The "Title" should be a short description of the change. The "Summary" is a
more detailed expanded description. The "TestPlan" is a description of how we
are going it test this change.

- All commits should be small and focused on one thing
- The commit title should not start with an uppercase letter or the type
- Each section should be followed by a empty line

### Types

Commit types are hard coded and cannot be changed. The following commit types
must be used. The ensures changelogs and semantic visioning can be used
correctly.

- **build**: Changes that affect the build system or external dependencies
- **ci**: Changes to our CI configuration files and scripts
- **docs**: Documentation only changes
- **feat**: A new feature
- **fix**: A bug fix
- **improvement**: A improvement to an existing feature
- **perf**: A code change that improves performance
- **refactor**: A code change that neither fixes a bug nor adds a feature
- **revert**: For when your reverting commits with [git](https://git-scm.com/docs/git-revert)
- **style**: Changes that do not affect the meaning of the code (white-space, formatting, etc)
- **test**: Adding missing tests or correcting existing tests

# Source Control / PR workflow

- For this we are using SaplingScm with a stacked pr workflow.
- Changes in the stack are linked to a GitHub PR
- The PR may have comments that we need to address
- The mainline branch is origin/master
