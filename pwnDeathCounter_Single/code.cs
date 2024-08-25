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
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Globalization;
using Newtonsoft.Json;
using Streamer.bot.Common.Events;

public class CPHInline
{
	/*  DISCLAIMER: the string globalVariableName is the only thing I allow you to change, if you change anything else
		I will not take responsibility on the code not working properly.
		This is if the need would arise to have two deathcounter going on so you would could import another
		deathcounter and make it **not** overwrite the current actions then change the value of the string to for example
		"pwn_PerGameDeathCounter_Single_Steve" so there would now be a second death counter running. 

		HOWEVER remember to also change your commands accordingly if you want specific ones for "Steve"
		TOUCHY ZONE START
	*/	
	static readonly string globalVariableName = "pwn_PerGameDeathCounter_Single";
	/*
		TOUCHY ZONE END HERE
	*/
	static readonly string  versionNum = "1.6.0";
	private static readonly HttpClient _client = new HttpClient{Timeout = TimeSpan.FromSeconds(30)};
	private static readonly object _lock = new object();
	static DeathCounter _dcInfo;
	static Dictionary<string,GameDeathInfo> _dcDict;
	static GameDeathInfo _currentDeathGame;
	static string _pdcsMessageType = "";
	static int _pdcsMessageTypeResult = 0;
	static string eventName = "pwnDeathCounter_PG_Single_Change";

	static string _nonValidGame = "";
	static string _blankGameBoxArt = "";
	static string _artDimension = "";
	static string _inputVariable = "";

	static Dictionary<string,string> _currentGame = new Dictionary<string,string>()
	{
		{"gameId",""},
		{"gameName",""},
		{"gameBoxArt",""},
		{"gameIgdbId",""}
	};

	public void Init()
	{	 		 
		CPH.LogInfo($"[pwn DeathCounter Single][{versionNum}]- This deathcounter extension was developed by pwnyy. Contact: contact@pwnyy.tv , Socials: https://twitch.tv/pwnyytv , https://x.com/pwnyy, https://ko-fi.com/pwnyy, https://pwnyy.tv");
		string[] contextMenu = {"[pwn] Extensions","DeathCounter Single"};
		string triggerName = "DeathCounter Update";
		if(!CPH.RegisterCustomTrigger(triggerName, eventName, contextMenu))
		{	 		
			int attempts = 5;
			bool success = false;
			for(int i= 0;i<attempts;i++)
			{		 
				triggerName+= " ["+(i+1)+"]";
				if(CPH.RegisterCustomTrigger(triggerName, eventName, contextMenu))
				{
					success = true;
					break;
				}
			}
			if(!success)
			{		 		
				CPH.LogError($"[pwn DeathCounter Single][{versionNum}] - Was not able to register custom trigger {triggerName} with {attempts} attempts.");
			}
		}
		string json = CPH.GetGlobalVar<string?>(globalVariableName, true);
		if (!string.IsNullOrEmpty(json) && json != "null")
		{		 		
			_dcInfo = JsonConvert.DeserializeObject<DeathCounter>(json);
		}
		else
		{
		    _dcInfo = new DeathCounter();
		}
		_dcDict = _dcInfo.Counters;
		_nonValidGame = _dcInfo.NonValidGame;
		_blankGameBoxArt = _dcInfo.BlankGameBoxArt;
		_artDimension = _dcInfo.ArtDimension;
		_inputVariable = _dcInfo.InputVariable;
		if(!_dcInfo.InitFetchOldCounters)UpdateOldVersion();
		GetCurrentGame();
	}
	
	public void GetCurrentGame()
	{	 		 
		TwitchUserInfoEx userInfo = CPH.TwitchGetExtendedUserInfoById(CPH.TwitchGetBroadcaster().UserId);
		_currentGame["gameId"] = String.IsNullOrEmpty(userInfo.GameId) ? "0" : userInfo.GameId;
		_currentGame["gameName"] = String.IsNullOrEmpty(userInfo.Game) ? _nonValidGame : userInfo.Game;
		
		string gameName = userInfo.Game;
		string gameId = userInfo.GameId;
		if(_dcDict.ContainsKey(userInfo.GameId) && !String.IsNullOrEmpty(_dcDict[userInfo.GameId].GameBoxArt))
		{	 		
			_currentGame["gameBoxArt"] = _dcDict[userInfo.GameId].GameBoxArt;
			
		}else if(String.IsNullOrEmpty(_currentGame["gameBoxArt"]))
		{		 
			CPH.LogDebug($"[pwn DeathCounter Single][{versionNum}] - Fetching image of current game {gameName} ({gameId}).");
			List<string> gameIds = [gameId];
			int timeoutMilliseconds = 5000;
			Task<List<Game>> getGameInfo = GetGameInfos(gameIds, true);
			if(getGameInfo.Wait(timeoutMilliseconds))
			{		 		
				_currentGame["gameBoxArt"] = getGameInfo.Result[0].box_art_url;
				_currentGame["gameIgdbId"] = getGameInfo.Result[0].igdb_id;
			}else{		 		
				CPH.LogDebug($"[pwn DeathCounter Single][{versionNum}] - Fetching image of current game {gameName} ({gameId}) took more than {timeoutMilliseconds}ms, will set to blank.");
				_currentGame["gameBoxArt"] = "";
				_currentGame["gameIgdbId"] = "";
 			}
		}
	}
	
