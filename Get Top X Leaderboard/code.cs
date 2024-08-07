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
using System.Linq;	 		
using System.Collections.Generic;		 	 		

public class CPHInline		 		
{
	public bool persisted = true;
	public string broadcaster;
	public string broadcastUserId;
	public Dictionary<string,HashSet<string>> ignoredGroups;
	public string[] secondsLang;
	public string[] minutesLang;
	public string[] hoursLang;
	public string[] daysLang;
	public string[] monthsLang;
	public string[] yearsLang;
    
    public bool Execute()
    {	
    	RemoveInvisChar7TV();
    	CPH.TryGetArg("userVariable",out string userVar);
    	CPH.TryGetArg("defaultTop",out long defaultTop);
        CPH.TryGetArg("isPersistedVariable",out persisted);
        
        CPH.TryGetArg("userId",out string userId);
        CPH.TryGetArg("user",out string user);
		CPH.TryGetArg("userName",out string userName);
        string platform = CPH.TryGetArg("userType", out platform) ? platform.ToLower() : "twitch";
        
        CPH.TryGetArg("broadcastUserId",out broadcastUserId);
        CPH.TryGetArg("broadcastUserName", out string broadcastUserName);
        CPH.TryGetArg(platform == "youtube" ? "broadcastUserName" : "broadcastUser", out broadcaster);
        
        bool broadcasterUsage = Convert.ToBoolean(args["broadcasterUsage"]);
        if (userId == broadcastUserId && !broadcasterUsage)return false;
        
        CPH.TryGetArg("ignoreCommandInput", out bool ignoreInput);
        CPH.TryGetArg("minInput", out long minInput);
        CPH.TryGetArg("maxInput", out long maxInput);
        
        string varInput = "input0";
        CPH.TryGetArg(varInput,out string input);
        bool hasInputInt = int.TryParse(input, out int topInput);
		
		if(!ignoreInput && hasInputInt && (topInput > maxInput || topInput < minInput) )
		{
			CPH.SetArgument("messageType",-1);
			return true;
		}
		
		bool isTime = CPH.TryGetArg("isTimeInSeconds", out isTime) ? isTime : false;		 		
		bool shortF = true;		 
		bool showSeconds = false;		 		 
		if(isTime)		 		
		{
			shortF = CPH.TryGetArg("shortFormTime", out shortF) ? shortF : true;		 		 
			showSeconds = CPH.TryGetArg("showSeconds", out showSeconds) ? showSeconds : false;		 		
			secondsLang = CPH.TryGetArg("secondsLang", out string secondsLangStr) ? secondsLangStr.Split(','):"second,seconds".Split(',');		 
			minutesLang = CPH.TryGetArg("minutesLang", out string minutesLangStr) ? minutesLangStr.Split(','):"minute,minutes".Split(',');		 		
			hoursLang = CPH.TryGetArg("hoursLang", out string hoursLangStr) ? hoursLangStr.Split(','):"hour,hours".Split(',');		 		
			daysLang = CPH.TryGetArg("daysLang", out string daysLangStr) ? daysLangStr.Split(','):"day,days".Split(',');
			monthsLang = CPH.TryGetArg("monthsLang", out string monthsLangStr) ? monthsLangStr.Split(','):"month,months".Split(',');
			yearsLang = CPH.TryGetArg("yearsLang", out string yearsLangStr) ? yearsLangStr.Split(','):"year,years".Split(',');
		}

		GetIgnoredGroups();
		
        CPH.TryGetArg("includeBroadcasterLeaderboard",out bool includeBroadcaster);
		List<UserValue> userIdsWithVariable = GetUserList(userVar, platform, includeBroadcaster);
		
		CPH.TryGetArg("orderByDescending", out bool orderByDes);
		userIdsWithVariable = orderByDes ? userIdsWithVariable.OrderByDescending(u => u.Value).ToList() : userIdsWithVariable.OrderBy(u => u.Value).ToList();
       	
       	decimal divider = CPH.TryGetArg("valueDivider",out var dividerValue) ? decimal.TryParse(dividerValue.ToString(), out divider) ? divider : 1 : 1;
       	divider = divider == 0 ? 1 : divider;
		CPH.TryGetArg("valueFormat",out string vFormat);
		if(userIdsWithVariable.Count > 0 && !isTime)
		{
			try
			{
				string test = userIdsWithVariable[0].Value.ToString(vFormat);
			}catch(Exception e)
			{
				CPH.LogError($"[pwn LeaderBoard TopX] - Invalid valueFormat \"{vFormat}\" used, using \"F0\" instead.");
				vFormat = "F0";
			}
		}
		
		CPH.TryGetArg("populateRedeemerVariables",out bool populateRedeemer);
		
		if(populateRedeemer)
		{
			int redeemerIndex = userIdsWithVariable.FindIndex(u => u.UserId == userId);
			CPH.SetArgument("lbRedeemerId",userId);
			CPH.SetArgument("lbRedeemer",user);
			CPH.SetArgument("lbRedeemerLogin",userName);
			CPH.SetArgument("lbRedeemerRank",redeemerIndex == -1 ? userIdsWithVariable.Count()+1 : redeemerIndex +1);
			if(redeemerIndex > -1)
			{
				CPH.SetArgument("lbRedeemerValue",isTime ? SecToTime((long)Math.Floor(userIdsWithVariable[redeemerIndex].Value / divider), showSeconds,shortF) : (userIdsWithVariable[redeemerIndex].Value / divider).ToString(vFormat));
			}else{
				CPH.SetArgument("lbRedeemerValue",isTime ? SecToTime((long)Math.Floor(0 / divider), showSeconds,shortF) : (0 / divider).ToString(vFormat));
			}
		}

		List<string> rankingList = new List<string>();
		int showRanks = !ignoreInput && hasInputInt ? topInput : (int)defaultTop;
		List<UserValue> topRanks = userIdsWithVariable.Take(showRanks).ToList();		 		 

		
		if(topRanks.Count == 0)
		{
			CPH.SetArgument("messageType",0);
			return true;
		}
		CPH.TryGetArg("rankFormat",out string rankFormat);
		
		Dictionary<string,(string,string,string)> outputDict = new Dictionary<string,(string,string,string)>();
		int i = 1;
		foreach(UserValue rank in topRanks)
		{
			string singleRank = rankFormat.Replace("%rankPosition%",i.ToString());
			
			singleRank = singleRank.Replace("%rankUser%",rank.User);
			string value = isTime ? SecToTime((long)Math.Floor(rank.Value / divider), showSeconds,shortF) : (rank.Value / divider).ToString(vFormat);
			singleRank = singleRank.Replace("%rankValue%",value); 
			rankingList.Add(singleRank);
			CPH.SetArgument("lbUserRank"+i,i);
			CPH.SetArgument("lbUser"+i,rank.User);
			CPH.SetArgument("lbUserLogin"+i,rank.UserLogin);
			CPH.SetArgument("lbUserId"+i,rank.UserId);
			CPH.SetArgument("lbUserValue"+i,value);
			outputDict.Add(rank.UserId,(rank.User,rank.UserLogin,value));
			i++;
		}
		
		string topRankString = String.Join(", ",rankingList);
		string topRankObs = String.Join("\n",rankingList);
		
		CPH.SetArgument("messageType",1);
		CPH.SetArgument("topNumber",i-1);
		CPH.SetArgument("rankDictionary",outputDict);
		CPH.SetArgument("rankList",topRankString);
		CPH.SetArgument("rankListObs",topRankObs);
		
        return true;
    }

