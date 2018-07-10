using ChanDiscordBot.Bot;
using ChanDiscordBot.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ChanDiscordBot.Chan
{
    class ChanDataStore
    {
		public ChanDataStoreModel DataStore { get; private set; }
		public readonly string FilePath = "Data/ChanDataStore.json";

		public BaseResult Load()
		{
			DataStore = null;
			var result = new BaseResult();
			if (File.Exists(FilePath))
			{
				try
				{
					DataStore = HelperFunctions.ReadJsonFile<ChanDataStoreModel>(FilePath);
					if (DataStore == null)
					{
						result.Success = false;
						result.Message = "FAIL_LOAD_DATA_STORE";
					}
				}
				catch (Exception ex)
				{
					result.Success = false;
					result.Message = "FAIL_LOAD_DATA_STORE";
					ErrorLog.WriteError(ex);
					return result;
				}
			}
			else
			{
				try
				{
					DataStore = new ChanDataStoreModel();
					HelperFunctions.WriteJsonFile(FilePath, DataStore);
				}
				catch (Exception ex)
				{
					ErrorLog.WriteError(ex);
					result.Success = false;
					result.Message = "FAIL_CREATE_DATA_STORE";
					return result;
				}
			}
			result.Success = true;
			return result;
		}

		public BaseResult Save()
		{
			var result = new BaseResult();
			try
			{
				HelperFunctions.WriteJsonFile(FilePath, DataStore);
			}
			catch (Exception ex)
			{
				ErrorLog.WriteError(ex);
				result.Success = false;
				result.Message = "FAIL_SAVE_DATA_STORE";
			}
			result.Success = true;
			return result;
		}
	}

	[Serializable]
	public class ChanDataStoreModel
	{
		public List<long> PostIdsPosted = new List<long>();
	}
}
