## Get Color from Input
Use the input from anywhere check whether the html color name, hex value, rgb value or argb is found or and get the needed ARGB, HEX or OBS color values. 
Extension includes "!color" command itself as quick example, can quickly be altered with a reward redemption trigger with input, or other ways just needing to adjust the inputColor argument value to get the input.

**Example usages:**
- !color green
- !color hex #123456
- !color rgb 123 123 1232
- !color rgba 123 123 132 123
- !color argb 123 123 123 123
- !color 123 , will result in rgb with 123, 0 , 0
- !color 50 123 70 42 , will result in argb 50, 123, 70, 42
- !color #123 , will result in hex #112233
- !color #123456, will result in hex #123456
- !color #FFAABBCC , will result in hex of #AABBCC and FF determines the alpha value

Color Aliases that are set will always take priority. They get searched for first.

**Populated arguments:**
- foundColor    - Is True/False
If foundColor is True then the following arguments are added as well:
- colorName    - if color does not have a name then the value will be "none"
- colorA
- colorR
- colorG
- colorB
- colorHex
- colorAlphaHex
- colorHexAlpha
- colorObs


The "a" stands for alpha and is used to adjust the transparency of the color. The lower the number the more transparent it is.

You can look up what color names are available either here:  https://learn.microsoft.com/en-us/dotnet/api/system.drawing.knowncolor?view=net-8.0 
or here: https://htmlcolorcodes.com/color-names/
