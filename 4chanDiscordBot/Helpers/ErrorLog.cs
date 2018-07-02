using System;
using System.IO;

namespace ChanDiscordBot.Helpers
{
	public static class ErrorLog
	{
		private static readonly object _lockobj = new object();
		private static readonly string _folderLocation = string.Empty;
		static ErrorLog()
		{
			_folderLocation = "Data/Logs";
			if (!Directory.Exists(_folderLocation))
			{
				Directory.CreateDirectory(_folderLocation);
			}
		}

		public static void WriteLog(string log)
		{
			try
			{
				lock (_lockobj)
				{
					var dateString = DateTime.Now.ToString("yyyyMMdd");
					var path = $"Data/Logs/{dateString}_Errors.log";
					if (!File.Exists(path))
						using (File.Create(path)) { }
					using (var writer = File.AppendText(path))
					{
						var fullDateString = DateTime.Now.ToString("[yyyy-MM-dd hh:MM:ss]");
						writer.WriteLine($"{fullDateString} - {log}");
					}
				}
			}
			catch(Exception ex)
			{
				Console.WriteLine($"Shits fucked yo\n(Failed to write to error log: {ex.Message})");
			}
		}

		public static void WriteError(Exception ex)
		{
			try
			{
				lock (_lockobj)
				{
					var dateString = DateTime.Now.ToString("yyyyMMdd");
					var path = $"Data/Logs/{dateString}_Errors.log";
					if (!File.Exists(path))
						using (File.Create(path)) { }
					using (var writer = File.AppendText(path))
					{
						var fullDateString = DateTime.Now.ToString("[yyyy-MM-dd hh:MM:ss]");
						writer.WriteLine($"{fullDateString} - Message: {ex.Message}, Stack Trace: {ex.StackTrace}");
					}
				}
			}
			catch (Exception e)
			{
				Console.WriteLine($"Shits fucked yo\n(Failed to write to error log: {e.Message})");
			}
		}
	}
}
