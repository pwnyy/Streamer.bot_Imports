// Author: pwnyy , https://twitch.tv/pwnyytv , https://x.com/pwnyy, https://ko-fi.com/pwnyy
// Contact: contact@pwnyy.tv , or on the above mentioned social media.
//
// This code is licensed under the GNU General Public License Version 3 (GPLv3).
// 
// The GPLv3 is a free software license that ensures end users have the freedom to run,
// study, share, and modify the software. Key provisions include:
// 
// - Copyleft: Modified versions of the code must also be licensed under the GPLv3.
// - Source Code: You must provide access to the source code when distributing the software.
// - Credit: You must credit the original author of the software, by mentioning either contact e-mail or their social media.
// - No Warranty: The software is provided "as-is," without warranty of any kind.
// 
// For more details, see https://www.gnu.org/licenses/gpl-3.0.en.html.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Xml;

public class CPHInline
{
    private static readonly HttpClient _client = new HttpClient
    {
        Timeout = TimeSpan.FromSeconds(10)
    };
    public void Dispose()
    {
        _client?.Dispose();
    }

    public bool Execute()
    {
        //List<string> emoteTypes = ["bitstier","follower","subscriptions","all"];
        CPH.TryGetArg("emoteTypes", out string filterInput);
        string[] parts = filterInput.Split(',');
        List<string> emoteFilters = new List<string>();
        foreach (string part in parts)
        {
            string cleaned = part.Trim().ToLower();
            emoteFilters.Add(cleaned);
        }

        CPH.TryGetArg("fromUserId", out string userId);
        Task<List<Emote>> getEmoteList = FunctionCallTwitchAPI(userId);
        getEmoteList.Wait();
        CPH.SetArgument("emotesFound", getEmoteList.Result.Count > 0 ? true : false);
        int i = 0;
        if (getEmoteList.Result.Count > 0)
        {
            CPH.SetArgument("emotesTotal", getEmoteList.Result.Count);
            CPH.SetArgument("emotesFilteredCount",0);
            int filterCount = 0;
            
            string prefix = "";
			foreach (char c in getEmoteList.Result[0].name)
			{
				if (char.IsUpper(c))
					break;

				prefix += c;
			}
			CPH.SetArgument("emotePrefix", prefix);
			List<string> emoteList = new List<string>();
			CPH.SetArgument("emoteList",emoteList);
			CPH.SetArgument("emoteListString","");
			
            foreach (Emote currentEmote in getEmoteList.Result)
            {
				if( !emoteFilters.Contains(currentEmote.emote_type)) continue;
            	filterCount++;
            	emoteList.Add(currentEmote.name);
            	SetPropertyArguments(currentEmote,$"emote{i}.");
            	i++;
            }
            CPH.SetArgument("emotesFilteredCount",filterCount);
            CPH.SetArgument("emoteList",emoteList);
            CPH.SetArgument("emoteListString",String.Join(",",emoteList));
        }

        return true;
    }
    
	public void SetPropertyArguments(object obj, string prefix = "")
    {	 		 
    	if(obj == null) return;
    	
    	Type type = obj.GetType();
    	
    	if (obj is IEnumerable list )
		{	 		
			int index = 0;
			foreach (var item in list)
			{		 
				SetPropertyArguments(item, $"{prefix}[{index}].");
				index++;
			}
			return;
		}
		
		PropertyInfo[] properties = type.GetProperties();
		foreach (var prop in properties)
		{		 		
			string name = prefix + prop.Name;
			object value = prop.GetValue(obj, null);
			
			if(value is DateTime dt)
			{		 		
				CPH.SetArgument(name+"_utc", dt);
				CPH.SetArgument(name+"_unix", new DateTimeOffset(DateTime.SpecifyKind(dt, DateTimeKind.Utc)).ToUnixTimeSeconds());
				value = dt.ToLocalTime();
			}
			
			if (value == null)
				continue;

			if (value is IEnumerable enumerableValue && !(value is string))
			{	 		 
				CPH.SetArgument($"{name}",value);
				var items = new List<string>();
				foreach (var item in (IEnumerable)value)
				{	 		
					items.Add(item?.ToString() ?? "null");
				}
				CPH.SetArgument(name+"_string", string.Join(",", items));
				continue;
			}
			
			if (prop.PropertyType.IsClass && prop.PropertyType != typeof(string))
			{		 
				SetPropertyArguments(value, name+".");
			}
			else
			{		 		
				CPH.SetArgument(name, value);
			}		 		
		}
    }
    
    public async Task<List<Emote>> FunctionCallTwitchAPI(string userId)
    {
        string tokenValue = CPH.TwitchOAuthToken;
        string clientIdValue = CPH.TwitchClientId;
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("client-ID", clientIdValue);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenValue);
        HttpResponseMessage response = await _client.GetAsync($"https://api.twitch.tv/helix/chat/emotes?broadcaster_id={userId}");
        List<Emote> emoteList = new List<Emote>();
        if (response.IsSuccessStatusCode)
        {
            HttpContent responseContent = response.Content;
            string responseBody = await response.Content.ReadAsStringAsync();
            TwitchResponse root = JsonConvert.DeserializeObject<TwitchResponse>(responseBody);
            if (root.data.Count == 0)
            {
                return emoteList;
            }

            emoteList.AddRange(root.data);
        }
        else
        {
            CPH.LogError($"[Get Channel Emotes] - ERR - {response}");
        }

        return emoteList;
    }
}

public class Emote
{
    public string id { get; set; }
    public string name { get; set; }
    public EmoteImage images { get; set; }
    public string tier { get; set; }
    public string emote_type { get; set; }
    public string emote_set_id { get; set; }
    public string[] format { get; set; }
    public string[] scale { get; set; }
    public string[] theme_mode { get; set; }
}

public class EmoteImage
{
    public string url_1x { get; set; }
    public string url_2x { get; set; }
    public string url_4x { get; set; }
}

public class TwitchResponse
{
    public List<Emote> data { get; set; }
}
