using Cocona;

namespace LyricsFinder.CommandParameters;

public record QuerySearchParameters(
    [Option('a', Description = "The name of the artist")]
    string Artist,
    [Option('t', Description = "The title of the track")]
    string Title,
    [Option('o', Description = "The path to save the lyrics file")]
    string Output) : ICommandParameterSet;