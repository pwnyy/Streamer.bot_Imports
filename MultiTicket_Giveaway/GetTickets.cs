using System;

public class CPHInline
{	 		 
    public bool Execute()
    {	 		
        CPH.TryGetArg("giveawayId", out string id);
        CPH.TryGetArg("ticketCost", out long ticketCost);
        CPH.TryGetArg("maxUserTickets", out long maxTickets);
        CPH.TryGetArg("nonValidInputIsTicket", out bool nvIsTicket);
        CPH.TryGetArg("userVarUsed", out string varName);
        CPH.TryGetArg("user", out string user);
        CPH.TryGetArg("userId", out string userId);
        CPH.TryGetArg("ignoreInput",out bool ignoreInput);
        CPH.TryGetArg("rawInput", out string input);
		
		//Disabling use of other platforms than Twitch (multiplatform comes later)
		CPH.TryGetArg("userType",out string platform);
		if(platform.ToLower() != "twitch") return false;
		
        // Get user's value
        string valueString = CPH.GetTwitchUserVarById<string>(userId, varName, true) ?? "0";
        if (!long.TryParse(valueString, out long userValue))
        {		 
            LogError($"Error in user variable for {user}({userId}): {varName}", valueString);
            return true;
        }

        // Get user's current ticket count
        string userTicketVar = $"pwnTwitchMultiTicketGiveaway_{id}";
        string currentTicketsString = CPH.GetTwitchUserVarById<string>(userId, userTicketVar, true) ?? "0";
        if (!long.TryParse(currentTicketsString, out long currentTickets))
        {		 		
            LogError($"Error in user variable for {user}({userId}): {userTicketVar}", currentTicketsString);
            return true;
        }

        // Determine ticket amount from input
        input = input?.Trim() ?? string.Empty;
        long ticketAmount = 1;
        long ticketAmountInput = 0;
        bool isValidInput = String.IsNullOrEmpty(input) || long.TryParse(input,out ticketAmountInput);
        
        ticketAmount = isValidInput && !String.IsNullOrEmpty(input) ? ticketAmountInput : ticketAmount;
        
        ticketAmount = ignoreInput ? 1 : ticketAmount;
        
        bool tryGetTicket = isValidInput || nvIsTicket;

        // Calculate new ticket amount and check validity
        long newTicketAmount = currentTickets + ticketAmount;
        bool validAmount = maxTickets <= 0 || newTicketAmount <= maxTickets;

        if (tryGetTicket && validAmount)
        {		 		
            long totalCost = ticketAmount * ticketCost;
            if (totalCost <= userValue)
            {	 		 
                // Update user variables
                CPH.SetTwitchUserVarById(userId, userTicketVar, newTicketAmount, true);
                CPH.SetTwitchUserVarById(userId, varName, userValue - totalCost, true);
                SetResult(1, ticketAmount, totalCost, newTicketAmount, userValue);
            }
            else
            {	 		
                SetResult(0, ticketAmount, totalCost, currentTickets, userValue);
            }
        }
        else
        {		 
            SetResult(validAmount ? -2 : -1, ticketAmount, ticketAmount * ticketCost, currentTickets, userValue);
        }

        return true;
    }

    private void LogError(string message, string currentValue)
    {		 		
        CPH.LogError($"[pwn Twitch MultiTicketGiveaway] {message}. Current value is \"{currentValue}\".");
        CPH.SetArgument("messageType", -3);
    }

    private void SetResult(int result, long ticketAmount, long totalCost, long totalTickets, long currentUserValue)
    {		 		
        CPH.SetArgument("messageType", result);
        CPH.SetArgument("boughtTickets", ticketAmount);
        CPH.SetArgument("totalCost", totalCost);
        CPH.SetArgument("totalTickets", totalTickets);
        CPH.SetArgument("currentUserValue", currentUserValue);
        if(result == 1)
        {
			CPH.SetArgument("newUserValue", currentUserValue - totalCost);
        }
    }
}
