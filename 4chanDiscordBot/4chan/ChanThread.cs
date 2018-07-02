using ChanDiscordBot.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace ChanDiscordBot.Chan
{
    public class ChanUpdateThread
    {
		private System.Timers.Timer _checkUpdates;
		private bool _isStarted = false;
		private ElapsedEventHandler updateAction;
		private DateTime lastUpdate = DateTime.UtcNow;
		private readonly Dictionary<long, ChanThreadModel> currentOps = new Dictionary<long, ChanThreadModel>();

		public void Start()
		{
			if (_isStarted)
				return;

			lastUpdate = DateTime.UtcNow.AddDays(-1);
			updateAction = (sender, e) => Task.Run(() => CheckForUpdate(sender, e));
			_isStarted = true;
			var updateTime = ChanDiscordBot.BotConfig.BotInfo.UpdateSeconds * 1000;
			if (updateTime < 15000)
				updateTime = 15000;
			_checkUpdates = new System.Timers.Timer(updateTime);
			_checkUpdates.Elapsed += updateAction;
			_checkUpdates.AutoReset = false;
			_checkUpdates.Start();
		}

		public void Stop()
		{
			if (!_isStarted)
				return;

			try
			{
				_checkUpdates.Elapsed -= updateAction;
				_checkUpdates.Stop();
				_checkUpdates.Dispose();
			}
			catch (Exception ex) { ErrorLog.WriteError(ex); }
			_checkUpdates = null;
			updateAction = null;
			_isStarted = false;
		}

		private async Task CheckForUpdate(object sender, ElapsedEventArgs e)
		{
			lastUpdate = DateTime.UtcNow;
			for (var i = 0; i < ChanDiscordBot.BotConfig.BotInfo.BoardsToCheck.Count; ++i)
			{
				var b = ChanDiscordBot.BotConfig.BotInfo.BoardsToCheck[i];
				//4chan requests a limit for thread updates at 10 seconds at least
				if (i > 0)
					await Task.Delay(10000);

				var catalog = await ChanApi.GetCatalog(b.BoardName, lastUpdate);
				List<ChanThreadModel> threads = new List<ChanThreadModel>();
				foreach(var p in catalog.pages)
				{
					try
					{
						threads.AddRange(p.threads.Where(x => b.NamesToSearch.Any(n => !string.IsNullOrWhiteSpace(x.sub) ?
							x.sub.ToUpperInvariant().Contains(n.ToUpperInvariant()) :
							x.name.ToUpperInvariant().Contains(n.ToUpperInvariant()))));
					}
					catch(Exception ex)
					{
						ErrorLog.WriteError(ex);
					}
				}
				//look into handling multiple in the future if its needed.
				ChanThreadModel thread = threads.First();
				foreach(var t in threads)
				{
					if (t.TimeStamp > thread.TimeStamp)
						thread = t;
				}

				//$"https://i.4cdn.org/{b.BoardName}/{thread.tim}{thread.ext}"
			}

			var updateTime = ChanDiscordBot.BotConfig.BotInfo.UpdateSeconds * 1000;
			if (updateTime < 15000)
				updateTime = 15000;
			_checkUpdates.Interval = updateTime;
		}
    }
}