    public List<UserValue> GetUserList(string varName, string platform, bool includeBroadcaster)
    {
		List<UserVariableValue<string>> objectList = new List<UserVariableValue<string>>();
		switch(platform)
		{
			case "twitch":
				objectList = CPH.GetTwitchUsersVar<string>(varName, persisted);
				break;
			case "youtube":
				objectList = CPH.GetYouTubeUsersVar<string>(varName, persisted);
				break;
			case "trovo":
				objectList = CPH.GetTrovoUsersVar<string>(varName, persisted);
				break;
		}
		
		List<UserValue> resultList = new List<UserValue>();		 		 
		
		foreach(UserVariableValue<string> uvv in objectList)	 		
		{		 
			
			decimal parsed = 0;		 		
			bool check = uvv.Value != null && decimal.TryParse(uvv.Value.ToString(),out parsed) ? true : false;		 		
			if(!check)
			{
				string valueString = uvv.Value == null ? "null" : uvv.Value;
				CPH.LogError($"[pwn LeaderBoard TopX] Could not parse to number, please fix: Varname: {varName} - Value: {valueString} Platform: {platform} - User: {uvv.UserName} / {uvv.UserLogin}({uvv.UserId})");
			}

			if (!ignoredGroups[platform].Contains(uvv.UserId) && (includeBroadcaster || uvv.UserId != broadcastUserId))		 		 
			{		 		
				resultList.Add(new UserValue(){User=uvv.UserName, UserLogin = uvv.UserLogin, UserId = uvv.UserId, Value = parsed});		 
			}		 		
		}		 		
		return resultList;
    }
    
