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
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public enum MethodResult
{	 		 
	Error = -2,
	InvalidInput = -1,
	ObsNotConnected = 0,
	CanvasNotFound = 1,
	SceneNotFound = 2,
	SourceNotFound = 3,
	FilterNotFound = 4,
	OutputNotFound = 5,
	DockNotFound = 6,
	DockModeNotFound = 7,
	Success = 42,
}

public class CPHInline
{	 		

	static readonly string currentCodeVersion = "1.0.1";
	static readonly string customTriggerCategory ="Aitum Suite Bridge";
	static readonly string vendorName = "aitum-stream-suite";
	static readonly string methodResult = "extMethodResult";
	static int _obsIndex = 0;

	static ExtensionData _extData = new ExtensionData();

	public bool AddChapter()
	{		 
		string extMethod = "addChapter";
		var dictArgs = InitArgsDict(extMethod, (int)MethodResult.InvalidInput);
		
		try
		{
			CPH.TryGetArg("chapterOutputNames", out string chapterOutputNames);
			CPH.TryGetArg("chapterName", out string chapterName);

			if(CheckObsConnection(out int obsIndex))
			{
				Dictionary<string,JObject> outputDict = new();

				GetOutputItems(obsIndex, out outputDict);
				List<JObject> requestList = new();

				if(string.IsNullOrWhiteSpace(chapterOutputNames))
				{
					foreach(KeyValuePair<string,JObject> item in outputDict)
					{
						bool isActive = (bool)(item.Value.SelectToken("active") ?? false);
						string type = item.Value.SelectToken("type")?.ToString();
						if(isActive && type == "record")
						{
							JObject requestSingle = new JObject(
								new JProperty("requestType","CallVendorRequest"),
								new JProperty("requestData", new JObject(
									new JProperty("vendorName", vendorName),
									new JProperty("requestType", "add_chapter"),
									new JProperty("requestData", new JObject (
										new JProperty("output", item.Key),
										new JProperty("chapter_name", chapterName)
									))
								))
							);
							requestList.Add(requestSingle);
						}
					}
				}else{
					List<string> chapterOutputList = chapterOutputNames.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
						.Select(output => output.Trim())
						.Where(output => !string.IsNullOrWhiteSpace(output))
						.ToList();
						
					if(chapterOutputList.Count > 0)
					{
						foreach(string outputName in chapterOutputList)
						{
							if(outputDict.TryGetValue(outputName, out var output))
							{
								bool isActive = (bool)(output.SelectToken("active") ?? false);
								string type = output.SelectToken("type")?.ToString();
								if(isActive && type == "record")
								{
									JObject requestSingle = new JObject(
										new JProperty("requestType","CallVendorRequest"),
										new JProperty("requestData", new JObject(
											new JProperty("vendorName", vendorName),
											new JProperty("requestType", "add_chapter"),
											new JProperty("requestData", new JObject (
												new JProperty("output", outputName),
												new JProperty("chapter_name", chapterName)
											))
										))
									);
									requestList.Add(requestSingle);
								}
							}
						}
					}
				}

				if(requestList.Count > 0)
				{
					SendRawBatchRequest(requestList, obsIndex);
					dictArgs[methodResult] = (int)MethodResult.Success;
				}else{
					dictArgs[methodResult] = (int)MethodResult.OutputNotFound;
				}
			}else{
				dictArgs[methodResult] = (int)MethodResult.ObsNotConnected;
			}
		}catch (Exception ex)
		{
			OnCodeError(ex, extMethod, dictArgs);
		} finally
		{
			SetFinalArguments(dictArgs);
		}
		return true;
	}

    public bool CanvasSwitchScene()
	{		 		
		string extMethod = "canvasSwitchScene";
		var dictArgs = InitArgsDict(extMethod, (int)MethodResult.InvalidInput);
		try
		{
			CPH.TryGetArg("inputCanvasName", out string canvasName);
			CPH.TryGetArg("inputSceneName", out string sceneName);

			PathResults path = VerifyPath(canvasName, sceneName);
			dictArgs[methodResult] = (int)path.Result;

			if(path.Success)
			{
				JObject request = new JObject(
                new JProperty("vendorName", vendorName),
                new JProperty("requestType", "switch_scene"),
                new JProperty("requestData", new JObject(
                    new JProperty("canvas", canvasName),
                    new JProperty("scene", path.Scene["name"])
					))
				);
				SendRawRequest("CallVendorRequest", request, path.ObsIndex);
				dictArgs[methodResult] = (int)MethodResult.Success;
			}
		}catch (Exception ex)
		{
			OnCodeError(ex, extMethod, dictArgs);
		} finally
		{
			SetFinalArguments(dictArgs);
		}
		return true;
	}

	public bool GetCanvasCurrentScene()
	{		 		
		string extMethod = "getCanvasCurrentScene";
		var dictArgs = InitArgsDict(extMethod, (int)MethodResult.InvalidInput);
	
		try
		{
			bool result = false;

			CPH.TryGetArg("searchCanvasName", out string canvasName);

			PathResults path = VerifyPath(canvasName);
			dictArgs[methodResult] = (int)path.Result;
			
			if(path.Success)
			{
				JObject rawRequest = new JObject(
						new JProperty("vendorName",vendorName),
						new JProperty("requestType","current_scene"),
						new JProperty("requestData", new JObject (
							new JProperty("canvas",canvasName)
						))
					);

					JObject response = SendRawRequest("CallVendorRequest", rawRequest, path.ObsIndex);
					var successToken = response.SelectToken("responseData.success");

					if(successToken != null && successToken.Type == JTokenType.Boolean)
					{
						bool isSuccess = (bool)successToken;
						if(isSuccess)
						{
							string foundSceneName = response.SelectToken("responseData.scene").ToString();
							string foundSceneUuid = response.SelectToken("responseData.scene_uuid").ToString();

							if(!string.IsNullOrEmpty(foundSceneName) && !string.IsNullOrEmpty(foundSceneUuid))
							{
								dictArgs["currentScene"] = foundSceneName;
								dictArgs["currentSceneUuid"] = foundSceneUuid;
								dictArgs[methodResult] = (int)MethodResult.Success;
							}else{
								dictArgs[methodResult] = (int)MethodResult.SceneNotFound;
							}
						}
					}else{
						dictArgs[methodResult] = (int)MethodResult.SceneNotFound;
					}
			}
		}catch (Exception ex)
		{
			OnCodeError(ex, extMethod, dictArgs);
		} finally
		{
			SetFinalArguments(dictArgs);
		}
		
		return true;
	}


