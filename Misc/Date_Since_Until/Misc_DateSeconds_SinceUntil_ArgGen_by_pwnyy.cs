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
	public string[] secondsLang;
	public string[] minutesLang;
	public string[] hoursLang;
	public string[] daysLang;
	public string[] monthsLang;
	public string[] yearsLang;
	
	public bool Execute()
	{	 		
		CPH.TryGetArg("inputIsSeconds", out bool isSeconds);
		bool addResult = false;
		DateTime chosenDate = new DateTime();
		DateTime currentDate = DateTime.Now;
		CPH.SetArgument("addDateArgumentsResult",addResult);
		if(!isSeconds)
		{		 
			try
			{		 		
				CPH.TryGetArg("inputValue",out DateTime dateValue);
			}
			catch (Exception e)
			{		 		
				CPH.LogError("[Misc Date/Seconds Since Arg Generator] - inputValue was not a date as indicated. Check if inputIsSeconds is set to True, if yes turn to False");
				return true;
			}
		}else{	 		 
			CPH.TryGetArg("inputValue",out string stringValue);		
			string correctedValue = DecimalSeparatorConvert(stringValue);
			if (!double.TryParse(correctedValue, out double doubleValue)) return true;
			long result = (long)Math.Floor(doubleValue);
			chosenDate = currentDate.AddSeconds(-result);
		}
		addResult = true;
		CPH.SetArgument("addDateArgumentsResult",addResult);

		bool shortF = CPH.TryGetArg("shortFormTime", out shortF) ? shortF : true;		 		 
		bool showSeconds = CPH.TryGetArg("showSeconds", out showSeconds) ? showSeconds : false;		 		
		secondsLang = CPH.TryGetArg("secondsLang", out string secondsLangStr) ? secondsLangStr.Split(','):"second,seconds".Split(',');		 
		minutesLang = CPH.TryGetArg("minutesLang", out string minutesLangStr) ? minutesLangStr.Split(','):"minute,minutes".Split(',');		 		
		hoursLang = CPH.TryGetArg("hoursLang", out string hoursLangStr) ? hoursLangStr.Split(','):"hour,hours".Split(',');		 		
		daysLang = CPH.TryGetArg("daysLang", out string daysLangStr) ? daysLangStr.Split(','):"day,days".Split(',');
		monthsLang = CPH.TryGetArg("monthsLang", out string monthsLangStr) ? monthsLangStr.Split(','):"month,months".Split(',');
		yearsLang = CPH.TryGetArg("yearsLang", out string yearsLangStr) ? yearsLangStr.Split(','):"year,years".Split(',');
		
		CPH.SetArgument("dateChecked",chosenDate);
		bool hasPassed = chosenDate <= currentDate;
		CPH.SetArgument("dateHasPassed", hasPassed);
		string output = SecToTime(currentDate,chosenDate,showSeconds,shortF);
		
		CPH.SetArgument("timeOutput",output);
		
		return true;
	}
	
	public string SecToTime(DateTime current, DateTime chosen, bool showSeconds,bool shortForm)	 		 
    {	 		
		TimeSpan diff = chosen <= current ? current - chosen : chosen - current;

        int years = diff.Days / 365;		 		
		int remainingDays = diff.Days % 365;		 		
		int months = (int)(remainingDays / 30.44);
		int days = (int)(remainingDays % 30.44);
		int hours = diff.Hours;
		int minutes = diff.Minutes;
		int seconds = diff.Seconds;
		
		CPH.SetArgument("yearsDiff",years);
		CPH.SetArgument("yearsDiffTotal",Math.Floor((int)diff.TotalDays / 365.2425));
		CPH.SetArgument("daysDiff",remainingDays);
		CPH.SetArgument("daysDiffTotal",(int)diff.TotalDays);
		CPH.SetArgument("monthsDiff",months);
		CPH.SetArgument("hoursDiff",hours);
		CPH.SetArgument("hoursDiffTotal",(int)diff.TotalHours);
		CPH.SetArgument("minutesDiff",minutes);
		CPH.SetArgument("minutesDiffTotal",(int)diff.TotalMinutes);
		CPH.SetArgument("secondsDiff",seconds);
		CPH.SetArgument("secondsDiffTotal",(int)diff.TotalSeconds);
		
		string secondsString 	= diff.Seconds == 1 ? secondsLang[0].Trim() : secondsLang.Length > 1? secondsLang[1].Trim() : secondsLang[0].Trim();
		string minutesString 	= diff.Minutes == 1 ? minutesLang[0].Trim() : minutesLang.Length > 1? minutesLang[1].Trim() : minutesLang[0].Trim();
		string hoursString		= diff.Hours == 1 	? hoursLang[0].Trim() 	: hoursLang.Length > 1 	? hoursLang[1].Trim() 	: hoursLang[0].Trim();
		string daysString 		= diff.Days == 1 	? daysLang[0].Trim() 	: daysLang.Length > 1 	? daysLang[1].Trim() 	: daysLang[0].Trim();
		string monthsString 	= months == 1 		? monthsLang[0].Trim() 	: monthsLang.Length > 1 ? monthsLang[1].Trim() 	: monthsLang[0].Trim();
		string yearsString 		= years == 1 		? yearsLang[0].Trim() 	: yearsLang.Length > 1 	? yearsLang[1].Trim() 	: yearsLang[0].Trim();
		
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
    
	public string DecimalSeparatorConvert(string inputNumber)
	{		 		
		CultureInfo systemCulture = CultureInfo.CurrentCulture;
		NumberFormatInfo nbInfo = systemCulture.NumberFormat;
		string decSep = nbInfo.NumberDecimalSeparator;
		CultureInfo cultureWithComma = new CultureInfo("fr-FR");
		CultureInfo invariantCulture = CultureInfo.InvariantCulture;

		if (inputNumber.Contains(",") && !inputNumber.Contains(".") && decSep == "." && double.TryParse(inputNumber, NumberStyles.Any, cultureWithComma, out double result))
		{		 		
			inputNumber = result.ToString("N",invariantCulture);
		}else if (inputNumber.Contains(".") && !inputNumber.Contains(",") && decSep == "," && double.TryParse(inputNumber, NumberStyles.Any, invariantCulture, out double result2))
		{	 		 
			inputNumber = result2.ToString("N",cultureWithComma);
		}
		
		return inputNumber;
	}
}
