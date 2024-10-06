using System;
using System.Drawing;
using System.Collections.Generic;
public class CPHInline
{
	public bool Execute()
	{
		
		CPH.TryGetArg("inputColor",out string inputColor);
		Dictionary<string,string> inputs = BreakUpInput(inputColor);
		
		Color foundColor = Color.Empty;
		KnownColor knownC;
		
		int a;
		string aString;
		int r;
		string rString;
		int g;
		string gString;
		int b;
		string bString;
		string hexString;
		
		bool found = false;
		if(inputs.Count > 0)
		{
			switch(inputs["input0"].ToLower())
			{
				case "hex":
					bool x = inputs.TryGetValue("input1",out hexString);
					if(inputs["input1"][0] != '#') hexString = "#" + hexString;
					foundColor = ColorTranslator.FromHtml(hexString);
						
					break;
				case "rgb":
					r = inputs.TryGetValue("input1",out rString) ? int.TryParse(rString,out r)? r :0: 0;
					g = inputs.TryGetValue("input2",out gString) ? int.TryParse(gString,out g)? g :0: 0;
					b = inputs.TryGetValue("input3",out bString) ? int.TryParse(bString,out b)? b :0: 0;
					r = r > 255 ? 255 : r < 0 ? 0 : r;
					g = g > 255 ? 255 : g < 0 ? 0 : g;
					b = b > 255 ? 255 : b < 0 ? 0 : b;
					foundColor = Color.FromArgb(r, g, b);
					
					break;
				case "argb":
					a = inputs.TryGetValue("input1",out aString) ? int.TryParse(aString,out a)? a :0: 0;
					r = inputs.TryGetValue("input2",out rString) ? int.TryParse(rString,out r)? r :0: 0;
					g = inputs.TryGetValue("input3",out gString) ? int.TryParse(gString,out g)? g :0: 0;
					b = inputs.TryGetValue("input4",out bString) ? int.TryParse(bString,out b)? b :0: 0;
					a = a > 255 ? 255 : a < 0 ? 0 : a;
					r = r > 255 ? 255 : r < 0 ? 0 : r;
					g = g > 255 ? 255 : g < 0 ? 0 : g;
					b = b > 255 ? 255 : b < 0 ? 0 : b;
					foundColor = Color.FromArgb(a, r, g, b);
					break;
				default:
					string nameInput = "";
					foreach(KeyValuePair<string,string> word in inputs)
					{
						nameInput += word.Value;
					}
					
					found = Enum.TryParse(nameInput,true, out knownC);
					foundColor = found ? Color.FromKnownColor(knownC): Color.Empty;
					break;
			}
		}
		
		found = foundColor != Color.Empty;
		CPH.SetArgument("foundColor",found);
		if(found)
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
	
	public Dictionary<string,string> BreakUpInput(string input)
	{	
		Dictionary<string,string> output = new Dictionary<string,string>();
		if(!String.IsNullOrEmpty(input))
		{
			string[] wordArray = input.Split(' ');	
			for(int i = 0;i<wordArray.Length;i++)
			{
				output.Add("input"+i,wordArray[i]);
			}
		}
		return output;
	}
}
