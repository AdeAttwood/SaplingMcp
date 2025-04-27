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
