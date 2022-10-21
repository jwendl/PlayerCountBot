using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PlayerCountBot.Settings;
using RconSharp;
using System.Text.RegularExpressions;

namespace PlayerCountBot.Processors
{
    public interface IFactorioStatusProcessor
    {
        Task ProcessStatusAsync(SocketCategoryChannel socketCategoryChannel);
    }

    public class FactorioStatusProcessor
        : IFactorioStatusProcessor
    {
        private readonly ILogger<FactorioStatusProcessor> _logger;
        private readonly FactorioSettings _factorioSettings;

        public FactorioStatusProcessor(ILogger<FactorioStatusProcessor> logger, IOptions<FactorioSettings> factorioOptions)
        {
            _logger = logger;
            _factorioSettings = factorioOptions.Value;
        }

        public async Task ProcessStatusAsync(SocketCategoryChannel socketCategoryChannel)
        {
            var channelName = "Factorio";
            _logger.LogInformation("Running processor for {Name}", channelName);

            var factorioClient = RconClient.Create(_factorioSettings.IpAddress, _factorioSettings.Port);
            await factorioClient.ConnectAsync();

            var authenticated = await factorioClient.AuthenticateAsync(_factorioSettings.RconPassword);
            if (!authenticated)
            {
                _logger.LogError("[FactorioStatusProcessor] Couldn't login to Conan RCON");

                var categoryChannel = socketCategoryChannel.Channels.Where(scc => scc.Name.Contains(channelName)).First();
                await categoryChannel.ModifyAsync(gcp =>
                {
                    gcp.Name = $"{channelName} Error";
                });
            }
            else
            {
                var currentPlayers = "0";
                var maxPlayers = "20";
                var factorioResponse = await factorioClient.ExecuteCommandAsync("/players online count");

                var regex = new Regex("Online players \\((?<CurrentPlayers>\\d+)\\)");
                var match = regex.Match(factorioResponse);
                currentPlayers = match.Groups["CurrentPlayers"].Value;

                var categoryChannel = socketCategoryChannel.Channels.Where(scc => scc.Name.Contains(channelName)).First();
                await categoryChannel.ModifyAsync(gcp =>
                {
                    gcp.Name = $"{currentPlayers}/{maxPlayers} {channelName}";
                });
            }

            factorioClient.Disconnect();
        }
    }
}
