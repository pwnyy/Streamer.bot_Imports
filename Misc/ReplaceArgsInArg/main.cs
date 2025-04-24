// Author: pwnyy , https://twitch.tv/pwnyytv , https://x.com/pwnyy, https://ko-fi.com/pwnyy, https://pwnyy.tv
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Globalization;

public class CPHInline
{
	public bool Execute()
	{
		CPH.TryGetArg("argumentName",out string argumentName);
		CPH.TryGetArg(argumentName,out string replaceText);
		
		CPH.SetArgument(argumentName,ReplaceWithArgs(replaceText,args));
		
		return true;
	}
	
	public string ReplaceWithArgs(string message,Dictionary<string,object> argDict)
	{	 		 
		var regex = new Regex("%(.*?)(?::(.*?))?%");
		string input = message;
		var matches = regex.Matches(input);
		
		foreach(Match match in matches)
		{	 		
			string fullMatch = match.Groups[0].Value;
			string keyword = match.Groups[1].Value;
			string format = match.Groups[2].Value;
			if(argDict.TryGetValue(keyword,out object value))
			{		 
				if (argDict[keyword] is IFormattable formObject)
				{		 		
					try
					{
						input = input.Replace(fullMatch,formObject.ToString(format, CultureInfo.CurrentCulture));
					}catch (FormatException)
					{
						input = input.Replace(fullMatch,argDict[keyword].ToString());
					}
				}else{		 		
					input = input.Replace(fullMatch,argDict[keyword].ToString());
				}
			}

		}
		return input;
	}
}
