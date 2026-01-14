## Ads Reminder on StreamDeck for Twitch
Requirement for it to show on your StreamDeck:
In Streamer.bot go to Integration > Elgato Stream Deck and make sure the server is running.

Download the .streamDeckAction and drag and drop the action on the button you want on your StreamDeck.
Once you see the button, click on it and make sure there is an action connection to Streamer.bot, if not set up a connection.

Next would be importing the .sb into your Streamer.bot. Either download the file and drag and drop it into the Streamer.bot import window, or copy the raw text into the window.

You'll get 3 actions:
- [Ads Reminder] *Code
- [Ads Reminder] Run Ads
- [Ads Reminder] Windows Notification 5/10Min

The *Code is where you would set the time where you want to be reminded to run an ad. Default is 3600 (1 hour). As you can see on the triggers, it has a Test trigger, which you can use if you wouldn't see the Custom Triggers yet. The Ad Run trigger is required for the code to know when it should reset it's Countdown again, as it will only react/reset when an ad is run.
Run Ads, is simply an action which will run an ad. This action is used by the StreamDeck button, when you hold it.
Windows Notification will run a Toast Notification 10 and 5 minutes before the ad should be run, and also another notification when time is up. This action is also used as an example as the Code contains custom triggers which can be used.


Keep in mind this code was just quickly thrown together so don't expect super magical awesome code lol

