## Description
The Death Counter lets you track deaths for specific games during your stream. Simply set the game as a death counter, and then you or your moderators can manually update the counter. It syncs with your current Twitch category and adjusts when you switch games. You can also trigger actions when the counter changes or when you set/remove a category as death counter. The commands also work on YouTube and Trovo, always referencing your Twitch category.

⚠ **Version 1.6.0 of this death counter requires Streamer.bot version 0.2.4. If you are not at least on that version I can't guarantee that it's working correctly.** ⚠

If you already installed my previous death counter (version 1.4.0), your old death counters will automatically be converted to the new one after importing and executing any of the new actions. If there are any counters left you can check the pwnedCounter_Games global variable and do a manual conversion again which is described later.

## Import File




## Installation

In Streamer.bot click the `Import` button in the top menu. Drag the **.sb** file into the `Import String` field. In total, it should be 12 Actions, 2 queues, and 8 commands.


### After Import
After you import the death counter, go into the Commands tab and enable the commands that are in the `[pwn] Per Game DeathCounter (Single)` group. Then make sure each command has the correct permissions sets. 

By default the following commands are Moderator only:
* Counter Decrease
* Counter Increase
* Counter Set
* Game Add
* Game Remove

If you previously had other death counter commands, I'd recommend you disable them now while you are in the commands tab.

## Commands
**Input for commands are only valid if they are non-decimal numbers and smaller or equals to the max value of an integer.**
| **Command**            | **Description** |
|---|---|
| **!deathgame+** | Registers the current game as a death counter |
| **!deathgame-** | Un-registers the current game from the death counters |
| **!death+** | Increases the current games counter by 1 or by the input number. If game has a counter |
| **!death-** | Decreases the current games counter by 1 or by the input number. If game has a counter |
| **!deathset** | Sets the current games counter by the input number. If game has a counter |
| **!deaths** | Shows the current amount of deaths of the current game |
| **!totaldeaths** | Shows the amount of deaths of all registered games |
| **!deathboard** | Will show a leaderboard in your chat with the defined amount of registered games |

## Configuration
Now let's get to the actions and how you define your output messages! 
A quick note on the actions that rely on commands. There will only ever be an output if the argument `userType` is present. This means that if you were to trigger a counter increase with your StreamDeck, it would not output a message into chat. 

However by adding an argument to the StreamDeck action, or from where you want to trigger the action, and you also set the argument called `userType` to either  `Twitch`, `YouTube`, or `Trovo` then it will the message accordingly. For YouTube, it will send the message to all streams if you are multistreaming. If done by command only to the specific stream.

Generally, all of the command actions have the same setup. You define the messages via the Set Argument sub-actions. There you **only** change the value of those messages, not the variable name. You can use any arguments you normally have in a command trigger but also the additional arguments listed below and also shown in the action:

<details>
<summary> Available Arguments for all Actions  </summary>

| **Argument Name**           | **Value**          | **Description**                                                                                                  |
|------------------------|--------------------|------------------------------------------------------------------------------------------------------------------|
| pdcsGame | string/text | Current game you are in |
| pdcsGameId | string/text  | Current game's id |
| pdcsGameBoxArtUrl | string/text  | Game box art URL of the current game. Default size is 600x800  |
| pdcsTotalDeaths | long/number  | Total amount of deaths in all registered games   |
| pdcsTotalGames | int/number  | Total amount of registered games |
| pdcsHasCounter | True/False   | Whether or not the current game is registered as a death counter |
| pdcsCounter | long/number  | The amount of deaths the current game has. Only available if pdcsHasCounter is True |
</details>

### Counter Increase, Decrease, Set Actions 
In the Counter Increase and Decrease action you have a Set Argument setting called `userInput`, with this you can define whether or not an input via command or somewhere else will be used. If userInput is set to True then it will check for the `rawInput` argument, by default. You can change which argument would be used as input in another action if needed. 

The Counter Set action does not have the `userInput` option, as you'd always expect an input for this action.

These actions have the following message output arguments which you can define:
| **Argument Name**           | **Value**          | **Description**                                                                                                  |
|------------------------|--------------------|------------------------------------------------------------------------------------------------------------------|
| nonValidInputMsg| string/text | Message if a userInput is not a valid input |
| noDeathCounterMsg | string/text  | Message if the current game is not a death counter |
| decreaseCountMsg | string/text | Message if decreasing the counter was successful |
| increaseCountMsg  | string/text | Message if increasing the counter was successful |
| setCountMsg | string/text | Message if setting the counter was successful |



### Game Add/Remove
These actions are used to add or remove the current game to or from the death counters.
#### Game Add Set Arguments
| **Argument Name**           | **Value**          | **Description**                                                                                                  |
|------------------------|--------------------|------------------------------------------------------------------------------------------------------------------|
| hasDeathCounterMsg| string/text | Message if the current game is already set as death counter |
| addedDeathCounterMsg | string/text  | Message if the current game has been set as death counter |
#### Game Remove Set Arguments
| **Argument Name**           | **Value**          | **Description**                                                                                                  |
|------------------------|--------------------|------------------------------------------------------------------------------------------------------------------|
| noDeathCounterMsg | string/text | Message if the current game is not currently a death counter |
| removedDeathCounterMsg | string/text  | Message if the current game has been remove from the death counters |