	public void Dispose()
	{	 		 
		string json = JsonConvert.SerializeObject(_dcInfo,Formatting.None);
		CPH.LogDebug($"[pwn DeathCounter Single][{versionNum}] Log on Dispose: "+json);
		_client?.Dispose();
	}	 		
	
	public bool UpdateCurrentGame()
	{	 		 
		CheckNullGameCounters();
		if(CPH.TryGetArg("gameId",out string gameId))
		{	 		
			CPH.TryGetArg("gameName",out string gameName);
			gameName = gameId == "0" ? _nonValidGame : gameName;
			CPH.TryGetArg("gameBoxArt",out string gameBoxArt);
			CPH.TryGetArg("gameIgdbId",out string gameIgdbId);
			_currentGame["gameId"] = gameId;
			_currentGame["gameName"] = gameName;
			_currentGame["gameBoxArt"] = String.IsNullOrEmpty(gameBoxArt)? "":gameBoxArt.Replace("300x300","{width}x{height}");
			_currentGame["gameIgdbId"] = gameIgdbId;
			if( _dcDict.ContainsKey(gameId) &&  (_dcDict[gameId].GameBoxArt == "" || _dcDict[gameId].IgdbId == "" ))
			{		 
				if(_dcDict[gameId].GameBoxArt == "" && _dcDict[gameId].GameBoxArt != gameBoxArt) _dcDict[gameId].GameBoxArt = gameBoxArt;
				if(_dcDict[gameId].IgdbId == "" && _dcDict[gameId].IgdbId != gameIgdbId ) _dcDict[gameId].IgdbId = gameIgdbId;
			}

		}else{		 		
			GetCurrentGame();
		}		 		
		
		AddCurrentGameArgs();
		return true;
	}
	
	public bool IncreaseDeathCount()
	{	 		 
		CheckNullGameCounters();
		Remove7TVWhiteSpace();
		string gameId = _currentGame["gameId"];
		int messageType = 0;
		CPH.TryGetArg(_inputVariable,out string rawInput);
		CPH.TryGetArg("useInput",out bool useInput);
		int inputNum = 1;

		if(_dcDict.ContainsKey(gameId))
		{	 		
			if(useInput && !string.IsNullOrEmpty(rawInput) && int.TryParse(rawInput,out inputNum) && inputNum <= int.MaxValue)
			{		 
				messageType = 1;
				inputNum = Math.Abs(inputNum);
				_dcDict[gameId].Count += inputNum;
				_dcInfo.TotalDeaths += inputNum;
			}else if(!useInput || string.IsNullOrEmpty(rawInput)){
				messageType = 1;
				_dcDict[gameId].Count++;
				_dcInfo.TotalDeaths++;
			}else{		 		
				//Value not correct
				messageType = -1;
			}
		}
		
		AddCurrentGameArgs();
		if(messageType > 0)
		{		 		
			SaveGameCounters();
			TriggerOnChange();
		}
		CPH.SetArgument("pdcsMessageType","increaseDeathCount");
		CPH.SetArgument("pdcsMessageTypeResult",messageType);
		_pdcsMessageType = "increaseDeathCount";
		_pdcsMessageTypeResult = messageType;
		SendDeathCounterMessage();
		return true;
	}

