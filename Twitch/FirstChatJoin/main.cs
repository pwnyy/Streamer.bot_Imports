// Author: pwnyy , https://twitch.tv/pwnyytv , https://x.com/pwnyy, https://ko-fi.com/pwnyy, https://pwnyy.tv
// Contact: contact@pwnyy.tv , or on the above mentioned social media.
// Make sure to contact me if you are using my code somewhere, so I can see where it's being used!
//
// This program is licensed under the GNU General Public License Version 3 (GPLv3).
// 
// The GPLv3 is a free software license that ensures end users have the freedom to run,
// study, share, and modify the software. Key provisions include:
// 
// - Copyleft: Modified versions of the software must also be licensed under the GPLv3.
// - Source Code: You must provide access to the source code when distributing the software.
// - Credit: You must credit the original author of the software, by mentioning either contact e-mail or their social media.
// - No Warranty: The software is provided "as-is," without warranty of any kind.
// 
// For more details, see https://www.gnu.org/licenses/gpl-3.0.en.html

using System;
using System.Linq;
using System.Collections.Generic;
using Streamer.bot.Common.Events;

public class CPHInline
{
	public HashSet<string> groupUsers = new();
	public HashSet<string> seenUsers = new();

	//default value of userVarName = "pwn_FirstChatJoin_state"
	static string globalVarName = "pwn_FirstChatJoin_state";
	//default value of triggerIdentifier = "" , this should only be changed IF you duplicate the code to use it for something else and also create different triggers for that
	static string triggerIdentifier = "";

	//
	/*  DISCLAIMER
		End of being able to edit and I still give support, anything below being changed by others, no support guaranteed.
		
	*/

	const string currentCodeVersion = "1.0.0";

	static string customTriggerCategory = string.IsNullOrWhiteSpace(triggerIdentifier)
    	? "First Chat Join"
    	: $"First Chat Join {triggerIdentifier}";

	static string triggerPrefix = "pwn_FirstChatJoin"+triggerIdentifier;

	Dictionary<string,string> triggerContext = new Dictionary<string,string>()
	{		 
		{triggerPrefix+"_UserJoinedChat","User Joined"},
	};

	int MaxRetryAttempts = 5;
	
	public void Init()
	{	 		 
		ExtensionLog($"This extension was developed by pwnyy. Contact: contact@pwnyy.tv , Socials: https://pwnyy.tv");
		ExtensionLog("Init Custom Triggers",1);
		string[] contextMenu = {"[pwn] Extensions",customTriggerCategory};
		HydrateSeenUsers();
		foreach(KeyValuePair<string,string> kvp in triggerContext)
		{
			string triggerName = kvp.Value;
			string eventName = kvp.Key;
			if(!CPH.RegisterCustomTrigger(triggerName, eventName, contextMenu))
			{	 		
				bool success = false;
				for(int i = 0; i < MaxRetryAttempts; i++)
				{		 
					triggerName += " [" + (i + 1) + "]";
					if(CPH.RegisterCustomTrigger(triggerName, eventName, contextMenu))
					{
						success = true;
						break;
					}
				}
				if(!success)
				{		 		
					ExtensionLog($"Was not able to register custom trigger {triggerName} with {MaxRetryAttempts} attempts.", -1);
				}
			}
		}
	}

	public bool Execute()
	{
		string triggerEvent = "UserJoinedChat";
		EventType eventy = CPH.GetEventType();
		EventSource source = CPH.GetSource();
		if(source == EventSource.Twitch && eventy == EventType.TwitchPresentViewers)
		{
			GetGroupUsers();
			CPH.TryGetArg("users",out List<Dictionary<string,object>> users);
			
			if (groupUsers.IsSubsetOf(seenUsers))  return true;
			int targetCount = groupUsers.Count;
			foreach(Dictionary<string,object> userInfo in users)
			{
				if (seenUsers.Count >= targetCount) break;

				string userId = userInfo["id"].ToString();
				if(!seenUsers.Contains(userId) && groupUsers.Contains(userId))
				{
					Dictionary<string,object> userArgs = new Dictionary<string,object>()
					{
						{"userId",userId},
						{"user",userInfo["display"]},
						{"userName",userInfo["userName"]},
						{"userType","twitch"},
						{"isSubscribed",userInfo["isSubscribed"]},
						{"isVip",userInfo["role"].ToString() == "2"},
						{"isModerator",userInfo["role"].ToString() == "3"}
					};
					SetSeenUser(userId);
					TriggerOn($"{triggerPrefix}_{triggerEvent}", userArgs);
				}
			}
		}

		return true;
	}
	
	public bool ResetFirstJoin()
	{
		CPH.UnsetAllUsersVar(globalVarName, true);
		seenUsers.Clear();
		ExtensionLog($"Resetting First Join Users reset for all platforms.");
		return true;
	}
	
	public void HydrateSeenUsers()
	{
		List<UserVariableValue<bool>> users = CPH.GetTwitchUsersVar<bool>(globalVarName, true);
		foreach( var user in users)
		{
			if(user.Value) seenUsers.Add(user.UserId);
		}
	}
	
	public void SetSeenUser(string userId)
	{
		CPH.SetTwitchUserVarById(userId, globalVarName, true, true);
		seenUsers.Add(userId);
	}
	
	public void GetGroupUsers()
    {	
		CPH.TryGetArg("groups",out string groups);
		List<string> groupList = groups.Split(',').Select(group => group.Trim()).ToList();
		foreach(string groupName in groupList)
		{
			List<GroupUser> groupUserList = CPH.UsersInGroup(groupName);
			foreach(GroupUser user in groupUserList)
			{
				groupUsers.Add(user.Id);
			}
		}
    }

	public void TriggerOn(string eventName, Dictionary<string,object> triggerArgs)
	{	 		 
		triggerArgs["pwnExtensionTriggerEvent"] = eventName;
		CPH.TriggerCodeEvent(eventName, triggerArgs);
	}

	public void ExtensionLog(string message, int logType = 0)
	{		 		
		string name = customTriggerCategory;
		string output = $"[{name}] - {message}";

		switch(logType)
		{
			case -2:
				CPH.LogError(output);
				break;
			case -1:
				CPH.LogWarn(output);
				break;
			case 1:
				CPH.LogDebug(output);
				break;
			case 2:
				CPH.LogVerbose(output);
				break;
			default:
				CPH.LogInfo(output);
				break;
		}
	}
}
