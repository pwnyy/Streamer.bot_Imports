This is *currently* only a simple stats tracker, which will provide you with the amount of subs and bits there were given to you at the end of a HypeTrain.

### Arguments:

- **hypeTrainEnd**    - True/False  - If True this is an indicator that the hypetrain ended, which will then provide the next arguments.

- **totalBits**    - Number    - Number of total bits spend in the hype train. This does **not** include bits spend on Twitch Extensions like Sound Alerts etc. As this is not possible to track via Twitch Events if you are not the original creator of the extension.
- **totalSubs**    - Number    - Simply the amount of subs/resub/gifted subs you received in total during the hypetrain.
- **totalEventsCount**    - Number    - The amount of events that happened which contributed toward the hypetrain.

Updated to also populate lists of contributors with the following arguments:
- userDisplayList
- userDisplayListString
- userDisplayListObs
- userDisplayListFile
- userLoginList
- userLoginListString
- userLoginListObs
- userLoginListFile
- userIdList
- userIdListString
- userIdListObs
- userIdListFile

List is simple a List<string> object, 
ListString is a comma separated text,
ListObs is a \n separated text to use in OBS
ListFile is a new file line separated text to directly write to a file

1.0.4:
Added Arguments IF top.subscription.user is available.

- top.subscription.sub - amount of sub/resub the user did. Usually 0 or 1
- top.subscription.gifts - amount of subs the user gifted. Is not categorized in tiers.

You will have to define your power-up prices yourself as well, as currently this information is also not provided by the Automatic Reward Redemption event.
This may change in the future but that is the current standpoint for 0.2.6.

This was tested previously by someone with a few hypetrains and should work fine, however if there are any issues just let me know.
