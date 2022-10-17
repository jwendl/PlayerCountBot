// Install bot with https://discord.com/api/oauth2/authorize?client_id=1031378783489495071&permissions=16&scope=bot%20applications.commands
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PlayerCountBot;
using PlayerCountBot.Processors;
using PlayerCountBot.Settings;
using Timer = System.Timers.Timer;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false)
    .AddEnvironmentVariables()
    .Build();

var serviceCollection = new ServiceCollection();
serviceCollection.AddSingleton<IGameServerBot, GameServerBot>();
serviceCollection.AddSingleton<IMinecraftStatusProcessor, MinecraftStatusProcessor>();
serviceCollection.AddSingleton<IArkStatusProcessor, ArkStatusProcessor>();
serviceCollection.AddSingleton<IConanStatusProcessor, ConanStatusProcessor>();

serviceCollection.AddLogging(builder =>
{
    builder.AddConsole();
});

serviceCollection.Configure<DiscordSettings>(configuration.GetSection(nameof(DiscordSettings)));
serviceCollection.Configure<MinecraftSettings>(configuration.GetSection(nameof(MinecraftSettings)));
serviceCollection.Configure<ArkSettings>(configuration.GetSection(nameof(ArkSettings)));
serviceCollection.Configure<ConanSettings>(configuration.GetSection(nameof(ConanSettings)));
serviceCollection.Configure<RustSettings>(configuration.GetSection(nameof(RustSettings)));

var serviceProvider = serviceCollection.BuildServiceProvider();

var gameServerBot = serviceProvider.GetRequiredService<IGameServerBot>();
await gameServerBot.LoginAndStartAsync();

var timer = new Timer(10000);
timer.Elapsed += gameServerBot.PollInterval;
timer.Start();

await Task.Delay(-1);
