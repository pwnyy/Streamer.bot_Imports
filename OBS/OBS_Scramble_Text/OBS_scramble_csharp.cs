// Author: pwnyy , https://twitch.tv/pwnyytv , https://x.com/pwnyy, https://ko-fi.com/pwnyy
// Contact: contact@pwnyy.tv , or on the above mentioned social media.

//Original Idea taken from https://codepen.io/chaseottofy 
//https://codepen.io/chaseottofy/pen/PodxKpp

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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class CPHInline
{
	public bool Execute()
	{
		Remove7TVAllArgs();
		CPH.TryGetArg("scrambleWhiteSpaceChar",out string whiteSpaceString);
		char[] whiteSpaceChars = whiteSpaceString.ToCharArray();
		CPH.TryGetArg("scrambleCharacters",out string scrambleString);
		char[] randomChars = scrambleString.ToCharArray();
		
		CPH.TryGetArg("obsConnectionIndex",out long obsIndex);
		obsIndex = obsIndex > int.MaxValue ? int.MaxValue : obsIndex < 0 ? 0 : obsIndex; 
		CPH.TryGetArg("sourceName",out string sourceName);
		
		CPH.TryGetArg("usePerCharacterMs",out bool usePCM);
		CPH.TryGetArg("perCharacterMs",out long perCharacter);
		CPH.TryGetArg("maxTransitionMs",out long maxTransition);
		
		CPH.TryGetArg("fillMode",out long fillMode);
		fillMode = fillMode > 3 ? 3 : fillMode < 0 ? 0 : fillMode;

		CPH.TryGetArg("inputValue",out string origin);
		origin = String.IsNullOrEmpty(origin) ? " " : origin;
		int len = origin.Length;
		int mid = (int) Math.Floor((double)len / 2);
		
		if(CPH.ObsIsConnected((int)obsIndex))
		{
			long sleepy = 0;
			if(usePCM)
			{
				sleepy = perCharacter * len >= maxTransition ? (int) Math.Ceiling((double)maxTransition / len) : (int) Math.Ceiling((double)(perCharacter * len) / len);
			}else{
				sleepy = (int) Math.Ceiling((double)maxTransition / len);
			}
			
			
			Random rng = new Random();
			JObject waitRequest = new JObject(
				new JProperty("requestType","Sleep"),
				new JProperty("requestData", new JObject(
					new JProperty("sleepMillis", sleepy)
				))
			);
			List<JObject> requests = new List<JObject>();
			 
			char[] randomArr = origin.Select(c => Char.IsWhiteSpace(c) ? whiteSpaceChars[rng.Next(whiteSpaceChars.Length)] : randomChars[rng.Next(randomChars.Length)]).ToArray();
			string scrambleText = String.Join("",randomArr);
			JObject firstScramble = new JObject(
					new JProperty("requestType","SetInputSettings"),
					new JProperty("requestData", new JObject(
						new JProperty("inputName",sourceName),
						new JProperty("inputSettings", new JObject (
							new JProperty("text",scrambleText)
						))
					))
				);
			requests.Add(firstScramble);
			for(int i = 0; i<len;i++)
			{
				if(fillMode == 0 || fillMode == 2)randomArr[i] = origin[i];
				if(fillMode == 1 || fillMode == 2)
				{
					int revIndex = (len-1-i)< 0 ? 0 : (len-1-i);
					randomArr[revIndex] = origin[revIndex];
				}
				
				if(fillMode == 3)
				{
					if(mid-i >= 0)
					{
						randomArr[mid-i] = origin[mid-i];
					}
					if(mid+i < len)
					{
						if(i == 0)
						{
							randomArr[mid] = origin[mid];
						}else{
							randomArr[mid+i] = origin[mid+i];
						}
						
					}
					
				}
				requests.Add(waitRequest);
				scrambleText = String.Join("",randomArr);
				JObject scrambleRequest = new JObject(
					new JProperty("requestType","SetInputSettings"),
					new JProperty("requestData", new JObject(
						new JProperty("inputName",sourceName),
						new JProperty("inputSettings", new JObject (
							new JProperty("text",scrambleText)
						))
					))
				);
				requests.Add(scrambleRequest);
				
				if(scrambleText == origin)break;
			}
			string data = JsonConvert.SerializeObject(requests);
			CPH.ObsSendBatchRaw(data, true, 0, (int)obsIndex);
		}
		
		
		return true;
	}
	
	public void Remove7TVAllArgs()
	{
		Dictionary<string,object> tempArg = new Dictionary<string,object>(args);
		foreach(KeyValuePair<string,object> arg in tempArg)
		{
			if((arg.Value is string) && arg.Value.ToString().IndexOf("󠀀") != -1)
			{
				string temp = arg.Value.ToString();
				temp = temp.Replace("󠀀","").Trim();
				args[arg.Key] = temp;
				CPH.SetArgument(arg.Key,temp);
			}
		}
	}
}