	public bool DecreaseDeathCount()
	{	 		 
		CheckNullGameCounters();
		Remove7TVWhiteSpace();
		string gameId = _currentGame["gameId"];
		
		int messageType = 0;
		CPH.TryGetArg(_inputVariable,out string rawInput);
		CPH.TryGetArg("useInput",out bool useInput);
		int inputNum = 0;
		if(_dcDict.ContainsKey(gameId))
		{	 		
			long oldGameCount = _dcDict[gameId].Count;
			if(useInput && !string.IsNullOrEmpty(rawInput) && int.TryParse(rawInput,out inputNum)  && inputNum <= int.MaxValue)
			{		 
				messageType = 1;
				inputNum = Math.Abs(inputNum);
				_dcDict[gameId].Count = (_dcDict[gameId].Count - inputNum) >= 0 ? _dcDict[gameId].Count - inputNum : 0;
			}else if(!useInput || string.IsNullOrEmpty(rawInput)){
				messageType = 1;
				if(_dcDict[gameId].Count > 0)_dcDict[gameId].Count--;
				if(_dcInfo.TotalDeaths > 0)_dcInfo.TotalDeaths--;
			}else{		 		
				//Value not correct
				messageType = -1;
			}
			if(oldGameCount != _dcDict[gameId].Count)
			{		 		
				_dcInfo.TotalDeaths = _dcDict.Values.Sum(gameInfo => gameInfo.Count);
			}
		}
		AddCurrentGameArgs();
		if(messageType > 0)
		{
			SaveGameCounters();
			TriggerOnChange();
		}
		CPH.SetArgument("pdcsMessageType","decreaseDeathCount");
		CPH.SetArgument("pdcsMessageTypeResult",messageType);
		_pdcsMessageType = "decreaseDeathCount";
		_pdcsMessageTypeResult = messageType;
		SendDeathCounterMessage();
		return true;
	}
	
	public bool SetDeathCount()
	{	 		 
		CheckNullGameCounters();
		Remove7TVWhiteSpace();
		string gameId = _currentGame["gameId"];
		
		int messageType = 0;
		CPH.TryGetArg(_inputVariable,out string rawInput);
		int inputNum = 0;
		if(_dcDict.ContainsKey(gameId))
		{	 		
			if(!string.IsNullOrEmpty(rawInput) && int.TryParse(rawInput,out inputNum) && inputNum >= 0)
			{		 
				messageType = 1;
				_dcInfo.TotalDeaths -= _dcDict[gameId].Count;
				_dcDict[gameId].Count = inputNum;
				_dcInfo.TotalDeaths += inputNum;
			}else{		 		
				messageType = -1;
			}
		}
		AddCurrentGameArgs();
		if(messageType > 0)
		{		 		
			SaveGameCounters();
			TriggerOnChange();
		}
		CPH.SetArgument("pdcsMessageType","setDeathCount");
		CPH.SetArgument("pdcsMessageTypeResult",messageType);
		_pdcsMessageType = "setDeathCount";
		_pdcsMessageTypeResult = messageType;
		SendDeathCounterMessage();
		return true;
	}

	public bool GetGameCounter()
	{	 		 
		CheckNullGameCounters();
		int messageType = 0;
		string gameId = _currentGame["gameId"];
		if(_dcDict.ContainsKey(gameId))
		{	 		
			messageType = 1;
		}		 
		AddCurrentGameArgs();
		CPH.SetArgument("pdcsMessageType","getGameCounter");
		CPH.SetArgument("pdcsMessageTypeResult",messageType);
		_pdcsMessageType = "getGameCounter";
		_pdcsMessageTypeResult = messageType;
		SendDeathCounterMessage();
		return true;
	}

	public bool GetTotalDeaths()
	{	 		 
		CheckNullGameCounters();
		int messageType = 0;
		string gameId = _currentGame["gameId"];
		if(_dcDict.ContainsKey(gameId))
		{	 		
			messageType = 1;
		}		 
		AddCurrentGameArgs();
		CPH.SetArgument("pdcsMessageType","getTotalDeaths");
		CPH.SetArgument("pdcsMessageTypeResult",messageType);
		_pdcsMessageType = "getTotalDeaths";
		_pdcsMessageTypeResult = messageType;
		SendDeathCounterMessage();
		return true;
	}
	
	public bool AddGameToCounter()
	{	 		 
		CheckNullGameCounters();
		int messageType = 0;
		if(!_dcDict.ContainsKey(_currentGame["gameId"]))
		{	 		
			messageType = 1;
			GameDeathInfo info = new GameDeathInfo()
			{		 
				GameName = _currentGame["gameName"],
				GameBoxArt = _currentGame["gameBoxArt"].Replace("300x300",_artDimension),
				IgdbId = _currentGame["gameIgdbId"]
			};
			_dcDict.Add(_currentGame["gameId"],info);
			SaveGameCounters();
			TriggerOnChange();
		}
		AddCurrentGameArgs();
		CPH.SetArgument("pdcsMessageType","addDeathCounter");
		CPH.SetArgument("pdcsMessageTypeResult",messageType);
		_pdcsMessageType = "addDeathCounter";
		_pdcsMessageTypeResult = messageType;
		SendDeathCounterMessage();
		return true;
	}
	
