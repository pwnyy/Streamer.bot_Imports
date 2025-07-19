using System;
using System.Linq;
using System.Collections.Generic;
using Streamer.bot.Common.Events;

public class CPHInline
{
	public Dictionary<string,HashSet<string>> groupUsers;
	public string codeEvent = "pwnMisc_PV_First_Chat_Join";
	public string logTitle = "[pwn Misc][First Chat Join] -";
	public void Init()
	{
		string triggerName = "First Chat Join";
		string[] contextMenu = {"[pwn] Misc","Present Viewers"};
		if(!CPH.RegisterCustomTrigger(triggerName, codeEvent, contextMenu))
		{
			int attempts = 5;
			bool success = false;
			for(int i= 0;i<attempts;i++)
			{
				triggerName+= " ["+(i+1)+"]";
				if(CPH.RegisterCustomTrigger(triggerName, codeEvent, contextMenu))
				{
					success = true;
					break;
				}
			}
			if(!success)
			{
				CPH.LogError($"{logTitle} Was not able to register custom trigger {triggerName}.");
			}
		}
	}
	
	public bool Execute()
	{
		
		EventType eventy = CPH.GetEventType();
		if(eventy == EventType.TwitchPresentViewers)
		{
			GetGroupUsers();
			CPH.TryGetArg("users",out List<Dictionary<string,object>> users);
			EventSource source = CPH.GetSource();
			string platform = source.ToString().ToLower();
			foreach(Dictionary<string,object> userInfo in users)
			{
				string userId = userInfo["id"].ToString();
				if(groupUsers[platform].Contains(userId))
				{
					bool firstJoin = GetUserVar(platform,userId);
					if(!firstJoin)
					{
						Dictionary<string,object> userDict = new Dictionary<string,object>()
						{
							{"userId",userId},
							{"user",userInfo["display"]},
							{"userName",userInfo["userName"]},
							{"userType",platform},
							{"isSubscribed",userInfo["isSubscribed"]},
							{"isVip",userInfo["role"].ToString() == "2"},
							{"isModerator",userInfo["role"].ToString() == "3"}
						};
						SetUserVar(userId);
						CPH.TriggerCodeEvent(codeEvent,userDict);
					}
				}
			}
		}

		return true;
	}
	
	public bool ResetFirstJoin()
	{
		CPH.UnsetAllUsersVar(codeEvent, true);
		CPH.LogInfo($"{logTitle} Resetting First Join Users reset for all platforms.");
		return true;
	}
	
	public bool GetUserVar(string platform, string userId)
	{
		bool output = false;
		switch(platform)
		{
			case "twitch":
				output = CPH.GetTwitchUserVarById<bool?>(userId, codeEvent, true) ?? false;
				break;
		}
		return output;
	}
	
	public void SetUserVar(string userId)
	{
		CPH.SetTwitchUserVarById(userId, codeEvent, true, true);
	}
	
	public void GetGroupUsers()
    {
    	Dictionary<string,HashSet<string>> groupOut = new Dictionary<string,HashSet<string>>()
    	{	 		 
			{"twitch",new HashSet<string>()}
    	};	 		

		CPH.TryGetArg("groups",out string groups);
		List<string> groupList = groups.Split(',').Select(group => group.Trim()).ToList();
		foreach(string groupName in groupList)
		{
			List<GroupUser> groupUsers = CPH.UsersInGroup(groupName);
			foreach(GroupUser user in groupUsers)
			{
				string type = user.Type.ToLower();
				if(!groupOut.ContainsKey(type))
				{
					groupOut.Add(type,new HashSet<string>());
				}
				groupOut[type].Add(user.Id);
			}
		}
    	
		groupUsers = groupOut;
    }
}
