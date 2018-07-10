using ChanDiscordBot.Bot;
using ChanDiscordBot.Chan;
using ChanDiscordBot.Config;
using ChanDiscordBot.Helpers;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace ChanDiscordBot
{
	class ChanDiscordBot
	{
		public static DiscordSocketClient Client { get; private set; }
		public static BotConfig BotConfig { get; private set; }
		public static ChanDataStore ChanData { get; private set; }
		private static BotCommandHandler cHandler;
		private static BotCommandProcessor cProcessor;
		private static ChanUpdateThread ChanThread;
		public static bool Ready { get; set; }
		private static long ConnectedTimes = 0;

		static void Main(string[] args)
		{
			new ChanDiscordBot().RunBot().GetAwaiter().GetResult();
		}

		public async Task RunBot()
		{
			try
			{
				BotConfig = new BotConfig();
				var botCResult = BotConfig.LoadCredConfig();
				BotConfig.SaveCredConfig();

				#region 4chan Setup
				ChanData = new ChanDataStore();
				ChanData.Load();
				ChanData.Save();
				ChanThread = new ChanUpdateThread();
				ChanThread.Start();
				#endregion

				#region Discord Client
				//create new discord client and log
				Client = new DiscordSocketClient(new DiscordSocketConfig()
				{
					MessageCacheSize = 10,
					ConnectionTimeout = int.MaxValue,
					LogLevel = LogSeverity.Warning
				});
				Client.Connected += OnConnected;
				Client.Disconnected += OnDisconnected;
				Client.Log += (message) =>
				{
					Console.WriteLine($"Discord Error:{message.ToString()}");
					ErrorLog.WriteLog($"Discord Error:{message.ToString()}");
					return Task.CompletedTask;
				};
				await Client.LoginAsync(TokenType.Bot, BotConfig.BotInfo.Token);
				await Client.StartAsync();
				#endregion

				//Delay until application quit
				await Task.Delay(-1);

				#region Cleanup
				ChanThread.Stop();
				#endregion

				Console.WriteLine("Exiting");
			}
			catch (Exception ex)
			{
				ErrorLog.WriteError(ex);
				return;
			}
		}

		private async Task OnDisconnected(Exception arg)
		{
			try
			{
				await cHandler.RemoveCommandService();
				cHandler = null;
				cProcessor = null;
			}
			catch (Exception ex)
			{
				ErrorLog.WriteError(ex);
				Console.WriteLine(ex.Message);
			}

			Console.WriteLine("Disconnected");
			Console.WriteLine(arg.Message);
			Ready = false;
		}

		private async Task OnConnected()
		{
			var serviceProvider = ConfigureServices();
			await cHandler.InstallCommandService(serviceProvider);
			Ready = true;
			Console.WriteLine("Ready!");
			ConnectedTimes++;
		}

		private IServiceProvider ConfigureServices()
		{
			//setup and add command service.
			cHandler = new BotCommandHandler();
			cProcessor = new BotCommandProcessor();

			var services = new ServiceCollection()
				.AddSingleton(Client)
				.AddSingleton(BotConfig)
				.AddSingleton(cHandler);
			var provider = new DefaultServiceProviderFactory().CreateServiceProvider(services);
			return provider;
		}
	}
}
