using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PlayerCountBot.Settings;
using RconSharp;

namespace PlayerCountBot.Processors
{
    public interface IConanStatusProcessor
    {
        Task ProcessStatusAsync(SocketCategoryChannel socketCategoryChannel);
    }

    public class ConanStatusProcessor
        : IConanStatusProcessor
    {
        private readonly ILogger<ConanStatusProcessor> _logger;
        private readonly ConanSettings _conanSettings;

        public ConanStatusProcessor(ILogger<ConanStatusProcessor> logger, IOptions<ConanSettings> conanOptions)
        {
            _logger = logger;
            _conanSettings = conanOptions.Value;
        }

        public async Task ProcessStatusAsync(SocketCategoryChannel socketCategoryChannel)
        {
            var channelName = "Conan";
            _logger.LogInformation("Running processor for {Name}", channelName);

            var conanClient = RconClient.Create(_conanSettings.IpAddress, _conanSettings.Port);
            await conanClient.ConnectAsync();

            var authenticated = await conanClient.AuthenticateAsync(_conanSettings.RconPassword);
            if (!authenticated)
            {
                _logger.LogError("[ConanStatusProcessor] Couldn't login to Conan RCON");

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
                var listPlayersResponse = await conanClient.ExecuteCommandAsync("listplayers");
                var lines = listPlayersResponse.Split('\n');
                if (lines.Length == 2)
                {
                    var categoryChannel = socketCategoryChannel.Channels.Where(scc => scc.Name.Contains(channelName)).First();
                    await categoryChannel.ModifyAsync(gcp =>
                    {
                        gcp.Name = $"{currentPlayers}/{maxPlayers} {channelName}";
                    });
                }
                else
                {
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