    public void GetIgnoredGroups(List<string> methodGroups = null)
    {
    	Dictionary<string,HashSet<string>> groupOut = new Dictionary<string,HashSet<string>>()
    	{	 		 
			{"twitch",new HashSet<string>()},
			{"youtube",new HashSet<string>()},
			{"trovo",new HashSet<string>()}
    	};	 		
		
    	if(methodGroups != null)		 
    	{		 		
			foreach(string groupName in methodGroups)
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
    	}else{
    		CPH.TryGetArg("ignoredGroups",out string ignoredGroups);
			List<string> groupList = ignoredGroups.Split(',').Select(group => group.Trim()).ToList();
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
    	}
		ignoredGroups = groupOut;
    }
    
    public string SecToTime(long secondsInput, bool showSeconds,bool shortForm)	 		 
    {	 		
        TimeSpan diff = TimeSpan.FromSeconds(secondsInput);		 
        int years = diff.Days / 365;		 		
		int remainingDays = diff.Days % 365;		 		
		int months = remainingDays / 31; // Assuming an average month has 31 days
		int days = remainingDays % 31;
		int hours = diff.Hours;
		int minutes = diff.Minutes;
		int seconds = diff.Seconds;

        //Define multiples or not
		string secondsString 	= diff.Seconds == 1 ? secondsLang[0].Trim() : secondsLang.Length > 1? secondsLang[1].Trim() : secondsLang[0].Trim();
		string minutesString 	= diff.Minutes == 1 ? minutesLang[0].Trim() : minutesLang.Length > 1? minutesLang[1].Trim() : minutesLang[0].Trim();
		string hoursString		= diff.Hours == 1 	? hoursLang[0].Trim() 	: hoursLang.Length > 1 	? hoursLang[1].Trim() 	: hoursLang[0].Trim();
		string daysString 		= diff.Days == 1 	? daysLang[0].Trim() 	: daysLang.Length > 1 	? daysLang[1].Trim() 	: daysLang[0].Trim();
		string monthsString 	= months == 1 		? monthsLang[0].Trim() 	: monthsLang.Length > 1 ? monthsLang[1].Trim() 	: monthsLang[0].Trim();
		string yearsString 		= years == 1 		? yearsLang[0].Trim() 	: yearsLang.Length > 1 	? yearsLang[1].Trim() 	: yearsLang[0].Trim();
		
		//Set First Letter to Upper for year, month & day if shortform
		if(shortForm)	 		 
		{	 		
			yearsString = char.ToUpper(yearsString[0]) + yearsString.Substring(1);
			monthsString = char.ToUpper(monthsString[0]) + monthsString.Substring(1);
			daysString = char.ToUpper(daysString[0]) + daysString.Substring(1);
		}		 
		Dictionary<string,long> wtValues = new Dictionary<string,long>();
		if(years > 0) wtValues.Add(yearsString,years);
		if(months > 0) wtValues.Add(monthsString,months);
        if(days > 0) wtValues.Add(daysString,days);
        if(hours > 0) wtValues.Add(hoursString,hours);
        if(minutes > 0) wtValues.Add(minutesString,minutes);
        if(seconds > 0 && showSeconds) wtValues.Add(secondsString,seconds);

		string joinedStr = shortForm ? string.Join(", ", wtValues.Select(kv => $"{kv.Value}{kv.Key[0]}")) : string.Join(", ", wtValues.Select(kv => $"{kv.Value} {kv.Key}"));
		if(String.IsNullOrEmpty(joinedStr)) joinedStr = shortForm ?$"0m{minutesString[0]}" : $"0 {minutesString}";
		
		return joinedStr;
    }
    
    public void RemoveInvisChar7TV()
	{
		if(CPH.TryGetArg("rawInput",out string rawInput) && rawInput.IndexOf("󠀀") != -1)
		{
			rawInput = rawInput.Replace("󠀀","");
			rawInput = rawInput.Trim();
			args["rawInput"] = rawInput;
			CPH.SetArgument("rawInput",rawInput);
			string[] inputList = rawInput.Split(' ');
			int i = inputList.Length - 1;
			if(CPH.TryGetArg("input"+i, out string input))
			{
				string newInput = input.Replace("󠀀","").Trim();
				args["input"+i] = newInput;
				CPH.SetArgument("input"+i,newInput);
			}
		}
	}
}

public class UserValue	 		 
{	 		
	public string User{get;set;}
	public string UserLogin{get;set;}
	public string UserId{get;set;}		  		
	public decimal Value{get;set;}
	public string FormattedValue{get;set;}		 			
}		 		