	public bool GetCanvasInfo()
	{	 		 
		string extMethod = "getCanvasInfo";
		var dictArgs = InitArgsDict(extMethod, (int)MethodResult.InvalidInput);
		string dataType = "canvas";
		
		try
		{
			CPH.TryGetArg("searchCanvasName", out string searchName);
			if(CheckObsConnection(out int obsIndex))
			{
				GetDataItems(dataType, searchName, dictArgs, obsIndex);
			}else{
				dictArgs[methodResult] = (int)MethodResult.ObsNotConnected;
			}
		}catch (Exception ex)
		{
			OnCodeError(ex, extMethod, dictArgs);
		} finally
		{
			SetFinalArguments(dictArgs);
		}
		
		return true;
	}

	public bool GetCanvasItemInfo()
	{	 		
		string extMethod = "getCanvasSourceInfo";
		var dictArgs = InitArgsDict(extMethod, (int)MethodResult.InvalidInput);

		try
		{
			CPH.TryGetArg("searchCanvasName", out string canvasName);
			CPH.TryGetArg("searchSceneName", out string sceneName);
			CPH.TryGetArg("searchSourceName", out string sourceName);
			
			PathResults path = VerifyPath(canvasName, sceneName, sourceName);
			dictArgs[methodResult] = (int)path.Result;

			if(path.Success)
			{
				var targetObject = path.Source != null ? path.Source : path.Scene;
				var targetToken = path.Source != null ? targetObject.SelectToken("sourceUuid") : targetObject.SelectToken("uuid");
				dictArgs["targetUuid"] = targetToken.ToString();
				JTokenToDict(dictArgs, targetObject, "item");
			}
		}catch (Exception ex)
		{
			OnCodeError(ex, extMethod, dictArgs);
		} finally
		{
			SetFinalArguments(dictArgs);
		}
		return true;
	}

	public bool GetDockInfo()
	{		 
		string extMethod = "getDockInfo";
		var dictArgs = InitArgsDict(extMethod, (int)MethodResult.InvalidInput);
		string dataType = "dock";
		
		try
		{
			CPH.TryGetArg("searchDockName", out string searchName);
			if(CheckObsConnection(out int obsIndex))
			{
				GetDataItems(dataType, searchName, dictArgs, obsIndex);
			}else{
				dictArgs[methodResult] = (int)MethodResult.ObsNotConnected;
			}
		}catch (Exception ex)
		{
			OnCodeError(ex, extMethod, dictArgs);
		} finally
		{
			SetFinalArguments(dictArgs);
		}
		
		return true;
	}

	public bool GetDockModeInfo()
	{		 		
		string extMethod = "getDockModeInfo";
		var dictArgs = InitArgsDict(extMethod, (int)MethodResult.InvalidInput);
		string dataType = "dockMode";
		
		try
		{
			CPH.TryGetArg("searchDockModeName", out string searchName);
			if(CheckObsConnection(out int obsIndex))
			{
				GetDataItems(dataType, searchName, dictArgs, obsIndex);
			}else{
				dictArgs[methodResult] = (int)MethodResult.ObsNotConnected;
			}
		}catch (Exception ex)
		{
			OnCodeError(ex, extMethod, dictArgs);
		} finally
		{
			SetFinalArguments(dictArgs);
		}
		
		return true;
	}

    public bool GetOutputInfo()
	{		 		
		string extMethod = "getOutputInfo";
		var dictArgs = InitArgsDict(extMethod, (int)MethodResult.InvalidInput);
		string dataType = "output";
		
		try
		{
			CPH.TryGetArg("searchOutputName", out string searchName);
			if(CheckObsConnection(out int obsIndex))
			{
				GetDataItems(dataType, searchName, dictArgs, obsIndex);
			}else{
				dictArgs[methodResult] = (int)MethodResult.ObsNotConnected;
			}
		}catch (Exception ex)
		{
			OnCodeError(ex, extMethod, dictArgs);
		} finally
		{
			SetFinalArguments(dictArgs);
		}
		
		return true;
	}

