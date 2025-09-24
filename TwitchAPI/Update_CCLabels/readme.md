# Updating Content Classification Labels

Download or copy the text in the .sb file and import it into Streamer.bot.

1. **By Bitmask** 
2. **By Singular Arguments**

---

## 1. Using Bitmask

Set `useBitmask` to `True` and provide a value for `cclBitmaskValue`.

Each label corresponds to a bit position in the mask:

| Value | Label                          |
|-------|--------------------------------|
| 1     | DebatedSocialIssuesAndPolitics |
| 2     | DrugsIntoxication              |
| 4     | SexualThemes                   |
| 8     | ViolentGraphic                 |
| 16    | Gambling                       |
| 32    | ProfanityVulgarity             |

### Example

- **Bitmask = 42**
- This enables:  
  - DrugsIntoxication 
  - ViolentGraphic  
  - ProfanityVulgarity

<details>
  
|Value|DSIP|DI|ST|VG|G|PV|
|-----|----|--|--|--|-|--|
|0|x|x|x|x|x|x|
|1|✔|x|x|x|x|x|
|2|x|✔|x|x|x|x|
|3|✔|✔|x|x|x|x|
|4|x|x|✔|x|x|x|
|5|✔|x|✔|x|x|x|
|6|x|✔|✔|x|x|x|
|7|✔|✔|✔|x|x|x|
|8|x|x|x|✔|x|x|
|9|✔|x|x|✔|x|x|
|10|x|✔|x|✔|x|x|
|11|✔|✔|x|✔|x|x|
|12|x|x|✔|✔|x|x|
|13|✔|x|✔|✔|x|x|
|14|x|✔|✔|✔|x|x|
|15|✔|✔|✔|✔|x|x|
|16|x|x|x|x|✔|x|
|17|✔|x|x|x|✔|x|
|18|x|✔|x|x|✔|x|
|19|✔|✔|x|x|✔|x|
|20|x|x|✔|x|✔|x|
|21|✔|x|✔|x|✔|x|
|22|x|✔|✔|x|✔|x|
|23|✔|✔|✔|x|✔|x|
|24|x|x|x|✔|✔|x|
|25|✔|x|x|✔|✔|x|
|26|x|✔|x|✔|✔|x|
|27|✔|✔|x|✔|✔|x|
|28|x|x|✔|✔|✔|x|
|29|✔|x|✔|✔|✔|x|
|30|x|✔|✔|✔|✔|x|
|31|✔|✔|✔|✔|✔|x|
|32|x|x|x|x|x|✔|
|33|✔|x|x|x|x|✔|
|34|x|✔|x|x|x|✔|
|35|✔|✔|x|x|x|✔|
|36|x|x|✔|x|x|✔|
|37|✔|x|✔|x|x|✔|
|38|x|✔|✔|x|x|✔|
|39|✔|✔|✔|x|x|✔|
|40|x|x|x|✔|x|✔|
|41|✔|x|x|✔|x|✔|
|42|x|✔|x|✔|x|✔|
|43|✔|✔|x|✔|x|✔|
|44|x|x|✔|✔|x|✔|
|45|✔|x|✔|✔|x|✔|
|46|x|✔|✔|✔|x|✔|
|47|✔|✔|✔|✔|x|✔|
|48|x|x|x|x|✔|✔|
|49|✔|x|x|x|✔|✔|
|50|x|✔|x|x|✔|✔|
|51|✔|✔|x|x|✔|✔|
|52|x|x|✔|x|✔|✔|
|53|✔|x|✔|x|✔|✔|
|54|x|✔|✔|x|✔|✔|
|55|✔|✔|✔|x|✔|✔|
|56|x|x|x|✔|✔|✔|
|57|✔|x|x|✔|✔|✔|
|58|x|✔|x|✔|✔|✔|
|59|✔|✔|x|✔|✔|✔|
|60|x|x|✔|✔|✔|✔|
|61|✔|x|✔|✔|✔|✔|
|62|x|✔|✔|✔|✔|✔|
|63|✔|✔|✔|✔|✔|✔|

</details>

## 2. Using Arguments
If you don’t want to use bitmasking, so useBitmask = False, you can set each label individually with arguments and setting them to True or False. You can also disable the Set Argument of one to disable it.

| Argumentname |
|-------|
| ccl_DebatedSocialIssuesAndPolitics |
| ccl_DrugsIntoxication |
| ccl_SexualThemes |
| ccl_ViolentGraphic |
| ccl_Gambling |
| ccl_ProfanityVulgarity |
