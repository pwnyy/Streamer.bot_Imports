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
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
 
public class CPHInline
{	 		 
	List<string> originFiles = new List<string>();
	List<string> currentPlaylist = new List<string>();

	bool continuePlayback = true;
	bool isPlaying = false;
	
	bool playerLog = false;
	
	string _obsName = "";
	int _obsConnection = -1;
	string mediaSceneName = "";
	string mediaSourceName = "";
	
	string previous = "";
	string currentMedia = "";
	string currentFolder = "";
	bool restartFromLastMedia = false;
	bool playRandom = false;
	 
	Dictionary<string,string> brbSettings = null;
	
	static string eventPrefix = "pwnBrbPlayer_";
	string lastMediaGlobal = $"{eventPrefix}LastMediaFile";
	
	public bool Execute()
	{	 		
		CPH.TryGetArg("mediaSourceName",out mediaSourceName);
		CPH.TryGetArg("mediaSceneName",out mediaSceneName);
		
		CPH.TryGetArg("brbSceneName",out string brbName);
		CPH.TryGetArg("usedObsName",out _obsName);
		int obsConnection = CPH.ObsGetConnectionByName(_obsName);
		
		CPH.TryGetArg("obs.name",out string eventObsName);
		CPH.TryGetArg("restartFromLastMedia",out restartFromLastMedia);
		
		brbSettings = CPH.GetGlobalVar<Dictionary<string,string>?>(lastMediaGlobal,true) ?? new Dictionary<string,string>();
		
		if(brbSettings.Count == 0 && restartFromLastMedia)
		{
			brbSettings = new Dictionary<string,string>()
			{
				{"restartFromLastMedia","True"},
				{"lastMedia",currentMedia}
			};
			SaveBrbSettings();
		}else if(brbSettings.Count > 0 && !restartFromLastMedia)
		{
			CPH.UnsetGlobalVar(lastMediaGlobal, true);
		}
		
		CPH.TryGetArg("playerLogging", out playerLog); 
		
		if(_obsConnection != obsConnection)
		{		 
			_obsConnection = obsConnection;
			PlayerLogger($"Resetting player - Reason: First ObsConnection was not the same as current. Potentially OBS Instances swapped.");
			ResetPlayer();
		}
		
		EventType eventType = CPH.GetEventType();
		
		switch(eventType)
		{		 		
			case EventType.ObsConnected:
				if(!isPlaying && _obsName == eventObsName)
				{		 		
					string currentScene = CPH.ObsGetCurrentScene(obsConnection);
					if(currentScene == brbName)
					{
						StartPlayer(_obsConnection);
					}
				}
				break;
			case EventType.ObsDisconnected:
				if(isPlaying && _obsName == eventObsName)
				{
					PlayerLogger($"Resetting player - Reason: Obs Disconnected");
					ResetPlayer();
				}
				break;
			case EventType.ObsSceneChanged:
				CPH.TryGetArg("obs.sceneName",out string changedScene);
				if(_obsName == eventObsName)
				{
					if(isPlaying && changedScene != brbName)
					{
						PlayerLogger($"Resetting player - Reason: Scene Change to non set scene");
						ResetPlayer();

					}else if(!isPlaying && changedScene == brbName)
					{
						
						StartPlayer(_obsConnection);
						ResetPlayer();
					}
				}
				break;
			case EventType.Test:
				PlayerLogger($"Resetting current list of played files and updating origin files");
				UpdatePlaylist();
				if(_obsName == eventObsName)
				{
					ResetPlayer();
					StartPlayer(_obsConnection);
				}

				break;
		}
		
		return true;
	}
	
