# pwn Basic Dictionaries

## Overview
The extension manages multiple named dictionaries. Each dictionary stores string key/value pairs. Meaning you can for example add a word into a dictionary with the value of it being the meaning of the word, or anything else that you want to store. Key/Values in a dictionary are "Entries".

All commands can be used by Twitch, YouTube, Trovo and Kick. By default they have only Twitch set as source. The responses will be send to the respective platform from where the command was used.

All commands, except for `Entry Get` and `Dictionary Get`, have their permissions set to Moderators by default.

Initially a dictionary called `default` will be generated, so you don't have to first create a new one. You can rename it afterwards of course.

Entry methods: `GetEntry`, `AddEntry`, `UpdateEntry`, `RemoveEntry`
Dictionary methods: `GetDictionary`, `AddDictionary`, `RenameDictionary`, `RemoveDictionary`
Custom triggers: `Entry Added/Updated/Removed`, `Dictionary Added/Renamed/Removed`

## Common Argument Keys
Every method has the following arguments:
- `extMethod` — short tag for the method (e.g. `entryAdd`, `dictRename`).
- `extMethodResult` — number which relates to the message being chosen.
- `extMethodMessage` — resulting message output

Possible Results:
- -1 = Invalid Input
- 0 = Dictionary not found
- 2 = Entry not found
- 3 = Dictionary exists
- 5 = Entry exists
- 7 = Success

## Entry Actions

All entry related actions have a searchDictName argument which defines which dictionary should be used.
Alternatively you can use `your dict name;` directly after the command and before the entry inputs to directly check in another dictionary. The searchDictName argument will be ignored in that case. Using a `\` before the `;` will ignore it and searchDictName will be used normally.

Example: `!what fruits; apple`
- Dictionary being searched: `fruits`
- Entry being looked up: `apple`

### Entry Get (`entryGet`)
Entry Get example: `!what my key`

If argument `searchDictName` is null/empty or does not exist, it will search through all dictionaries available. Which is why dictName entryKey etc have an index suffix in this action.

Argument Available for each message result:
|  Result | Arguments  |
|---|---|
| Invalid Input  |  `rawInput` |
| Dict Not Found  |  `dictFound`(bool), `dictName0` |
| Entry Not Found |  `dictFound`(bool), `dictName0` , `searchTerm`, `entryFound`(bool), `entryFoundCount` |
| Success |  `searchTerm`, `entryFound`(bool), `entryFoundCount`, `dictName#`, `entryKey#`, `entryValue#` |

### Entry Add (`entryAdd`)
Entry Add example: `!entryadd mykeyword : my value`

Argument Available for each message result:
|  Result | Arguments  |
|---|---|
| Invalid Input  |  `rawInput` |
| Dict Not Found  |  `dictFound`(bool), `dictName` |
| Entry Exists |  `dictFound`(bool), `dictName` , `keyName`, `keyValue`, `entryFound`(bool), `entryKey` , `entryValue` |
| Success |  `dictFound`(bool), `dictName`, `keyName`, `keyValue`, `entryKey`, `entryValue` |

### Entry Update (`entryUpdate`)
Entry Update example: `!entryupdate mykeyword : my new value`

Argument Available for each message result:
|  Result | Arguments  |
|---|---|
| Invalid Input  |  `rawInput` |
| Dict Not Found  |  `dictFound`(bool), `dictName`, `keyName`, `keyValue` |
| Entry Not Found |  `dictFound`(bool), `dictName` , `keyName`, `keyValue`, `entryFound`(bool) |
| Success |  `dictFound`(bool), `dictName`, `keyName`, `keyValue`, `entryKey`, `entryValue` |

### Entry Remove (`entryRemove`)
Entry Remove example: `!entryremove mykeyword`

Argument Available for each message result:
|  Result | Arguments  |
|---|---|
| Invalid Input  |  `rawInput` |
| Dict Not Found  |  `dictFound`(bool), `dictName`, `keyName`, `keyValue` |
| Entry Not Found |  `dictFound`(bool), `dictName` , `keyName`, `keyValue`, `entryFound`(bool) |
| Success |  `dictFound`(bool), `dictName`, `entryFound`, `entryKey`, `entryValue` |

## Dictionary Actions

### Dictionary Get (`dictGet`)
Dictionary Get example: `!dictget dictname`

Argument Available for each message result:
|  Result | Arguments  |
|---|---|
| Invalid Input  |  `rawInput` |
| Dict Not Found  |  `dictFound`(bool), `dictName` |
| Success |  `dictFound`(bool), `dictName`, `dictEntries`(Dictionary<string,string>, `dictEntryCount`) |

### Dictionary Add (`dictAdd`)
Dictionary Add example: `!dictget dictname`

Argument Available for each message result:
|  Result | Arguments  |
|---|---|
| Invalid Input  |  `rawInput` |
| Dict Exists  |  `dictFound`(bool), `dictName`, `dictEntries`(Dictionary<string,string>), `dictEntryCount`|
| Success |  `dictFound`(bool), `dictName`, `dictEntries`(Dictionary<string,string>), `dictEntryCount` |

### Dictionary Rename (`dictRename`)
Dictionary Rename example: `!dictrename dictname : new dictname`

Argument Available for each message result:
|  Result | Arguments  |
|---|---|
| Invalid Input  |  `rawInput` |
| Dict Not Found  | `oldDictName`, `newDictName`, `dictFound`(bool), `dictName` |
| Dict Exists  |  `oldDictName`, `newDictName`, `dictFound`(bool), `dictName`, `dictEntries`(Dictionary<string,string>), `dictEntryCount`|
| Success |  `oldDictName`, `newDictName`, `dictName`, `dictEntries`(Dictionary<string,string>), `dictEntryCount` |

### Dictionary Remove (`dictRemove`)
Dictionary Remove example: `!dictremove dictname`

Argument Available for each message result:
|  Result | Arguments  |
|---|---|
| Invalid Input  |  `rawInput` |
| Dict Not Found  | `oldDictName`, `newDictName`, `dictFound`(bool), `dictName` |
| Success |  `dictFound`(bool), `dictName`, `dictEntries`(Dictionary<string,string>), `dictEntryCount` |
