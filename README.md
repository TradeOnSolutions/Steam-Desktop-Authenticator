# TradeOnSDA

Multi-platform desktop version of the authenticator, replacing the mobile app steam. Developed by the TradeOn team on a volunteer basis. You can integrate maFile from any SDA version and TradeOnSDA will work with it.
## Disclaimer: We are in no way affiliated with Steam and this is not an official development. We do not provide personal support for TradeOnSDA and everything you see is done on a volunteer basis. Our development is open source and you can make sure that we do not receive any data from your accounts, so we are not responsible for any problems with your accounts that were added. Our application was created for experienced users who understand exactly why they need the desktop version of the authenticator

# [Click to download](https://github.com/TradeOnSolutions/Steam-Desktop-Authenticator/releases/download/release_1_4/TradeOn.SDA.v_1_4.zip)
Now works on Windows, in the future on mac and linux.

![enter image description here](https://sun9-68.userapi.com/impg/LvYPbDvlNLgYQAHeBzy79WT2Ep9cEO3EZFWDPQ/lNxauA6UsgU.jpg?size=807x495&quality=96&sign=dca595a13a43240080942e3c61b5fee7&type=album)
##  Update list:
23.06.2023 - TradeOnSda 1.1 - added auto-confirmations of exchanges and expositions to the trading floor.
##
24.06.2023 - The following features have been fixed
- bugs for load/save maFile
- load proxy
- fix error login steam
- scroll on order page
- improving auto-confirmation
##
28.06.2023 - The following features we added
- Added proxy check. Proxy status is displayed by coloring the icon.
- Added password change confirmation in steam
- Added detailed response to steem requests on re-login
- Added refresh button on confirmation page 
- Added ability to accept/reject all trades
- Added ability to set value in (sec) of manual timing in auto-confirm for each acct (when import account and when right-clicking)
- Now always shown Guard-Code , when you open the application, automatically selected the first account.
- Added highlighting of the account with which you work (exchanges open or selected to show the codes)
- Added work in tray
##
06.07.2023 - The following features we added
- Added ability to bind Guard account
-Fixed Auto-Confirm, more checks, more reliability.
-Added more error messages on all program actions
-Updated bottom bar, added program version and "About Us" button
-Added support for maFiles from SDA


##  How to update?
1. Download the current version at the link below.
2. Unzip the downloaded archive.
3. Move contents of the archive with replacement into the folder with the previous version of TradeOnSda.

## Detailed setup instructions

1. **We recommend that you check your computer with an antivirus beforehand to make sure that you do not have malicious programs that can steal your accounts.**
2. Check the [latest versions page](https://github.com/TradeOnSolutions/Steam-Desktop-Authenticator/releases), and download the latest .zip (not the source code one).
3. Run `TradeOnSda.exe` and click `import account`
   ![enter image description here](https://sun9-79.userapi.com/impg/hLepstRd4cHKVn-IZiYlY7q9kliotiXoZITVrA/8ZbWh-8I7pg.jpg?size=510x451&quality=96&sign=5ba90dfab68cfb3bbdb0a842c516c3f8&type=album)
4. After that, find the miFile files on your computer that you want to add to TradeOnSda (you can select many accounts at once)
5. After adding MaFile - you need to `enter the password` from the added steam account and click `Try login`. You will also be able to specify the HTTP Proxy, through which this account will work (this is not required, you can not specify a proxy)
   You can also immediately enable auto-confirmation of exchanges for the added account (if necessary, you can turn off). The program itself will check every 60 seconds for unconfirmed exchanges or expositions to the trading floor steam, and if something needs to be confirmed - it will automatically confirm.
   ![enter image description here](https://sun9-7.userapi.com/impg/e-yZci2P2_819_WCI61EM8UNWEJa7kTNwNi5kQ/HU1RJoIFX1M.jpg?size=807x593&quality=96&sign=675701da2ce1df5b759fe44307dd0cfc&type=album)
7. **You are great!** Your account has now been added to TradeOnSDA, and you get authentication codes and have the ability to confirm actions in steam.
   ![enter image description here](https://sun9-19.userapi.com/impg/43ilOTK_zYqXpTvXj_8xMhJMH9qZyYgF0VKI0Q/hYGPlwSXkAA.jpg?size=563x854&quality=96&sign=fb88de2ff71bce4e2856a8a244117ace&type=album)


## Functions, features, and use of controls.

1.  TradeOnSda supports proxy integration under each account. You can add a proxy when integrating an account, or you can do it after adding an account by clicking on the "proxy" icon and entering data in the format `IP:PORT:LOG:PASS `(HTTP only)
    ![enter image description here](https://sun9-67.userapi.com/impg/xLxNuuWQsgWxykkVwsvXYSdcMiu5m6xqrfJJew/0GT6yBE_EJ4.jpg?size=807x220&quality=96&sign=6930678d7866b715d1211ad5cc5e68bf&type=album)
2. You can enable or disable auto-confirmation of exchanges and bids on the trading floor by clicking on the "Auto" icon. If the button is green - Auto Confirmations are on. White - off.
   The program itself will check every 60 seconds for unconfirmed exchanges or expositions to the trading floor steam, and if something needs to be confirmed - will confirm it automatically.
   ![enter image description here](https://sun9-38.userapi.com/impg/dTIru05FoU36VWzHbAgCs6cPwfY-68Zozc2RXw/RxTX0q9EPX0.jpg?size=519x316&quality=96&sign=36503d299f80928840c5ed665f8f5f01&type=album)
3.  You can receive authentication codes. In the background of the code you see a moving line that shows the time that the code will still be valid. </br>
    ![enter image description here](https://sun9-12.userapi.com/impg/5NNRhTpk979y6AomTD-DYnlj9VhLvymCZn_AQw/jjUbdkANmas.jpg?size=549x312&quality=96&sign=b698bda314f93542b17ab03348a95858&type=album)
4. You can confirm outgoing exchanges or put items up for sale. To do this, click on the `exchanges` icon and a new window will open with offers waiting to be confirmed.
   ![enter image description here](https://sun9-42.userapi.com/impg/tbr0fk0CbI-u4dpxv2XeDGMhkhVGVfOMDNtPKg/SPscxrzbh8U.jpg?size=807x355&quality=96&sign=5748ad71de0945f42f645b87a53519b2&type=album)
>You can double-click on your account login, this will also open an exchange offer window.
5. TradeOnSDA automatically remembers the login:password you specified. In case you have lost your session - you can click `Re-login` and your session will be renewed automatically without any additional inputs.
   ![enter image description here](https://sun9-53.userapi.com/impg/SDRskio2kx9VPF63y1skPddVbYDs9YqgjmXPow/3gg4CnIgesU.jpg?size=515x338&quality=96&sign=0b1b31e1dd72375d421c10a51a1ea44a&type=album)
6. By right-clicking on the account - you can remove it from TradeOnSda.![enter image description here](https://img2.teletype.in/files/1c/1e/1c1e540b-222d-46e1-97c3-38457c1c4526.png)
7. You can use TradeOnSda on different devices at the same time, using the same maFile.
8. You can stretch the program window, adjusting the display to your own comfort.

## Roadmap for developing and adding new features.

Here you will see our upcoming upgrade plans.
>We have a lot of work on the basic tools of TradeOnBots for autotrading in steam, so we cannot say exactly when new features will be introduced for TradeOnSda (we are doing this free development in our spare time)

1. Mac and Linux versions.
2. Check if the password is correct and notify if it is incorrect (when adding an account, and when re-login)
3. Adding a proxy status indicator.
4. Adding additional checkbox, which allows to enable auto-confirmation of outgoing trades.
5. Adding an additional checkbox that allows you to automatically accept incoming trades in which you give nothing (auto-accept gifts)
6. Adding a detailed window in the exchanges confirmation, which will show all the items involved in the exchange.
7. Ability to add a guard to empty accounts that do not have a guard.

## Additional useful information

1. Be sure to resurrect your `maFile` from the accounts. If you lose your `maFile`, you may lose access to your accounts.
2. If your computer will be infected with a virus - `fraudsters can steal your maFile`, be careful and do not download applications from unverified sources from the Internet.
3. If you lost your `maFiles` OR lost your encryption key, go [here](https://store.steampowered.com/twofactor/manage) and click `Remove Authenticator` then enter your revocation code that you wrote down when you first added your account to SDA.
4. If you did not follow the directions and did not write your revocation code down, you're well and truly screwed. The only option is beg to [Steam Support](https://support.steampowered.com/) and say you lost your mobile authenticator and the revocation code.

## Possible problems and solutions

This branch will continue to grow, depending on users' questions.
