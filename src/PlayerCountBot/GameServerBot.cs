using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PlayerCountBot.Processors;
using PlayerCountBot.Settings;
using System.Timers;

namespace PlayerCountBot
{
	public interface IGameServerBot
	{
		Task LoginAndStartAsync(string discordBotName, string discordBotToken, string channelCategory);

		void PollInterval(object? sender, ElapsedEventArgs elapsedEventArgs);
	}

	public class GameServerBot
		: IGameServerBot
	{
		private readonly DiscordSocketClient _discordSocketClient;
		private readonly IMinecraftStatusProcessor _minecraftStatusProcessor;
		private readonly IConanStatusProcessor _conanStatusProcessor;
		private readonly IRustStatusProcessor _rustStatusProcessor;
		private readonly IArkStatusProcessor _arkStatusProcessor;
		private readonly IFactorioStatusProcessor _factorioStatusProcessor;
		private readonly ILogger<GameServerBot> _logger;

		private string _guildName = default!;
		private string _channelCategory = default!;

		public GameServerBot(
			DiscordSocketClient discordSocketClient,
			IMinecraftStatusProcessor minecraftStatusProcessor,
			IConanStatusProcessor conanStatusProcessor,
			IRustStatusProcessor rustStatusProcessor,
			IArkStatusProcessor arkStatusProcessor,
			IFactorioStatusProcessor factorioStatusProcessor,
			ILogger<GameServerBot> logger)
		{
			_discordSocketClient = discordSocketClient;
			_minecraftStatusProcessor = minecraftStatusProcessor;
			_conanStatusProcessor = conanStatusProcessor;
			_rustStatusProcessor = rustStatusProcessor;
			_arkStatusProcessor = arkStatusProcessor;
			_factorioStatusProcessor = factorioStatusProcessor;
			_logger = logger;
		}

		public async Task LoginAndStartAsync(string discordBotName, string discordBotToken, string channelCategory)
		{
			_guildName = discordBotName;
			_channelCategory = channelCategory;

			_logger.LogInformation("Logging into {DiscordBotName}", discordBotName);

			await _discordSocketClient.LoginAsync(TokenType.Bot, discordBotToken);
			await _discordSocketClient.StartAsync();

			_logger.LogInformation("Logged into {DiscordBotName}", discordBotName);
		}

		public async void PollInterval(object? sender, ElapsedEventArgs elapsedEventArgs)
		{
			if (_discordSocketClient.ConnectionState == ConnectionState.Connected && _discordSocketClient.LoginState == LoginState.LoggedIn)
			{
				var guild = _discordSocketClient.Guilds.Where(g => g.Name.Contains(_guildName)).FirstOrDefault();
				await PublishGameStatus(_channelCategory, guild);
			}
		}

		private async Task PublishGameStatus(string channelCategory, SocketGuild? guild)
		{
			if (guild != null)
			{
				_logger.LogInformation("Using guild {Name} with Id {Id}", guild.Name, guild.Id);

				var categoryChannel = guild.CategoryChannels.Where(scc => scc.Name.Contains(channelCategory, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
				_logger.LogInformation("Checking to see if category named {Name} exists", categoryChannel?.Name);

				if (categoryChannel == null)
				{
					var createdCategoryChannel = await guild.CreateCategoryChannelAsync(channelCategory);

					_logger.LogInformation("Created category {ChannelCategory}", channelCategory);
				}
				categoryChannel = guild.CategoryChannels.Where(scc => scc.Name.Contains(channelCategory, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

				await CreateOrUpdateChannelAsync(guild, categoryChannel!, "Minecraft");
				await CreateOrUpdateChannelAsync(guild, categoryChannel!, "Ark");
				await CreateOrUpdateChannelAsync(guild, categoryChannel!, "Conan");
				await CreateOrUpdateChannelAsync(guild, categoryChannel!, "Rust");
				await CreateOrUpdateChannelAsync(guild, categoryChannel!, "Factorio");

				try
				{
					var tasks = new List<Task>()
						{
							_minecraftStatusProcessor.ProcessStatusAsync(categoryChannel!),
							_conanStatusProcessor.ProcessStatusAsync(categoryChannel!),
							_rustStatusProcessor.ProcessStatusAsync(categoryChannel!),
							_arkStatusProcessor.ProcessStatusAsync(categoryChannel!),
							_factorioStatusProcessor.ProcessStatusAsync(categoryChannel!),
						};

					await Task.WhenAll(tasks);
				}
				catch (AggregateException aggregateException)
				{
					foreach (var exception in aggregateException?.InnerExceptions!)
					{
						_logger.LogError(exception, "[GameServerBot] {Message}", exception.Message);
					}
				}
				catch (Exception exception)
				{
					_logger.LogError(exception, "[GameServerBot] {Message}", exception.Message);
				}
			}
		}

		private async Task CreateOrUpdateChannelAsync(SocketGuild guild, SocketCategoryChannel categoryChannel, string channelName)
		{
			var currentChannel = guild.Channels.Where(sgc => sgc.Name.Contains(channelName)).FirstOrDefault();

			if (currentChannel == null)
			{
				await guild.CreateVoiceChannelAsync($"{channelName} Connecting", (vcp) =>
				{
					vcp.CategoryId = categoryChannel.Id;
				});
				_logger.LogInformation("Created channel {Name}", channelName);
			}
		}
	}
}