	public bool SaveBackTracks()
	{	 		 
		string extMethod = "saveBacktracks";
		var dictArgs = InitArgsDict(extMethod, (int)MethodResult.InvalidInput);
		
		try
		{
			CPH.TryGetArg("backtrackOutputNames", out string backtrackOutputNames);

			if(CheckObsConnection(out int obsIndex))
			{
				Dictionary<string,JObject> outputDict = new();

				GetOutputItems(obsIndex, out outputDict);
				List<JObject> requestList = new();

				if(string.IsNullOrWhiteSpace(backtrackOutputNames))
				{
					foreach(KeyValuePair<string,JObject> item in outputDict)
					{
						bool isActive = (bool)(item.Value.SelectToken("active") ?? false);
						string type = item.Value.SelectToken("type")?.ToString();
						if(isActive && type == "backtrack")
						{
							JObject requestSingle = new JObject(
								new JProperty("requestType","CallVendorRequest"),
								new JProperty("requestData", new JObject(
									new JProperty("vendorName", vendorName),
									new JProperty("requestType", "save_backtrack"),
									new JProperty("requestData", new JObject (
										new JProperty("output",item.Key)
									))
								))
							);
							requestList.Add(requestSingle);
						}
					}
				}else{
					List<string> backtrackOutputList = backtrackOutputNames.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
						.Select(output => output.Trim())
						.Where(output => !string.IsNullOrWhiteSpace(output))
						.ToList();
						
					if(backtrackOutputList.Count > 0)
					{
						foreach(string outputName in backtrackOutputList)
						{
							if(outputDict.TryGetValue(outputName, out var output))
							{
								bool isActive = (bool)(output.SelectToken("active") ?? false);
								string type = output.SelectToken("type")?.ToString();

								if(isActive && type == "backtrack")
								{
									JObject requestSingle = new JObject(
										new JProperty("requestType","CallVendorRequest"),
										new JProperty("requestData", new JObject(
											new JProperty("vendorName", vendorName),
											new JProperty("requestType", "save_backtrack"),
											new JProperty("requestData", new JObject (
												new JProperty("output",outputName)
											))
										))
									);
									requestList.Add(requestSingle);
								}
							}
						}
					}
				}

				if(requestList.Count > 0)
				{
					SendRawBatchRequest(requestList, obsIndex);
					dictArgs[methodResult] = (int)MethodResult.Success;
				}else{
					dictArgs[methodResult] = (int)MethodResult.OutputNotFound;
				}
			}else{
				dictArgs[methodResult] = (int)MethodResult.ObsNotConnected;
			}
		}catch (Exception ex)
		{
			OnCodeError(ex, extMethod, dictArgs);
		} finally
		{
			SetFinalArguments(dictArgs);
		}
		return true;
	}
    
	public bool SetCanvasItemFilterState()
	{	 		
		string extMethod = "setCanvasItemFilterState";
		var dictArgs = InitArgsDict(extMethod, (int)MethodResult.InvalidInput);

		try
		{
			CPH.TryGetArg("inputCanvasName", out string canvasName);
			CPH.TryGetArg("inputSceneName", out string sceneName);
			CPH.TryGetArg("inputSourceName", out string sourceName);
			CPH.TryGetArg("inputFilterName", out string filterName);
			CPH.TryGetArg("inputFilterState", out int filterState);
			
			PathResults path = VerifyPath(canvasName, sceneName, sourceName, filterName);
			dictArgs[methodResult] = (int)path.Result;

			if(path.Success)
			{
				string targetUuid = path.Source != null 
					? path.Source["sourceUuid"].ToString() 
					: path.Scene["uuid"].ToString();
				bool requestSuccess = SetObsFilterState(targetUuid, filterName, filterState, path.ObsIndex);
				if(!requestSuccess)
					dictArgs[methodResult] = (int)MethodResult.FilterNotFound;
			}
					
		}catch (Exception ex)
		{
			OnCodeError(ex, extMethod, dictArgs);
		} finally
		{
			SetFinalArguments(dictArgs);
		}
		return true;
	}

	public bool SetCanvasSourceVisibility()
	{		 
		string extMethod = "setCanvasSourceVisibility";
		var dictArgs = InitArgsDict(extMethod, (int)MethodResult.InvalidInput);

		try
		{
			CPH.TryGetArg("inputCanvasName", out string canvasName);
			CPH.TryGetArg("inputSceneName", out string sceneName);
			CPH.TryGetArg("inputSourceName", out string sourceName);
			CPH.TryGetArg("inputVisibilityState", out int visibilityState);
			
			PathResults path = VerifyPath(canvasName, sceneName, sourceName);
			dictArgs[methodResult] = (int)path.Result;

			if(path.Success)
			{
				string targetUuid = path.Scene.SelectToken("uuid").ToString();
				var sceneItemIdToken = path.Source.SelectToken("sceneItemId");
				var sceneItemStateToken = path.Source.SelectToken("sceneItemEnabled");
				bool sceneItemState = false;
				int sceneItemId = 0;
				
				if(sceneItemIdToken != null && sceneItemIdToken.Type == JTokenType.Integer)
				{
					sceneItemId = (int)sceneItemIdToken;
				}

				if(sceneItemStateToken != null && sceneItemStateToken.Type == JTokenType.Boolean)
				{
					sceneItemState = (bool)sceneItemStateToken;
					
					bool inputState = visibilityState == 2 ? !sceneItemState : visibilityState == 1;

					JObject rawRequest = new JObject(
						new JProperty("sceneUuid", targetUuid),
						new JProperty("sceneItemId", sceneItemId),
						new JProperty("sceneItemEnabled", inputState)
					);
					
					JObject response = SendRawRequest("SetSceneItemEnabled", rawRequest, path.ObsIndex);
					CPH.SendMessage(response.ToString());
					bool requestSuccess = response.HasValues;
					
					if(!requestSuccess)
						dictArgs[methodResult] = (int)MethodResult.SourceNotFound;
				}else{
					dictArgs[methodResult] = (int)MethodResult.SourceNotFound;
				}
			}
		}catch (Exception ex)
		{
			OnCodeError(ex, extMethod, dictArgs);
		} finally
		{
			SetFinalArguments(dictArgs);
		}
		return true;
	}

