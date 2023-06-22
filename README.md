# TradeOnSDA

Multi-platform desktop version of the authenticator, replacing the mobile app steam. Developed by the TradeOn team on a volunteer basis. You can integrate maFile from any SDA version and TradeOnSDA will work with it.
## Disclaimer: We are in no way affiliated with Steam and this is not an official development. We do not provide personal support for TradeOnSDA and everything you see is done on a volunteer basis. Our development is open source and you can make sure that we do not receive any data from your accounts, so we are not responsible for any problems with your accounts that were added. Our application was created for experienced users who understand exactly why they need the desktop version of the authenticator

# [Click to download](https://github.com/TradeOnSolutions/Steam-Desktop-Authenticator/releases/download/release_1_0/TradeOn.SDA.v_1_0.zip) 
Now works on Windows, in the future on mac and linux.


![enter image description here](https://img1.teletype.in/files/cd/90/cd90e028-c746-4b8e-ba50-0112e59667a6.png)

## Detailed setup instructions

 1. **We recommend that you check your computer with an antivirus beforehand to make sure that you do not have malicious programs that can steal your accounts.**
 2. Check the [latest versions page](https://github.com/TradeOnSolutions/Steam-Desktop-Authenticator/releases/latest), and download the latest .zip (not the source code one).
 3. Run `TradeOnSda.exe` and click `import account`<br/>
![enter image description here](https://img2.teletype.in/files/d3/f2/d3f2560b-9d57-4b03-a4da-6b969b2b013d.png)
 4. After that, find the miFile files on your computer that you want to add to TradeOnSda (you can select many accounts at once)
 5. After adding MaFile - you need to `enter the password` from the added steam account and click `Try login`. You will also be able to specify the HTTP Proxy, through which this account will work (this is not required, you can not specify a proxy)![enter image description here](https://img2.teletype.in/files/18/1d/181d5f24-9206-4fed-8fea-ea3645253b2c.png)
 6. **You are great!** Your account has now been added to TradeOnSDA, and you get authentication codes and have the ability to confirm actions in steam.<br/>
![enter image description here](https://img2.teletype.in/files/55/bf/55bffa75-2c27-4aaa-a638-cdefdbb97119.png)


## Functions, features, and use of controls. 

1.  TradeOnSda supports proxy integration under each account. You can add a proxy when integrating an account, or you can do it after adding an account by clicking on the "proxy" icon and entering data in the format `IP:PORT:LOG:PASS `(HTTP only) <br/>
 ![enter image description here](https://img4.teletype.in/files/39/a8/39a87d80-6451-4216-9a95-3f3f22f6fefe.png)![enter image description here](https://img1.teletype.in/files/8e/ba/8eba6611-7e7a-4e3c-8bf6-76079428db1c.png)
		
2. You can receive authentication codes. In the background of the code you see a moving line that shows the time that the code will still be valid.<br/>
![enter image description here](https://img2.teletype.in/files/9d/9e/9d9e6e1d-2ad0-4b34-b59a-98bd273c47f8.png)
3. You can confirm outgoing exchanges or put items up for sale. To do this, click on the `exchanges` icon and a new window will open with offers waiting to be confirmed.<br/> ![enter image description here](https://img2.teletype.in/files/9a/41/9a415365-79d6-4139-816f-ecae3a62f19f.png)
>You can double-click on your account login, this will also open an exchange offer window. 
4. TradeOnSDA automatically remembers the login:password you specified. In case you have lost your session - you can click `Re-login` and your session will be renewed automatically without any additional inputs. ![enter image description here](https://img1.teletype.in/files/03/fb/03fb4a59-2b1b-44dd-9972-372b8bf61dd2.png)
5. By right-clicking on the account - you can remove it from TradeOnSda.<br/> ![enter image description here](https://img2.teletype.in/files/1c/1e/1c1e540b-222d-46e1-97c3-38457c1c4526.png)
6. You can use TradeOnSda on different devices at the same time, using the same maFile.
7. You can stretch the program window, adjusting the display to your own comfort.

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
