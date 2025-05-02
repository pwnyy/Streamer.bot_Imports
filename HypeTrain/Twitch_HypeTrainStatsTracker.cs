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
using Streamer.bot.Common.Events;
public class CPHInline
{

	static string _hypeTrainId = null;
	static string _initUserId = null;
	static bool _hypeTrainEnd = false;
	static DateTime _hypeTrainStart = DateTime.Now;
	
	static Dictionary<string, User> _hypeTrainUsers = new Dictionary<string, User>();
	static List<HypeTrainEvent> _trainEvents = new List<HypeTrainEvent>();
	
	public bool Execute()
	{

		_hypeTrainEnd = false;
		DateTime currentEvent = DateTime.Now;
		
		CPH.TryGetArg("userId",out string userId);
		CPH.TryGetArg("userName",out string userLogin);
		CPH.TryGetArg("user",out string userDisplay);
		User eventUser = new User()
		{
			userId = userId,
			userLogin = userLogin,
			userDisplay = userDisplay,
		};
		EventType eventType = CPH.GetEventType();
		
			switch(eventType)
			{
				//Subs/Resubs/GiftSubs/GiftBomb Part
				case EventType.TwitchSub:
				case EventType.TwitchReSub:
					_trainEvents.Add( new HypeTrainEvent(){
						eventTime = currentEvent,
						type = eventType,
						user = eventUser,
						value = 1
						});
					if(!_hypeTrainUsers.ContainsKey(userId))
					{
						_hypeTrainUsers.Add(userId,eventUser);
					}
					break;
				case EventType.TwitchGiftSub:
					CPH.TryGetArg("fromGiftBomb",out bool fromGiftBomb);
					if(fromGiftBomb) return false;
					_trainEvents.Add( new HypeTrainEvent(){
						eventTime = currentEvent,
						type = eventType,
						user = eventUser,
						value = 1
						});
					if(!_hypeTrainUsers.ContainsKey(userId))
					{
						_hypeTrainUsers.Add(userId,eventUser);
					}
					break;
				case EventType.TwitchGiftBomb:
					CPH.TryGetArg("gifts",out int gifts);
					_trainEvents.Add( new HypeTrainEvent(){
						eventTime = currentEvent,
						type = eventType,
						user = eventUser,
						value = gifts
					});
					if(!_hypeTrainUsers.ContainsKey(userId))
					{
						_hypeTrainUsers.Add(userId,eventUser);
					}
					break;
				//Bits Part
				case EventType.TwitchAutomaticRewardRedemption:
					CPH.TryGetArg("rewardType",out string rewardType);
					if(rewardType == "celebration" || rewardType == "gigantify_an_emote" || rewardType == "message_effect")
					{
						
						CPH.TryGetArg(rewardType+"_price",out long cost);
						_trainEvents.Add( new HypeTrainEvent(){
							eventTime = currentEvent,
							type = eventType,
							user = eventUser,
							value = cost
							});
						if(!_hypeTrainUsers.ContainsKey(userId))
						{
							_hypeTrainUsers.Add(userId,eventUser);
						}
					}else{
						return false;
					}
					break;
				case EventType.TwitchCheer:
					CPH.TryGetArg("bits",out long bits);
					_trainEvents.Add( new HypeTrainEvent(){
						eventTime = currentEvent,
						type = eventType,
						user = eventUser,
						value = bits
						});
					if(!_hypeTrainUsers.ContainsKey(userId))
					{
						_hypeTrainUsers.Add(userId,eventUser);
					}
					break;
				case EventType.TwitchHypeTrainStart:
					_hypeTrainStart = DateTime.Now.AddSeconds(-601);

					break;
				case EventType.TwitchHypeTrainEnd:
					CPH.SetGlobalVar("pwn_HypeTrainOnGoing", false, true);
					_hypeTrainEnd = true;
					long subCounter = 0;
					long cheerCounter = 0;
					
					foreach(HypeTrainEvent evt in _trainEvents)
					{
						if(evt.eventTime >= _hypeTrainStart)
						{
							switch(evt.type)
							{
								case EventType.TwitchSub:
								case EventType.TwitchReSub:
								case EventType.TwitchGiftSub:
								case EventType.TwitchGiftBomb:
									subCounter += evt.value;
									break;
								case EventType.TwitchAutomaticRewardRedemption:
								case EventType.TwitchCheer:
									cheerCounter += evt.value;
									break;
							}
						}
					}
					CPH.SetArgument("totalEventsCount",_trainEvents.Count);
					CPH.SetArgument("totalSubs",subCounter);
					CPH.SetArgument("totalBits",cheerCounter);
					List<string> displayNames = new List<string>();
					List<string> loginNames = new List<string>();
					List<string> userIds = new List<string>();
					foreach(KeyValuePair<string,User> htUser in _hypeTrainUsers)
					{
						userIds.Add(htUser.Key);
						displayNames.Add(htUser.Value.userDisplay);
						loginNames.Add(htUser.Value.userLogin);
					}
					CPH.SetArgument("userDisplayList",displayNames);
					CPH.SetArgument("userDisplayListString",String.Join(", ",displayNames));
					CPH.SetArgument("userLoginList",loginNames);
					CPH.SetArgument("userLoginListString",String.Join(", ",loginNames));
					CPH.SetArgument("userIdList",userIds);
					CPH.SetArgument("userIdListString",String.Join(", ",userIds));
					_hypeTrainUsers.Clear();
					_trainEvents = new List<HypeTrainEvent>();
					break;
			}
			CPH.SetArgument("hypeTrainEnd",_hypeTrainEnd);

		return true;
	}

	public class HypeTrainEvent{
		public DateTime eventTime {get;set;}
		public EventType type {get;set;}
		public User user {get;set;}
		public long value {get;set;}
	}
	
	public class User
	{
		public string userId {get;set;}
		public string userLogin {get;set;}
		public string userDisplay {get;set;}
	}
}