	public bool RemoveGameFromCounter()
	{	 		 
		CheckNullGameCounters();
		int messageType = 0;
		AddCurrentGameArgs();
		if(_dcDict.ContainsKey(_currentGame["gameId"]))
		{	 		
			messageType = 1;
			CPH.LogDebug($"[pwn DeathCounter Single] - "+JsonConvert.SerializeObject(_dcInfo,Formatting.None));
			_dcInfo.TotalDeaths -= _dcDict[_currentGame["gameId"]].Count;
			_dcDict.Remove(_currentGame["gameId"]);
			SaveGameCounters();
			TriggerOnChange();
		}		 
		CPH.SetArgument("pdcsMessageType","removeDeathCounter");
		CPH.SetArgument("pdcsMessageTypeResult",messageType);
		_pdcsMessageType = "removeDeathCounter";
		_pdcsMessageTypeResult = messageType;
		SendDeathCounterMessage();
		return true;
	}
	
	public bool GetLeaderboard()
	{		 		 
		Remove7TVWhiteSpace();
		int messageType = 0;
		CPH.TryGetArg(_inputVariable,out string rawInput);
		CPH.TryGetArg("ignoreInput",out bool ignoreInput);
		long minBoard = CPH.TryGetArg("minBoardSize",out minBoard) ? minBoard >= 1 ? minBoard : 1 : 1;
		long maxBoard = CPH.TryGetArg("minBoardSize",out maxBoard) ? maxBoard >= 1 ? maxBoard : 1 : 5;
		long boardSize = CPH.TryGetArg("defaultBoardSize",out boardSize) ? boardSize >=1 ? boardSize : 1 : 5;
		CPH.TryGetArg("orderByDescending",out bool orderDes);
		bool hasInput = ignoreInput ? false : !String.IsNullOrEmpty(rawInput);
		
		bool validInput = !hasInput || (hasInput && long.TryParse(rawInput,out boardSize) && boardSize <= int.MaxValue && boardSize >= minBoard && boardSize <= maxBoard);
		if((hasInput && validInput) || !hasInput && _dcDict.Count > 0)
		{	 		
			messageType = 1;
			Dictionary<string,GameDeathInfo> lbRatings = new Dictionary<string,GameDeathInfo>();
			Dictionary<string,Dictionary<string,object>> SingleLb = new Dictionary<string,Dictionary<string,object>>();
				
			lbRatings = orderDes ? _dcDict
				.OrderByDescending(kvp => kvp.Value.Count) 
				.ToDictionary(kvp => kvp.Key, kvp => kvp.Value) :  
				_dcDict.OrderBy(kvp => kvp.Value.Count) 
				.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

			long countUp = 0;
			foreach (var kvp in lbRatings.Take((int)boardSize))
			{		 
				string key = kvp.Key;
				GameDeathInfo gameDeathInfo = kvp.Value;
				long count = gameDeathInfo.Count;
				Dictionary<string, object> entry = new Dictionary<string, object>
				{
					{ "gameName", gameDeathInfo.GameName },
					{ "gameBoxArt", String.IsNullOrEmpty(gameDeathInfo.GameBoxArt)? _blankGameBoxArt : gameDeathInfo.GameBoxArt },
					{ "igdbId",gameDeathInfo.IgdbId},
					{ "deathCount", count }
				};
				CPH.SetArgument($"pdcsGameId[{countUp}]",key);
				CPH.SetArgument($"pdcsGameName[{countUp}]",gameDeathInfo.GameName);
				CPH.SetArgument($"pdcsGameBoxArt[{countUp}]",String.IsNullOrEmpty(gameDeathInfo.GameBoxArt)? _blankGameBoxArt : gameDeathInfo.GameBoxArt.Replace("{width}x{height}",_artDimension));
				CPH.SetArgument($"pdcsIgdbId[{countUp}]",gameDeathInfo.IgdbId);
				CPH.SetArgument($"pdcsDeathCount[{countUp}]",count);
				args[$"pdcsGameId[{countUp}]"] = key;
				args[$"pdcsGameName[{countUp}]"] = gameDeathInfo.GameName;
				args[$"pdcsGameBoxArt[{countUp}]"] = String.IsNullOrEmpty(gameDeathInfo.GameBoxArt)? _blankGameBoxArt : gameDeathInfo.GameBoxArt.Replace("{width}x{height}",_artDimension);
				args[$"pdcsIgdbId[{countUp}]"] = gameDeathInfo.IgdbId;
				args[$"pdcsDeathCount[{countUp}]"] = count;
				SingleLb[key] = entry;
				countUp++;
			}
			
			CPH.TryGetArg("leaderboardFormat",out string lbFormat);
			List<string> lbStrings = new List<string>();
			for(int i = 0;i<SingleLb.Count;i++)
			{		 		
				args["pdcsPosition"] = i+1;
				args["pdcsGame"] = SingleLb.ElementAt(i).Value["gameName"];
				args["pdcsCount"] = SingleLb.ElementAt(i).Value["deathCount"];
				lbStrings.Add(ReplaceWithArgs(lbFormat,args));
			}
			string joinedString = string.Join(", ", lbStrings);
			string joinedObsString = string.Join("\n",lbStrings);
			string joinedFileString = string.Join(Environment.NewLine,lbStrings);
			CPH.SetArgument("pdcsLeaderboardCount",SingleLb.Count);
			CPH.SetArgument("pdcsLeaderboardDict",SingleLb);
			CPH.SetArgument("pdcsLeaderboard",joinedString);
			CPH.SetArgument("pdcsLeaderboardObs",joinedObsString);
			CPH.SetArgument("pdcsLeaderboardFile",joinedFileString);
			args["pdcsLeaderboardCount"] = SingleLb.Count;
			args["pdcsLeaderboardDict"] = SingleLb;
			args["pdcsLeaderboard"] = joinedString;
			args["pdcsLeaderboardObs"] = joinedObsString;
			args["pdcsLeaderboardFile"] = joinedFileString;
		}else if(!validInput)
		{		 		
			messageType = -1;
		}
		AddCurrentGameArgs();
		CPH.SetArgument("pdcsMessageType","getLeaderboard");
		CPH.SetArgument("pdcsMessageTypeResult",messageType);
		args["pdcsTotalDeaths"] = _dcInfo.TotalDeaths;
		_pdcsMessageType = "getLeaderboard";
		_pdcsMessageTypeResult = messageType;
		SendDeathCounterMessage();
		return true;
	}

