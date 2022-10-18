using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PlayerCountBot.Clients.Rust.Commands.Messages;
using PlayerCountBot.Processors;
using PlayerCountBot.Settings;
using System.Timers;

namespace PlayerCountBot
{
    public interface IGameServerBot
    {
        Task LoginAndStartAsync();
        void PollInterval(object? sender, ElapsedEventArgs elapsedEventArgs);
    }

    public class GameServerBot
        : IGameServerBot
    {
        private readonly DiscordSocketClient _discordSocketClient;
        private readonly DiscordSettings _discordSettings;
        private readonly IMinecraftStatusProcessor _minecraftStatusProcessor;
        private readonly IConanStatusProcessor _conanStatusProcessor;
        private readonly IRustStatusProcessor _rustStatusProcessor;
        private readonly IArkStatusProcessor _arkStatusProcessor;
        private readonly ILogger<GameServerBot> _logger;

        public GameServerBot(
            IOptions<DiscordSettings> discordOptions,
            IMinecraftStatusProcessor minecraftStatusProcessor,
            IConanStatusProcessor conanStatusProcessor,
            IRustStatusProcessor rustStatusProcessor,
            IArkStatusProcessor arkStatusProcessor,
            ILogger<GameServerBot> logger)
        {
            _discordSocketClient = new DiscordSocketClient();
            _discordSettings = discordOptions.Value;
            _minecraftStatusProcessor = minecraftStatusProcessor;
            _conanStatusProcessor = conanStatusProcessor;
            _rustStatusProcessor = rustStatusProcessor;
            _arkStatusProcessor = arkStatusProcessor;
            _logger = logger;
        }

        public async Task LoginAndStartAsync()
        {
            await _discordSocketClient.LoginAsync(TokenType.Bot, _discordSettings.Token);
            await _discordSocketClient.StartAsync();

            _logger.LogInformation($"Logged into Discord");
        }

        public async void PollInterval(object? sender, ElapsedEventArgs elapsedEventArgs)
        {
            if (_discordSocketClient.ConnectionState == ConnectionState.Connected && _discordSocketClient.LoginState == LoginState.LoggedIn)
            {
                var guild = _discordSocketClient.Guilds.First();
                _logger.LogInformation("Using guild {Name} with Id {Id}", guild.Name, guild.Id);

                var categoryChannel = guild.CategoryChannels.Where(scc => scc.Name.Contains("MAG Servers", StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                _logger.LogInformation("Checking to see if category named {Name} exists", categoryChannel?.Name);

                if (categoryChannel == null)
                {
                    var createdCategoryChannel = await guild.CreateCategoryChannelAsync("MAG Servers");

                    _logger.LogInformation($"Created category MAG Servers");
                }
                categoryChannel = guild.CategoryChannels.Where(scc => scc.Name.Contains("MAG Servers", StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

                await CreateOrUpdateChannelAsync(guild, categoryChannel!, "MAG Minecraft");
                await CreateOrUpdateChannelAsync(guild, categoryChannel!, "MAG Ark");
                await CreateOrUpdateChannelAsync(guild, categoryChannel!, "MAG Conan");
                await CreateOrUpdateChannelAsync(guild, categoryChannel!, "MAG Rust");

                await _minecraftStatusProcessor.ProcessStatusAsync(guild);
                await _conanStatusProcessor.ProcessStatusAsync(guild);
                await _rustStatusProcessor.ProcessStatusAsync(guild);
                await _arkStatusProcessor.ProcessStatusAsync(guild);
            }
        }

        private async Task CreateOrUpdateChannelAsync(SocketGuild guild, SocketCategoryChannel categoryChannel, string channelName)
        {
            var currentChannel = guild.Channels.Where(sgc => sgc.Name.Contains(channelName)).FirstOrDefault();

            if (currentChannel == null)
            {
                var minecraftChannel = await guild.CreateVoiceChannelAsync($"{channelName} Connecting", (vcp) =>
                {
                    vcp.CategoryId = categoryChannel.Id;
                });
                _logger.LogInformation("Created channel {Name}", channelName);
            }
        }
    }
}