	public void StartPlayer(int obsConnection)
	{	 		 
		PlayerLogger("Starting player");
		UpdatePlaylist();
		continuePlayback = true;
		CPH.ObsHideSource(mediaSceneName, mediaSourceName, obsConnection);

		CPH.TryGetArg("videoFolder",out string myDirectory);
		CPH.TryGetArg("useSubdirectories",out bool useSubdir);
		CPH.TryGetArg("useReadLines",out bool useReadLines);
		CPH.TryGetArg("playEachFileOnce",out bool playEachFileOnce);
		
		CPH.TryGetArg("noDirectRepeat",out bool noDirectRepeat);
		CPH.TryGetArg("delayBetweenMedia",out int mediaDelay);
		CPH.TryGetArg("startDelayMs",out int startDelay);
		
		CPH.Wait(startDelay);
		CPH.ObsShowSource(mediaSceneName, mediaSourceName, obsConnection);
		
		
		//originFiles = new List<string>(Directory.GetFiles(myDirectory,"*.*", useSubdir ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly));
		
		if(originFiles.Count == 0 && continuePlayback)
		{	 		
			PlayerLogger($"No files found in {myDirectory}", true);
			
		}else{
			isPlaying = true;
			PlayerEvent("Player_Started");
			if(playRandom)
			{
				if(originFiles.Count == 1 || !playEachFileOnce)
				{		 
					while(continuePlayback)
					{		 		
						string nextMedia = originFiles[CPH.Between(0,originFiles.Count - 1)];
						PlayerLogger($"Next Media: {nextMedia}");
						PlayNextMedia(mediaSceneName, mediaSourceName, nextMedia, _obsConnection);
						CPH.Wait(mediaDelay);
					}
				}else if(playEachFileOnce)
				{		 		
					currentPlaylist = new List<string>(originFiles);
					while(continuePlayback)
					{
						if(currentPlaylist.Count <= 0){
							currentPlaylist = new List<string>(originFiles);
							PlayerLogger($"Fill Media List from origin");
						} 
						
						int nextMediaIndex = CPH.Between(0,currentPlaylist.Count - 1);
						string nextMedia = currentPlaylist[nextMediaIndex];
						if(previous == nextMedia)
						{
							nextMediaIndex = (nextMediaIndex +1) < currentPlaylist.Count ? nextMediaIndex +1 : (nextMediaIndex - 1) >= 0 ? nextMediaIndex -1 : nextMediaIndex;  
							nextMedia = currentPlaylist[nextMediaIndex];
						}
						previous = nextMedia;
						currentPlaylist.RemoveAt(nextMediaIndex);
						PlayerLogger($"Next Media: {nextMedia}");
						PlayNextMedia(mediaSceneName, mediaSourceName, nextMedia, _obsConnection);
						CPH.Wait(mediaDelay);
					}
				}
			}else{
				
				if(!useReadLines) originFiles = SortPlaylist(originFiles);
				
				int nextMediaIndex = 0;
				
				if(restartFromLastMedia && brbSettings.Count > 0)
				{
					string lastMedia = null;
					brbSettings.TryGetValue("lastMedia",out lastMedia);
					if(!String.IsNullOrEmpty(lastMedia))
					{
						int tempIndex = originFiles.IndexOf(lastMedia);
						nextMediaIndex = tempIndex >= 0 ?  tempIndex : 0;
					}
				}
				while(continuePlayback)
				{
					if (nextMediaIndex >= originFiles.Count)
					{
						nextMediaIndex = 0;
					}
					string nextMedia = originFiles[nextMediaIndex];
					PlayerLogger($"Next Media: {nextMedia}");
					if(restartFromLastMedia && brbSettings.Count > 0)
					{
						brbSettings["lastMedia"] = nextMedia;
						SaveBrbSettings();
					}
					PlayNextMedia(mediaSceneName, mediaSourceName, nextMedia, _obsConnection);
					CPH.Wait(mediaDelay);
					nextMediaIndex++;
				}
			}
			
		}
	}

	
	public void PlayNextMedia( string sceneName, string sourceName, string filePath, int connection)
	{	 		 
		try{
			currentMedia = filePath;
			CPH.ObsSetMediaSourceFile(sceneName, sourceName, filePath, connection);
			CPH.Wait(100);
			CPH.ObsMediaRestart(sceneName, sourceName, connection);
			CPH.Wait(150);
			string responseJson = CPH.ObsSendRaw("GetMediaInputStatus", "{\"inputName\":\""+ sourceName +"\"}", connection);
			JObject jsonObject = JObject.Parse(responseJson);
			long mediaCursor = jsonObject["mediaCursor"]?.Value<long>() ?? 0;
			long mediaDuration = jsonObject["mediaDuration"]?.Value<long>() ?? 0;
			int cursorChecker = 0;
			
			PlayerLogger($"Current Media ({Path.GetFileName(filePath)}) Duration: {mediaDuration}");
			PlayerEvent("New_Media_Played", filePath, mediaDuration);
			
			string folderName = new DirectoryInfo(Path.GetDirectoryName(filePath)).Name;
			
			while (mediaCursor < mediaDuration && continuePlayback)
			{	 		
				mediaCursor += 500;
				cursorChecker += 500;
				if((cursorChecker % 5000) == 0)
				{		 
					string checkResponse = CPH.ObsSendRaw("GetMediaInputStatus", "{\"inputName\":\""+ sourceName +"\"}", connection);
					JObject checker = JObject.Parse(checkResponse);
					mediaCursor = checker["mediaCursor"]?.Value<long>() ?? 0;
				}		 		
				CPH.Wait(500);
			}	
		}
		catch (InvalidCastException ex){
			PlayerLogger(ex.Message,true);
		}
	}
	
