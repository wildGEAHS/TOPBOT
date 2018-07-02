using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

//https://github.com/4chan/4chan-API
namespace ChanDiscordBot.Chan
{
    public class CatalogModel
    {
		public ChanCatalogPageModel[] pages;
	}

	public class ChanCatalogPageModel
	{
		public int page;
		public ChanThreadModel[] threads;
	}

	public class ChanThreadModel
	{
		public int bumplimit; //Bump limit met? 0 (no), 1 (yes)
		public string com; //Comment (includes escaped HTML)
		public int custom_spoiler; //Custom spoilers? 1-99
		public string ext; //File extension .jpg, .png, .gif, .pdf, .swf, .webm
		public string filename; //Original filename
		public long fsize; //File size 0-10485760
		public int h; //Image height 1-10000
		public int imagelimit; //Image limit met? 0 (no), 1 (yes)
		public int images; //# images total 0-99999
		public int last_modified; //Time when last modified UNIX timestamp
		public ChanLastReplyModel[] last_replies;
		public string md5; //File MD5 24 character, packed base64 MD5 hash
		public string name; //Name
		public long no; //Post number 1-9999999999999
		public string now; //Date and time MM/DD/YY(Day)HH:MM (:SS on some boards), EST/EDT timezone
		public int omitted_images; //# image replies omitted 1-10000
		public int omitted_posts; //# replies omitted 1-10000
		public int replies; //# replies total 0-99999
		public long resto; //Reply to 0 (is a thread OP), 1-9999999999999
		public string semantic_url; //Thread URL slug
		public string sub; //Subject
		public long tim; //Renamed filename UNIX timestamp + milliseconds
		public long time; //UNIX timestamp
		[JsonIgnore]
		public DateTime TimeStamp => DateTimeOffset.FromUnixTimeSeconds(time).UtcDateTime;
		public int tn_h; //Thumbnail height 1-250
		public int tn_w; //Thumbnail width 1-250
		public int unique_ips; //No documentation ¯\_(ツ)_/¯ Should be self explanatory
		public int w; //Image width 1-10000
	}

	public class ChanLastReplyModel
	{
		public string com; //Comment (includes escaped HTML)
		public string ext; //File extension .jpg, .png, .gif, .pdf, .swf, .webm
		public string filename; //Original filename
		public long fsize; //File size 0-10485760
		public int h; //Image height 1-10000
		public string md5; //File MD5 24 character, packed base64 MD5 hash
		public string name; //Name
		public long no; //Post number 1-9999999999999
		public string now; //Date and time MM/DD/YY(Day)HH:MM (:SS on some boards), EST/EDT timezone
		public int resto; //Reply to 0 (is a thread OP), 1-9999999999999
		public long tim; //Renamed filename UNIX timestamp + milliseconds
		public long time; //UNIX timestamp
		[JsonIgnore]
		public DateTime TimeStamp => DateTimeOffset.FromUnixTimeSeconds(time).UtcDateTime;
		public int tn_h; //Thumbnail height 1-250
		public int tn_w; //Thumbnail width 1-250
		public int w; //Image width 1-10000
	}
}