### Get Game Counter / Total Deaths
These actions are only there to display the counters in the chat.
#### Get Game Counter
Display the current games death counter
| **Argument Name**           | **Value**          | **Description**                                                                                                  |
|------------------------|--------------------|------------------------------------------------------------------------------------------------------------------|
| noDeathCounterMsg | string/text | Message if the current game is not set as death counter |
| deathCountMsg | string/text  | Message if the current game has a death counter that can be shown|
#### Get Total Deaths
Display the total amount of deaths in all registered death counter
| **Argument Name**           | **Value**          | **Description**                                                                                                  |
|------------------------|--------------------|------------------------------------------------------------------------------------------------------------------|
| noDeathCounterMsg | string/text | Message if the current game is not set as death counter |
| deathCountMsg | string/text  | Message if the current game has a death counter that can be shown|

### Get Leaderboard (TopX)
This action provides you with a leaderboard which will be posted into chat, or if needed in your OBS, Discord, or in a File. To use it separately from the command and have it for example post in your Discord after a stream you can duplicate the action, get rid of the command trigger and add the Twitch > Channel > Stream Offline trigger, and then use the argument pdcsLeaderboardObs in the webhook sub-action.

<details>
<summary> Additional Arguments for Leaderboard  </summary>
`#` is used as an index number for the leaderboard, starting at 0.

| **Argument Name**           | **Value**          | **Description**                                                                                                  |
|------------------------|--------------------|------------------------------------------------------------------------------------------------------------------|
| pdcsGame[#] | string/text | Game name |
| pdcsGameId[#] | string/text  |Game id |
| pdcsGameBoxArtUrl[#] | string/text  | Game box art URL  |
| pdcsIgdbId[#]| long/number  | IGDBid of game   |
| pdcsDeathCount[#]| long/number  | Amount of deaths for game |
| pdcsLeaderboardCount | int/number   | Amount of games in the leaderboard |
| pdcsLeaderboardDict | Dictionary<string, Dictionary<string,object>>  | Dictionary based on the game id. The value dictionary has the following entries: `gameName`(string), `gameBoxArt`(string), `igdbId`(string), `deathCount`(long) |
| pdcsLeaderboard| string/text  | Simple text output like the given format, separated by a comma |
| pdcsLeaderboardObs | string/text  | Same as the normal text output but with \n instead of comma to display as a list in OBS  |
| pdcsLeaderboardFile | string/text  | Same as the normal text output but with NewLine instead of comma to save it properly listed as a file  |
</details>

#### Settings
Settings Variables are also available for the messages.
| **Argument Name**           | **Value**          | **Description**                                                                                                  |
|------------------------|--------------------|------------------------------------------------------------------------------------------------------------------|
| leaderboardFormat | string/text | Define how the single listings of the leaderboard should look like. Use any of the variables mentioned above|
| ignoreInput | True/False  | Whether or not the user input should be considered. If not will use the defaultBoardSize |
| minBoardSize| long/number  | Minimum boardsize a user can enter |
| maxBoardSize | long/number  | Maximum boardsize a user can enter |
| defaultBoardSize | long/number  | Default boardsize which will be outputted. If amount of death counters isn't as high, will only display the amount that is available |
| orderByDescending | True/False  | Whether or not the leaderboard should be displayed in descending order, related to the number of deaths |

### On Change Event / Update Game
These actions are based on changes happening. 
#### On Change Event
This action will trigger anytime the current death counter is being changed, or if a game has been added/removed to/from the counters. It is disabled by default just in case.

It uses a custom trigger which you can use yourself on any action you want. Under Triggers: `Custom > [pwn] Extensions > DeathCounter Single > DeathCounter Update`

#### Update Game
This action will trigger every time you change your category on Twitch. This action will also have the normal arguments that are generated by the death count available so you can do actions for example when you switch to a game and if it has a death counter do something, like enabling your death counter overlay for example.

### Upgrade / Settings
This action is used to either upgrade from the old death counter (v1.4.0) if not all counters were transferred on the initial execution of the new counter or to set certain settings values. 

This action is executed by right-clicking the test trigger and selecting "Test Trigger"

| **Argument Name**           | **Value**          | **Description**                                                                                                  |
|------------------------|--------------------|------------------------------------------------------------------------------------------------------------------|
| nonValidGameName | string/text | This is really only used for the empty Twitch category which has the gameId 0, and is not a valid category for death counters|
| blankGameBoxArt | string/text  | This URL will be used when a games game box art URL was not able to be retrieved. The default is "about:blank" so it's a blank browser source |
| gameBoxDimensions | string/text  | This is used to set the width and height of the game box art URL which is provided in most if not all actions |
| usedInputVariable| string/text  | This is used to determine the input variable used in the commands. The default is `rawInput`, however, you can change it to `input0` if needed. However, this is for all commands |
| convertFromOld | True/False  | This is used to convert games from the `pwnedCounter_Games` (Dictionary<string,long> string is game name) global variable. The default is False. A maximum of 100 games per run can be tried to convert. It should show a Toast Notification or message in the logs. |

That should be everything you can change/define/set etc. If some things are still not clear, you know where to find me.. probably.