	public void UpdatePlaylist()
	{	 		 
		CPH.TryGetArg("useReadLines",out bool useReadLines);
		CPH.TryGetArg("playRandom",out playRandom);
		List<string> newFiles = new List<string>();
		
		if(useReadLines)
		{	 		
			CPH.TryGetArg("fileFound",out bool fileFound);
			CPH.TryGetArg("lineCount",out int lineCount);
			if(fileFound && lineCount > 0)
			{		 
				for(int i=0;i<lineCount;i++)
				{		 		
					if(CPH.TryGetArg("line"+i,out string line))
					{		 		
						if(!line.StartsWith("//"))
						{	 		 
							line = line.Trim('"');
							try{	 		
								string fullPath = Path.GetFullPath(line);
								newFiles.Add(fullPath);
							}catch(Exception ex)
							{		 
								PlayerLogger($"FilePath \"{line}\" was not valid, will be skipped.",true);
							}
						}

					}else{		 		
						break;
					}
				}
			}
		}else{		 		
			CPH.TryGetArg("videoFolder",out string myDirectory);
			CPH.TryGetArg("useSubdirectories",out bool useSubdir);
			try{
				newFiles = new List<string>(Directory.GetFiles(myDirectory,"*.*", useSubdir ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly));
			}catch (Exception ex)
			{
				PlayerLogger($"FolderPath \"{myDirectory}\" was not valid. \n {ex}",true);
			}
		}
		
		if(newFiles.Count > 0)
		{
			PlayerLogger($"Updating Origin Playlist. Count of files in playlist:{newFiles.Count}");
			originFiles = newFiles;
			if(playRandom)
			{
				currentPlaylist = originFiles;
			}
		}
	}
	
	public List<string> SortPlaylist(List<string> playlist)
	{
		var sortedFiles = playlist
			.OrderByDescending(path => path.Count(c => c == Path.DirectorySeparatorChar)) // deeper folders first
			.ThenBy(path => Regex.Replace(
				Path.GetFileNameWithoutExtension(path),
				@"\d+",
				match => match.Value.PadLeft(10, '0'))
			)
			.ToList();

		return sortedFiles;
	}
	