	public bool SettingConvert()
	{	 		 
		CPH.TryGetArg("convertFromOld",out bool convertFromOld);
		if(convertFromOld)
		{	 		
			UpdateOldVersion();
		}		 
		CPH.TryGetArg("gameBoxDimensions",out string gameBoxDimensions);
		CPH.TryGetArg("blankGameBoxArt",out string blankGameBoxArt);
		CPH.TryGetArg("nonValidGameName", out string nonValidGameName);
		CPH.TryGetArg("usedInputVariable",out string usedInputVariable);
		_dcInfo.NonValidGame = nonValidGameName ?? _dcInfo.NonValidGame;
		_dcInfo.BlankGameBoxArt = blankGameBoxArt ?? _dcInfo.BlankGameBoxArt;
		_dcInfo.ArtDimension = gameBoxDimensions ?? _dcInfo.ArtDimension;
		_dcInfo.InputVariable = usedInputVariable ?? _dcInfo.InputVariable;
		_nonValidGame = _dcInfo.NonValidGame;
		_blankGameBoxArt = _dcInfo.BlankGameBoxArt;
		_artDimension = _dcInfo.ArtDimension;
		_inputVariable = _dcInfo.InputVariable;
		SaveGameCounters();
		return true;
	}

	public void UpdateOldVersion()
	{	 		 
		Dictionary<string,long> oldGameCounters = CPH.GetGlobalVar<Dictionary<string,long>?>("pwnedCounter_Games", true) ?? new Dictionary<string,long>();
		if(oldGameCounters.Count > 0)
		{	 		
			string json = JsonConvert.SerializeObject(oldGameCounters,Formatting.None);
			CPH.LogDebug($"[pwn DeathCounter Single][{versionNum}] - pwnedCounter_Games Json: {json}");
		}
		if(oldGameCounters != null)
		{		 
			List<string> gamesList = new List<string>(oldGameCounters.Keys);
			List<string> formattedGameList = new List<string>();
			int toConvertGamesCount = gamesList.Count;
			foreach(string game in gamesList.Take(100))
			{		 		
				string formatted = WebUtility.UrlEncode(game);
				formattedGameList.Add(formatted);
			}
			Dictionary<string,GameDeathInfo> convertedCounters = new Dictionary<string,GameDeathInfo>();
			
			Task<List<Game>> getGameInfo = GetGameInfos(formattedGameList);
			int timeoutms = 10000;
			if(getGameInfo.Wait(timeoutms))
			{		 		
				List<Game> gottenGames = getGameInfo.Result;
				if(gottenGames.Count > 0)
				{	 		 
					foreach(Game game in gottenGames)
					{	 		
						
						string uniConverted = ConvertUnicodeEscapes(game.name);
						
						if(oldGameCounters.ContainsKey(uniConverted))
						{		 
							GameDeathInfo gameInfo = new GameDeathInfo()
							{
								GameName = uniConverted,
								GameBoxArt = game.box_art_url,
								IgdbId = game.igdb_id,
								Count = oldGameCounters[uniConverted]
							};
							convertedCounters.Add(game.id,gameInfo);
							oldGameCounters.Remove(uniConverted);
						}
					}
					
					CPH.SetGlobalVar("pwnedCounter_Games", oldGameCounters, true);
				}
				if(convertedCounters.Count > 0) 
				{		 		
					int newlyAdded = 0;
					int updatedCounters = 0;
					foreach(KeyValuePair<string,GameDeathInfo> cCounter in convertedCounters)
					{		 		
						if(_dcDict.ContainsKey(cCounter.Key))
						{	 		 
							_dcDict[cCounter.Key].Count += cCounter.Value.Count;
							if(string.IsNullOrEmpty(_dcDict[cCounter.Key].IgdbId))
							{
								_dcDict[cCounter.Key].IgdbId = cCounter.Value.IgdbId;
							}
							if(string.IsNullOrEmpty(_dcDict[cCounter.Key].GameBoxArt))
							{
								_dcDict[cCounter.Key].GameBoxArt = cCounter.Value.GameBoxArt;
							}
							updatedCounters++;
						}else{	 		
							newlyAdded++;
							_dcDict.Add(cCounter.Key,cCounter.Value);
						}
					}
					
					_dcInfo.Counters = _dcDict;
					
					_dcInfo.TotalDeaths = _dcDict.Values.Sum(gameInfo => gameInfo.Count);
					CPH.LogInfo($"[pwn DeathCounter Single][{versionNum}] - Attempted to convert {toConvertGamesCount} games from old pwnedCounter_Games global variable. "+
						$"{convertedCounters.Count}/{toConvertGamesCount} games were successfully converted. {oldGameCounters.Count} were not able to be found with Twitch API. "+
						$"In total there are {oldGameCounters.Count} games that need manually conversion or adjustements of names in the global variable.");
					 CPH.ShowToastNotification("pwnDeathCounterSingle_"+versionNum, "Upgrade DeathCounter", $"Converted {convertedCounters.Count}/{toConvertGamesCount} Games.{oldGameCounters.Count} not converted." ,"", "https://pwnyy.tv/logo-small-round-green.png");
					if(oldGameCounters.Count <= 0) CPH.UnsetGlobalVar("pwnedCounter_Games", true);
					if(!_dcInfo.InitFetchOldCounters)
					{		 
						_dcInfo.InitFetchOldCounters = true;
						SaveGameCounters();
					}
				}else{		 		
					CPH.LogInfo($"[pwn DeathCounter Single][{versionNum}] - Either no games were in the pwnedCounter_Games global variable, or none were successfully converted. If the latter please check/adjust the game names.");
					CPH.ShowToastNotification("pwnDeathCounterSingle_"+versionNum , "Upgrade DeathCounter", "Either no games were in the pwnedCounter_Games global variable, or none were successfully converted.", "", "https://pwnyy.tv/logo-small-round-green.png");
				}
			}else{		 		
				CPH.LogDebug($"[pwn DeathCounter Single][{versionNum}] - Fetching game infos of old deathcounter games took longer than {timeoutms}ms. Better luck next time.");
				CPH.ShowToastNotification("pwnDeathCounterSingle_"+versionNum, "Upgrade DeathCounter", $"Fetching game infos took longer than {timeoutms}ms. Try manual conversion next time.", "", "https://pwnyy.tv/logo-small-round-green.png");
			}
		}
		if(!_dcInfo.InitFetchOldCounters) _dcInfo.InitFetchOldCounters = true;
	}
	
