using System;

public class CPHInline
{
	public void Init()
	{
		TwitchUserInfo broadcaster = CPH.TwitchGetBroadcaster();
		TwitchUserInfo bot = CPH.TwitchGetBot();
		if(broadcaster != null)
		{
			broadcasterId = broadcaster.UserId;
		}
		if(bot != null)
		{
			botId = bot.UserId;
		}
	}
	
	string broadcasterId = "";
	string botId = "";
	
	public bool Execute()
	{
		CPH.TryGetArg("userId",out string userId);
		
		if(userId == broadcasterId || userId == botId) return false;
		
		CPH.TryGetArg("message",out string currentMsg);
		string userVarName = "pwn_lastMessage";
		string userVarNameRepeat = "pwn_messageRepeatCount";
		
		
		string lastMessage = CPH.GetTwitchUserVarById<string>(userId, userVarName, false);
		CPH.SetTwitchUserVarById(userId, userVarName, currentMsg, false);
		if(!lastMessage.Equals(currentMsg,StringComparison.OrdinalIgnoreCase))
		{
			CPH.SetTwitchUserVarById(userId, userVarNameRepeat, -1, false);
			return false;
		}else{
			CPH.TryGetArg("maxRepeat",out int maxRepeat);
			CPH.IncrementOrCreateTwitchUsersVarById([userId], userVarNameRepeat, 1, false);
			CPH.TryGetArg("excludeVips",out bool excludeVips);
			CPH.TryGetArg("excludeMods",out bool excludeMods);
			CPH.TryGetArg("isVip",out bool isVip);
			CPH.TryGetArg("isModerator",out bool isMod);
			if((excludeVips && isVip) || (excludeMods && isMod)) return false;
			if(maxRepeat <= -1) return false;
			int currentRepeat = CPH.GetTwitchUserVarById<int>(userId, userVarNameRepeat, false);
			if(currentRepeat >= maxRepeat)
			{
				CPH.TryGetArg("msgId",out string messageId);
				CPH.TwitchDeleteChatMessage(messageId, false);
			}
		}
		return true;
	}
}
