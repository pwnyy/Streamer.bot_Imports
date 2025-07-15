// Author: pwnyy , https://pwnyy.tv https://twitch.tv/pwnyytv , https://x.com/pwnyy, https://ko-fi.com/pwnyy
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
using Newtonsoft.Json.Linq;

public class CPHInline
{
	Dictionary<string,string> triggerContext = new Dictionary<string,string>()
	{
		{"pwn_CreatorCards_Connected","Connected"},
		{"pwn_CreatorCards_Disconnected","Disconnected"},
		{"pwn_CreatorCards_Purchase","Purchase"},
		{"pwn_CreatorCards_SingleCard","Single Card"}
	};
	string extensionName = "pwn Creator Cards";
	
	public void Init()
	{
		ExtensionLog("Init Custom Triggers");
		string[] contextMenu = {"[pwn] Extensions","Creator Cards"};
		
		foreach(KeyValuePair<string,string> kvp in triggerContext)
		{
			string triggerName = kvp.Value;
			string eventName = kvp.Key;
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
					ExtensionLog($"Was not able to register custom trigger {triggerName} with {attempts} attempts.",-1);
				}
			}
		}

	}
	
	public void ExtensionLog(string message, int logType = 0)
	{
		message = $"[{extensionName}] - {message}";
		switch(logType)
		{
			case -1:
				CPH.LogError(message);
				break;
			case 0:
				CPH.LogInfo(message);
				break;
			case 1:
				CPH.LogDebug(message);
				break;
			case 2:
				CPH.LogVerbose(message);
				break;
		}
	}
	
	public bool Execute()
	{
		EventSource sourceBy = CPH.GetSource();
		EventType triggeredBy = CPH.GetEventType();
		CPH.TryGetArg("wsHost",out string wsHostCheck);
		if(!wsHostCheck.Equals("wss.creator.cards"))
		{
			ExtensionLog($"Not using the correct websocket URL/Host for Creator Cards. Currently {wsHostCheck}");
			return false;
		}
		
		switch(triggeredBy)
		{
			case EventType.WebsocketClientClose:
				ExtensionLog("Client Disonnected",1);
				CPH.TriggerCodeEvent("pwn_CreatorCards_Disconnected", true);
				break;
				
			case EventType.WebsocketClientOpen:
				ExtensionLog("Client Connected",1);
				CPH.TriggerCodeEvent("pwn_CreatorCards_Connected", true);
				break;
				
			case EventType.WebsocketClientMessage:
				CPH.TryGetArg("message",out string json);
				MessageHandler(json);
				break;
		}
		return true;
	}
	
	private void MessageHandler(string json)
	{
		JObject obj = JObject.Parse(json);
		string messageType = obj["type"].ToString();
		switch(messageType.ToLower())
		{
			case "purchase":
				PurchaseHandler(obj,json);
				break;
			//more cases added potentially later on
		}
	}
	
	private void PurchaseHandler(JObject purchaseObj,string json)
	{
		Dictionary<string,object> purchaseArgs = new Dictionary<string,object>();
		purchaseArgs.Add("cc.json",json);
		FlattenJToken(purchaseObj, "cc",purchaseArgs);
		CPH.TriggerCodeEvent("pwn_CreatorCards_Purchase", purchaseArgs);
		
		SingleCardTrigger(purchaseArgs["cc.user"].ToString(),purchaseObj);
	}
	
	private void SingleCardTrigger(string user,JToken token)
	{
		JToken cardsObj = token["cards"];

		foreach (var property in cardsObj.Children<JProperty>())
		{
			string cardTitle = property.Name;
			JArray cardInfo = property.Value as JArray;

			if (cardInfo == null)
				continue;

			for (int i = 0; i < cardInfo.Count; i++)
			{
				var card = cardInfo[i];
				var cardArgs = new Dictionary<string, object>();
				
				cardArgs["user"] = user;

				FlattenJToken(card, "card", cardArgs);

				CPH.TriggerCodeEvent("pwn_CreatorCards_SingleCard", cardArgs);
			}
		}
	}
	
	private void FlattenJToken(JToken token, string prefix, Dictionary<string,object> argsDict)
	{
		if (token is JObject obj)
		{
			//objects in objects (like tier_amounts , cards)
			if (IsDynamicObjectGroup(obj))
			{
				int index = 0;
				argsDict[$"{prefix}.count"] = obj.Properties().Count();
				foreach (var property in obj.Properties())
				{
					string baseKey = $"{prefix}[{index}]";
					//add basekey argument as well (probably not needed)
					argsDict[$"{baseKey}.name"] = property.Name;
					FlattenJToken(property.Value, baseKey,argsDict);
					index++;
				}
			}
			else
			{
				foreach (var property in obj.Properties())
				{
					string newPrefix = $"{prefix}.{property.Name}";
					FlattenJToken(property.Value, newPrefix,argsDict);
				}
			}
		}
		else if (token is JArray array)
		{
			argsDict[$"{prefix}.count"] = array.Count;
			for (int i = 0; i < array.Count; i++)
			{
				string newPrefix = $"{prefix}[{i}]";
				FlattenJToken(array[i], newPrefix,argsDict);
			}
		}
		else
		{
			object value = token.Type == JTokenType.Null ? "" : token.ToString();
			if (!argsDict.ContainsKey(prefix))
			{
				argsDict[prefix] = value;
			}
		}
	}
	
	private bool IsDynamicObjectGroup(JObject obj)
	{
		return obj.Properties().All(p => p.Value.Type == JTokenType.Object || p.Value.Type == JTokenType.Array);
	}
	
}
