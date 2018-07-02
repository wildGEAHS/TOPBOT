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
						//If the thread doesn't have a title just ignore it
						threads.AddRange(p.threads.Where(x => b.NamesToSearch.Any(n => !string.IsNullOrWhiteSpace(x.sub) && x.sub.ToUpperInvariant().Contains(n.ToUpperInvariant()))));
					}
					catch(Exception ex)
					{
						ErrorLog.WriteError(ex);
					}
				}

				//Find the closest match and latest thread to what we are searching for.
				//We need to check the distance of each threads title to all the names we are searching for
				// to find the closest match to the names.
				ChanThreadModel thread = null;
				int bestDistance = int.MaxValue;
				foreach(var t in threads)
				{
					foreach (var name in b.NamesToSearch)
					{
						var distance = HelperFunctions.LevenshteinDistance(name, t.sub);
						if (distance < bestDistance)
						{
							//Im not breaking after this because it could still find and mark a better distance.
							bestDistance = distance;
							thread = t;
						}
					}
				}

				//Now we want to find the old thread in our update list if its there so we can replace it
				// if this is a more recent one.
				ChanThreadModel oldThread = null;
				bestDistance = int.MaxValue;
				foreach (var t in currentOps)
				{
					var distance = HelperFunctions.LevenshteinDistance(t.Value.sub, thread.sub);
					if (distance < bestDistance && thread.sub.ToUpper().Contains(t.Value.sub.ToUpper()))
						oldThread = t.Value;
				}
				//If we found an oldthread and its timestamp is less than the new one remove it and add the new one.
				if (oldThread != null && oldThread.TimeStamp < thread.TimeStamp)
				{
					currentOps.Remove(oldThread.no);
					currentOps.Add(thread.no, thread);
				}
				//if we found an oldthread and it doesnt have a new timestamp just use the old one.
				else if(oldThread != null)
				{
					thread = oldThread;
				}
				//If there were no old threads jsut add this as a new one
				else
				{
					currentOps.Add(thread.no, thread);
				}

				//Wait before we request the actual thread data
				await Task.Delay(10000);

				//$"https://i.4cdn.org/{b.BoardName}/{thread.tim}{thread.ext}"
			}

			var updateTime = ChanDiscordBot.BotConfig.BotInfo.UpdateSeconds * 1000;
			if (updateTime < 15000)
				updateTime = 15000;
			_checkUpdates.Interval = updateTime;
		}
    }
}
