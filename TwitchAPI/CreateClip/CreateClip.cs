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
    string apiEndPoint = "Create Clip";

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
        TwitchUserInfo broadcaster = CPH.TwitchGetBroadcaster();
        DateTime currentTime = DateTime.Now;
        
        CPH.SetArgument("createClipSuccess", false);

        if(broadcaster != null)
        {
            string broadcasterId = broadcaster.UserId;
            string broadcasterUserName = broadcaster.UserLogin;
            Task<TwitchResponse> getData = FunctionCallTwitchAPI(broadcasterId);
            getData.Wait();
            bool clipCreate = getData.Result.data.Count > 0;
            CPH.SetArgument("createClipSuccess", clipCreate);
            if(clipCreate)
            {
                string clipId = getData.Result.data[0].id;
                CPH.SetArgument("createClipId", getData.Result.data[0].id);
                CPH.SetArgument("createClipCreatedAt", currentTime);
                CPH.SetArgument("createClipUrl", $"https://www.twitch.tv/{broadcasterUserName}/clip/{clipId}");
                CPH.SetArgument("createClipUrlEmbed", $"https://clips.twitch.tv/embed?clip={clipId}");
            }
        }
        
        return true;
    }
    
    public async Task<TwitchResponse> FunctionCallTwitchAPI(string broadcasterId)
    {
        string tokenValue = CPH.TwitchOAuthToken;
        string clientIdValue = CPH.TwitchClientId;
        
		using var request = new HttpRequestMessage(
			HttpMethod.Post,
			$"https://api.twitch.tv/helix/clips?broadcaster_id={broadcasterId}"
		);
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("client-ID", clientIdValue);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenValue);
        HttpResponseMessage response = await _client.SendAsync(request);
        TwitchResponse data = null;

        if (response.IsSuccessStatusCode)
        {
            HttpContent responseContent = response.Content;
            string responseBody = await response.Content.ReadAsStringAsync();
            data = JsonConvert.DeserializeObject<TwitchResponse>(responseBody);
        }
        else
        {
            CPH.LogError($"{apiEndPoint} - ERR - {response}");
        }

        return data;
    }
}

public class Data
{
	public string edit_url { get; set; }
    public string id { get; set; }
}


public class TwitchResponse
{
    public List<Data> data { get; set; }
}
