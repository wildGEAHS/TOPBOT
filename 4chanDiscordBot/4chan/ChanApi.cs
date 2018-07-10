using ChanDiscordBot.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ChanDiscordBot.Chan
{
    public static class ChanApi
    {
		private static HttpClient _webClient = null;
		private static readonly string _catalogUrl = "https://a.4cdn.org/{0}/catalog.json";
		private static readonly string _postsUrl = "https://a.4cdn.org/{0}/thread/{1}.json";

		static ChanApi()
		{
			_webClient = new HttpClient();
			//var test = (HttpWebRequest)WebRequest.Create(_catalogUrl);
		}

		public static async Task<CatalogModel> GetCatalog(string board, DateTime modifiedSince)
		{
			try
			{
				var request = new HttpRequestMessage()
				{
					Method = HttpMethod.Get,
					RequestUri = new Uri(string.Format(_catalogUrl, board))
				};
				request.Headers.Add("If-Modified-Since", modifiedSince.ToUniversalTime().ToString("R"));

				var response = await _webClient.SendAsync(request);
				if (!response.IsSuccessStatusCode)
					throw new Exception($"Code: {(int)response.StatusCode} ({Enum.GetName(typeof(HttpStatusCode), response.StatusCode)}) Reason: {response.ReasonPhrase}");
				var contentString = await response.Content?.ReadAsStringAsync();
				var retval = new CatalogModel
				{
					pages = JsonConvert.DeserializeObject<ChanCatalogPageModel[]>(contentString)
				};
				return retval;
			}
			catch(Exception ex)
			{
				ErrorLog.WriteError(ex);
				return null;
			}
		}

		public static async Task<ChanThreadPostsModel> GetPosts(string board, long postno, DateTime modifiedSince)
		{
			try
			{
				var request = new HttpRequestMessage()
				{
					Method = HttpMethod.Get,
					RequestUri = new Uri(string.Format(_postsUrl, board, postno))
				};
				request.Headers.Add("If-Modified-Since", modifiedSince.ToUniversalTime().ToString("R"));

				var response = await _webClient.SendAsync(request);
				if (!response.IsSuccessStatusCode)
					throw new Exception($"Code: {(int)response.StatusCode} ({Enum.GetName(typeof(HttpStatusCode), response.StatusCode)}) Reason: {response.ReasonPhrase}");
				var contentString = await response.Content?.ReadAsStringAsync();
				var retval = JsonConvert.DeserializeObject<ChanThreadPostsModel>(contentString);
				return retval;
			}
			catch (Exception ex)
			{
				ErrorLog.WriteError(ex);
				return null;
			}
		}
	}
}
