using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PlayerCountBot.Settings;
using RconSharp;

namespace PlayerCountBot.Processors
{
    public interface IArkStatusProcessor
    {
        Task ProcessStatusAsync(SocketGuild guild);
    }

    public class ArkStatusProcessor
        : IArkStatusProcessor
    {
        private readonly ILogger<ArkStatusProcessor> _logger;
        private readonly ArkSettings _arkSettings;

        public ArkStatusProcessor(ILogger<ArkStatusProcessor> logger, IOptions<ArkSettings> arkOptions)
        {
            _logger = logger;
            _arkSettings = arkOptions.Value;
        }

        public async Task ProcessStatusAsync(SocketGuild guild)
        {
            var channelName = "MAG Ark";

            var arkClient = RconClient.Create(_arkSettings.IpAddress, _arkSettings.Port ?? 0);
            await arkClient.ConnectAsync();

            var authenticated = await arkClient.AuthenticateAsync(_arkSettings.RconPassword);
            if (!authenticated)
            {
                _logger.LogError("[ArkStatusProcessor] Couldn't login to Ark RCON");

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
                var listPlayersResponse = await arkClient.ExecuteCommandAsync("listplayers");
                if (listPlayersResponse.Contains("No Players Connected"))
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
                    var lines = listPlayersResponse.Split('\n');
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
