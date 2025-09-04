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
using System.Globalization;

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
        CPH.TryGetArg("user", out string user);
        CPH.TryGetArg("userId", out string userId);
        CPH.TryGetArg("userName", out string userName);
        CPH.TryGetArg("userType", out string platform);
        CPH.TryGetArg("input0", out string input);
        //bool isTrovo = platform == "trovo";
        CPH.TryGetArg("broadcastUser",out string broadcaster);
        
        bool shortF = true;		 
		bool showSeconds = false;	
		
        shortF = CPH.TryGetArg("shortFormTime", out shortF) ? shortF : true;		 		 
		showSeconds = CPH.TryGetArg("showSeconds", out showSeconds) ? showSeconds : false;		 		
		secondsLang = CPH.TryGetArg("secondsLang", out string secondsLangStr) ? secondsLangStr.Split(','):"second,seconds".Split(',');		 
		minutesLang = CPH.TryGetArg("minutesLang", out string minutesLangStr) ? minutesLangStr.Split(','):"minute,minutes".Split(',');		 		
		hoursLang = CPH.TryGetArg("hoursLang", out string hoursLangStr) ? hoursLangStr.Split(','):"hour,hours".Split(',');		 		
		daysLang = CPH.TryGetArg("daysLang", out string daysLangStr) ? daysLangStr.Split(','):"day,days".Split(',');
		monthsLang = CPH.TryGetArg("monthsLang", out string monthsLangStr) ? monthsLangStr.Split(','):"month,months".Split(',');
		yearsLang = CPH.TryGetArg("yearsLang", out string yearsLangStr) ? yearsLangStr.Split(','):"year,years".Split(',');
		
		if(!String.IsNullOrEmpty(input) && platform == "twitch")
		{
			TwitchUserInfo inputUser = CPH.TwitchGetUserInfoByLogin(input);
			if(inputUser != null)
			{
				userId = inputUser.UserId;
				user = inputUser.UserName;
				userName = inputUser.UserLogin;
				
			}
		}
		long watchtime = GetWatchtime(userId,user,userName, platform);
		string formattedtime = SecToTime(watchtime, showSeconds, shortF);

		CPH.SetArgument("watchtimeFormatted", formattedtime);
        CPH.SetArgument("targetUser",user);
        CPH.SetArgument("targetUserId",userId);
        CPH.SetArgument("targetUserName",userName);
        return true;
        
    }
    
    public string SecToTime(long secondsInput, bool showSeconds,bool shortForm)	 		 
    {	 		
        TimeSpan diff = TimeSpan.FromSeconds(secondsInput);		 
        CPH.SetArgument("totalSeconds", diff.TotalSeconds);
        CPH.SetArgument("totalHours", diff.TotalMinutes);
        CPH.SetArgument("totalHours", diff.TotalHours);
        CPH.SetArgument("totalDays", diff.TotalDays);
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

		string joinedStr = shortForm ? string.Join(" ", wtValues.Select(kv => $"{kv.Value}{kv.Key[0]}")) : string.Join(", ", wtValues.Select(kv => $"{kv.Value} {kv.Key}"));
		if(String.IsNullOrEmpty(joinedStr)) joinedStr = shortForm ?$"0{minutesString[0]}" : $"0 {minutesString}";
		
		return joinedStr;
    }
    
    public long GetWatchtime(string userId,string user, string userName, string platform)
    {
    	string checkNum = "";
		switch(platform)
		{
			case "twitch":
				checkNum = CPH.GetTwitchUserVarById<string?>(userId, "watchtime", true) ?? "0";
				break;
			case "youtube":
				checkNum = CPH.GetYouTubeUserVarById<string?>(userId, "watchtime", true) ?? "0";
				break;
			case "trovo":
				checkNum = CPH.GetTrovoUserVarById<string?>(userId, "watchtime", true) ?? "0";
				break;
			case "kick":
				checkNum = CPH.GetKickUserVarById<string?>(userId, "watchtime", true) ?? "0";
				break;
		}
		bool check = long.TryParse(checkNum,out long currentWatchtime);
		if(!check)
		{
			CPH.LogError($"[Watchtime] - {platform} - {user} / {userName}({userId}) - Watchtime seems to have been in wrong format. Current value: \"{checkNum}\". Will be reset to 0.");
		}
		return check ? currentWatchtime : 0;
    }
    
}
