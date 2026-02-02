using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

public class CPHInline
{
    const string ApiEndPoint = "Create Clip";

    private static readonly HttpClient _client = new HttpClient
    {
        Timeout = TimeSpan.FromSeconds(10)
    };

    private static readonly SemaphoreSlim _clipLock = new SemaphoreSlim(1, 1);

    public bool DoClip()
    {
        try
        {
            return DoClipInternalAsync()
                .GetAwaiter()
                .GetResult();
        }
        catch (Exception ex)
        {
            CPH.LogError($"{ApiEndPoint} - EX - {ex}");
            CPH.SetArgument("createClipSuccess", false);
            return false;
        }
    }

    private async Task<bool> DoClipInternalAsync()
    {
        await _clipLock.WaitAsync();
        try
        {
            TwitchUserInfo broadcaster = CPH.TwitchGetBroadcaster();
            if (broadcaster == null)
            {
                CPH.SetArgument("createClipSuccess", false);
                return false;
            }

            if(CPH.TryGetArg("clipTitleInput",out string clipTitle) && !string.IsNullOrEmpty(clipTitle))
            {
                clipTitle = clipTitle.Substring(0, Math.Min(clipTitle.Length, 100)).Trim();
            }
            
            float clipDuration = 30;
            if(CPH.TryGetArg("clipDurationInput", out clipDuration))
            {
                clipDuration = Math.Max(5, Math.Min(clipDuration, 60));
            }
            
            DateTime createdAt = DateTime.Now;

            TwitchResponse response = await CreateCallWithRetry(broadcaster.UserId, clipTitle, clipDuration);

            bool clipCreated = response?.data?.Count > 0;

            CPH.SetArgument("clipTitle", clipDuration);
            CPH.SetArgument("clipDuration", clipDuration);
            CPH.SetArgument("createClipSuccess", clipCreated);

            if (!clipCreated)
                return false;

            string clipId = response.data[0].id;

            CPH.SetArgument("createClipId", clipId);
            CPH.SetArgument("createClipCreatedAt", createdAt);
            CPH.SetArgument("createClipUrl", $"https://www.twitch.tv/{broadcaster.UserLogin}/clip/{clipId}");
            CPH.SetArgument("createClipUrlEmbed", $"https://clips.twitch.tv/embed?clip={clipId}");

            return true;
        }
        finally
        {
            _clipLock.Release();
        }
    }

    private async Task<TwitchResponse> CreateCallWithRetry(string broadcasterId, string clipTitle , float clipDuration)
    {
        const int maxAttempts = 3;

        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            TwitchResponse response = await FunctionCallTwitchAPI(broadcasterId, clipTitle, clipDuration);

            if (response?.data?.Count > 0)
                return response;

            await Task.Delay(500);
        }

        return null;
    }

    private async Task<TwitchResponse> FunctionCallTwitchAPI(string broadcasterId, string clipTitle , float clipDuration)
    {
        string defaultRequest = $"https://api.twitch.tv/helix/clips?broadcaster_id={broadcasterId}";
        if(!String.IsNullOrEmpty(clipTitle))
        {
            clipTitle = CPH.UrlEncode(clipTitle);
            defaultRequest += $"&title={clipTitle}";
        }
        if(clipDuration >= 5 && clipDuration <= 60)
        {
            defaultRequest += $"&duration={clipDuration}";
        }

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            defaultRequest
        );

        request.Headers.Add("Client-ID", CPH.TwitchClientId);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", CPH.TwitchOAuthToken);

        HttpResponseMessage response = await _client.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            CPH.LogError($"{ApiEndPoint} - ERR - {response.StatusCode}");
            return null;
        }

        string json = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<TwitchResponse>(json);
    }
}

public class TwitchResponse
{
    public List<TwitchClipData> data { get; set; }
}

public class TwitchClipData
{
    public string id { get; set; }
    public string edit_url { get; set; }
}