	public void SaveGameCounters()
	{
		if(_dcInfo != null)
		{
			string json = JsonConvert.SerializeObject(_dcInfo);
			CPH.SetGlobalVar(globalVariableName, json, true);
			
		}else{
			CPH.LogDebug($"[pwn DeathCounter Single][{versionNum}] - Check previously executed action of the death counter.");	
		}
	}
	
	public void AddCurrentGameArgs()
	{	 		 
			string game 	= _currentGame["gameName"];
			string gameId 	= _currentGame["gameId"];
			string gameBoxArt	= String.IsNullOrEmpty(_currentGame["gameBoxArt"]) ? _blankGameBoxArt : _currentGame["gameBoxArt"].Replace("{width}x{height}",_artDimension);
			string igdbId = _currentGame["gameIgdbId"];
			long totalDeaths = _dcInfo.TotalDeaths;
			int totalGames = _dcDict.Count;
			bool hasCounter = _dcDict.ContainsKey(gameId);
			
			CPH.SetArgument("pdcsGame",game);
			CPH.SetArgument("pdcsGameId",gameId);
			CPH.SetArgument("pdcsGameBoxArtUrl",gameBoxArt);
			CPH.SetArgument("pdcsIgdbId",igdbId);
			CPH.SetArgument("pdcsTotalDeaths",totalDeaths);
			CPH.SetArgument("pdcsTotalGames",totalGames);
			CPH.SetArgument("pdcsHasCounter",hasCounter);
			
			args["pdcsGame"] = game;
			args["pdcsGameId"] = gameId;
			args["pdcsGameBoxArtUrl"] = gameBoxArt;
			args["pdcsIgdbId"] = igdbId;
			args["pdcsTotalDeaths"] = totalDeaths;
			args["pdcsTotalGames"] = totalGames;
			args["pdcsHasCounter"] = hasCounter;
			
			if(hasCounter)
			{	 		
				long gameCounter = _dcDict[gameId].Count;
				CPH.SetArgument("pdcsCounter",gameCounter);
				args["pdcsCounter"] = gameCounter;
			}		 
	}
	
