# pwn Aitum Suite Bridge

## Overview
Download the .sb file and drag it into the import window of Streamer.bot, don't forget to also enable the commands after import.

The extension tries to make it easier for users to handle Sources, Scenes, Filters and other features of the Aitum Stream Suite, a plugin for OBS, via Streamer.bot actions.

Included Actions:
- Backtrack Save
- Canvas Get Current Scene
- Canvas Get Info
- Canvas Get Item Info
- Canvas Switch Scene
- Chapter Add
- Dock Get Info
- Dock Mode Get Info
- Dock Mode Switch
- Dock Show Hide
- Filter Set Settings Example
- Filter Set Visibility
- Output Get Info
- Output Start
- Output Stop
- Recordings Start Stop All
- Source Set Visibility
- Streams Start Stop All

Using the `Execute Method` sub-action, you can also create your own combinations of course.

Just remember I don't take any responibility if you yourself change the code and it's not working anymore. You are on your own in that case.

## Common Argument Keys
Every method has the following arguments:
- `pwnExtensionName` — name of the extension
- `pwnExtensionNamePrefix` — general prefix for argument of the extension
- `pwnExtensionVersion` — version of the extension

**All other methods that are generated will have the prefix attached, like the two arguments below
- `pasbExtMethod` — short tag for the method (e.g. `addChapter`, `canvasSwitchScene`).
- `pasbExtMethodResult` — result of the chose method

Possible Results:
- -2 = Error
- -1 = Invalid Input
- 0 = Obs not Connected
- 1 = Canvas not found
- 2 = Scene not found
- 3 = Source not found
- 4 = Filter not found
- 5 = Output not found
- 6 = Dock not found
- 7 = Dock Mode not found
- 42 = Success


## More Information about actions:
All methods accept an additional argument called `obsConnectionIndex`, with this you can decide to which connected OBS you will send the requests to. In most cases you can just not add this argument, as by default it will use the OBS connection which is set as default, which would be the index value `-1`.

You can look up your OBS connection indexes in Streamer.bot by going to Stream Apps > OBS Studio, and the very first column is the index of the connection. You can also set your default OBS here, by right-clicking a connection and setting it as default.

Every Argumentnames with `#` in their name, indicate an index. Meaning `#` will be replaced with a number starting from 0.

<details>
<summary> Details about all provided actions </summary>


### Backtrack Save
This action will save either specific active backtrack outputs or all of the active ones.

Input Arguments:
|  Argument | Description of Value  |
|---|---|
| backtrackOutputNames |  Backtrack output names which you want to save. Names can be separated by `,`. This argument can be left empty or disabled if you want all active backtrack outputs to be saved. |

### Canvas Get Current Scene
Will get the current scene a canvas is currently set

Following arguments are needed:
|  Argument | Description of Value  |
|---|---|
| searchCanvasName |  Name of the canvas you want the current scene from |

Argument on success:
|  Argument | Description of Value  |
|---|---|
| pasbCurrentScene |  Name of the canvas you want the current scene from |
| pasbCurrentSceneUuid |  Name of the canvas you want the current scene from |


### Canvas Get Info
Will get specific or all canvases info that are available in Aitum Stream Suite

Input Arguments:
|  Argument | Description of Value  |
|---|---|
| searchCanvasName |  Name of the canvas you want information of. Leave empty or disabled if you want to get information about all canvases. |

Argument on success:
|  Argument | Description of Value  |
|---|---|
| pasbCanvasFound | bool — Whether a canvas was found or not |
| pasbCanvasTotalCount |  Only available when looking up all canvases. Will give the total amount of canvases available. |
| pasbCanvas#.name |  Name of canvas |
| pasbCanvas#.type |  Type of canvas. `extra`, `clone`|
| pasbCanvas#.uuid |  Uuid of canvas |
| pasbCanvas#.width |  Width of canvas |


### Canvas Get Item Info
A canvas item would be either a scene or a source. With this you can get for example the UUID of a specific scene, or of a specific source in a scene. UUIDs can then be used in combination with OBS Raw sub-action for example to set filter settings of these items in the canvases. An example of this is shown in the `Filter Set Settings Example`. 

If you want to get the information of a scene, you would either leave the `searchSourceName` argument value empty, or disable the Set Argument as a whole.
I will not list all available arguments from an item, but only the main ones. You can look at your action history for additional information.

Input Arguments:
|  Argument | Description of Value  |
|---|---|
| searchCanvasName |  Name of the canvas you want to get the information from |
| searchSceneName |  Name of the scene you want the info from |
| searchSourceName |  Name of the source you want the info from. |

Argument on success:
|  Argument | Description of Value  |
|---|---|
| pasbTargetUuid | A general UUID of the item will be given, as scene and source items have different property names. This argument you can use always. |
| pasbItem.sourceName|  Name of the source. Only available for source item |
| pasbItem.sourceUuid |  UUID of the source. Only available for the source item |
| pasbItem.name|  Name of the scene. Only available for the scene item |
| pasbItem.uuid | UUID of the scene. Only available for the scene item |


### Canvas Switch Scene
Switch the scene on a specific canvas.

Following arguments are needed:
|  Argument | Description of Value  |
|---|---|
| inputCanvasName |  Name of the canvas you want to switch the scene in |
| inputSceneName |  Name of the scene you want to switch to |

