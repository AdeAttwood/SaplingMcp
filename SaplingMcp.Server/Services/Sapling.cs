using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace SaplingMcp.Server.Services;

public class Commit
{
  [System.Text.Json.Serialization.JsonPropertyName("node")]
  public required string Node { get; set; }

  [System.Text.Json.Serialization.JsonPropertyName("desc")]
  public required string Description { get; set; }

  [System.Text.Json.Serialization.JsonPropertyName("author")]
  public required string Author { get; set; }

  [System.Text.Json.Serialization.JsonPropertyName("file_adds")]
  public required List<string> FilesAdded { get; set; }

  [System.Text.Json.Serialization.JsonPropertyName("file_dels")]
  public required List<string> FilesRemoved { get; set; }

  [System.Text.Json.Serialization.JsonPropertyName("file_mods")]
  public required List<string> FilesModified { get; set; }

  [System.Text.Json.Serialization.JsonPropertyName("phase")]
  public required string Phase  { get; set; }
}

public class Sapling
{
  private readonly string _repoDir;

  public Sapling(string repoDir)
  {
    _repoDir = repoDir;
  }

  public Sapling(ILogger<Sapling> logger)
  {
    _repoDir = Directory.GetCurrentDirectory();
  }

  public string RepositoryId()
  {
    var result = this.RunCommand($"config bookstack.repository").Trim();
    return result;
  }

  public IList<Commit> Stack() => this.Commits("bottom::top");

  public IList<Commit> Public() => this.Commits("public()");

  public IList<Commit> Commits(string rev)
  {
    // TODO: Sort out the ref so we con't do shell injection
    var command = "log -r " + rev + " -T\"{dict(node, author, desc, file_adds, file_dels, file_mods, phase)|json}\\n\"";

    var output = this.RunCommand(command);

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

  private string RunCommand(string command)
  {
    var process = new Process();

    Console.Error.WriteLine($"Command: {command}");

    process.StartInfo.UseShellExecute = false;
    process.StartInfo.RedirectStandardOutput = true;
    process.StartInfo.RedirectStandardError = true;
    process.StartInfo.FileName = "sl";
    process.StartInfo.Arguments = command;
    process.StartInfo.WorkingDirectory = _repoDir;

    process.Start();

    string output = process.StandardOutput.ReadToEnd();
    string error = process.StandardError.ReadToEnd();


    process.WaitForExit();

    Console.Error.WriteLine($"The out is {output}");
    Console.Error.WriteLine($"The err is {error}");

    return output;
  }
}
