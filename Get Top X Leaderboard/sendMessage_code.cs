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
using System.Reflection;

public class CPHInline
{
	public bool Execute()
	{
        CPH.SendMessage(CPH.GetVersion());
		CPH.TryGetArg("userType",out string platform);
		platform = platform.ToLower();
		CPH.TryGetArg("messageType",out int messageType);
		string message ="";
		
		switch(messageType)
		{
			case -1 :
				CPH.TryGetArg("wrongInputMessage",out message);
				break;
			case 0:
				CPH.TryGetArg("noUsersMessage",out message);
				break;
			case 1 :
				CPH.TryGetArg("defaultMessage",out message);
				break;
		}
		if(String.IsNullOrEmpty(message)) message = "No message defined.";
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
	
public void SendPlatformMessage(string platform, string messageOutput)
{
    switch (platform)
    {
        case "twitch":
            CPH.SendMessage(messageOutput);
            break;
        case "youtube":
            YouTubeUserInfo botInfo = CPH.YouTubeGetBot();
            bool botSend = botInfo != null;
            CPH.TryGetArg("broadcast.id", out string broadcastId);

            if (broadcastId != null)
            {
                SendYouTubeVersionMessage(messageOutput, botSend, broadcastId);
            }
            else
            {
                CPH.SendYouTubeMessage(messageOutput, botSend);
            }
            break;
        case "trovo":
            CPH.SendTrovoMessage(messageOutput);
            
            break;
    }
}
private void SendYouTubeVersionMessage(string message, bool botSend, string broadcastId)
    {
        Type cphType = CPH.GetType();
        MethodInfo sendMethod;

        if (IsVersionOrNewer(CPH.GetVersion(), "1.0.0"))
        {
            sendMethod = cphType.GetMethod("SendYouTubeMessage",
                new Type[] { typeof(string), typeof(bool), typeof(bool), typeof(string) });

            if (sendMethod != null)
            {
                sendMethod.Invoke(CPH, new object[] { message, botSend, true, broadcastId });
                return;
            }
        }
        else
        {
            sendMethod = cphType.GetMethod("SendYouTubeMessage",
                new Type[] { typeof(string), typeof(bool), typeof(string) });

            if (sendMethod != null)
            {
                sendMethod.Invoke(CPH, new object[] { message, botSend, broadcastId });
                return;
            }
        }

        CPH.LogError("[pwn LeaderBoard TopX] Message - SendYouTubeMessage method not found for this version!");
    }

    public bool IsVersionOrNewer(string currentVersion, string targetVersion)
    {
        Version current = new Version(currentVersion);
        Version target = new Version(targetVersion);
        return current >= target;
    }
}
