using System;
using System.Collections.Generic;

public class CPHInline
{
	static Dictionary<string,TimeoutInfo> userDict = new Dictionary<string,TimeoutInfo>();
	
	public bool Execute()
	{
		EventType eventSource = CPH.GetEventType();
		CPH.TryGetArg("fromUserId",out string fromUserId);
		DateTime currentDT = DateTime.Now;
		
		
		if(eventSource == EventType.TwitchUserTimedOut)
		{
			CPH.TryGetArg("userId",out string userId);
			CPH.TryGetArg("duration",out long duration);
			if(userDict.TryGetValue(userId,out TimeoutInfo info))
			{
				info.TimeoutStart = currentDT;
				info.InitialDuration = duration;
				
				userDict[userId] = info; 
			}else{
				TimeoutInfo newInfo = new TimeoutInfo(){
					TimeoutStart = currentDT,
					InitialDuration = duration
				};
				userDict.Add(userId,newInfo);
			}
			return false;
		}else if(eventSource == EventType.TwitchUserUntimedOut)
		{
			CPH.TryGetArg("userId",out string userId);
			userDict.Remove(userId);
			return false;
		}
		
		bool inDict = userDict.TryGetValue(fromUserId, out TimeoutInfo infoCheck);
		
		if(inDict)
		{
			bool inTimeout = infoCheck.IsStillTimedOut(currentDT);
			long remaining = infoCheck.GetRemainingTime(currentDT);
			
			CPH.SetArgument("inTimeout",inTimeout);
			
			if(inTimeout)
			{
				CPH.SetArgument("timeoutDateTime",infoCheck.TimeoutStart);
				CPH.SetArgument("timeoutDuration",infoCheck.InitialDuration);
				CPH.SetArgument("timeoutDurationLeft",remaining);
			}else if(inDict && !inTimeout){
				userDict.Remove(fromUserId);
			}
		}else{
			CPH.SetArgument("inTimeout",false);
		}

		
		return true;
	}
	
	public class TimeoutInfo
	{
		public DateTime TimeoutStart {get;set;} = DateTime.Now;
		public long InitialDuration {get;set;} = -1;
		
		public long GetRemainingTime(DateTime now)
		{
			var endTime = TimeoutStart.AddSeconds(InitialDuration);
			var remaining = (long)(endTime - now).TotalSeconds;
			return remaining;
		}
		
		public bool IsStillTimedOut(DateTime now)
		{
			return now < TimeoutStart.AddSeconds(InitialDuration);
		}
	}
}