	public void CheckNullGameCounters()
	{	 		 
		if(_dcInfo == null)
		{	 		
			string json = CPH.GetGlobalVar<string?>(globalVariableName, true) ?? null;
			if(json != null || json != "null")
			{		 
				CPH.LogDebug($"[pwn DeathCounter Single][{versionNum}] - Retrieving global variable and parsing into death counter object.");
				_dcInfo = JsonConvert.DeserializeObject<DeathCounter>(json);
			}
			else
			{
				CPH.LogDebug($"[pwn DeathCounter Single][{versionNum}] - Creating new death counter object as no global variable exists or was null.");
				_dcInfo = new DeathCounter();
			}
			_dcDict = _dcInfo.Counters;
		}
	}
	
	public void TriggerOnChange()
	{	 		 
		string gameId = _currentGame["gameId"];
		Dictionary<string,object> triggerArgs = new Dictionary<string,object>();
		triggerArgs["pdcsGameId"] = gameId;
		triggerArgs["pdcsGame"] = _currentGame["gameName"];
		triggerArgs["pdcsGameBoxArtUrl"] = String.IsNullOrEmpty(_currentGame["gameBoxArt"]) ? _blankGameBoxArt : _currentGame["gameBoxArt"].Replace("{width}x{height}",_artDimension);
		triggerArgs["pdcsIgdbId"] = _currentGame["gameIgdbId"];
		triggerArgs["pdcsTotalDeaths"] = _dcInfo.TotalDeaths;
		triggerArgs["pdcsHasCounter"] = _dcDict.ContainsKey(gameId);

		if(_dcDict.ContainsKey(gameId))
		{	 		
			triggerArgs["pdcsCounter"] = _dcDict[gameId].Count;
		}		 
		
		CPH.TriggerCodeEvent(eventName, triggerArgs);
	}

