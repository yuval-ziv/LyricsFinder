using Cocona;

namespace LyricsFinder.CommandParameters;

public record CommonParameters(
    [Option('D', Description = "When toggled, no files will be saved (default to true)")]
    bool DryRun = false,
    [Option('O', Description = "When toggled, will overwrite existing files (defaults to false)")]
    bool OverwriteExisting = false,
    [Option('B', Description = "When toggled, will back up existing files before overwriting them (defaults to false)")]
    bool BackupExisting = false,
    [Option('L', Description = "Set the maximum amount of concurrent request per provider. Set 0 for no limit. (defaults to 0)")]
    int ConcurrentRequestLimit = 0) : ICommandParameterSet;