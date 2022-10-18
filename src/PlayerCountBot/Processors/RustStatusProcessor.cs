using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PlayerCountBot.Clients.Rust;
using PlayerCountBot.Clients.Rust.Commands.Server;
using PlayerCountBot.Settings;

namespace PlayerCountBot.Processors
{
    public interface IRustStatusProcessor
    {
        Task ProcessStatusAsync(SocketGuild guild);
    }

    public class RustStatusProcessor
        : IRustStatusProcessor
    {
        private readonly ILogger<RustStatusProcessor> _logger;
        private readonly RustSettings _rustSettings;
        private readonly IRustClient _rustClient;

        public RustStatusProcessor(ILogger<RustStatusProcessor> logger,IRustClient rustClient, IOptions<RustSettings> rustOptions)
        {
            _logger = logger;
            _rustClient = rustClient;
            _rustSettings = rustOptions.Value;
        }

        public async Task ProcessStatusAsync(SocketGuild guild)
        {
            var channelName = "MAG Rust";
            _logger.LogInformation("Running processor for {Name}", channelName);

            _rustClient.Connect(_rustSettings.IpAddress!, _rustSettings.Port, _rustSettings.RconPassword!);

            _rustClient.SendCommand(new GetServerInformation(async (serverInfo) =>
            {
                var channel = guild.Channels.Where(scc => scc.Name.StartsWith(channelName)).First();
                var guildChannel = guild.GetChannel(channel.Id);
                await guildChannel.ModifyAsync(gcp =>
                {
                    gcp.Name = $"{channelName} {serverInfo.Players}/{serverInfo.MaxPlayers}";
                });
            }));

            await Task.FromResult(1);
        }
    }
}
