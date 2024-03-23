using Cocona;

namespace LyricsFinder.CommandParameters;

public record FileSearchParameters(
    [Option('p', Description = "The path to the track file")]
    string Path) : ICommandParameterSet;