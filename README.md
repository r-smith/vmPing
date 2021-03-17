vmPing
======
[![Donate to this project using PayPal](https://img.shields.io/badge/Donate-PayPal-green.svg?style=flat)](https://paypal.me/SmithRyn/15)
[![Join the Discord chat](https://img.shields.io/discord/817276291262447697.svg?style=flat)](https://discord.gg/Guf66Zk6US)
[![Total downloads](https://img.shields.io/github/downloads/r-smith/vmping/total)](https://github.com/R-Smith/vmPing/releases)

vmPing (Visual Multi Ping) is a graphical ping utility for monitoring multiple hosts.  Numerous host monitors can be added and removed, and each monitor dynamically resizes with the application window.  Color-coding allows you to tell at a glance the status of each host.  In addition to standard ICMP pings, you can also perform a TCP 'port ping', where the application continuously connects to a specified port and displays whether or not the port is open.  A fast trace route utility and a basic packet generator / stress tester is also included.

**[Click here to download the latest release](https://github.com/R-Smith/vmPing/releases/latest/download/vmPing.exe)**
##### (1.3.15 released on March 17, 2021)

##### Notes
* There is no installer.  Just run the .exe.
* .NET 4.5 or greater is required.


Code Signing Certificate
------------------------
I'm looking to purchase a code signing certificate and would welcome any donations. So far I have received $35.23. I would purchase through https://www.ksoftware.net and is currently priced at $84 for 1-year, $159 for 2-year, and $209 for 3-year (length of certificate is the duration it can be used for signing and signed code remains valid after expiration). This would get rid of the warning Windows gives when opening an unsigned application and also help with antivirus vendors that automatically flag unsigned applications.


Screenshots
-----------
##### Main Window
![Main Window](screenshots/vmPing-MainWindow02.png "Main Window")

##### Options
![Options Window](screenshots/vmPing-Options.gif)

##### Popup Notification
![Popup Notification](screenshots/vmPing-PopupNotification.gif)


Recent Changes
--------------
### Version 1.3.15
* __Startup mode__. You can now configure how vmPing starts. Choose between a blank window (this is the normal, default mode), show the list input window, or load a favorite. For the blank window, you can also set the number of blank probes and initial column count. You will find startup mode settings on the General tab in the Options window. Don't forget to select *Save as vmPing defaults* to make your settings persist.
* When you open the list input window, it now pre-populates with the addresses of the current pings.
* Forced uppercase has been removed from all text boxes.
* Dates and times should now display formatted according to your set region in Windows.


### Version 1.3.14
* __Portable mode.__ Previously, your configuration file had to be stored at %LocalAppData%\vmPing\vmPing.xml. The configuration file includes all favorites, aliases, and default settings for the application. Now vmPing.xml can reside in the same folder as the application, making everything easily portable. Additionally for new users, a simple prompt is displayed if you do anything that would require saving an initial config file. The prompt informs of config file creation, and gives an opportunity to select portable mode if you want your config file created alongside vmPing.exe.
* There's a whole new look to the traceroute window. Check it out. An MTR feature isn't there yet, but should follow in a future release.


### Version 1.3.12
* __New multi-address input.__ Access using the keyboard shortcut F2 or by clicking the option in vmPing's menu. This displays a window with a multi-line textbox. Type your target addresses one per line or comma-separated, click OK, and vmPing populates with your targets and immediately begins pinging. So simple. If you have a text file containing addresses, just drag and drop it onto the window.
* Column count can now be set to anything (within the limits). Previously the column count could never be set higher than the number of probes. This has always annoyed me. Now you can choose any column count and it'll stick. An example would be: Two probes. Set column count to four. vmPing displays two columns since there's only two probes. But as you add more probes, new columns will be made until the four column count is satisfied.
* Improved drag & drop re-ordering of probes. This was a little buggy when first introduced in v1.3.9, but should now perform well. Usage: Hovering your mouse over a probe displays the usual probe title bar. Click and hold in the title bar area, then drag to the desired new location, and release.
* Loading a favorite sets the application title bar to the title of the favorite.
* On the Create New Favorite Set window, you can now drag & drop a text file right onto the list of addresses.
* Minor UI style changes on the Manage Aliases and Manage Favorites window.


### Version 1.3.11
* The recent drag & drop feature was causing issues. Please update to this version if you downloaded 1.3.9-10. The drag & drop functionality is still there, but you must click and drag on the bar that appears across the top of each probe window.


### Version 1.3.10
* If you downloaded version 1.3.9, please replace it with version 1.3.10. The new drag & drop feature introduced a bug that disabled the button toolbar on each probe window. This is fixed in 1.3.10.
* __Drag & drop reordering.__ Click and hold anywhere on a probe, then drag to the desired new location, and release.
* Full control over creating and editing favorite sets. When creating or editing a favorite, you are now given a large multi-line textbox for entering hosts within the favorite. Hosts in the textbox can be one per line, comma-separated, or any combination of new lines and commas.
* Numerous minor UI changes.


### Version 1.3.8
* __New options window.__ The options window was given a minor makeover and now uses a vertical tab layout. This allows room for additional option screens in the future.
* New Notifications tab in the options window for configuring popup notifications. You can now save a default popup notification setting.
* New feature to auto-dismiss popup notifications. This is currently configured through the Notifications tab in the options window.
* New feature to play a sound when a host comes up (previously you could only play sounds when a host went down).
* If you have an alias set for a host, the alias is now shown in popup notifications and email alerts.
* Bug fix: TCP port pings to IPv6 targets now work. If pinging a port on an IPv6 address, wrap the address in brackets, such as *[::1]:443* where *::1* is the target IPv6 address and *443* is the target TCP port.


### Version 1.3.4
* Bug fix: A crash would occur if popup notifications were set to display only when minimized and a host status change occurred.  Thanks @chuckbales for the bug report!


### Version 1.3.3
* Improved handling of loading hosts from a text file to account for empty lines and comments.  Thanks @larntz for adding this feature!
* Added a __Test__ button to the __Email Alerts__ tab so that you can validate and test email alerting.  Thanks @bodagetta for the suggestion.
* Added a __Test__ button to the __Audio Alerts__ tab that plays the audio file you've selected.
* Added simple informational text to the __Email Alerts__ and __Audio Alerts__ tabs.
* Bug fix:  Logging status changes would fail if multiple hosts changed status at the same time.  This has been corrected.  Thanks @Sola1991 for the bug report.
* Bug fix:  When setting a default configuration that includes email alerts with authentication, the config write would fail.  This has been corrected.  Thanks @WNDNCG for the bug report.
* Bug fix:  Aliases would not show when loading hosts from the command line or after selecting a favorite set.  This has been corrected.  Thanks @MeatyFresh for the bug report.
* Bug fix:  Added error checking to audio alert playback.


### Version 1.3.2
* New option to trigger playing a sound when a host goes down.  The setting is found under the Audio Alerts tab on the options window.  Thanks @larntz for adding this feature!
* Bug fix:  Using the log file feature would not work on IPv6 hosts or hosts doing a TCP port ping due to invalid characters in the file name that was being generated.  This is now checked and logging will work in those situations.


### Version 1.3.1
* __New traceroute probe!__  In the hostname box, type __'T/name_or_ip'__ and it'll perform a traceroute right in a probe window.  This feature will become more apparent in a future release, and additional probe types are planned.  Eventually, the old separate traceroute window will go away.
* New option to log status changes to a text file.  This option only writes to the log when a host goes down or up.  The output is in a tab delimited format that is suitable for Excel or database imports.  Note: vmPing does not lock the file, so logging will fail if the file is opened by Excel and vmPing tries to write to the file.
* Fix for vmPing no longer reading hostnames from the command line.
* New command line option to specify a file containing a list of hostnames.  One host per line.  The file is read at startup and vmPing immediately begins pinging each host.  Remember to use quotes if your file path contains spaces.  Usage:  __'vmPing.exe <path_to_file>'__
* Darkened the default color for DNS and traceroute probes.  This is not yet customizable.
* The control bar that appears when you hover over a probe window is now darker (50% black with 50% transparency).  The icons are now darker and the highlight style was changed to accommodate the new colors.
* Re-styled the custom dialog window so it's wider and uses a smaller font.
* Extra tooltips explaining the options under Options -> Log Output.


Features
--------
* Quickly and easily ping multiple hosts.
* Color coding allows you to instantly determine the status of each host.  Green means up.  Red means down.  Orange means error.
* Each host monitor dynamically resizes and scales with the applications window.
* Options to configure the interval between each ping, the timeout, TTL, and message size.
* Monitor TCP ports.  vmPing will continuously connect to a given TCP port and will display whether the port is open or closed.  Enter _HOSTNAME:PORT_ for the hostname.  Example -  _WebserverA:80_
* Option to log everything to a text file.
* Option to log only status changes to a text file.  A status change is when a host you are pinging goes down, or a down host comes back up.
* Popup notifications to alert you of status changes.
* Email notifications on status changes.
* All colors can be customized.
* Favorites.  Easily save a collections of hosts to be recalled instantly at a later time.
* Aliases.  You can assign a friendly display name for any given host.
* Traceroute.  For the hostname, enter _T/HOSTNAME_ to perform a traceroute.
* DNS forward and reverse lookups.  For the hostname, enter _D/NAME_OR_IP_ to perform a DNS lookup.
* Specify any number of hosts from the command line to instantly begin pinging when the application opens.
* Specify a file containing a list of hosts to load and instantly ping when the application launches.
* Command line usage:
  * vmPing.exe [-i interval] [-w timeout] [`<target_host>`...] [`<file_to_load>`...]


Donations
---------
If this tool has been useful to you, consider donating using PayPal.
[![Donate to this project using PayPal](https://img.shields.io/badge/Donate-PayPal-green.svg?style=flat)](https://paypal.me/SmithRyn/15)
