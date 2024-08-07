## Description
This extension provides a way to display a leaderboard using any numeric user variables. It will also provide arguments for displaying the same list in OBS and other things. This leaderboard will also work with multiple platforms, so if someone from YouTube uses the command it will pull the user variables from YouTube users and create a leaderboard. Meaning the leaderboards are separated by platform.
After importing you can just duplicate the action and define another user variable and command to make multiple leaderboards.

## Import File

[Leaderboard_Get_Top_X_by_pwnyy.sb](https://github.com/pwnyy/StreamerBot_FreeCodes/blob/main/Get%20Top%20X%20Leaderboard/Leaderboard_Get_Top_X_by_pwnyy.sb)



## Installation

In Streamer.bot click the `Import` button in the top menu. Drag the **.sb** file into the `Import String` field. The import will include one action and one command. 



### Initial Step
Since the import includes a command you will have to enable the command by going to the Commands tab and then searching for the `[Leaderboard] Get Top X`  command, right-clicking it, and selecting "Enable". If you don't need the command and want to create your own you can also just delete it.

## Configuration
To be fair, I might have overdone it with the amount of configurations one can in this extension for just a leaderboard, but most, well hopefully all settings have a use-case. The configurations are done via Set Argument subactions. Please make sure you only change the value and not the Variable name.

### General Settings
| **Argument Name**           | **Value**          | **Description**                                                                                                  |
|------------------------|--------------------|------------------------------------------------------------------------------------------------------------------|
| userVariable| string/text | Set the user variable name you want to make a leaderboard of. Must be a numeric user variable. |
| defaultTop | long/number  | Set the default amounf of leadboard users shown. Non-decimal number |
| orderByDescending | True/False         | Whether the leaderboard is shown in a value descending order or not   |
| ignoredGroups | string/text         | Which user groups should be excluded from the leaderboard. Separate them by comma. Example: Bots, NotWorthyGroup   |

### Additional Settings
<details>
<summary> Settings Table </summary>

| **Argument Name**           | **Value**          | **Description**                                                                                                  |
|------------------------|--------------------|------------------------------------------------------------------------------------------------------------------|
| includeBroadcasterLeaderboard | True/False | Whether or not broadcaster is included in the leaderboard |
| broadcasterUsage | True/False  | Wheter or not the broadcaster can use the command |
| orderByDescending | True/False         | Whether the leaderboard is shown in a value descending order or not   |
| populateRedeemerVariables | True/False         | Whether or not to populate redeemer variables, can decrease response time   |
| rankFormat| string/text  | Format how the user ranking should be shown rankPosition rankUser rankValue. Default: `%rankPosition% - %rankUser% (%rankValue%)` |
| valueDivider| numeric         | Value by which the end value, shown in the leaderboard, will be divided by   |
| valueFormat | string/text         | Format of value - Possible formats [here](https://learn.microsoft.com/en-us/dotnet/standard/base-types/standard-numeric-format-strings) only choose ones that are compatible with decimal type.  Default: `F0` |
</details>

### Command Input Settings
<details>
<summary> Settings Table </summary>

| **Argument Name**           | **Value**          | **Description**                                                                                                  |
|------------------------|--------------------|------------------------------------------------------------------------------------------------------------------|
| ignoreCommandInput | True/False | Whether or not the input a user gives matters. If True then the defaultTop will be used |
| minInput | long/number  | The minimum a user input should be. Non-decimal number |
| maxInput| long/number  | The maximum a user input should be. Non-decimal number   |
</details>

### Time Settings
These are settings only meant if your user variable are supposed to be showing something like watchtime, so the value of the user variable needs to be in **seconds**.
<details>
<summary> Settings Table </summary>

| **Argument Name**           | **Value**          | **Description**                                                                                                  |
|------------------------|--------------------|------------------------------------------------------------------------------------------------------------------|
| isTimeInSeconds | True/False | Whether or not the value is time in seconds and should be formatted |
| shortFormTime | True/False  | Whether or not the short form format should be used. Example: 1h 1m 2s |
| showSeconds| True/False  | If seconds should be displayed or not   |

**Language Settings**
Set your language for years, months, hours, minutes and seconds.
First the singular version and then the plural version as shown in the example, so separated by a comma.

| **Argument Name**           | **Value**          | **Description**                                                                                                  |
|------------------------|--------------------|------------------------------------------------------------------------------------------------------------------|
| secondsLang| string/text | second,seconds |
| minutesLang| string/text  | minute,minutes |
| hoursLang| string/text  | hour,hours |
| daysLang | string/text | day,days |
| monthsLang | string/text  | month,months |
| yearsLang | string/text  | year,years |
</details>

### Arguments Generated
Most of these can be used in the Message Outputs
<details>
<summary> Argument List </summary>

| **Argument Name**           | **Value**          | **Description**                                                                                                  |
|------------------------|--------------------|------------------------------------------------------------------------------------------------------------------|
| rankList | string/text | List of users in the rankFormat you defined |
| rankListObs| string/text  | List of users in the rankFormat for OBS, so every new user is it's own line |
| rankDictionary | Dictionary  | For use in C# Dictionary<string,(string,string,string)> Key = userId, Value = Displayname, LoginName and Value |
| topNumber | long/number  | The number of leaderboard users that are shown  |
||||
|Redeemer Variables| only if populateRedeemerVariable = True||
| lbRedeemer | string/text | Display name of redeemer |
| lbRedeemerLogin | string/text  | Login name of redeemer |
| lbRedeemerId | string/text  | User id of redeemer |
| lbRedeemerRank | long/number  | Rank of redeemer. If redeemer does not have user variable, will be last overall place. |
| lbRedeemerValue | string/text  | The value of the users rank  |
||||
|Single Users of Leaderboard | # is user rank start at 1 ||
| lbUserRank# | long/number  | Rank of current user |
| lbUser# | string/text  | Display name of user |
| lbUserLogin# | string/text  | Login name of user  |
| lbUserId# | string/text  | User id of user |
| lbUserValue# | string/text  | Value of user  |

</details>

### Message Outputs
Due to multiple platform support and also cause leaderboard can sometimes be a bit more lengthy you will have to define your message in arguments. They will then get send to the correct platform and also get split up.

| **Argument Name**           | **Value**          | **Description**                                                                                                  |
|------------------------|--------------------|------------------------------------------------------------------------------------------------------------------|
| noUsersMessage | string/text | Message which will be output if there are currently no users in the leaderboard |
| defaultMessage | string/text  | Message output when users are in the leaderboard |
| wrongInputMessage | string/text  | Message which will be output when the input is an incorrect value |

## Command
Make sure if you want your leaderboard to be usable by multiple platforms that in your command settings you also check the Sources for Twitch/YouTube/Trovo message.

## Testing Phase Log
### Version 1
-  Initial Version. Technically there have already been several version but we'll call it v1 for now.
### Version 2
- Adding rankDictionary for C# usage

<div data-theme-toc="true"> </div>
