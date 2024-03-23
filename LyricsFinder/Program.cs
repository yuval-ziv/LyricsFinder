using Cocona;
using Cocona.Builder;
using LyricsFinder.CommandParameters;
using LyricsFinder.Configurations;
using LyricsFinder.Providers;
using LyricsFinder.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using ShironetLyricsProvider = LyricsFinder.Providers.ShironetLyricsProvider;

CoconaAppBuilder builder = CoconaApp.CreateBuilder();

builder.Configuration.AddJsonFile("appsettings.json");
builder.Host.UseSerilog((context, configuration) => configuration.ReadFrom.Configuration(context.Configuration));

IServiceCollection services = builder.Services;
services.AddSingleton<ILyricsProvider, ShironetLyricsProvider>();
services.AddSingleton<ILyricsFinderService, LyricsFinderService>();
services.AddSingleton<IApiInjectorService, ApiInjectorService>();
services.AddSingleton<ITrackFileService, TrackFileService>();
services.AddSingleton<ITrackPathParserService, TrackPathParserService>();
services.AddSingleton<IDirectoryService, DirectoryService>();


ConfigurationManager configuration = builder.Configuration;
services.Configure<ApiInjectorConfiguration>(configuration.GetSection(ApiInjectorConfiguration.Section));
services.Configure<ShironetConfiguration>(configuration.GetSection(ShironetConfiguration.Section));
services.Configure<TracksPathParserConfiguration>(configuration.GetSection(TracksPathParserConfiguration.Section));
services.Configure<TrackFileConfiguration>(configuration.GetSection(TrackFileConfiguration.Section));


CoconaApp app = builder.Build();

app.AddSubCommand("search", command =>
{
    command.AddCommand("query", (ILyricsFinderService service, CommonParameters commonParameters, QuerySearchParameters parameters) =>
        service.FindLyricsByQuery(commonParameters, parameters));
    command.AddCommand("directory", (ILyricsFinderService service, CommonParameters commonParameters, DirectorySearchParameters parameters) =>
        service.FindLyricsByDirectory(commonParameters, parameters));
    command.AddCommand("file", (ILyricsFinderService service, CommonParameters commonParameters, FileSearchParameters parameters) =>
        service.FindLyricsByFileAsync(commonParameters, parameters));
});

await app.RunAsync();