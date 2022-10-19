using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PlayerCountBot;
using PlayerCountBot.Clients.Rust;
using PlayerCountBot.Processors;
using PlayerCountBot.Settings;
using Timer = System.Timers.Timer;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false)
    .AddEnvironmentVariables()
    .Build();

var serviceCollection = new ServiceCollection();
serviceCollection.AddSingleton<DiscordSocketClient>();
serviceCollection.AddSingleton<IRustClient, RustClient>();

serviceCollection.AddSingleton<IGameServerBot, GameServerBot>();
serviceCollection.AddSingleton<IMinecraftStatusProcessor, MinecraftStatusProcessor>();
serviceCollection.AddSingleton<IFactorioStatusProcessor, FactorioStatusProcessor>();
serviceCollection.AddSingleton<IConanStatusProcessor, ConanStatusProcessor>();
serviceCollection.AddSingleton<IRustStatusProcessor, RustStatusProcessor>();
serviceCollection.AddSingleton<IArkStatusProcessor, ArkStatusProcessor>();

serviceCollection.AddLogging(builder =>
{
    builder.AddConsole();
});

serviceCollection.Configure<DiscordSettings>(configuration.GetSection(nameof(DiscordSettings)));

serviceCollection.Configure<MinecraftSettings>(configuration.GetSection(nameof(MinecraftSettings)));
serviceCollection.Configure<FactorioSettings>(configuration.GetSection(nameof(FactorioSettings)));
serviceCollection.Configure<ConanSettings>(configuration.GetSection(nameof(ConanSettings)));
serviceCollection.Configure<RustSettings>(configuration.GetSection(nameof(RustSettings)));
serviceCollection.Configure<ArkSettings>(configuration.GetSection(nameof(ArkSettings)));

var serviceProvider = serviceCollection.BuildServiceProvider();

var discordOptions = serviceProvider.GetRequiredService<IOptions<DiscordSettings>>();
var discordSettings = discordOptions.Value;

var gameServerBot = serviceProvider.GetRequiredService<IGameServerBot>();
await gameServerBot.LoginAndStartAsync();

var timer = new Timer(TimeSpan.FromSeconds(discordSettings.PollInterval).TotalMilliseconds);
timer.Elapsed += gameServerBot.PollInterval;
timer.Start();

var discordSocketClient = serviceProvider.GetRequiredService<DiscordSocketClient>();
var processor = serviceProvider.GetRequiredService<IFactorioStatusProcessor>();

await Task.Delay(-1);
