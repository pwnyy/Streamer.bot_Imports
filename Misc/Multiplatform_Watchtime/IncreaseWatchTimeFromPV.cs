// by pwnyy / https://twitch.tv/pwnyytv  /  https://pwnyy.tv / https://bsky.app/profile/pwnyy.tv
using System;
using System.Collections.Generic;
using Streamer.bot.Common.Events;

public class CPHInline
{
    public bool Execute()
    {
        EventSource source = CPH.GetSource();
        
        long pvMinutes = PresentViewerMinutes(source);
        long wtSeconds = pvMinutes*60;
		
        CPH.TryGetArg("users",out List<Dictionary<string, object>> users);
        CPH.TryGetArg("isOfflineTest",out bool isTest);
        bool isLive = CPH.TryGetArg("isLive",out isLive)? isTest ? true : isLive : true ;

        if(isLive)
        {
        	List<string> userIds = new List<string>();
            foreach(Dictionary<string,object> user in users)
            {
				string userId = user["id"].ToString();
				userIds.Add(userId);
				
            }
            SetWatchtime(userIds,source,wtSeconds);
        }
        return true;
    }
    
    
    public void SetWatchtime(List<string> userIds, EventSource platform, long addedTime)
    {
    	string varName = "watchtime";
		switch(platform)
		{
			case EventSource.Twitch:
				CPH.IncrementOrCreateTwitchUsersVarById(userIds, varName, addedTime, true);
				break;
			case EventSource.YouTube:
				CPH.IncrementOrCreateYouTubeUsersVarById(userIds, varName, addedTime, true);
				break;
			case EventSource.Trovo:
				CPH.IncrementOrCreateTrovoUsersVarById(userIds, varName, addedTime, true);
				break;
			case EventSource.Kick:
				CPH.IncrementOrCreateKickUsersVarById(userIds, varName, addedTime, true);
				break;
		}
    }
    
    public long PresentViewerMinutes(EventSource platform)
    {
    	long minutes = 1;
		switch(platform)
		{
			case EventSource.Twitch:
				minutes = CPH.TryGetArg("pvTwitchMinutes", out long twitchMinutes) ? twitchMinutes: minutes;
				break;
			case EventSource.YouTube:
			case EventSource.Trovo:
			case EventSource.Kick:
				//Due to YouTube, Trovo and Kick being always triggered every minute, 1 minute would be correct and there is no need for further options via arguments
				minutes = 1;
				break;
		}
		return minutes;
    }
}