	public bool SendDeathCounterMessage()
	{	 		 
		CPH.TryGetArg("userType",out string platform);
		platform = String.IsNullOrEmpty(platform) ? platform : platform.ToLower();
		string message = "";
		switch(_pdcsMessageType)
		{	 		
			case "increaseDeathCount":
				switch(_pdcsMessageTypeResult)
				{		 
					case -1:
						CPH.TryGetArg("nonValidInputMsg",out message);
						break;
					case 0:
						CPH.TryGetArg("noDeathCounterMsg",out message);
						break;
					case 1:
						CPH.TryGetArg("increaseCountMsg",out message);
						break;
				}
				break;
			case "decreaseDeathCount":
				switch(_pdcsMessageTypeResult)
				{		 		
					case -1:
						CPH.TryGetArg("nonValidInputMsg",out message);
						break;
					case 0:
						CPH.TryGetArg("noDeathCounterMsg",out message);
						break;
					case 1:
						CPH.TryGetArg("decreaseCountMsg",out message);
						break;
				}
				break;
			case "setDeathCount":
				switch(_pdcsMessageTypeResult)
				{		 		
					case -1:
						CPH.TryGetArg("nonValidInputMsg",out message);
						break;
					case 0:
						CPH.TryGetArg("noDeathCounterMsg",out message);
						break;
					case 1:
						CPH.TryGetArg("setCountMsg",out message);
						break;
				}
				break;
			case "getGameCounter":
				switch(_pdcsMessageTypeResult)
				{	 		 
					case 0:
						CPH.TryGetArg("noDeathCounterMsg",out message);
						break;
					case 1:
						CPH.TryGetArg("deathCountMsg",out message);
						break;
				}
				break;
			case "getTotalDeaths":
				switch(_pdcsMessageTypeResult)
				{	 		
					case 0:
						CPH.TryGetArg("onlyTotalDeathsMsg",out message);
						break;
					case 1:
						CPH.TryGetArg("totalPlusCurrentDeathsMsg",out message);
						break;
				}
				break;
			case "addDeathCounter":
				switch(_pdcsMessageTypeResult)
				{		 
					case 0:
						CPH.TryGetArg("hasDeathCounterMsg",out message);
						break;
					case 1:
						CPH.TryGetArg("addedDeathCounterMsg",out message);
						break;
				}
				break;
			case "removeDeathCounter":
				switch(_pdcsMessageTypeResult)
				{		 		
					case 0:
						CPH.TryGetArg("noDeathCounterMsg",out message);
						break;
					case 1:
						CPH.TryGetArg("removedDeathCounterMsg",out message);
						break;
				}
				break;
			case "getLeaderboard":
				switch(_pdcsMessageTypeResult)
				{		 		
					case -1:
						CPH.TryGetArg("nonValidInputMsg",out message);
						break;
					case 0:
						CPH.TryGetArg("noDeathCountersMsg",out message);
						break;
					case 1:
						CPH.TryGetArg("leaderboardMsg",out message);
						break;
				}
				break;
			default:
				message = null;
				break;
		}
		//Reset
		_pdcsMessageType = "";
		_pdcsMessageTypeResult = 0;
		if(message == null) return true;
		message = ReplaceWithArgs(message,args);
		CPH.SetArgument("pdcsResultMessage",message);
		if(String.IsNullOrEmpty(platform)) return true;
		string[] splitMessage = message.Split(' ');
        int maxChars = 200;
        switch(platform)
        {	 		 
            case "twitch":
                maxChars = 500;
                break;
            case "youtube":
                maxChars = 200;
                break;
            case "trovo":
                maxChars = 300;
                break;
        }
        string output = "";
        foreach(string word in splitMessage)
        {	 		
            if ((output + " " + word).Length > maxChars)
            {
                SendPlatformMessage(platform,output);
                output = word;
                CPH.Wait(100);
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
					CPH.SendYouTubeMessage(messageOutput,botSend,broadcastId);
				}else{
					CPH.SendYouTubeMessage(messageOutput,botSend);
				}
                
                break;
            case "trovo":
				CPH.SendTrovoMessage(messageOutput);
                break;
        }
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

	static string ConvertUnicodeEscapes(string input)
    {
        return Regex.Replace(input, @"\\u([0-9A-Fa-f]{4})", match =>
        {
            return char.ConvertFromUtf32(int.Parse(match.Groups[1].Value, System.Globalization.NumberStyles.HexNumber));
        });
    }
    
	public async Task<List<Game>> GetGameInfos(List<string> infoList,bool areIds = false)
    {	 		 
        string tokenValue = CPH.TwitchOAuthToken;
        string clientIdValue = CPH.TwitchClientId;
		string gameNames = "name=" + String.Join("&name=",infoList);
		string gameIds = "id=" + string.Join("&id=",infoList);
		string urlInput = areIds ? gameIds : gameNames;
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("client-ID", clientIdValue);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenValue);
        HttpResponseMessage response = await _client.GetAsync($"https://api.twitch.tv/helix/games?{urlInput}");
		List<Game> gameList = new List<Game>();
		if(response.IsSuccessStatusCode)
		{	 		
			HttpContent responseContent = response.Content;
			string responseBody = await response.Content.ReadAsStringAsync();
			TwitchResponse root = JsonConvert.DeserializeObject<TwitchResponse>(responseBody);
			if(root.data.Count > 0) gameList = root.data;
		}		 
        return gameList;
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
	
	public class DeathCounter	 		 
	{	 		
		public Version Version {get;set;} = new Version("1.6.0");		 
		public long TotalDeaths{get;set;} = 0;		 		
		public Dictionary<string,GameDeathInfo> Counters{get;set;} = new Dictionary<string,GameDeathInfo>();		 		
		public bool InitFetchOldCounters = false;
		public string NonValidGame = "Non Valid Category";
		public string BlankGameBoxArt = "about:blank";
		public string ArtDimension = "600x800";
		public string InputVariable = "rawInput";
	}
	public class GameDeathInfo
	{
		public string GameName{get;set;} = "";
		public string GameBoxArt{get;set;} = "";
		public string IgdbId{get;set;} = "";
		public long Count{get;set;} = 0;
	}
	public class Game
	{
		public string id { get; set; }
		public string name { get; set; }
		public string box_art_url { get; set; }
		public string igdb_id { get; set; }
	}

	public class Pagination
	{
		public string cursor { get; set; }
	}

	public class TwitchResponse
	{
		public List<Game> data { get; set; }
		public Pagination pagination { get; set; }
	}
}
