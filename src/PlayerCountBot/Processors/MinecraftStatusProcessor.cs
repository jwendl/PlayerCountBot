using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PlayerCountBot.Clients.Minecraft;
using PlayerCountBot.Clients.Minecraft.Models;
using PlayerCountBot.Settings;
using System.Text.RegularExpressions;

namespace PlayerCountBot.Processors
{
    public interface IMinecraftStatusProcessor
    {
        Task ProcessStatusAsync(SocketGuild guild);
    }

    public class MinecraftStatusProcessor
        : IMinecraftStatusProcessor
    {
        private readonly ILogger<MinecraftStatusProcessor> _logger;
        private readonly MinecraftSettings _minecraftSettings;

        public MinecraftStatusProcessor(ILogger<MinecraftStatusProcessor> logger, IOptions<MinecraftSettings> minecraftOptions)
        {
            _logger = logger;
            _minecraftSettings = minecraftOptions.Value;
        }

        public async Task ProcessStatusAsync(SocketGuild guild)
        {
            var channelName = "MAG Minecraft";
            var minecraftClient = new MinecraftClient(_minecraftSettings?.IpAddress!, _minecraftSettings?.Port ?? 0);
            if (!minecraftClient.Authenticate(_minecraftSettings?.RconPassword ?? string.Empty))
            {
                _logger.LogError("[MinecraftStatusProcessor] Couldn't login to Minecraft RCON");

                var channel = guild.Channels.Where(scc => scc.Name.StartsWith(channelName)).First();
                var guildChannel = guild.GetChannel(channel.Id);
                await guildChannel.ModifyAsync(gcp =>
                {
                    gcp.Name = $"{channelName} Error";
                });
            }
            else
            {
                if (!minecraftClient.SendCommand("list", out MinecraftMessage minecraftMessage))
                {
                    _logger.LogError("[MinecraftStatusProcessor] Couldn't send 'list' command to Minecraft RCON");

                    var channel = guild.Channels.Where(scc => scc.Name.StartsWith(channelName)).First();
                    var guildChannel = guild.GetChannel(channel.Id);
                    await guildChannel.ModifyAsync(gcp =>
                    {
                        gcp.Name = $"{channelName} Error";
                    });
                }
                else
                {
                    var channel = guild.Channels.Where(scc => scc.Name.StartsWith(channelName)).First();
                    var guildChannel = guild.GetChannel(channel.Id);
                    await guildChannel.ModifyAsync(gcp =>
                    {
                        var minecraftResponse = minecraftMessage.Body;
                        var regex = new Regex("§6There are §c(?<CurrentPlayers>\\d+)§6 out of maximum §c(?<MaxPlayers>\\d+)§6 players online.");
                        var match = regex.Match(minecraftResponse);
                        var currentPlayers = match.Groups["CurrentPlayers"].Value;
                        var maxPlayers = match.Groups["MaxPlayers"].Value;
                        gcp.Name = $"{channelName} {currentPlayers}/{maxPlayers}";
                    });
                }
            }
        }
    }
}