	public void ResetPlayer()
	{
		if(isPlaying)CPH.ObsHideSource(mediaSceneName, mediaSourceName, _obsConnection);
		continuePlayback = false;
		isPlaying = false;
		currentPlaylist.Clear();
		PlayerEvent("Player_Stopped");
		SaveBrbSettings();
	}
	
	public void PlayerLogger(string message,bool error = false) 
	{
		string logMessage = "[pwnBRB Player] - " + message;
		if(playerLog && !error)
		{
			CPH.LogInfo(logMessage);
		}else if (playerLog)
		{
			CPH.LogError(logMessage);
		}
	}
	
	public void PlayerEvent(string playerEvent, string filePath = null, long duration = 0)
	{	 		 
		Dictionary<string,object> fileDict = new Dictionary<string,object>();
		string parentFolder = "";
		if(!String.IsNullOrEmpty(filePath))
		{	 		
			parentFolder = new DirectoryInfo(Path.GetDirectoryName(filePath)).Name;
			string fileName =  Path.GetFileNameWithoutExtension(filePath);
			string fileNameWithExtension = Path.GetFileName(filePath);
			string extension = Path.GetExtension(filePath);

			fileDict.Add("mediaFilePath",filePath);
			fileDict.Add("mediaFileFolder",parentFolder);
			fileDict.Add("mediaFileName",fileName);
			fileDict.Add("mediaFile",fileNameWithExtension);
			fileDict.Add("mediaFileExtension",extension);
			
			fileDict.Add("mediaFileDuration",duration);
		}

		
		switch(playerEvent)
		{		 
			case "New_Media_Played":
				if(String.IsNullOrEmpty(currentFolder))
				{		 		
					currentFolder = parentFolder;
				}else{
					if(currentFolder != parentFolder)
					{		 		
						currentFolder = parentFolder;
						CPH.TriggerCodeEvent($"{eventPrefix}Media_Folder_Changed", fileDict);
					}
				}
				
				CPH.TriggerCodeEvent($"{eventPrefix}{playerEvent}", fileDict);
				
				break;
			case "Player_Started":
				CPH.TriggerCodeEvent($"{eventPrefix}{playerEvent}", true);
				break;
			case "Player_Stopped":
				CPH.TriggerCodeEvent($"{eventPrefix}{playerEvent}", true);
				break;
		}
	}
	
	public void SaveBrbSettings()
	{	 		 
		if(restartFromLastMedia)
		{	 		
			string json = JsonConvert.SerializeObject(brbSettings);
			PlayerLogger("Saving brbSettings: "+json);
			CPH.SetGlobalVar(lastMediaGlobal, brbSettings, true);
		}
	}
	
	
	public void Init()
	{		 
		Dictionary<string,string> triggerDict = new Dictionary<string,string>()
		{
			{"New Media",$"{eventPrefix}New_Media_Played"},
			{"Media Folder Changed",$"{eventPrefix}Media_Folder_Changed"},
			{"Player Started",$"{eventPrefix}Player_Started"},
			{"Player Stopped",$"{eventPrefix}Player_Stopped"},
		};
		string[] contextMenu = {"[pwn] Extensions","BRB Player"};

		foreach(KeyValuePair<string,string> kvp in triggerDict)
		{		 		
			string triggerName = kvp.Key;
			if(!CPH.RegisterCustomTrigger(triggerName, kvp.Value, contextMenu))
			{		 		
				int attempts = 5;
				bool success = false;
				for(int i= 0;i<attempts;i++)
				{		 
					triggerName+= " ["+(i+1)+"]";
					if(CPH.RegisterCustomTrigger(triggerName, kvp.Value, contextMenu))
					{
						success = true;
						break;
					}
				}
				if(!success)
				{		 		
					PlayerLogger($"Was not able to register custom trigger {kvp.Key} with {attempts} attempts.",true);
				}
			}
		}
	}
	
	public void Dispose()
	{
		SaveBrbSettings();
	}
}
