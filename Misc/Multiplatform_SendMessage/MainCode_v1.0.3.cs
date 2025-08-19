using System;
using System.Reflection;

public class CPHInline
{
    string[] platforms = ["twitch","youtube","trovo","kick"];

	public bool SendMessage()
	{
		CPH.TryGetArg("userType",out string platform);
        CPH.TryGetArg("sendToAllPlatforms", out bool sendToAll);
        if(sendToAll)
        {
            foreach(string plat in platforms)
            {
                Splitter(plat);
            }
        }else{
            Splitter(platform);
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
	
    public void Splitter(string platform)
    {
        string message = GetMessage(platform);
		
		string[] splitMessage = message.Split(' ');
        int maxChars = 200;
        switch(platform)
        {	 		 
            case "twitch":
            case "kick":
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
    }

	public void SendPlatformMessage(string platform,string messageOutput)
    {
        bool botSend = true;
        if(CPH.TryGetArg("useBroadcasterAccount",out bool useBot)) botSend = !useBot;

        switch(platform)
        {
            case "twitch":
                CPH.SendMessage(messageOutput, botSend);
                break;
            case "youtube":
				CPH.TryGetArg("broadcast.id",out string broadcastId);
				if(broadcastId != null)
				{
					CPH.SendYouTubeMessage(messageOutput, botSend, broadcastId:broadcastId);
				}else{
					CPH.SendYouTubeMessage(messageOutput,botSend);
				}
                
                break;
            case "trovo":
				CPH.SendTrovoMessage(messageOutput, botSend);
                break;
            case "kick":
				CPH.SendKickMessage(messageOutput, botSend);
                break;
        }
    }
    
}
