using Cocona;

namespace LyricsFinder.CommandParameters;

public record DirectorySearchParameters(
    [Option('p', Description = "The path to the directory containing tracks")]
    string Path,
    [Option('r', Description = "search lyrics for tracks in subdirectories recursively")]
    bool Recursive = false) : ICommandParameterSet;