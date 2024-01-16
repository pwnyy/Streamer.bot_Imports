using System;
using System.Linq;
using System.Collections.Generic;

public class CPHInline
{
    public bool Execute()
    {
        string userVarName = args["userVarName"].ToString();
        int top = Convert.ToInt32(args["top"].ToString());
        string userName = args["userName"].ToString(); // Assuming you have the username as an argument.
        List<UserVariableValue<long>> userIdsWithVariable = CPH.GetTwitchUsersVar<long>(userVarName, true);

        // Get Broadcaster Info
        TwitchUserInfo broadcaster = CPH.TwitchGetBroadcaster();
        // Array of bot names
        string[] botNames = { "bot1", "bot2", "bot3" }; // Replace with your actual bot names

        // Remove occurrences of bot names in userIdsWithVariable
        userIdsWithVariable.RemoveAll(e => botNames.Contains(e.UserLogin, StringComparer.OrdinalIgnoreCase));
        // Remove Broadcasting from userIdsWithVariable
        userIdsWithVariable.RemoveAll(e => e.UserId == broadcaster.UserId);

        //Sort Users by Value
        userIdsWithVariable = userIdsWithVariable.OrderByDescending(u => u.Value).ToList();

        int userPlace = -1; // Initialize to -1 in case the user is not found in the list.

        for (int count = 0; count < userIdsWithVariable.Count && count < top; count++)
        {
            UserVariableValue<long> number = userIdsWithVariable[count];

            string numberString;
            if (userVarName == "watchtime")
            {
                numberString = TimeDisplay(number.Value);
            }
            else
            {
                numberString = number.Value.ToString("N0");
            }

            string message = $"{count + 1}) {number.UserName} - {numberString}";

            CPH.LogInfo(message);
            CPH.SendMessage(message, true);

            if (userName.Equals(number.UserName, StringComparison.OrdinalIgnoreCase))
            {
                userPlace = count + 1; // Add 1 to make it a 1-based index.
            }
        }

        // Send a message to the user who issued the command indicating their place among filtered users.
        if (userPlace > 0)
        {
            CPH.SendMessage($"You are in {userPlace} place among the filtered users.", true);
        }
        else
        {
            CPH.SendMessage("You are not in the list of filtered users. Your place is not available.", true);
        }

        return true;
    }
        // The TimeDisplay method remains unchanged.
    private string TimeDisplay(long timeDisplaySeconds)
    {
        TimeSpan timeDifference = TimeSpan.FromSeconds(timeDisplaySeconds);

        int years = (int)Math.Floor(timeDifference.TotalDays / 365);
        int days = (int)Math.Floor(timeDifference.TotalDays % 365);
        int hours = timeDifference.Hours;
        int minutes = timeDifference.Minutes;
        int seconds = timeDifference.Seconds;

        // Adjust for leap years
        int leapYears = years / 4; // assuming every 4 years is a leap year

        days += leapYears;

        string timeDisplay = "";

        if (years > 0)
        {
            timeDisplay += years + " year" + (years > 1 ? "s " : " ");
        }

        if (days > 0)
        {
            timeDisplay += days + " day" + (days > 1 ? "s " : " ");
        }

        if (hours > 0)
        {
            timeDisplay += hours + " hour" + (hours > 1 ? "s " : " ");
        }

        if (minutes > 0)
        {
            timeDisplay += minutes + " minute" + (minutes > 1 ? "s " : " ");
        }

        if (seconds > 0)
        {
            timeDisplay += seconds + " second" + (seconds > 1 ? "s " : " ");
        }
        return timeDisplay;
    }
}
