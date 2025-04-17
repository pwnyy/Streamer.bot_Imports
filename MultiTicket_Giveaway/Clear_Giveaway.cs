using System;

public class CPHInline
{
	public bool Execute()
	{
		CPH.TryGetArg("giveawayId",out string id);
		string varName = "pwnTwitchMultiTicketGiveaway_"+id;
		var entries = CPH.GetTwitchUsersVar<long>(varName, true);
		bool hadEntries = entries.Count > 0;
		if(hadEntries)
		{
			CPH.UnsetAllUsersVar("pwnTwitchMultiTicketGiveaway_"+id, true);
		}
		string prompt = hadEntries ? $"{entries.Count} User(s) have been cleared from Giveaway with id \"{id}\"" : $"No user entries found for giveaway with id \"{id}\"";
		CPH.SetArgument("resultPrompt",prompt);
		return true;
	}
}