	public bool ShowHideDocks()
	{		 		
		string extMethod = "showHideDocks";
		var dictArgs = InitArgsDict(extMethod, (int)MethodResult.InvalidInput);
		
		try
		{
			CPH.TryGetArg("inputCanvasName", out string inputCanvasName);
			CPH.TryGetArg("inputDockNames", out string inputDockNames);
			CPH.TryGetArg("dockVisibilityState", out bool dockState);

			PathResults path = VerifyPath(inputCanvasName);
			dictArgs[methodResult] = (int)path.Result;

			if(path.Success)
			{
				Dictionary<string,JObject> dockDict = new();

				GetDockItems(path.ObsIndex, out dockDict);
				List<JObject> requestList = new();

				if(string.IsNullOrWhiteSpace(inputDockNames))
				{
					dictArgs[methodResult] = (int)MethodResult.InvalidInput;
				}else{
					List<string> dockList = inputDockNames.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
						.Select(output => output.Trim())
						.Where(output => !string.IsNullOrWhiteSpace(output))
						.ToList();
					
					string canvasName = path.Canvas.SelectToken("name")?.ToString();
					if(dockList.Count > 0)
					{
						foreach(string dockName in dockList)
						{
							if(dockDict.TryGetValue(dockName, out var output))
							{
								JObject requestSingle = new JObject(
									new JProperty("requestType","CallVendorRequest"),
									new JProperty("requestData", new JObject(
										new JProperty("vendorName", vendorName),
										new JProperty("requestType", dockState ? "dock_show" : "dock_hide"),
										new JProperty("requestData", new JObject (
											new JProperty("canvas", canvasName),
											new JProperty("dock", dockName)
										))
									))
								);
								requestList.Add(requestSingle);
							}
						}
					}
				}

				if(requestList.Count > 0)
				{
					SendRawBatchRequest(requestList, path.ObsIndex);
					dictArgs[methodResult] = (int)MethodResult.Success;
				}else{
					dictArgs[methodResult] = (int)MethodResult.OutputNotFound;
				}
			}
		}catch (Exception ex)
		{
			OnCodeError(ex, extMethod, dictArgs);
		} finally
		{
			SetFinalArguments(dictArgs);
		}
		return true;
	}

	public bool StartOutputs()
	{		 		
		string extMethod = "startOutputs";
		var dictArgs = InitArgsDict(extMethod, (int)MethodResult.InvalidInput);
		try
		{
			CPH.TryGetArg("startOutputNames", out string outputNames);

			if(CheckObsConnection(out int obsIndex))
			{
				Dictionary<string,JObject> outputDict = new();

				GetOutputItems(obsIndex, out outputDict);

				if(string.IsNullOrWhiteSpace(outputNames))
				{
					JObject requestAll = new JObject(
						new JProperty("vendorName", vendorName),
						new JProperty("requestType", "start_all_outputs"),
						new JProperty("requestData", new JObject(
						))
					);
					SendRawRequest("CallVendorRequest", requestAll, obsIndex);
				}else{
					List<string> startOutputList = outputNames.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
						.Select(output => output.Trim())
						.Where(output => !string.IsNullOrWhiteSpace(output))
						.ToList();
						
					if(startOutputList.Count > 0)
					{
						List<JObject> requestList = new();

						foreach(string outputName in startOutputList)
						{
							if(outputDict.ContainsKey(outputName))
							{
								JObject requestSingle = new JObject(
									new JProperty("requestType","CallVendorRequest"),
									new JProperty("requestData", new JObject(
										new JProperty("vendorName", vendorName),
										new JProperty("requestType", "start_output"),
										new JProperty("requestData", new JObject (
											new JProperty("output",outputName)
										))
									))
								);
								requestList.Add(requestSingle);
							}
						}
						if(requestList.Count > 0)
						{
							SendRawBatchRequest(requestList, obsIndex);
							dictArgs[methodResult] = (int)MethodResult.Success;
						}
					}else{
						dictArgs[methodResult] = (int)MethodResult.OutputNotFound;
					}
				}
			}else{
				dictArgs[methodResult] = (int)MethodResult.ObsNotConnected;
			}
		}catch (Exception ex)
		{
			OnCodeError(ex, extMethod, dictArgs);
		} finally
		{
			SetFinalArguments(dictArgs);
		}
		return true;
	}

	public bool StopOutputs()
	{	 		 
		string extMethod = "stopOutputs";
		var dictArgs = InitArgsDict(extMethod, (int)MethodResult.InvalidInput);
		try
		{
			CPH.TryGetArg("stopOutputNames", out string outputNames);

			if(CheckObsConnection(out int obsIndex))
			{
				Dictionary<string,JObject> outputDict = new();

				GetOutputItems(obsIndex, out outputDict);

				if(string.IsNullOrWhiteSpace(outputNames))
				{
					JObject requestAll = new JObject(
						new JProperty("vendorName", vendorName),
						new JProperty("requestType", "stop_all_outputs"),
						new JProperty("requestData", new JObject(
						))
					);
					SendRawRequest("CallVendorRequest", requestAll, obsIndex);
				}else{
					List<string> startOutputList = outputNames.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
						.Select(output => output.Trim())
						.Where(output => !string.IsNullOrWhiteSpace(output))
						.ToList();
						
					if(startOutputList.Count > 0)
					{
						List<JObject> requestList = new();

						foreach(string outputName in startOutputList)
						{
							if(outputDict.ContainsKey(outputName))
							{
								JObject requestSingle = new JObject(
									new JProperty("requestType","CallVendorRequest"),
									new JProperty("requestData", new JObject(
										new JProperty("vendorName", vendorName),
										new JProperty("requestType", "stop_output"),
										new JProperty("requestData", new JObject (
											new JProperty("output",outputName)
										))
									))
								);
								requestList.Add(requestSingle);
							}
						}
						if(requestList.Count > 0)
						{
							SendRawBatchRequest(requestList, obsIndex);
							dictArgs[methodResult] = (int)MethodResult.Success;
						}
					}else{
						dictArgs[methodResult] = (int)MethodResult.OutputNotFound;
					}
				}
			}else{
				dictArgs[methodResult] = (int)MethodResult.ObsNotConnected;
			}
		}catch (Exception ex)
		{
			OnCodeError(ex, extMethod, dictArgs);
		} finally
		{
			SetFinalArguments(dictArgs);
		}
		return true;
	}

