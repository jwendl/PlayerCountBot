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
        Task ProcessStatusAsync(SocketGuild guild);
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

        public async Task ProcessStatusAsync(SocketGuild guild)
        {
            var channelName = "MAG Factorio";
            _logger.LogInformation("Running processor for {Name}", channelName);

            var factorioClient = RconClient.Create(_factorioSettings.IpAddress, _factorioSettings.Port);
            await factorioClient.ConnectAsync();

            var authenticated = await factorioClient.AuthenticateAsync(_factorioSettings.RconPassword);
            if (!authenticated)
            {
                _logger.LogError("[FactorioStatusProcessor] Couldn't login to Conan RCON");

                var channel = guild.Channels.Where(scc => scc.Name.StartsWith(channelName)).First();
                var guildChannel = guild.GetChannel(channel.Id);
                await guildChannel.ModifyAsync(gcp =>
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
