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


	static string _prefix = "categories";
	
    public bool Execute()
    {
        CPH.TryGetArg("inputQuery", out string query);
        query = query.Trim();
        Task<List<TwitchDataObject>> getDataList = FunctionCallTwitchAPI(query);
        getDataList.Wait();
        int i = 0;
        CPH.SetArgument($"{_prefix}.ResultCount",getDataList.Result.Count);
        if (getDataList.Result.Count > 0)
        {
			
            foreach (TwitchDataObject dataObject in getDataList.Result)
            {
            	SetPropertyArguments(dataObject,$"{_prefix}{i}.");
            	i++;
            }
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
    
    public async Task<List<TwitchDataObject>> FunctionCallTwitchAPI(string query)
    {
        string tokenValue = CPH.TwitchOAuthToken;
        string clientIdValue = CPH.TwitchClientId;
        string broadcasterId = CPH.TwitchGetBroadcaster().UserId;
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("client-ID", clientIdValue);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenValue);
        HttpResponseMessage response = await _client.GetAsync($"https://api.twitch.tv/helix/search/categories?query={query}");
        List<TwitchDataObject> dataList = new List<TwitchDataObject>();
        if (response.IsSuccessStatusCode)
        {
            HttpContent responseContent = response.Content;
            string responseBody = await response.Content.ReadAsStringAsync();
            TwitchResponse root = JsonConvert.DeserializeObject<TwitchResponse>(responseBody);
            if (root.data.Count == 0)
            {
                return dataList;
            }

            dataList.AddRange(root.data);
        }
        else
        {
            CPH.LogError($"[pwn][Twitch][Search Categories] - ERR - {response}");
        }

        return dataList;
    }
}

public class TwitchDataObject
{
    public string box_art_url { get; set; }
    public string name { get; set; }
    public string id { get; set; }
}


public class TwitchResponse
{
    public List<TwitchDataObject> data { get; set; }
    public int total {get; set;}
}