	public bool StartStopRecordings()
	{	 		
		string extMethod = "startStopRecordings";
		var dictArgs = InitArgsDict(extMethod, (int)MethodResult.Error);
		try
		{
			CPH.TryGetArg("recordingState", out bool recState);

			if(CheckObsConnection(out int obsIndex))
			{
				JObject rawRequest = new JObject(
					new JProperty("vendorName", vendorName),
					new JProperty("requestType", recState ? "start_all_recordings" : "stop_all_recordings"),
					new JProperty("requestData", new JObject(
					))
				);

				JObject response = SendRawRequest("CallVendorRequest", rawRequest, obsIndex);
				var successToken = response.SelectToken("responseData.success");
				
				if(successToken != null && successToken.Type == JTokenType.Boolean)
				{
					bool isSuccess = (bool)successToken;
					if(isSuccess)
					{
						dictArgs[methodResult] = (int)MethodResult.Success;
					}
				}else{
					dictArgs[methodResult] = (int)MethodResult.ObsNotConnected;
				}
			}
		}catch (Exception ex)
		{
			OnCodeError(ex, extMethod, dictArgs);
		} finally
		{
			SetFinalArguments(dictArgs);
		}
		return true;
	}

	public bool StartStopStreams()
	{		 
		string extMethod = "startStopStreams";
		var dictArgs = InitArgsDict(extMethod, (int)MethodResult.Error);
		try
		{
			CPH.TryGetArg("streamState", out bool streamState);

			if(CheckObsConnection(out int obsIndex))
			{
				JObject rawRequest = new JObject(
					new JProperty("vendorName", vendorName),
					new JProperty("requestType", streamState ? "start_all_streams" : "stop_all_streams"),
					new JProperty("requestData", new JObject(
					))
				);

				JObject response = SendRawRequest("CallVendorRequest", rawRequest, obsIndex);
				var successToken = response.SelectToken("responseData.success");
				
				if(successToken != null && successToken.Type == JTokenType.Boolean)
				{
					bool isSuccess = (bool)successToken;
					if(isSuccess)
					{
						dictArgs[methodResult] = (int)MethodResult.Success;
					}
				}else{
					dictArgs[methodResult] = (int)MethodResult.ObsNotConnected;
				}
			}
		}catch (Exception ex)
		{
			OnCodeError(ex, extMethod, dictArgs);
		} finally
		{
			SetFinalArguments(dictArgs);
		}
		return true;
	}

	public bool SwitchDockMode()
	{		 		
		string extMethod = "switchDockMode";
		var dictArgs = InitArgsDict(extMethod, (int)MethodResult.InvalidInput);
		
		try
		{
			CPH.TryGetArg("inputDockMode", out string inputDockMode);

			if(CheckObsConnection(out int obsIndex))
			{
				Dictionary<string,JObject> dockModeDict = new();

				GetDockModeItems(obsIndex, out dockModeDict);
				if(!string.IsNullOrWhiteSpace(inputDockMode))
				{
					if(dockModeDict.TryGetValue(inputDockMode, out var dockModeInfo))
					{
						JObject rawRequest = new JObject(
							new JProperty("vendorName", vendorName),
							new JProperty("requestType", "switch_dock_mode"),
							new JProperty("requestData", new JObject(
								new JProperty("mode",inputDockMode)
							))
						);

						JObject response = SendRawRequest("CallVendorRequest", rawRequest, obsIndex);
						var successToken = response.SelectToken("responseData.success");
						
						if(successToken != null && successToken.Type == JTokenType.Boolean)
						{
							bool isSuccess = (bool)successToken;
							dictArgs[methodResult] = isSuccess ? (int)MethodResult.Success : (int)MethodResult.Error;
						}
					}else{
						dictArgs[methodResult] = (int)MethodResult.DockModeNotFound;
					}
				}else{
					dictArgs[methodResult] = (int)MethodResult.InvalidInput;
				}
			}
		}catch (Exception ex)
		{
			OnCodeError(ex, extMethod, dictArgs);
		} finally
		{
			SetFinalArguments(dictArgs);
		}
		return true;
	}

