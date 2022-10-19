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
        Task ProcessStatusAsync(SocketCategoryChannel socketCategoryChannel);
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

        public async Task ProcessStatusAsync(SocketCategoryChannel socketCategoryChannel)
        {
            var channelName = "Minecraft";
            _logger.LogInformation("Running processor for {Name}", channelName);

            var minecraftClient = new MinecraftClient(_minecraftSettings?.IpAddress!, _minecraftSettings?.Port ?? 0);
            if (!minecraftClient.Authenticate(_minecraftSettings?.RconPassword ?? string.Empty))
            {
                _logger.LogError("[MinecraftStatusProcessor] Couldn't login to Minecraft RCON");

                var categoryChannel = socketCategoryChannel.Channels.Where(scc => scc.Name.Contains(channelName)).First();
                await categoryChannel.ModifyAsync(gcp =>
                {
                    gcp.Name = $"{channelName} Error";
                });
            }
            else
            {
                if (!minecraftClient.SendCommand("list", out MinecraftMessage minecraftMessage))
                {
                    _logger.LogError("[MinecraftStatusProcessor] Couldn't send 'list' command to Minecraft RCON");

                    var categoryChannel = socketCategoryChannel.Channels.Where(scc => scc.Name.Contains(channelName)).First();
                    await categoryChannel.ModifyAsync(gcp =>
                    {
                        gcp.Name = $"{channelName} Error";
                    });
                }
                else
                {
                    var categoryChannel = socketCategoryChannel.Channels.Where(scc => scc.Name.Contains(channelName)).First();
                    await categoryChannel.ModifyAsync(gcp =>
                    {
                        var minecraftResponse = minecraftMessage.Body;
                        var regex = new Regex("§6There are §c(?<CurrentPlayers>\\d+)§6 out of maximum §c(?<MaxPlayers>\\d+)§6 players online.");
                        var match = regex.Match(minecraftResponse);
                        var currentPlayers = match.Groups["CurrentPlayers"].Value;
                        var maxPlayers = match.Groups["MaxPlayers"].Value;
                        gcp.Name = $"{currentPlayers}/{maxPlayers} {channelName}";
                    });
                }
            }
        }
    }
}
