namespace LyricsFinder.Configurations;

public class TrackFileConfiguration
{
    public const string Section = "TrackFile";

    public List<string> MusicExtensions { get; init; } =
    [
        "3gp", "aa", "aac", "aax", "act", "aiff", "alac", "amr", "ape", "au", "awb", "dss", "dvf", "flac", "gsm", "iklax", "ivs", "m4a", "m4b", "m4p", "mmf", "movpkg", "mp3",
        "mpc", "msv", "nmf", "ogg,.oga,.mogg", "opus", "ra,.rm", "raw", "rf64", "sln", "tta", "voc", "vox", "wav", "wma", "wv", "webm", "8svx", "cda"
    ];
}