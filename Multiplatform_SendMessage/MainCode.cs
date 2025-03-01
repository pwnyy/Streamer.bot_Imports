using System;
using System.Reflection;

public class CPHInline
{
	public bool SendMessage()
	{
		CPH.TryGetArg("userType",out string platform);
		platform = platform.ToLower();
		string message = GetMessage(platform);
		
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
	
	public string GetMessage(string platform)
	{
		string output = "";
		
		bool isSpecific = CPH.TryGetArg(platform+"MessageOutput",out output);
		
		if(!isSpecific) CPH.TryGetArg("defaultMessageOutput",out output);
		
		return output;
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
					SendYouTubeVersionMessage(messageOutput, botSend, broadcastId);
				}else{
					CPH.SendYouTubeMessage(messageOutput,botSend);
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

        CPH.LogError("[pwn Multiplatform] Message - SendYouTubeMessage method not found for this version!");
    }

    public bool IsVersionOrNewer(string currentVersion, string targetVersion)
    {
        Version current = new Version(currentVersion);
        Version target = new Version(targetVersion);
        return current >= target;
    }
}