### Chapter Add
Add a chapter in your recordings.

Input Arguments:
|  Argument | Description of Value  |
|---|---|
| chapterOutputNames |  Name of the recording outputs you want to add a chapter to. Can be multiple names separated by a `,`. This argument can be left empty or disabled if you want to add a chapter to all active recording outputs. |
| chapterName |  Name of the chapter. If left empty the chapter will be called "Unnamed #". |

### Dock Get Info
Will get information of a specific or all docks that are available.

Input Arguments:
|  Argument | Description of Value  |
|---|---|
| searchDockName |  Name of the dock you want information of. Leave empty or disabled if you want to get information about all canvases. |

Argument on success:
|  Argument | Description of Value  |
|---|---|
| pasbDockFound | bool — Whether a dock was found or not  |
| pasbDockTotalCount | Only available when looking up all docks. Total amount of all docks  |
| pasbDock#.floating | bool — Whether the dock is floating or attached in the UI |
| pasbDock#.name | Name of the dock  |
| pasbDock0.visible| bool — Whether the dock is visible or not  |


### Dock Mode Get Info
Will get information of a specific or all dock modes that are available.

Input Arguments:
|  Argument | Description of Value  |
|---|---|
| searchDockName |  Name of the dock you want information of. Leave empty or disabled if you want to get information about all docks. |

Argument on success:
|  Argument | Description of Value  |
|---|---|
| pasbDockModeFound | bool — Whether a dock was found or not  |
| pasbDockModeTotalCount | Total amount of all dock modes |
| pasbDockMode#.fixed | bool — If the dock mode is fixed at the top. Unsure tho. |
| pasbDockMode#.name | Name of the dock mode |


### Dock Mode Switch
Switch to a specific Dock Mode.

Following arguments are needed:
|  Argument | Description of Value  |
|---|---|
| inputDockMode |  Name of the dock mode you want to switch to |

### Dock Show Hide
Hide or Show specific Docks.

Following arguments are needed:
|  Argument | Description of Value  |
|---|---|
| inputCanvasName |  Name of the canvas you want the docks to show. I'm unsure why canvas name is needed but well! lol |
| inputDockNames |  Names of the docks you want to show or hide. Separated by `,`. Leaving it empty or disabled **WILL NOT** show/hide all docks. |
| dockVisibilityState | State of the selected docks. Whether they should show or hide.  `True` = `Show`, `False` = `Hide`|

### Filter Set Settings Example
As previously mentioned this is an example of how one can use the Canvas Get Item Info method to get the uuid of a source/scene and with that be able to change for example the fitler settings via a normal OBS Raw, just by using `sourceUuid` instead of `sourceName` in the raw.

### Filter Set Visibility
Set the filter visibility of a scene or source in a canvas.

Following arguments are needed:
|  Argument | Description of Value  |
|---|---|
| inputCanvasName |  Name of the canvas where the scene is located in. |
| inputSceneName |  Name of the scene |
| inputSourceName | Name of the source. If the filter is on the scene, then leave this empty or disable the argument.|
| inputFilterState | Set the visibility state of the filter, 0 = hidden, 1 = visible, 2 = toggle |


### Output Get Info
Will get information of a specific or all outputs that are available.

Input Arguments:
|  Argument | Description of Value  |
|---|---|
| searchOutputName |  Name of the output you want information of. Leave empty or disabled if you want to get information about all outputs. |

Argument on success:
|  Argument | Description of Value  |
|---|---|
| pasbOutputFound | bool — Whether a dock was found or not  |
| pasbOutputTotalCount | Only available when looking up all outputs. Total amount of outputs. |
| pasbOutput#.active | bool — If the output is currently active. |
| pasbOutput#.name | Name of the dock |
| pasbOutput#.type | Type of output. `stream`, `record`, `backtrack` |


### Output Start
Start specific or all outputs. Make sure that the outputs are also correctly configured, else you will receive a popup in OBS, and the action will still fail.

Input Arguments:
|  Argument | Description of Value  |
|---|---|
| startOutputNames |  Names of the outputs you want to start. Separated by `,`. Leaving it empty or disabled will try and start all outputs. |

### Output Stop
Stop specific or all outputs.

Input Arguments:
|  Argument | Description of Value  |
|---|---|
| startOutputNames |  Names of the outputs you want to stop. Separated by `,`. Leaving it empty or disabled will try and stop all outputs. |

### Recordings Start Stop All
Start of stop all recordings.

Following arguments are needed:
|  Argument | Description of Value  |
|---|---|
| recordingState | `True` = `Start Recording` , `False` = `Stop Recording` |


### Source Set Visibility
Set the visibility of a source.

Following arguments are needed:
|  Argument | Description of Value  |
|---|---|
| inputCanvasName |  Name of the canvas where the scene is located in. |
| inputSceneName |  Name of the scene |
| inputSourceName | Name of the source|
| inputVisibilityState | Set the visibility state of the source, 0 = hidden, 1 = visible, 2 = toggle |


### Streams Start Stop All
Start of stop all streams.

Following arguments are needed:
|  Argument | Description of Value  |
|---|---|
| streamState | `True` = `Start Streams` , `False` = `Stop Streams` |

</details>