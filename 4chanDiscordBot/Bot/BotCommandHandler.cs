using Discord;
using Discord.Commands;
using Discord.WebSocket;
using ChanDiscordBot.Config;
using ChanDiscordBot.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace ChanDiscordBot.Bot
{
	public class BotCommandHandler
	{
		private CommandService commands;
		private DiscordSocketClient Client;
		private BotCommandProcessor processor;
		private IServiceProvider map;
		private BotCommandsRunning running;

		public async Task InstallCommandService(IServiceProvider _map)
		{
			Client = _map.GetService(typeof(DiscordSocketClient)) as DiscordSocketClient;
			processor = _map.GetService(typeof(BotCommandProcessor)) as BotCommandProcessor;
			running = new BotCommandsRunning();
			commands = new CommandService();
			map = _map;
			await commands.AddModulesAsync(Assembly.GetEntryAssembly(), _map);
			Client.MessageReceived += HandleCommandDelgate;
		}

		public async Task<bool> RemoveCommandService()
		{
			var modules = commands.Modules.ToList();
			for (var i = modules.Count - 1; i > -1; --i)
			{
				await commands.RemoveModuleAsync(modules[i]);
			}
			running.Clear();
			Client.MessageReceived -= HandleCommandDelgate;
			return Task.FromResult<bool>(true).Result;
		}

		//Im doing this through another function so that it can just do a task.run so it can actually run async instead of waiting.
		public Task HandleCommandDelgate(SocketMessage m)
		{
			Task.Run(() => { HandleCommand(m).ConfigureAwait(false); });
			return Task.CompletedTask;
		}

		public async Task HandleCommand(SocketMessage e)
		{
			if (!(e is SocketUserMessage uMessage)) return;
			int argPos = 0;
			if (uMessage.HasMentionPrefix(Client.CurrentUser, ref argPos))
			{
				var context = new CommandContext(Client, uMessage);
				var result = await commands.ExecuteAsync(context, argPos, map);
				if (!result.IsSuccess)
					await context.Channel.SendMessageAsync(result.ErrorReason);
			}
		}
	}

	[RequireGuildMessage]
	[RequireModRole]
	public class AdminModule : ModuleBase
	{
		private readonly BotCommandProcessor processor;

		public AdminModule(IServiceProvider m)
		{
			processor = m.GetService(typeof(BotCommandProcessor)) as BotCommandProcessor;
		}

		[Command("shutdown"), Summary("Tells the bot to shutdown.")]
		public async Task Shutdown()
		{
			await Context.Channel.SendMessageAsync("Goodbye").ConfigureAwait(false);
			await Context.Client.StopAsync();
			await Task.Delay(2000).ConfigureAwait(false);
			Environment.Exit(0);
		}

		[Command("restart"), Summary("Tells the bot to restart")]
		public async Task Restart()
		{
			await Context.Channel.SendMessageAsync("Restarting...");
			await Context.Client.StopAsync();
			await Task.Delay(2000);
			System.Diagnostics.Process.Start(Assembly.GetExecutingAssembly().Location);
			Environment.Exit(0);
		}

#if DEBUG
		[Command("disconnect"), Summary("For debug purpose, disconnects bot")]
		public async Task RemoveCommands()
		{
			await Context.Client.StopAsync();
			return;
		}

		[Command("wait"), Summary("For debug purpose, waits x seconds")]
		public async Task WaitSeconds(int seconds)
		{
			Thread.Sleep(seconds * 1000);
			await Context.Channel.SendMessageAsync($"Waited {seconds} seconds");
			return;
		}
#endif
	}

	public class BotCommandsRunning
	{
		private Dictionary<Guid, SocketTextChannel> _commands;
		private object _commandsLock;
		private readonly TimerCallback timerCallback;
		private Timer timer;

		public BotCommandsRunning()
		{
			_commands = new Dictionary<Guid, SocketTextChannel>();
			timerCallback = CycleCommandCheck;
			timer = new Timer(timerCallback, null, 0, 5000);
			_commandsLock = new object();
		}

		private void CycleCommandCheck(object objectInfo)
		{
			if (_commands != null && _commands.Count > 0)
			{
				lock (_commandsLock)
				{
					foreach (var c in _commands)
					{
						try
						{
							if(c.Value != null)
								c.Value.TriggerTypingAsync();
						}
						catch (Exception ex)
						{
							ErrorLog.WriteError(ex);
							continue;
						}
					}
				}
			}
		}

		public void Add(Guid key, SocketTextChannel value)
		{
			lock (_commandsLock)
			{
				_commands.Add(key, value);
			}
		}

		public void Remove(Guid key)
		{
			lock (_commandsLock)
			{
				_commands.Remove(key);
			}
		}

		public void Clear()
		{
			lock(_commandsLock)
			{
				_commands.Clear();
			}
		}

		public void ResetCommandsTimer()
		{
			lock (_commandsLock)
			{
				_commands = new Dictionary<Guid, SocketTextChannel>();
				timer.Dispose();
			}
			_commandsLock = new object();
		}
	}
}
