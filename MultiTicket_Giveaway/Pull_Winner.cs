using System;
using System.Collections.Generic;
public class CPHInline
{
	public bool Execute()
	{
		CPH.TryGetArg("giveawayId",out string id);
		string varName = "pwnTwitchMultiTicketGiveaway_"+id;
		
		List<UserVariableValue<long>> usersVarList = CPH.GetTwitchUsersVar<long>(varName, true); 
		CPH.SetArgument("winnerFound",usersVarList.Count > 0);
		
		if(usersVarList.Count > 0)
		{
			List<UserVariableValue<long>> pullSet = new List<UserVariableValue<long>>();
			
			foreach(UserVariableValue<long> user in usersVarList)
			{
				for(int i = 0;i<user.Value;i++)
				{
					pullSet.Add(user);
				}
			}
			
			long totalUserEntries = usersVarList.Count;
			long totalEntries = pullSet.Count;
			
			UserVariableValue<long> winnerInfo = pullSet[CPH.Between(0, pullSet.Count-1)];
			
			CPH.SetArgument("winnerUserId",winnerInfo.UserId);
			CPH.SetArgument("winnerUserLogin",winnerInfo.UserLogin);
			CPH.SetArgument("winnerUser",winnerInfo.UserName);
			CPH.SetArgument("winnerEntries",winnerInfo.Value);
			double chance = Math.Round(((double)winnerInfo.Value / totalEntries) * 100,2);
			CPH.SetArgument("winnerChance",chance);
			CPH.SetArgument("giveawayTotalEntries",totalEntries);

			CPH.TryGetArg("removeWinner",out bool removeWinner);
			if(removeWinner)
			{
				CPH.UnsetTwitchUserVarById(winnerInfo.UserId, varName, true);
			}
		}
		
		return true;
	}
}
