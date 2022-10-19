using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PlayerCountBot.Settings;
using RconSharp;

namespace PlayerCountBot.Processors
{
    public interface IArkStatusProcessor
    {
        Task ProcessStatusAsync(SocketCategoryChannel socketCategoryChannel);
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

        public async Task ProcessStatusAsync(SocketCategoryChannel socketCategoryChannel)
        {
            var channelName = "Ark";
            _logger.LogInformation("Running processor for {Name}", channelName);

            var arkClient = RconClient.Create(_arkSettings.IpAddress, _arkSettings.Port);
            await arkClient.ConnectAsync();

            var authenticated = await arkClient.AuthenticateAsync(_arkSettings.RconPassword);
            if (!authenticated)
            {
                _logger.LogError("[ArkStatusProcessor] Couldn't login to Ark RCON");

                var categoryChannel = socketCategoryChannel.Channels.Where(scc => scc.Name.Contains(channelName)).First();
                await categoryChannel.ModifyAsync(gcp =>
                {
                    gcp.Name = $"{channelName} Error";
                });
            }
            else
            {
                var currentPlayers = 0;
                var maxPlayers = 70;
                var arkResponse = await arkClient.ExecuteCommandAsync("listplayers");
                if (arkResponse.Contains("No Players Connected"))
                {
                    var categoryChannel = socketCategoryChannel.Channels.Where(scc => scc.Name.Contains(channelName)).First();
                    await categoryChannel.ModifyAsync(gcp =>
                    {
                        gcp.Name = $"{currentPlayers}/{maxPlayers} {channelName}";
                    });
                }
                else
                {
                    var lines = arkResponse.Split('\n');
                    currentPlayers = lines.Length - 2;
                    if (currentPlayers < 0) currentPlayers = 0;

                    var categoryChannel = socketCategoryChannel.Channels.Where(scc => scc.Name.Contains(channelName)).First();
                    await categoryChannel.ModifyAsync(gcp =>
                    {
                        gcp.Name = $"{currentPlayers}/{maxPlayers} {channelName}";
                    });
                }
            }
        }
    }
}
