# Creator Cards Triggers
After Importing the .sb file, from [here](https://github.com/pwnyy/Streamer.bot_Imports/blob/main/Misc/CreatorCards/%5Bpwn%5D_CreatorCards_Triggers_by_pwnyy.sb) into Streamer.bot, go to https://creator.cards/dashboard/api and copy your API Token. Then in Streamer.bot go to Servers/Clients > Websocket Clients. There should be a custom websocket client with the name CreatorCards. Replace the "your-api-token-here" with your API token, then click "Ok". Then right-click the Client and click on Connect.

Once the status says "Open" the client should be connected.

Now you should also have Custom Triggers which were created. So go into any action or create a new action, then right-click in the trigger section and go to Custom > [pwn] Extensions > Creator Cards > , here you will see the Connected, Disconnected, Purchase and Single Card triggers.

## Purchase Trigger
Will give you all information about a purchase as following arguments:
| Argument  | Argument  |
|---|---|
| cc.json  | cc.currency  |
| cc.type  | cc.quantity |
| cc.user  | cc.total_price  |
| cc.alert.mode  | cc.tier_amounts.count |
| cc.alert.alignment  | cc.tier_amounts[#].name  |
| cc.alert.speed  | cc.tier_amounts[#].bronze  |
| cc.alert.sound  | cc.tier_amounts[#].silver  |
| cc.alert.volume  | cc.tier_amounts[#].gold  |
| cc.alert.font  | cc.tier_amounts[#].platinum  |
| cc.alert.font_color  | cc.test  |
| cc.alert.font_border_color  |   |
| cc.alert.font_size  |   |
| cc.alert.showFree  |   |
| cc.alert.freeSound  |   |
| cc.alert.platinum_sound  |   |
| cc.alert.override_platinum  |   |

As Cards are Listed as Objects with potential multiple cards you'll have a 2D array, first index is the card "type" depending on the cc.cards.count , second would be the card itself depending on the cc.cards[#].count 
| Card Arguments | 
|---|
| cc.cards.count  | 
| cc.cards[#].name  | 
| cc.cards[#].count  |
| cc.cards[#][#].currency  | 
| cc.cards[#][#].title  | 
| cc.cards[#][#].description  |
| cc.cards[#][#].image  |
| cc.cards[#][#].markdown  | 
| cc.cards[#][#].tier  | 
| cc.cards[#][#].edition  |
| cc.cards[#][#].creator  | 
| cc.cards[#][#].created |
| cc.cards[#][#].price  |
| cc.cards[#][#].version  |

## Single Card Trigger
Will give you the following arguments:
| Card Arguments | 
|---|
| user  | 
| card.currency | 
| card.title  |
| card.description  | 
| card.image  | 
| card.markdown  |
| card.tier  |
| card.edition  | 
| card.creator  | 
| card.created  |
| card.price  | 
| card.version |
