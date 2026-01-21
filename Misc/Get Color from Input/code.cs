// Author: pwnyy , https://twitch.tv/pwnyytv , https://x.com/pwnyy, https://ko-fi.com/pwnyy, https://pwnyy.tv
// Contact: contact@pwnyy.tv , or on the above mentioned social media.
// Make sure to contact me if you are using my code somewhere, so I can see where it's being used!
//
// This program is licensed under the GNU General Public License Version 3 (GPLv3).
// 
// The GPLv3 is a free software license that ensures end users have the freedom to run,
// study, share, and modify the software. Key provisions include:
// 
// - Copyleft: Modified versions of the software must also be licensed under the GPLv3.
// - Source Code: You must provide access to the source code when distributing the software.
// - Credit: You must credit the original author of the software, by mentioning either contact e-mail or their social media.
// - No Warranty: The software is provided "as-is," without warranty of any kind.
// 
// For more details, see https://www.gnu.org/licenses/gpl-3.0.en.html.

using System;
using System.Drawing;
using System.Collections.Generic;
public class CPHInline
{	 		 
	const string SevenTvWhitespace = "\u034F";
	static string versionNum = "1.0.1";
	static string extensionName = "pwn Get Color from Input";

	public bool Execute()
	{	 		
		
		CPH.TryGetArg("inputColor",out string inputColor);
		inputColor = inputColor.Trim();
		
		int i = 0;
		bool aliasFound = false;
		while(CPH.TryGetArg("colorAlias"+i, out string colorAlias) && !aliasFound)
		{		 
			i++;
			int sepIndex = colorAlias.LastIndexOf('|');
			if(sepIndex == -1 || sepIndex == colorAlias.Length-1) continue;
			string origin = colorAlias.Substring(0,sepIndex-1).Trim();
			string[] aliases = colorAlias.Substring(sepIndex+1).Split(',');
			foreach(string aliasName in aliases)
			{		 		
				if(inputColor.Equals(aliasName.Trim(), StringComparison.OrdinalIgnoreCase))
				{
					aliasFound = true;
					inputColor = origin;
					break;
				}
			}
		}

		Dictionary<string,string> inputs = BreakUpInput(inputColor.ToLower());
		int format = GetColorFormat(inputs);

		Color foundColor = Color.Empty;
		KnownColor knownC;
		bool colorWasFound = false;

		CPH.SetArgument("foundColor",colorWasFound);

		if(inputs.Count > 0 )
		{	 		 
			string firstInput = inputs["input0"].ToLower();
			bool hasLabel = (firstInput == "rgb" || firstInput == "argb" || firstInput == "rgba" || firstInput == "hex");
			int startIndex = hasLabel ? 1 : 0;
			
			switch (format)
			{	 		
				case 1:
					string hexPart = inputs.TryGetValue("input" + startIndex, out string hexValue) ? hexValue : firstInput;
					if (!hexPart.StartsWith("#")) hexPart = "#" + hexPart;

					if(hexPart.Length == 9)
					{
						try {
							int argb = Int32.Parse(hexPart.Replace("#", ""), System.Globalization.NumberStyles.HexNumber);
							foundColor = Color.FromArgb(argb);
						} catch { }
					} else {
						try 
						{ 
							foundColor = ColorTranslator.FromHtml(hexPart); 
						} catch { }
					}

					break;

				case 2:
					foundColor = Color.FromArgb(
						GetRGBSafeInt(inputs, startIndex), 
						GetRGBSafeInt(inputs, startIndex + 1), 
						GetRGBSafeInt(inputs, startIndex + 2)
					);
					break;

				case 3:
					foundColor = Color.FromArgb(
						GetRGBSafeInt(inputs, startIndex), 
						GetRGBSafeInt(inputs, startIndex + 1), 
						GetRGBSafeInt(inputs, startIndex + 2), 
						GetRGBSafeInt(inputs, startIndex + 3)
					);
					break;

				case 4:
					foundColor = Color.FromArgb(
						GetRGBSafeInt(inputs, startIndex + 3),
						GetRGBSafeInt(inputs, startIndex),
						GetRGBSafeInt(inputs, startIndex + 1),
						GetRGBSafeInt(inputs, startIndex + 2)
					);
					break;

				default:
					string nameInput = string.Join("", inputs.Values).ToLower().Replace("grey", "gray");
					if (Enum.TryParse(nameInput,true, out knownC))
						foundColor = Color.FromKnownColor(knownC);
					break;
			}
		}
		
		colorWasFound = foundColor != Color.Empty;
		CPH.SetArgument("foundColor",colorWasFound);
		if(colorWasFound)
		{		 
			
			int alpha = foundColor.A;
			int red = foundColor.R;
			int green = foundColor.G;
			int blue = foundColor.B;
			
			string hex = $"{red:X2}{green:X2}{blue:X2}";
			string hexAlpha = $"#{hex}{alpha:X2}";
			string alphaHex = $"#{alpha:X2}{hex}";
			long obsColor = CPH.ObsConvertRgb(alpha, red, green, blue);
			
			CPH.SetArgument("colorName",foundColor.IsNamedColor ? foundColor.Name : "none");
			CPH.SetArgument("colorA",alpha);
			CPH.SetArgument("colorR",red);
			CPH.SetArgument("colorG",green);
			CPH.SetArgument("colorB",blue);
			CPH.SetArgument("colorHex",hex);
			CPH.SetArgument("colorAlphaHex",alphaHex);
			CPH.SetArgument("colorHexAlpha",hexAlpha);
			CPH.SetArgument("colorObs",obsColor);
		}

		return true;
	}
	
