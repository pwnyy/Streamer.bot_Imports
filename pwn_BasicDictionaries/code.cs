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
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;

public enum MethodResult
{	 		 
	Error = -2,
	InvalidInput = -1,
	DictNotFound = 0,
	EntryNotFound = 2,
	DictExists = 3,
	EntryExists = 5,
	Success = 7,
}

public class CPHInline
{	 		
	// Start of being able to edit and I still give support, anything above being changed by others, no support guaranteed.
	static string globalVarName = "pwn_BasicDictionaries_default";
	const char KeyValueSeparator = ':';
	const char DictSeparator = ';';
	//End of being able to edit and I still give support, anything below being changed by others, no support guaranteed.

	static string versionNum = "1.0.0";
	static string extensionName = "pwn Basic Dictionaries";
	private readonly object _dictLock = new object();
	Dictionary<string, Dictionary<string,string>> _dictData = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);

	static string customTriggerCategory = "Basic Dictionaries";
	static string triggerPrefix = "pwn_BasicDictionaries";
	Dictionary<string,string> triggerContext = new Dictionary<string,string>()
	{		 
		{triggerPrefix+"_EntryAdded","Entry Added"},
		{triggerPrefix+"_EntryUpdated","Entry Updated"},
		{triggerPrefix+"_EntryRemoved","Entry Removed"},
		{triggerPrefix+"_DictAdded","Dictionary Added"},
		{triggerPrefix+"_DictRenamed","Dictionary Renamed"},
		{triggerPrefix+"_DictRemoved","Dictionary Removed"}
	};

	const string methodResult = "extMethodResult";
	const int MaxRetryAttempts = 5;
	const int CommandDelayMs = 100;
	public const string SevenTvWhitespace = "\u034F";
	
	//const string SevenTvWhitespace = "󠀀";

	
	static readonly Dictionary<string, int> PlatformMessageLimits = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
	{		 		
		{ "twitch", 500 },
		{ "kick", 500 },
		{ "youtube", 200 },
		{ "trovo", 300 }
	};
 
	public void Init()
	{		 		
		ExtensionLog($"This extension was developed by pwnyy. Contact: contact@pwnyy.tv , Socials: https://pwnyy.tv");
		ExtensionLog("Init Custom Triggers",1);
		string[] contextMenu = {"[pwn] Extensions",customTriggerCategory};
		
		foreach(KeyValuePair<string,string> kvp in triggerContext)
		{
			string triggerName = kvp.Value;
			string eventName = kvp.Key;
			if(!CPH.RegisterCustomTrigger(triggerName, eventName, contextMenu))
			{	 		
				bool success = false;
				for(int i = 0; i < MaxRetryAttempts; i++)
				{		 
					triggerName += " [" + (i + 1) + "]";
					if(CPH.RegisterCustomTrigger(triggerName, eventName, contextMenu))
					{
						success = true;
						break;
					}
				}
				if(!success)
				{		 		
					ExtensionLog($"Was not able to register custom trigger {triggerName} with {MaxRetryAttempts} attempts.", -1);
				}
			}
		}
		
		_dictData = CPH.GetGlobalVar<Dictionary<string,Dictionary<string,string>>?>(globalVarName, true) ?? new Dictionary<string,Dictionary<string,string>>(StringComparer.OrdinalIgnoreCase)
		{
			{"default", new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase)}
		};
		_dictData = NormalizeDict(_dictData);
		ExtensionLog($"Retrieved {_dictData.Count} dictionaries from global variable.",2);
	}

	public void Dispose()
	{	 		 
		SaveDictionaries();
		string json = JsonConvert.SerializeObject(_dictData,Formatting.None);
		ExtensionLog("Log on Dispose: "+json);
	}	

	public bool Execute()
	{	 		
		string extMethod = "entryGet";

		ExtensionLog($"{extMethod}: Reloading dictionaries.", 2);
		_dictData = CPH.GetGlobalVar<Dictionary<string,Dictionary<string,string>>?>(globalVarName, true) ?? new Dictionary<string,Dictionary<string,string>>(StringComparer.OrdinalIgnoreCase);
		_dictData = NormalizeDict(_dictData);

		return true;
	}

	public void GetDictName(string input, bool getEntry, out string dictName, out string rest)
	{		 
		dictName = default;
		rest = input;

		if (string.IsNullOrEmpty(input))
			return;

		rest = input;

		CPH.TryGetArg("searchDictName", out dictName);

		int kvpSepIndex = input.IndexOf(KeyValueSeparator);
		int dictSepIndex = input.IndexOf(DictSeparator);

		bool getCheck = kvpSepIndex <= dictSepIndex ? getEntry ? true : false : true; 

		if (dictSepIndex > 0 && getCheck && input[dictSepIndex - 1] != '\\')
		{
			dictName = input.Substring(0, dictSepIndex).Trim();
			rest = input.Substring(dictSepIndex + 1).Trim();
		}else if (dictSepIndex > 0 && input[dictSepIndex - 1] == '\\')
		{
			rest = input.Substring(0, dictSepIndex - 1) + input.Substring(dictSepIndex);
		}
	}

	public bool GetDictionaryInfo(string searchKey, out Dictionary<string,object> dictInfo, bool onlyName = false)
	{		 		
		dictInfo = InitArgsDict("dictFound", false, false);
		dictInfo["dictName"] = searchKey;
		if (string.IsNullOrWhiteSpace(searchKey))
		{
			return false;
		}

		foreach (var kvpOuter in _dictData)
		{
			if (string.Equals(kvpOuter.Key, searchKey, StringComparison.OrdinalIgnoreCase))
			{
				dictInfo["dictName"] = kvpOuter.Key;
				dictInfo["dictFound"] = true;
				if (!onlyName)
				{
					dictInfo["dictEntries"] = kvpOuter.Value;
					dictInfo["dictEntryCount"] = kvpOuter.Value.Count;
				}
				
				return true;
			}
		}

		return false;
	}

	public bool GetInnerDictInfo(string searchKey, Dictionary<string,string> dict, out Dictionary<string,object> entryInfo)
	{		 		
		entryInfo = new Dictionary<string,object>();
		entryInfo["entryFound"] = false;
		foreach( var kvpInner in dict)
		{
			if (string.Equals(kvpInner.Key, searchKey, StringComparison.OrdinalIgnoreCase))
			{
				entryInfo["entryFound"] = true;
				entryInfo["entryKey"] = kvpInner.Key;
				entryInfo["entryValue"] = kvpInner.Value;
				return true;
			}
		}
		return false;
	}

	public bool GetDictEntry(string search, string dictName, out string value)
	{	 		 
		value = default;
		if( _dictData.TryGetValue(dictName, out var iDict))
		{
			return iDict.TryGetValue(search,out value);
		}

		return false;
	}

	public Dictionary<string, KeyValuePair<string, string>> FindInAllDicts(string search)
	{	 		
		var results = new Dictionary<string, KeyValuePair<string, string>>(StringComparer.OrdinalIgnoreCase);

		foreach (var kvp in _dictData)
		{
			var dictName = kvp.Key;
			var iDict = kvp.Value;

			foreach (var inner in iDict)
			{
				if (string.Equals(inner.Key, search, StringComparison.OrdinalIgnoreCase))
				{
					results[dictName] = new KeyValuePair<string, string>(inner.Key, inner.Value);
					break;
				}
			}
		}

		return results;
	}

	public bool GetEntry()
	{		 
		string extMethod = "entryGet";

		try
		{
			Remove7TVWhiteSpace();
			CPH.TryGetArg("rawInput", out string raw);
			
			var entryArgs = InitArgsDict(extMethod, (int)MethodResult.InvalidInput);
			entryArgs["entryFound"] = false;
			entryArgs["entryFoundCount"] = 0;
			
			raw = raw.Trim();
			GetDictName(raw, true, out string searchDict, out raw);

			if (string.IsNullOrWhiteSpace(raw))
			{
				ExtensionLog($"{extMethod}: Raw input is null or empty", 2);
				entryArgs[methodResult] = (int)MethodResult.InvalidInput;
				PopulateArgs(entryArgs);
				return true;
			}

			entryArgs[methodResult] = (int)MethodResult.DictNotFound;
			entryArgs["searchTerm"] = raw;
			if (string.IsNullOrEmpty(searchDict))
			{
				Dictionary<string, KeyValuePair<string,string>> results = FindInAllDicts(raw);
				bool result = results.Count > 0;
				entryArgs["entryFound"] = result;
				
				int i = 0;
				foreach (var kvp in results)
				{
					entryArgs["dictName" + i] = kvp.Key;
					entryArgs["entryKey" + i] = kvp.Value.Key;
					entryArgs["entryValue" + i] = kvp.Value.Value;
					i++;
				}
				
				if (result)
				{
					entryArgs["entryFoundCount"] = i;
					entryArgs[methodResult] = (int)MethodResult.Success;
					ExtensionLog($"{extMethod}: Entry was found for {raw}", 2);
				}
			}
			else
			{
				if (GetDictionaryInfo(searchDict, out Dictionary<string, object> existDict, true))
				{
					string dictName = existDict["dictName"].ToString();
					var dictEntries = _dictData[dictName];
					AddOverwriteKeyValues(entryArgs, existDict, "", "0");
					bool entryFoundResult = GetInnerDictInfo(raw, dictEntries, out Dictionary<string, object> currentEntry);
					entryArgs[methodResult] = entryFoundResult ? (int)MethodResult.Success : (int)MethodResult.EntryNotFound;
					if(entryFoundResult)
					{
						entryArgs["entryFoundCount"] = 1;
						ExtensionLog($"{extMethod}: Entry was found for {raw}", 2);
					}	
					AddOverwriteKeyValues(entryArgs, currentEntry, suffix:"0");
				}
				else
				{
					AddOverwriteKeyValues(entryArgs, existDict, suffix:"0");
				}
			}
			
			PopulateArgs(entryArgs);
			return true;
		}
		catch (Exception ex)
		{
			ExtensionLog($"{extMethod} error: {ex.Message}", -2);
			return false;
		}
	}

	public bool AddEntry()
	{		 		
		string extMethod = "entryAdd";

		try
		{
			Remove7TVWhiteSpace();
			CPH.TryGetArg("rawInput", out string raw);
			
			var entryArgs = InitArgsDict(extMethod, (int)MethodResult.InvalidInput);
			
			if (string.IsNullOrWhiteSpace(raw))
			{
				ExtensionLog($"{extMethod}: Raw input is null or empty", 2);
				PopulateArgs(entryArgs);
				return true;
			}
			
			raw = raw.Trim();
			GetDictName(raw, false, out string searchDict, out raw);
			int sepIndex = raw.IndexOf(KeyValueSeparator);
			
			if (sepIndex >= 1)
			{
				string keyName = raw.Substring(0, sepIndex).Trim();
				string keyValue = raw.Substring(sepIndex + 1).Trim();
				
				if (string.IsNullOrWhiteSpace(keyName))
				{
					PopulateArgs(entryArgs);
					return true;
				}
				
				entryArgs["keyName"] = keyName;
				entryArgs["keyValue"] = keyValue;
				
				lock (_dictLock)
				{
					if (GetDictionaryInfo(searchDict, out Dictionary<string,object> existDict, true))
					{
						string dictName = existDict["dictName"].ToString();
						var dictEntries = _dictData[dictName]; 
						AddOverwriteKeyValues(entryArgs, existDict);
						
						if (!GetInnerDictInfo(keyName, dictEntries, out Dictionary<string,object> currentEntry))
						{
							_dictData[dictName][keyName] = keyValue;
							
							entryArgs["entryKey"] = keyName;
							entryArgs["entryValue"] = keyValue;
							
							entryArgs[methodResult] = (int)MethodResult.Success;
							SaveDictionaries();
							ExtensionLog($"{extMethod}: Entry was added for {raw} with value \"{keyValue}\" in dictionary {dictName}", 2);
							CPH.TriggerCodeEvent(triggerPrefix+"_EntryAdded", entryArgs);
						}
						else
						{
							AddOverwriteKeyValues(entryArgs, currentEntry);
							entryArgs[methodResult] = (int)MethodResult.EntryExists;
						}
					}
					else
					{
						AddOverwriteKeyValues(entryArgs, existDict);
						entryArgs[methodResult] = (int)MethodResult.DictNotFound;
					}
				}
			}
			
			PopulateArgs(entryArgs);
			return true;
		}
		catch (Exception ex)
		{
			ExtensionLog($"{extMethod} error: {ex.Message}", -2);
			return false;
		}
	}

	public bool UpdateEntry()
	{		 		
		string extMethod = "entryUpdate";

		try
		{
			Remove7TVWhiteSpace();
			CPH.TryGetArg("rawInput", out string raw);
			
			var entryArgs = InitArgsDict(extMethod, (int)MethodResult.InvalidInput);
			
			if (string.IsNullOrWhiteSpace(raw))
			{
				ExtensionLog($"{extMethod}: Raw input is null or empty", -1);
				PopulateArgs(entryArgs);
				return true;
			}
			
			raw = raw.Trim();
			GetDictName(raw, false, out string searchDict, out raw);
			int sepIndex = raw.IndexOf(KeyValueSeparator);

			if (sepIndex >= 1)
			{
				string keyName = raw.Substring(0, sepIndex).Trim();
				string keyValue = raw.Substring(sepIndex + 1).Trim();
				
				if (string.IsNullOrWhiteSpace(keyName))
				{
					PopulateArgs(entryArgs);
					return true;
				}
				
				entryArgs["keyName"] = keyName;
				entryArgs["keyValue"] = keyValue;
				
				lock (_dictLock)
				{
					if (GetDictionaryInfo(searchDict, out Dictionary<string, object> existDict, true))
					{
						string dictName = existDict["dictName"].ToString();
						var dictEntries = _dictData[dictName];
						AddOverwriteKeyValues(entryArgs, existDict);
						entryArgs["entryFound"] = false;
						entryArgs["searchTerm"] = raw;

						if (GetInnerDictInfo(keyName, dictEntries, out Dictionary<string, object> currentEntry))
						{
							string foundKey = currentEntry["entryKey"].ToString();
							entryArgs["entryFound"] = true;
							entryArgs["entryKey"] = foundKey;
							entryArgs["oldEntryValue"] = currentEntry["entryValue"].ToString();
							_dictData[dictName][foundKey] = keyValue;
							
							entryArgs["entryValue"] = keyValue;
							
							entryArgs[methodResult] = (int)MethodResult.Success;
							SaveDictionaries();
							ExtensionLog($"{extMethod}: Entry was updated for {keyName} with value from \"{entryArgs["oldEntryValue"]}\" to \"{keyValue}\" in dictionary {dictName}", 2);
							CPH.TriggerCodeEvent(triggerPrefix+"_EntryUpdated", entryArgs);
						}
						else
						{
							AddOverwriteKeyValues(entryArgs, currentEntry);
							entryArgs[methodResult] = (int)MethodResult.EntryNotFound;
						}
					}
					else
					{
						AddOverwriteKeyValues(entryArgs, existDict);
						entryArgs[methodResult] = (int)MethodResult.DictNotFound;
					}
				}
			}
			
			PopulateArgs(entryArgs);
			return true;
		}
		catch (Exception ex)
		{
			ExtensionLog($"{extMethod} error: {ex.Message}", -2);
			return false;
		}
	}

	public bool RemoveEntry()
	{	 		 
		string extMethod = "entryRemove";

		try
		{
			Remove7TVWhiteSpace();
			CPH.TryGetArg("rawInput", out string raw);
			
			var entryArgs = InitArgsDict(extMethod, (int)MethodResult.InvalidInput);
			
			if (string.IsNullOrWhiteSpace(raw))
			{
				ExtensionLog($"{extMethod}: Raw input is null or empty", 2);
				PopulateArgs(entryArgs);
				return true;
			}
			
			raw = raw.Trim();
			GetDictName(raw, true, out string searchDict, out raw);
				
			lock (_dictLock)
			{
				if (GetDictionaryInfo(searchDict, out Dictionary<string, object> existDict, true))
				{
					string dictName = existDict["dictName"].ToString();
					var dictEntries = _dictData[dictName];
					entryArgs["searchTerm"] = raw;
					AddOverwriteKeyValues(entryArgs, existDict);
					
					if (GetInnerDictInfo(raw, dictEntries, out Dictionary<string, object> currentEntry))
					{
						AddOverwriteKeyValues(entryArgs, currentEntry);
					
						ExtensionLog($"Removing Entry from \"{dictName}\". Key: {currentEntry["entryKey"]} , Value: {currentEntry["entryValue"]}", 2);
						_dictData[dictName].Remove(currentEntry["entryKey"].ToString());
						
						entryArgs[methodResult] = (int)MethodResult.Success;
						SaveDictionaries();
						CPH.TriggerCodeEvent(triggerPrefix+"_EntryRemoved", entryArgs);
					}
					else
					{
						AddOverwriteKeyValues(entryArgs, currentEntry);
						entryArgs[methodResult] = (int)MethodResult.EntryNotFound;
					}
				}
				else
				{
					AddOverwriteKeyValues(entryArgs, existDict);
					entryArgs[methodResult] = (int)MethodResult.DictNotFound;
				}
			}
			
			PopulateArgs(entryArgs);
			return true;
		}
		catch (Exception ex)
		{
			ExtensionLog($"{extMethod} error: {ex.Message}", -2);
			return false;
		}
	}

	public bool GetDictionary()
	{	 		
		string extMethod = "dictGet";

		try
		{
			Remove7TVWhiteSpace();
			CPH.TryGetArg("rawInput", out string raw);

			var dictArgs = InitArgsDict(extMethod, (int)MethodResult.InvalidInput);
			
			if (string.IsNullOrWhiteSpace(raw))
			{
				ExtensionLog($"{extMethod}: Raw input is null or empty", 2);
				PopulateArgs(dictArgs);
				return true;
			}
			
			raw = raw.Trim();

			if (GetDictionaryInfo(raw, out Dictionary<string, object> dictInfo))
			{
				string dictName = dictInfo["dictName"].ToString();
				Dictionary<string, string> entries = dictInfo["dictEntries"] as Dictionary<string, string>;
				dictArgs[methodResult] = (int)MethodResult.Success;
				AddOverwriteKeyValues(dictArgs, dictInfo);
				
				ExtensionLog($"{extMethod}: Dictionary \"{dictName}\" was successfully found.", 2);
			}
			else
			{
				AddOverwriteKeyValues(dictArgs, dictInfo);
				dictArgs[methodResult] = (int)MethodResult.DictNotFound;
			}

			PopulateArgs(dictArgs);
			return true;
		}
		catch (Exception ex)
		{
			ExtensionLog($"{extMethod} error: {ex.Message}", -2);
			return false;
		}
	}


	public bool AddDictionary()
	{		 
		string extMethod = "dictAdd";

		try
		{
			Remove7TVWhiteSpace();
			CPH.TryGetArg("rawInput", out string raw);
			
			var dictArgs = InitArgsDict(extMethod, (int)MethodResult.InvalidInput);
			
			if (string.IsNullOrWhiteSpace(raw))
			{
				ExtensionLog($"{extMethod}: Raw input is null or empty", 2);
				PopulateArgs(dictArgs);
				return true;
			}
			
			raw = raw.Trim();
			dictArgs[methodResult] = (int)MethodResult.DictNotFound;

			if (GetDictionaryInfo(raw, out Dictionary<string, object> dictInfo))
			{
				AddOverwriteKeyValues(dictArgs, dictInfo);
				dictArgs[methodResult] = (int)MethodResult.DictExists;
			}
			else
			{
				_dictData[raw] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

				dictArgs[methodResult] = (int)MethodResult.Success;
				AddOverwriteKeyValues(dictArgs, dictInfo);
				dictArgs["dictEntries"] = _dictData[raw];
				dictArgs["dictEntryCount"] = _dictData[raw].Count;
				SaveDictionaries();
				ExtensionLog($"{extMethod}: Dictionary \"{raw}\" was successfully added.", 2);
				CPH.TriggerCodeEvent(triggerPrefix+"_DictAdded", dictArgs);
			}
			
			PopulateArgs(dictArgs);
			return true;
		}
		catch (Exception ex)
		{
			ExtensionLog($"{extMethod} error: {ex.Message}", -2);
			return false;
		}
	}

	public bool RenameDictionary()
	{		 		
		string extMethod = "dictRename";

		try
		{
			Remove7TVWhiteSpace();
			CPH.TryGetArg("rawInput", out string raw);

			var dictArgs = InitArgsDict(extMethod, (int)MethodResult.InvalidInput);
			
			if (string.IsNullOrWhiteSpace(raw))
			{
				ExtensionLog($"{extMethod}: Raw input is null or empty", 2);
				PopulateArgs(dictArgs);
				return true;
			}
			
			raw = raw.Trim();
			int sepIndex = raw.IndexOf(KeyValueSeparator);
			
			if (sepIndex >= 1)
			{
				string oldDictName = raw.Substring(0, sepIndex).Trim();
				string newDictName = raw.Substring(sepIndex + 1).Trim();
				
				if (string.IsNullOrWhiteSpace(oldDictName) || string.IsNullOrWhiteSpace(newDictName))
				{
					PopulateArgs(dictArgs);
					return true;
				}
				
				dictArgs["oldDictName"] = oldDictName;
				dictArgs["newDictName"] = newDictName;
				
				lock (_dictLock)
				{
					if (GetDictionaryInfo(newDictName, out Dictionary<string, object> newDictInfo))
					{
						dictArgs["dictName"] = newDictName;
						AddOverwriteKeyValues(dictArgs, newDictInfo);
						dictArgs[methodResult] = (int)MethodResult.DictExists;
					}
					else if (GetDictionaryInfo(oldDictName, out Dictionary<string, object> oldDictInfo))
					{
						Dictionary<string, string> tempDict = oldDictInfo["dictEntries"] as Dictionary<string, string>;

						_dictData.Remove(oldDictName);
						_dictData[newDictName] = tempDict;

						dictArgs["dictName"] = newDictName;
						dictArgs["oldDictName"] = oldDictName;
						dictArgs["newDictName"] = newDictName;
						dictArgs["dictEntries"] = tempDict;
						dictArgs["dictEntryCount"] = tempDict.Count;
						dictArgs[methodResult] = (int)MethodResult.Success;

						SaveDictionaries();
						ExtensionLog($"{extMethod}: Dictionary \"{oldDictName}\" was successfully renamed to \"{newDictName}\".", 2);
						CPH.TriggerCodeEvent(triggerPrefix+"_DictRenamed", dictArgs);
					}
					else
					{
						dictArgs["dictName"] = oldDictName;
						dictArgs["oldDictName"] = oldDictName;
						dictArgs["newDictName"] = newDictName;
						dictArgs[methodResult] = (int)MethodResult.DictNotFound;
					}
				}
			}
			
			PopulateArgs(dictArgs);
			return true;
		}
		catch (Exception ex)
		{
			ExtensionLog($"{extMethod} error: {ex.Message}", -2);
			return false;
		}
	}

	public bool RemoveDictionary()
	{		 		
		string extMethod = "dictRemove";

		try
		{
			Remove7TVWhiteSpace();
			CPH.TryGetArg("rawInput", out string raw);
			raw = raw.Trim();
			var dictArgs = InitArgsDict(extMethod, (int)MethodResult.InvalidInput);
			
			if (string.IsNullOrWhiteSpace(raw))
			{
				ExtensionLog($"{extMethod}: Raw input is null or empty", 2);
				PopulateArgs(dictArgs);
				return true;
			}

			lock (_dictLock)
			{
				if (GetDictionaryInfo(raw, out Dictionary<string, object> dictInfo))
				{
					string dictName = dictInfo["dictName"].ToString();
					string jsonSaveDict = JsonConvert.SerializeObject(_dictData[dictName]);
					ExtensionLog($"Removing Dictionary \"{dictName}\". JSON Data: \"{dictName}\":{jsonSaveDict}", 1);
					
					dictArgs[methodResult] = (int)MethodResult.Success;
					AddOverwriteKeyValues(dictArgs, dictInfo);

					_dictData.Remove(dictName);
					SaveDictionaries();
					CPH.TriggerCodeEvent(triggerPrefix+"_DictRemoved", dictArgs);
				}
				else
				{
					AddOverwriteKeyValues(dictArgs, dictInfo);
					dictArgs[methodResult] = (int)MethodResult.DictNotFound;
				}
			}

			PopulateArgs(dictArgs);
			return true;
		}
		catch (Exception ex)
		{
			ExtensionLog($"{extMethod} error: {ex.Message}", -2);
			return false;
		}
	}

	public void AddOverwriteKeyValues(Dictionary<string,object> toDict, Dictionary<string,object> fromDict, string prefix = "", string suffix ="")
	{	 		 
		List<string> suffixExclude = ["entryFound","entryFoundCount","dictFound"];
		List<string> preFixExclude = ["extMethod"];

		foreach (var kvp in fromDict)
		{
			var actualSuffix = suffixExclude.Contains(kvp.Key) ? "" : suffix;
			var actualPreFix = kvp.Key == "extMethod" ? "" : prefix;
    		toDict[PrefixKey(kvp.Key,actualPreFix) + actualSuffix] = kvp.Value;
		}
	}

	public Dictionary<string,object> InitArgsDict(string key, object value, bool methodDict = true)
	{	 		
		var argDict = new Dictionary<string,object>();
		if(methodDict)
		{
			argDict["extMethod"] = key;
			argDict[methodResult] = value;
		}else{
			argDict[key] = value;
		}

		return argDict;
	}
	
	public Dictionary<string, Dictionary<string, string>> NormalizeDict(Dictionary<string, Dictionary<string, string>> sourceDict)
	{		 
		var result = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
		lock (_dictLock)
		{
			foreach (var outerKvp in sourceDict)
			{
				result[outerKvp.Key] = new Dictionary<string, string>(outerKvp.Value, StringComparer.OrdinalIgnoreCase);
			}
		}
		return result;
	}

	public string PrefixKey(string input,string prefix)
	{		 		
		if(!string.IsNullOrEmpty(prefix))
		{
			return prefix + char.ToUpper(input[0]) + input.Substring(1);
		}

		return input;
	}

	public void SaveDictionaries()
	{		 		
		CPH.SetGlobalVar(globalVarName,_dictData,true);
	}

	public bool SendMessage()
	{	 		 
		try
		{
			CPH.TryGetArg("extMethodResult",out int extMethodResult);
			string messageType = Enum.GetName(typeof(MethodResult), extMethodResult);
			messageType = char.ToLowerInvariant(messageType[0]) + messageType.Substring(1) + "Message";
			CPH.TryGetArg(messageType, out string message);
			CPH.SetArgument("extMethodMessage",message);
			CPH.TryGetArg("userType", out string platform);

			if (string.IsNullOrWhiteSpace(platform))
			{
				ExtensionLog("SendMessage: Platform not specified", -1);
				return true;
			}

			platform = platform.ToLower();
			if (string.IsNullOrEmpty(message)) 
				message = "No message defined.";

			string[] splitMessage = message.Split(' ');
			int maxChars = PlatformMessageLimits.TryGetValue(platform, out int limit) ? limit : 200;

			string output = "";
			foreach(string word in splitMessage)
			{	 		
				if ((output + " " + word).Length > maxChars)
				{
					SendPlatformMessage(platform,output);
					output = word;
					CPH.Wait(CommandDelayMs);
				}else{
					output += (String.IsNullOrEmpty(output)?"":" ") + word;
				}
				
			}
			if(!String.IsNullOrEmpty(output))
			{		 
				SendPlatformMessage(platform,output.Trim());
			}
			return true;
		}
		catch (Exception ex)
		{
			ExtensionLog($"SendMessage error: {ex.Message}", -2);
			return false;
		}
	}

    public void SendPlatformMessage(string platform,string messageOutput)
    {	 		
        switch(platform)
        {
            case "twitch":
                CPH.SendMessage(messageOutput);
                break;
            case "youtube":
				YouTubeUserInfo botInfo = CPH.YouTubeGetBot();
				bool botSend = botInfo != null;
				CPH.TryGetArg("broadcast.id",out string broadcastId);
				if(broadcastId != null)
				{
					CPH.SendYouTubeMessage(messageOutput,botSend, broadcastId: broadcastId);
				}else{
					CPH.SendYouTubeMessage(messageOutput,botSend);
				}
                break;
            case "trovo":
				CPH.SendTrovoMessage(messageOutput);
                break;
			case "kick":
				CPH.SendKickMessage(messageOutput);
                break;
        }
    }

	public void PopulateArgs(Dictionary<string,object> args)
	{		 
		foreach( var kvp in args)
		{
			CPH.SetArgument(kvp.Key,kvp.Value);
		}
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