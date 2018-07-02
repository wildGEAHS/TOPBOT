using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ChanDiscordBot.Bot
{
	public class RequireModRoleAttribute : PreconditionAttribute
	{
		public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
		{
			if (!(context.User is SocketGuildUser gUser))
				return Task.FromResult(PreconditionResult.FromError("Could not find user for command."));

			var hasPerm = false;
			foreach (var owner in ChanDiscordBot.BotConfig.BotInfo.OwnerIds)
			{
				if (owner == gUser.Id)
					hasPerm = true;
			}
			foreach (var permRole in ChanDiscordBot.BotConfig.BotInfo.CommandRoleIds)
			{
				var checkMatch = gUser.Roles.Where(r => r.Id == permRole).FirstOrDefault()?.Id;
				if (checkMatch.HasValue && checkMatch.Value != 0)
					hasPerm = true;
			}
			if (hasPerm)
				return Task.FromResult(PreconditionResult.FromSuccess());
			else
				return Task.FromResult(PreconditionResult.FromError("You do not have permission to run this command."));
		}
	}

	public class RequireGuildMessageAttribute : PreconditionAttribute
	{
		public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
		{
			if (!(context.Channel is SocketGuildChannel gChannel))
				return Task.FromResult(PreconditionResult.FromError("Command can only be used on a server"));

			return Task.FromResult(PreconditionResult.FromSuccess());
		}
	}
}
