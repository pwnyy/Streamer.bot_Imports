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
using System.Threading.Tasks;	 	 		
using System.Collections.Generic;
using System.Linq;
using System.Text;	 		
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;


public class CPHInline
{
    private static readonly HttpClient _client = new HttpClient
    {
        Timeout = TimeSpan.FromSeconds(10)
    };
    
    string extName = "[ Modify Twitch CCL (pwnyy)]";

    Dictionary<string,bool> ccls = new Dictionary<string,bool>()
    {
        {"DebatedSocialIssuesAndPolitics",false},
        {"DrugsIntoxication",false},
        {"SexualThemes",false},
        {"ViolentGraphic",false},
        {"Gambling",false},
        {"ProfanityVulgarity",false}
    };
    
    public void Dispose()
    {
        _client?.Dispose();
    }

    public bool Execute()
    { 		  		
        var currentCCL = new Dictionary<string,bool>(ccls);
        CPH.TryGetArg("useBitmask",out bool useBM);
    	if(useBM)
        {
            CPH.TryGetArg("cclBitmaskValue",out int bitmask);
            
            currentCCL["DebatedSocialIssuesAndPolitics"] = (bitmask & 1)  != 0;
            currentCCL["DrugsIntoxication"]              = (bitmask & 2)  != 0;
            currentCCL["SexualThemes"]                   = (bitmask & 4)  != 0;
            currentCCL["ViolentGraphic"]                 = (bitmask & 8)  != 0;
            currentCCL["Gambling"]                       = (bitmask & 16) != 0;
            currentCCL["ProfanityVulgarity"]             = (bitmask & 32) != 0;
        }else{
            CPH.TryGetArg("ccl_DebatedSocialIssuesAndPolitics",out bool ccl0);
            CPH.TryGetArg("ccl_DrugsIntoxication",out bool ccl1);
            CPH.TryGetArg("ccl_SexualThemes",out bool ccl2);
            CPH.TryGetArg("ccl_ViolentGraphic",out bool ccl3);
            CPH.TryGetArg("ccl_Gambling",out bool ccl4);
            CPH.TryGetArg("ccl_ProfanityVulgarity",out bool ccl5);

            currentCCL["DebatedSocialIssuesAndPolitics"] = ccl0;
            currentCCL["DrugsIntoxication"]              = ccl1;
            currentCCL["SexualThemes"]                   = ccl2;
            currentCCL["ViolentGraphic"]                 = ccl3;
            currentCCL["Gambling"]                       = ccl4;
            currentCCL["ProfanityVulgarity"]             = ccl5;
        }
    	
        Task<bool> setCCL = FunctionCallTwitchAPI(BuildPayload(currentCCL));
        setCCL.Wait();
        CPH.SetArgument("changeSuccess",setCCL.Result);

        return true;
    } 		  			 	 			 		

    public string BuildPayload(Dictionary<string, bool> dict)
    {
        var labels = dict.Select(kvp => new ContentClassificationLabel
        {
            id = kvp.Key,
            is_enabled = kvp.Value
        }).ToList();

        var ccLoad = new ContentClassificationPayload
        {
            content_classification_labels = labels
        };

        return JsonConvert.SerializeObject(ccLoad);
    }
    
    public async Task<bool> FunctionCallTwitchAPI(string payload)
    { 		  			 	 			 		
        string tokenValue = CPH.TwitchOAuthToken;
        string clientIdValue = CPH.TwitchClientId;
        string broadcasterId = CPH.TwitchGetBroadcaster().UserId;
        
        _client.DefaultRequestHeaders.Clear(); 		 
        _client.DefaultRequestHeaders.Add("client-ID", clientIdValue); 		
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenValue);	 
        _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));


        StringContent content = new StringContent(payload, Encoding.UTF8, "application/json");
        string apiUrl = $"https://api.twitch.tv/helix/channels?broadcaster_id={broadcasterId}";
        HttpResponseMessage response = await _client.PatchAsync(apiUrl, content);
        
        if(response.IsSuccessStatusCode){
        	
			return true;
        }
        
        return false;
    } 		  			 	 			 		

    public class ContentClassificationLabel
    {
        public string id { get; set; }
        public bool is_enabled { get; set; }
    }
    public class ContentClassificationPayload
    {
        public List<ContentClassificationLabel> content_classification_labels { get; set; }
    }
}

public static class HttpClientExtensions
{
	public static async Task<HttpResponseMessage> PatchAsync(this HttpClient client, string requestUri, HttpContent content)
	{
		var method = new HttpMethod("PATCH");
		var request = new HttpRequestMessage(method, requestUri)
		{
			Content = content
		};
		return await client.SendAsync(request);
	}
}
