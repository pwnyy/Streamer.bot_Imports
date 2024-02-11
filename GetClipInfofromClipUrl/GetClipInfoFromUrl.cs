using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;

public class CPHInline
{
    public bool Execute()
    {	
		//Get. -Clip- .url
    	string clipUrl = args["inputClipUrl"].ToString();
    	//Get. ClipID- from- Url 
    	string clipId = clipUrl.Substring(clipUrl.LastIndexOf('/')+1);
    	
		Task<Dictionary<string,object>> getClipInfo = FunctionCallTwitchAPI(clipId);
        getClipInfo.Wait();
        //.check- if no result
        if(getClipInfo.Result == null)
        {
			CPH.SetArgument("clipFound",false);
			return true;
        }
        //-Get. the- results-
        Dictionary<string,object> clipInfo = getClipInfo.Result;
		CPH.SetArgument("clipFound",true);
        //-Set. Arguments- accordingly-
        CPH.SetArgument("clipId",clipId);
        CPH.SetArgument("clipTitle",clipInfo["title"]);
        CPH.SetArgument("clipUrl",clipInfo["url"]);
        CPH.SetArgument("clipEmbed",clipInfo["embed_url"]);
        CPH.SetArgument("clipBroadcasterId",clipInfo["broadcasterId"]);
        CPH.SetArgument("clipBroadcasterDisplayName",clipInfo["broadcasterDisplayName"]);
        CPH.SetArgument("clipCreatorId",clipInfo["creatorId"]);
        CPH.SetArgument("clipCreatorDisplayName",clipInfo["creatorDisplayName"]);
        CPH.SetArgument("clipVodId",clipInfo["vodId"]);
        CPH.SetArgument("clipGameId",clipInfo["gameId"]);
        CPH.SetArgument("clipLanguage",clipInfo["language"]);
        CPH.SetArgument("clipTitle",clipInfo["title"]);
        CPH.SetArgument("clipViewCount",clipInfo["viewCount"]);
        CPH.SetArgument("clipThumbnailUrl",clipInfo["thumbnailUrl"]);
        CPH.SetArgument("clipDuration",clipInfo["duration"]);
        CPH.SetArgument("clipVodOFfset",clipInfo["vodOffset"]);
        CPH.SetArgument("clipIsFeatured",clipInfo["isFeatured"]);

        return true;
    }

    public static HttpClient client = new HttpClient();
    public async Task<Dictionary<string,object>> FunctionCallTwitchAPI(string clipId)
    {	//Getting AuthClient
        string tokenValue = CPH.TwitchOAuthToken;
        string clientIdValue = CPH.TwitchClientId;
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("client-ID", clientIdValue);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenValue);
        HttpResponseMessage response = await client.GetAsync($"https://api.twitch.tv/helix/clips?broadcaster_id=&id={clipId}&game_id=");
        HttpContent responseContent = response.Content;
        string responseBody = await response.Content.ReadAsStringAsync();
        TwitchResponse root = JsonConvert.DeserializeObject<TwitchResponse>(responseBody);
        Dictionary<string,object> clipResponse = new Dictionary<string,object>();
        if(root.data.Count == 0)
        {
			return null;
        }
		clipResponse.Add("id",root.data[0].id);
		clipResponse.Add("url",root.data[0].url);
		clipResponse.Add("embed_url",root.data[0].embed_url);
		clipResponse.Add("broadcasterId",root.data[0].broadcaster_id);
		clipResponse.Add("broadcasterDisplayName",root.data[0].broadcaster_name);
		clipResponse.Add("creatorId",root.data[0].creator_id);
		clipResponse.Add("creatorDisplayName",root.data[0].creator_name);
		clipResponse.Add("vodId",root.data[0].video_id);
		clipResponse.Add("gameId",root.data[0].game_id);
		clipResponse.Add("language",root.data[0].language);
		clipResponse.Add("title",root.data[0].title);
		clipResponse.Add("viewCount",root.data[0].view_count);
		clipResponse.Add("created_at",root.data[0].created_at);
		clipResponse.Add("thumbnailUrl",root.data[0].thumbnail_url);
		clipResponse.Add("duration",root.data[0].duration);
		clipResponse.Add("vodOffset",root.data[0].vod_offset != null ? root.data[0].vod_offset : -1);
		clipResponse.Add("isFeatured",root.data[0].is_featured);

        return clipResponse;
    }
}

public class Clip
{
    public string id { get; set; }
    public string url { get; set; }
    public string embed_url { get; set; }
    public string broadcaster_id { get; set; }
    public string broadcaster_name { get; set; }
    public string creator_id { get; set; }
    public string creator_name { get; set; }
    public string video_id { get; set; }
    public string game_id { get; set; }
    public string language { get; set; }
    public string title { get; set; }
    public int view_count { get; set; }
    public string created_at { get; set; }
    public string thumbnail_url { get; set; }
    public float duration { get; set; }
    public int? vod_offset { get; set; }
    public bool is_featured { get; set; }
}

public class Pagination
{
    public string cursor { get; set; }
}

public class TwitchResponse
{
    public List<Clip> data { get; set; }
    public Pagination pagination { get; set; }
}
