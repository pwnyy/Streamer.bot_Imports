using System;

public class CPHInline
{
	public bool Execute()
	{	//.Get- needed- info.
		string rewardId = args["rewardId"].ToString();
		string redemptionId = args["redemptionId"].ToString();
		string userId = args["userId"].ToString();
		int rewardCooldown = Convert.ToInt32(args["cooldownPerUser"]);
		//.Get- current- time and check uservar
		DateTime currentTime = DateTime.Now;
		DateTime rewardLastUsed = CPH.GetTwitchUserVarById<DateTime?>(userId,"twitchRewardCooldown_"+rewardId,true) ?? currentTime;
		//-Determine. cooldown
		TimeSpan secondsLeft = currentTime - rewardLastUsed;
		bool cooldownActive = false;
		//-Check. if- cooldown- is smaller than wanted cooldown if yes set cooldown to true
		if(secondsLeft.TotalSeconds < rewardCooldown && currentTime > rewardLastUsed)
		{
			cooldownActive = true;
			CPH.SetArgument("rewardCooldownLeft",Math.Round(rewardCooldown - secondsLeft.TotalSeconds,0));
		}else{
			CPH.SetTwitchUserVarById(userId, "twitchRewardCooldown_"+rewardId, currentTime, true);
		}
		//-Refund. if- wanted-
		if(args.ContainsKey("refundRewardPoints") && Convert.ToBoolean(args["refundRewardPoints"].ToString()) && cooldownActive)
		{
			CPH.TwitchRedemptionCancel(rewardId, redemptionId);
		}
		CPH.SetArgument("rewardOnCooldown", cooldownActive);
		
		return true;
	}
}
