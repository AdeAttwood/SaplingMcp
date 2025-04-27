using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

using Microsoft.Extensions.Logging;

namespace SaplingMcp.Server.Services;

public class Commit
{
    [JsonPropertyName("node")]
    public required string Node { get; set; }

    [JsonPropertyName("desc")]
    public required string Description { get; set; }

    [JsonPropertyName("author")]
    public required string Author { get; set; }

    [JsonPropertyName("file_adds")]
    public required List<string> FilesAdded { get; set; }

    [JsonPropertyName("file_dels")]
    public required List<string> FilesRemoved { get; set; }

    [JsonPropertyName("file_mods")]
    public required List<string> FilesModified { get; set; }

    [JsonPropertyName("phase")]
    public required string Phase { get; set; }
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

    public IList<Commit> Stack() => this.Commits("bottom::top");

    public IList<Commit> Public() => this.Commits("public()");

    public IList<Commit> Commits(string rev)
    {
        var arguments = new List<string>
    {
      "log",
      "-r",
      rev,
      "-T{dict(node, author, desc, file_adds, file_dels, file_mods, phase)|json}\\n"
    };

        var output = this.RunCommand(arguments);

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

    private string RunCommand(IList<string> arguments)
    {
        var process = new Process();

        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.FileName = "sl";
        process.StartInfo.WorkingDirectory = _repoDir;

        // Use ProcessStartInfo.ArgumentList instead of Arguments to avoid shell injection
        foreach (var arg in arguments)
        {
            process.StartInfo.ArgumentList.Add(arg);
        }

        Console.Error.WriteLine($"Command: sl {string.Join(" ", arguments)}");

        process.Start();

        string output = process.StandardOutput.ReadToEnd();
        string error = process.StandardError.ReadToEnd();

        process.WaitForExit();

        Console.Error.WriteLine($"The out is {output}");
        Console.Error.WriteLine($"The err is {error}");

        return output;
    }
}