	public bool GetDataItems(string dataType, string inputName, Dictionary<string,object> dictArgs, int obsIndex, bool moreInfo = true)
	{		 		
		Dictionary<string,JObject> dataDict = new();
		bool getDataResult = false;
		bool dataFound = false;
		string prefix = dataType;
		int notFoundValue = 0;

		switch(dataType)
		{
			case "canvas":
				getDataResult = GetCanvasItems(obsIndex, out dataDict);
				notFoundValue = (int)MethodResult.CanvasNotFound;
				break;
			case "scene":
				getDataResult = GetSceneItems(inputName, obsIndex, out dataDict);
				notFoundValue = (int)MethodResult.SceneNotFound;
				break;
			case "source":
				getDataResult = GetSceneItems(inputName, obsIndex, out dataDict);
				notFoundValue = (int)MethodResult.SourceNotFound;
				break;
			case "output":
				getDataResult = GetOutputItems(obsIndex, out dataDict);
				notFoundValue = (int)MethodResult.OutputNotFound;
				break;
			case "dockMode":
				getDataResult = GetDockModeItems(obsIndex, out dataDict);
				notFoundValue = (int)MethodResult.DockModeNotFound;
				break;
			case "dock":
				getDataResult = GetDockItems(obsIndex, out dataDict);
				notFoundValue = (int)MethodResult.DockNotFound;
				break;
			default:
				return false;
				break;
		}

		bool isInputEmpty = string.IsNullOrWhiteSpace(inputName);

		if(moreInfo) dictArgs[$"{prefix}Found"] = dataFound; 

		if(getDataResult && dataDict.Count > 0)
		{
			if(isInputEmpty)
			{
				if(moreInfo)
				{
					dictArgs[$"{prefix}TotalCount"] = dataDict.Count;
					int i = 0;
					foreach (var data in dataDict)
					{
						JTokenToDict(dictArgs, data.Value, $"{prefix}{i}");
						i++;
					}
				}

				dataFound = true; 
			}
			else if(dataDict.TryGetValue(inputName, out JObject foundDataItem))
			{
				if(moreInfo)
					JTokenToDict(dictArgs, foundDataItem, $"{prefix}0");
				dataFound = true;
			}
			
		}
		
		dictArgs[methodResult] = dataFound ? (int)MethodResult.Success : notFoundValue;
		
		if(moreInfo) dictArgs[$"{prefix}Found"] = dataFound;
		return dataFound;
	}



	public int GetObsFilterState(string inputUuid, string filterName, int obsIndex)
	{	 		 
		JObject rawRequest = new JObject(
			new JProperty("sourceUuid",inputUuid),
			new JProperty("filterName",filterName)
		);

		JObject response = SendRawRequest("GetSourceFilter", rawRequest, obsIndex);
		
		var enabledToken = response.SelectToken("filterEnabled");
		if(enabledToken != null && enabledToken.Type == JTokenType.Boolean)
		{
			return (bool)enabledToken ? 1 : 0;
		}
		return -1;
	}

	public bool SetObsFilterState(string inputUuid, string filterName, int filterState, int obsIndex)
	{	 		
		bool outputResult = false;
		int getFilterResult = filterState == 2 ? GetObsFilterState(inputUuid, filterName, obsIndex) : filterState;
		
		if(getFilterResult >= 0)
		{
			outputResult = true;
			if(filterState == 2)
			{
				filterState = getFilterResult == 1 ? 0 : 1;
			}

			JObject rawRequest = new JObject(
				new JProperty("sourceUuid",inputUuid),
				new JProperty("filterName",filterName),
				new JProperty("filterEnabled",filterState == 1 ? true : false)
			);

			JObject response = SendRawRequest("SetSourceFilterEnabled", rawRequest, obsIndex);
			outputResult = response.HasValues;
		}

		return outputResult;
	}

	public bool GetSceneData(string canvasName, string sceneName, int obsIndex, out JObject sceneInfo )
	{		 
		sceneInfo = null;

		JObject rawRequest = new JObject(
			new JProperty("vendorName",vendorName),
			new JProperty("requestType","get_scenes"),
			new JProperty("requestData", new JObject (
				new JProperty("canvas",canvasName)
			))
		);
		
		JObject response = SendRawRequest("CallVendorRequest", rawRequest, obsIndex);

		var successToken = response.SelectToken("responseData.success");
		bool sceneFound = false;

		if(successToken != null && successToken.Type == JTokenType.Boolean)
		{
			bool isSuccess = (bool)successToken;
			if(isSuccess)
			{
				var scenes = response.SelectToken("responseData.scenes");
				foreach(var scene in scenes)
				{
					if(scene["name"].ToString() == sceneName)
					{
						sceneInfo = scene as JObject;
						sceneFound = true;
						break;
					}
				}
			}
		}
		return sceneFound;
	}

	public bool GetSourceData(string sceneUuid, string sourceName, int obsIndex, out JObject sourceInfo)
	{		 		
		sourceInfo = null;

		JObject rawRequest = new JObject(
			new JProperty("sceneUuid",sceneUuid)
		);
		
		JObject response = SendRawRequest("GetSceneItemList", rawRequest, obsIndex);

		var sceneItems = response["sceneItems"] as JArray;
		bool sourceFound = false;

		if(sceneItems != null)
		{
			foreach(var item in sceneItems)
			{
				if(item["sourceName"].ToString() == sourceName)
				{
					sourceInfo = item as JObject;
					sourceFound = true;
					break;
				}
			}
		}
		return sourceFound;
	}

	public bool GetCanvasItems(int obsIndex, out Dictionary<string,JObject> canvasDict)
	{		 		
		canvasDict = new();

		JObject rawRequest = new JObject(
			new JProperty("vendorName",vendorName),
			new JProperty("requestType","get_canvas"),
			new JProperty("requestData", new JObject ())
		);
		
		JObject response = SendRawRequest("CallVendorRequest", rawRequest, obsIndex);

		var successToken = response.SelectToken("responseData.success");
		bool canvasFound = false;

		if(successToken != null && successToken.Type == JTokenType.Boolean)
		{
			bool isSuccess = (bool)successToken;
			if(isSuccess)
			{
				canvasFound = true;
				var canvases = response.SelectToken("responseData.canvas");
				foreach(var canvas in canvases)
				{
					string canvasItemName = canvas.SelectToken("name").ToString();
					canvasDict[canvasItemName] = canvas as JObject;
				}
			}
		}

		return canvasFound;
	}