	public void Remove7TVWhiteSpace()
	{		 		
		try
		{		 		
			Dictionary<string, object> tempArg = new Dictionary<string, object>(args);
			foreach (KeyValuePair<string, object> arg in tempArg)
			{	 		 
				if ((arg.Value is string) && arg.Value.ToString().Contains(SevenTvWhitespace))
				{
					string temp = arg.Value.ToString();
					temp = temp.Replace(SevenTvWhitespace, "").Trim();
					args[arg.Key] = temp;
					CPH.SetArgument(arg.Key, temp);
				}
			}
		}
		catch (Exception ex)
		{	 		
			ExtensionLog($"Remove7TVWhiteSpace error: {ex.Message}", -2);
		}
	}
	
	public Dictionary<string,string> BreakUpInput(string input)
	{		 
		Dictionary<string,string> output = new Dictionary<string,string>();
		if(!String.IsNullOrEmpty(input))
		{	 		 
			string[] wordArray = input.Split(new[] {' ',','}, StringSplitOptions.RemoveEmptyEntries);	
			for(int i = 0;i<wordArray.Length;i++)
			{	 		
				output.Add("input"+i,wordArray[i]);
			}		 
		}
		return output;
	}

	public int GetColorFormat(Dictionary<string, string> inputs)
	{		 		
		if (inputs.Count == 0) return 0;

		string first = inputs["input0"].ToLower();

		if (first == "hex" || first.StartsWith("#")) return 1;
		if (first == "rgb")  return 2;
		if (first == "argb") return 3;
		if (first == "rgba") return 4;
		
		if (int.TryParse(first, out _))
		{		 		
			return (inputs.Count == 4) ? 3 : 2;
		}

		return 0;
	}

	private int GetRGBSafeInt(Dictionary<string, string> inputs, int index)
    {	 		 
        if (inputs.TryGetValue("input" + index, out string value) && int.TryParse(value, out int result))
            return Math.Max(0, Math.Min(255, result));
        return 0;
    }

	public void ExtensionLog(string message, int logType = 0)
	{	 		
		string output = $"[{extensionName}][{versionNum}] - {message}";

		switch(logType)
		{		 
			case -2:
				CPH.LogError(output);
				break;
			case -1:
				CPH.LogWarn(output);
				break;
			case 1:
				CPH.LogDebug(output);
				break;
			case 2:
				CPH.LogVerbose(output);
				break;
			default:
				CPH.LogInfo(output);
				break;
		}
	}
}
