using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PlayerCountBot.Settings;
using RconSharp;

namespace PlayerCountBot.Processors
{
    public interface IConanStatusProcessor
    {
        Task ProcessStatusAsync(SocketGuild guild);
    }

    public class ConanStatusProcessor
        : IConanStatusProcessor
    {
        private readonly ILogger<ConanStatusProcessor> _logger;
        private readonly ConanSettings _conanSettings;

        public ConanStatusProcessor(ILogger<ConanStatusProcessor> logger, IOptions<ConanSettings> conanSettings)
        {
            _logger = logger;
            _conanSettings = conanSettings.Value;
        }

        public async Task ProcessStatusAsync(SocketGuild guild)
        {
            var channelName = "MAG Conan";

            var conanClient = RconClient.Create(_conanSettings.IpAddress, _conanSettings.Port ?? 0);
            await conanClient.ConnectAsync();

            var authenticated = await conanClient.AuthenticateAsync(_conanSettings.RconPassword);
            if (!authenticated)
            {
                _logger.LogError("Couldn't login to Conan RCON");

                var channel = guild.Channels.Where(scc => scc.Name.StartsWith(channelName)).First();
                var guildChannel = guild.GetChannel(channel.Id);
                await guildChannel.ModifyAsync(gcp =>
                {
                    gcp.Name = $"{channelName} Error";
                });
            }
            else
            {
                var currentPlayers = 0;
                var maxPlayers = 70;
                var listPlayers = await conanClient.ExecuteCommandAsync("listplayers");
                var lines = listPlayers.Split('\n');
                if (lines.Length == 2)
                {
                    var channel = guild.Channels.Where(scc => scc.Name.StartsWith(channelName)).First();
                    var guildChannel = guild.GetChannel(channel.Id);
                    await guildChannel.ModifyAsync(gcp =>
                    {
                        gcp.Name = $"{channelName} {currentPlayers}/{maxPlayers}";
                    });
                }
                else
                {
                    currentPlayers = lines.Length - 2;
                    if (currentPlayers < 0) currentPlayers = 0;

                    var channel = guild.Channels.Where(scc => scc.Name.StartsWith(channelName)).First();
                    var guildChannel = guild.GetChannel(channel.Id);
                    await guildChannel.ModifyAsync(gcp =>
                    {
                        gcp.Name = $"{channelName} {currentPlayers}/{maxPlayers}";
                    });
                }
            }
        }
    }
}