	public bool GetSceneItems(string canvasName, int obsIndex, out Dictionary<string,JObject> sceneDict)
	{	 		 
		sceneDict = new();

		JObject rawRequest = new JObject(
			new JProperty("vendorName",vendorName),
			new JProperty("requestType","get_scenes"),
			new JProperty("requestData", new JObject (
				new JProperty("canvas",canvasName)
			))
		);
		
		JObject response = SendRawRequest("CallVendorRequest", rawRequest, obsIndex);

		var successToken = response.SelectToken("responseData.success");
		bool sceneFound = false;

		if(successToken != null && successToken.Type == JTokenType.Boolean)
		{
			bool isSuccess = (bool)successToken;
			if(isSuccess)
			{
				sceneFound = true;
				var scenes = response.SelectToken("responseData.scenes");
				foreach(var scene in scenes)
				{
					string sceneItemName = scene.SelectToken("name").ToString();
					sceneDict[sceneItemName] = scene as JObject;
				}
			}
		}
		return sceneFound;
	}

	public bool GetOutputItems(int obsIndex, out Dictionary<string,JObject> outputDict)
	{	 		
		outputDict = new();

		JObject rawRequest = new JObject(
			new JProperty("vendorName",vendorName),
			new JProperty("requestType","get_outputs"),
			new JProperty("requestData", new JObject ())
		);
		
		JObject response = SendRawRequest("CallVendorRequest", rawRequest, obsIndex);

		var successToken = response.SelectToken("responseData.success");
		bool outputFound = false;

		if(successToken != null && successToken.Type == JTokenType.Boolean)
		{
			bool isSuccess = (bool)successToken;
			if(isSuccess)
			{
				outputFound = true;
				var outputs = response.SelectToken("responseData.outputs");
				foreach(var output in outputs)
				{
					string outputItemName = output.SelectToken("name").ToString();
					outputDict[outputItemName] = output as JObject;
				}
			}
		}

		return outputFound;
	}

	public bool GetDockItems(int obsIndex, out Dictionary<string,JObject> outputDict)
	{		 
		outputDict = new();

		JObject rawRequest = new JObject(
			new JProperty("vendorName",vendorName),
			new JProperty("requestType","get_docks"),
			new JProperty("requestData", new JObject ())
		);
		
		JObject response = SendRawRequest("CallVendorRequest", rawRequest, obsIndex);

		var docksToken = response.SelectToken("responseData.docks");
		bool outputFound = false;

		if(docksToken != null && docksToken.Type == JTokenType.Array && docksToken.HasValues)
		{
			outputFound = true;
			foreach(var output in docksToken)
			{
				string outputItemName = output.SelectToken("name").ToString();
				outputDict[outputItemName] = output as JObject;
			}
		}

		return outputFound;
	}

	public bool GetDockModeItems(int obsIndex, out Dictionary<string,JObject> outputDict)
	{		 		
		outputDict = new();

		JObject rawRequest = new JObject(
			new JProperty("vendorName",vendorName),
			new JProperty("requestType","get_dock_modes"),
			new JProperty("requestData", new JObject ())
		);

		JObject response = SendRawRequest("CallVendorRequest", rawRequest, obsIndex);

		var successToken = response.SelectToken("responseData.success");
		bool outputFound = false;

		if(successToken != null && successToken.Type == JTokenType.Boolean)
		{
			bool isSuccess = (bool)successToken;
			if(isSuccess)
			{
				outputFound = true;
				var outputs = response.SelectToken("responseData.modes");
				foreach(var output in outputs)
				{
					string outputItemName = output.SelectToken("name").ToString();
					outputDict[outputItemName] = output as JObject;
				}
			}
		}

		return outputFound;
	}

	public bool CheckObsConnection(out int obsIndex)
	{		 		
		obsIndex = _obsIndex;
		if(CPH.TryGetArg("obsConnectionIndex", out int inputIndex))
		{
			if(inputIndex >= 0)
			{
				obsIndex = inputIndex; 
			}
		}

		return CPH.ObsIsConnected(obsIndex);
	}

	public PathResults VerifyPath(string canvas, string scene = null, string source = null, string filter = null)
	{	 		 
		PathResults path = new PathResults();

		if(!CheckObsConnection(out path.ObsIndex))
		{
			path.Result = MethodResult.ObsNotConnected;
			return path;
		}
	
		if(string.IsNullOrWhiteSpace(canvas))
		{
			path.Result = MethodResult.InvalidInput;
			return path;
		}
		
		if(!GetCanvasItems(path.ObsIndex, out var canvasDict) || !canvasDict.ContainsKey(canvas))
		{
			path.Result = MethodResult.CanvasNotFound;
			return path;
		}else{
			path.Canvas = canvasDict[canvas];
		}

		if(!string.IsNullOrWhiteSpace(scene))
		{
			if (!GetSceneData(canvas, scene, path.ObsIndex, out path.Scene))
			{
				path.Result = MethodResult.SceneNotFound;
				return path;
			}
		}

		if(!string.IsNullOrWhiteSpace(source) && path.Scene != null)
		{
			string sceneUuid = path.Scene.SelectToken("uuid").ToString();
			if (!GetSourceData(sceneUuid, source, path.ObsIndex, out path.Source))
			{
				path.Result = MethodResult.SourceNotFound;
				return path;
			}
		}

		if(!string.IsNullOrWhiteSpace(filter))
		{
			string targetUuid = path.Source != null 
				? path.Source["sourceUuid"].ToString() 
				: path.Scene?.SelectToken("uuid")?.ToString();

			if(string.IsNullOrWhiteSpace(targetUuid) )
			{
				path.Result = MethodResult.FilterNotFound;
				return path;
			}
		}

		return path;
	}

