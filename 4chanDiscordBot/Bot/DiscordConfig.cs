using Newtonsoft.Json;
using ChanDiscordBot.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using ChanDiscordBot.Bot;

namespace ChanDiscordBot.Config
{
	public class BotConfig
	{
		public BotConfigModel BotInfo { get; private set; }
		public readonly string FilePath = "Data/Config.json";

		public BaseResult LoadCredConfig()
		{
			BotInfo = null;
			var result = new BaseResult();
			if (File.Exists(FilePath))
			{
				try
				{
					BotInfo = HelperFunctions.ReadJsonFile<BotConfigModel>(FilePath);
					if (BotInfo == null)
					{
						result.Success = false;
						result.Message = "FAIL_LOAD_CONFIG";
					}
					//The 4chan api documentation says to not make thread requests more than once every 10 seconds so im being safe.
					if (BotInfo.UpdateSeconds < 15)
						BotInfo.UpdateSeconds = 15;
				}
				catch (Exception ex)
				{
					result.Success = false;
					result.Message = "FAIL_LOAD_CONFIG";
					ErrorLog.WriteError(ex);
					return result;
				}
			}
			else
			{
				try
				{
					BotInfo = new BotConfigModel();
					HelperFunctions.WriteJsonFile(FilePath, BotInfo);
				}
				catch (Exception ex)
				{
					ErrorLog.WriteError(ex);
					result.Success = false;
					result.Message = "FAIL_CREATE_CONFIG";
					return result;
				}
			}
			result.Success = true;
			return result;
		}

		public BaseResult SaveCredConfig()
		{
			var result = new BaseResult();
			try
			{
				HelperFunctions.WriteJsonFile(FilePath, BotInfo);
			}
			catch (Exception ex)
			{
				ErrorLog.WriteError(ex);
				result.Success = false;
				result.Message = "FAIL_SAVE_CONFIG";
			}
			result.Success = true;
			return result;
		}
	}

	[Serializable]
	public class BotConfigModel
	{
		public string Token = "";
		public ulong ClientId = 0;
		public ulong BotId = 0;
		public List<ulong> OwnerIds = new List<ulong>();
		public List<ulong> CommandRoleIds = new List<ulong>();
		public int UpdateSeconds = 30;
		public List<BoardToCheck> BoardsToCheck = new List<BoardToCheck>();
		public List<ServerToSend> ServersToSend = new List<ServerToSend>();
	}

	[Serializable]
	public class ServerToSend
	{
		public ulong ServerId;
		public List<ulong> ChannelIds;
		public List<string> Boards;
	}

	[Serializable]
	public class BoardToCheck
	{
		public string BoardName = "";
		public List<string> NamesToSearch = new List<string>();
	}
}