	public JObject SendRawRequest(string requestType, JObject rawData, int obsIndex)
	{	 		
		JObject response = null;
		try{
			string requestData = JsonConvert.SerializeObject(rawData);
			string responseJson = CPH.ObsSendRaw(requestType, requestData, obsIndex);
			response = JObject.Parse(responseJson);
		}catch(Exception ex)
		{
			ExtensionLog(ex.ToString(), -2);
		}

		return response;
	}

	public JObject SendRawBatchRequest(List<JObject> requestList, int obsIndex)
	{		 
		JObject response = null;
		try{
			string requestData = JsonConvert.SerializeObject(requestList);
			string responseJson = CPH.ObsSendBatchRaw(requestData, connection:obsIndex);
			response = JObject.Parse(responseJson);
		}catch(Exception ex)
		{
			ExtensionLog(ex.ToString(), -2);
		}

		return response;
	}

	private static void JTokenToDict(Dictionary<string, object> dict, JToken token, string prefix = null)
    {		 		
        //Method from Whipstickgostop > https://docs.streamer.bot/examples/parse-json-utility
        switch (token.Type)
        {
            case JTokenType.Object:
                foreach (JProperty property in token.Children<JProperty>())
                {
                    string newPrefix = string.IsNullOrEmpty(prefix) ? property.Name : $"{prefix}.{property.Name}";
                    JTokenToDict(dict, property.Value, newPrefix);
                }
                break;
            case JTokenType.Array:
                int index = 0;
                foreach (JToken arrayItem in token.Children())
                {
                    string newPrefix = $"{prefix}{index}";
                    JTokenToDict(dict, arrayItem, newPrefix);
                    index++;
                }
                break;
            default:
                if (!string.IsNullOrEmpty(prefix))
                {
                    dict[prefix] = ((JValue)token).Value;
                }
                break;
        }
    }

	// Default Methods
    public void OnCodeError(Exception ex, string extMethod ,Dictionary<string,object> dictArgs)
	{		 		
		ExtensionLog($"{extMethod} error: {ex.Message} - {ex.StackTrace}", -2);
		dictArgs[methodResult] = (int)MethodResult.Error;
		ModifyKeyArguments(dictArgs, out dictArgs);
		PopulateArgs(dictArgs);
	}

	public void ModifyKeyArguments(Dictionary<string,object> fromDict, out Dictionary<string,object> toDict )
	{	 		 
        List<string> suffixExclude = [];
        List<string> preFixExclude = ["pwnExtensionName", "pwnExtensionNamePrefix", "pwnExtensionVersion", "pwnProductId"];
        toDict = new Dictionary<string,object>();

        string prefix = _extData?.Metadata?.Prefix;
        string suffix = "";

        foreach (var kvp in fromDict)
        {
            var actualPrefix = preFixExclude.Contains(kvp.Key) ? "" : prefix;
            var actualSuffix = suffixExclude.Contains(kvp.Key) ? "" : suffix;
            toDict[PrefixKey(kvp.Key, actualPrefix) + actualSuffix] = kvp.Value;
        }
	}

	public string PrefixKey(string input,string prefix)
	{	 		
		if(!string.IsNullOrEmpty(prefix))
		{
			return prefix + char.ToUpper(input[0]) + input.Substring(1);
		}

		return input;
	}

	public Dictionary<string,object> InitArgsDict(string key, object value, bool methodDict = true)
	{		 
		var argDict = new Dictionary<string,object>();
		argDict["pwnExtensionName"] = _extData?.Metadata?.Name;
		argDict["pwnExtensionNamePrefix"] = _extData?.Metadata?.Prefix;
		argDict["pwnExtensionVersion"] = _extData?.Metadata?.VersionString;
		argDict["pwnProductId"] = _extData?.Metadata?.ProductId;

		if(methodDict)
		{
			argDict["extMethod"] = key;
			argDict[methodResult] = value;
		}else{
			argDict[key] = value;
		}

		return argDict;
	}


	public void PopulateArgs(Dictionary<string,object> args, string prefix = null)
	{		 
		foreach( var kvp in args)
		{
			CPH.SetArgument(PrefixKey(kvp.Key,prefix),kvp.Value);
		}
	}

	public void SetFinalArguments(Dictionary<string, object> dictArgs)
	{		 		
		ModifyKeyArguments(dictArgs, out var finalArgs);
    	PopulateArgs(finalArgs);
	}

	public void ExtensionLog(string message, int logType = 0)
	{		 		
		string name = _extData?.Metadata?.Name ?? "pwn Aitum Suite Bridge";
		string version = _extData?.Metadata?.VersionString;
		string output = $"[{name}][{version}] - {message}";

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

	//Extension Structure
	[Serializable]
	public class ExtensionMetadata
	{	 		 
		[JsonProperty("name")]
		public string Name { get; set; } = "pwn Aitum Suite Bridge";
		[JsonProperty("prefix")]
		public string Prefix { get; set; } = "pasb";
		[JsonProperty("version")]
		public string VersionString { get; set; } = currentCodeVersion;
		[JsonIgnore]
		public Version Version => new Version(VersionString);
		[JsonProperty("productId")]
		public string ProductId { get; set; } = "PWN-SB-11";
	}
	
	[Serializable]
	public class ExtensionData	 
	{	 		
		[JsonProperty("metadata")]
		public ExtensionMetadata Metadata {get;set;} = new ExtensionMetadata();

	}

	public class PathResults
	{		 
		public MethodResult Result = MethodResult.Success;
		public JObject Canvas = null;
		public JObject Scene = null;
		public JObject Source = null;
		public JObject Filter = null;
		public int ObsIndex = -1;
		public bool Success => Result == MethodResult.Success;
	}

}