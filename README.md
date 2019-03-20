vmPing
============

vmPing (Visual Multi Ping) is a graphical ping utility for monitoring multiple hosts.  Numerous host monitors can be added and removed, and each monitor dynamically resizes with the application window.  Color-coding allows you to tell at a glance the status of each host.  In addition to standard ICMP pings, you can also perform a TCP 'port ping', where the application continuously connects to a specified port and displays whether or not the port is open.  A fast trace route utility and a basic packet generator / stress tester is also included.

###### (Latest version is 1.2.14 released on March 19, 2019)
### [Click here to download the latest .exe](https://github.com/R-Smith/vmPing/releases/download/v1.2.14/vmPing.exe)
### [Click here to download the source](https://github.com/R-Smith/vmPing/archive/master.zip)

##### Notes
* There is no installer.  Just run the .exe.
* .NET 4.0 or greater is required.

[![Join the chat at https://gitter.im/vmPing/Lobby](https://badges.gitter.im/vmPing/Lobby.svg)](https://gitter.im/vmPing/Lobby?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)


### Changes for v1.2.14
* Added the ability to customize the colors.  Most colors within that application can now be set to your liking.  To access: open the application options and then select the Layout tab.
* TODO: Add a script trigger option on host down / up.
* TODO: Add context menu to host monitor windows.
* TODO: Alternate compact view.


### Changes for v1.2.12
* Added advanced ICMP options:
  * Set a custom time to live value.
  * Option to set / unset the "don't fragment" flag in the packet.
  * Set ICMP message size.
  * Set custom ICMP message data.


### Changes for v1.2.11
* It's finally here!  Your vmPing options can now be saved to disk, so your settings will be remembered each time you open the application.
* TCP port ping intervals can now go below 4 seconds and is tied to your ping interval setting.
* If you downloaded v1.2.10, this releases fixes improper writing of SMTP credentials to the config file.


### Changes for v1.2.9
* You can now rename existing aliases and create new aliases directly from the alias management window.
* You can now rename favorites from the favorites management window.
* When you select a favorite from the favorites management window, it now displays the hosts that are in the selected entry.
* When saving a new favorite set, the dialog window now displays the hosts that will be saved in your set.
* When saving a new favorite set, if you haven't entered any hosts names, you will now get an error.


### Changes for v1.2.8
* Re-designed the options window, dialog popups, and the alias/favorites management windows.  Everything has a more consistent look.  No more fixed and unmovable dialog windows.  Email alerts and logging options are now found in the options window.
* When renaming an alias, if you set a blank name for the title, it deletes the alias entry rather than creating a blank alias.
* Bug fix: A crash would occur if you started a ping and then stopped it before any probes were sent - such as during DNS resolution. (Thanks @ichantio)


### Changes for v1.2.7
* The keyboard focus feature that was supposedly added to the previous version is now working.


### Changes for v1.2.6
* Aliases can now be managed from the main drop down menu.
* Keyboard focus now automatically shifts each time you add a new host monitor window.


### Older changes
* Aliases are now persistent and are saved to your local vmPing configuration file.
* New configuration file - %LocalAppData%\vmPing\vmPing.xml - Old configuration files are automatically upgraded to the new format.  The new configuration file supports favorites, aliases, and any other settings that would need to be saved in the future.
* After stopping a ping, basic statistics are added to the output window.  This is similar to command line ping utilities.
* There's a new button on the popup notification window that'll take you to the full status history window.
* If the status change history window is open, the popup notification window won't appear.  It didn't make sense to have both.
* New window for viewing a history of status changes (when a host goes down or up again) - To access: Click the arrow/menu icon in the top right and select 'Change Log'.
* New option for aliases - Display a custom title for each ping window.  To use: Hover over an active ping monitor to display the icon row.  Click the pencil icon and enter an alias.
* New isolated views - Open any ping monitor in it's own dedicated window.  To access: Hover over an active ping monitor to display the icon row.  The icon row currently consists of an 'X' (close) button and a 'rectangle' (isolated view) button.
* Added SMTP authentication options for email alerts (Contributed by @Nirad)
* Latency for TCP 'pings' are now displayed.  The value, in milliseconds, is the total time taken to establish and close a TCP connection on the given port.
* When pinging a hostname and no response is received, the resolved IP address is now printed at the top of the output.
* Visual change: There is no longer a gap between each host monitor.  They are now separated only by a 2px border.
* Visual change: When a ping is in progress, a glyph appears next to the hostname indicating whether the host is up, down, or indeterminate.
* Added an alerts threshold option.  Popup notifications and email alerts are not triggered until the specified number of consecutive pings are lost.  This helps avoid notifications for occasional lost packets.  The setting is configured through the options window (F10 or select from the drop-down menu).  The default value is two.
* Additional error handling and improved reporting of errors.
* The options window (F10) is now displayed as its own separate window.
* The help window (F1) is now displayed as its own separate window.
* Fixed parsing of command line arguments.  The application wasn't properly parsing values supplied by **-i** and **-w** on the command line.
* The interval between TCP port pings is now tied to the global probe interval setting.  Previously, the probe interval was only being applied to ICMP pings.  The minimum interval for TCP probes is currently locked at four seconds, but any setting greater than that will be applied.
* Popup notifications no longer steal focus.
* Click a popup notification to bring the main vmPing window into focus.
* Fixed an issue where after selecting a favorite, the application would continue to ping old hostnames that were active prior to selecting the favorite.
* Fixed an issue where traceroute would sometimes return 'Invalid hostname' for IP addresses.
* Fixed an issue where 'Stop / Start All' would not properly update after closing the last active ping.
* TCP port pings now timeout after 3 seconds rather than 20 seconds.
* Fixed an issue with TCP port pings where unreachable ports would trigger multiple notifications.
* Minor visual changes to main window.
* Fix menu bar cutoff issue on Windows 7.
* Visual improvements to popup notifications.
* Popup notifications now default to on.
* Added popup notifications.  When a host changes status (from up to down or from down to up), a notification will appear in the lower right corner of your screen.  By default, this is only enabled when the application is minimized.  You can use the main drop down menu and select Popup Notifications to change when the notification appears:  Always, When Minimized, or Never.


Features
========
* Quickly and easily ping multiple hosts.
* Color coding allows you to instantly determine the status of each host.  Green means up.  Red means down.  Orange means error.
* Each host monitor dynamically resizes with the application's window.  Take up your whole screen or reduce everything to a tiny window.
* Probe options to configure the interval between each ping request (anything from 1 second to several hours) and how many seconds until it is considered timed out.
* Monitor TCP ports.  vmPing can continously connect to a given TCP port and will display whether the port is open or closed.  Enter HOSTNAME:PORT for the hostname.  For example, WebserverA:80
* Option to log everything to a text file.
* Option to send email alerts.  Each time a host goes up or down, you will receive an email.
* Favorites system.  Easily save and recall sets of hosts at any time.
* FAST built-in trace route utility.
* A simple built-in packet generator / stress tester for flooding a specific host.
* Command line usage:
  * vmPing [-i interval] [-w timeout] [`<target host>`...]

Screenshots
===========
##### vmPing 1
![vmPing 1](https://github.com/R-Smith/supporting-docs/raw/master/vmPing/vmping01.png?raw=true "vmPing 1")

##### vmPing 2
![vmPing 2](https://github.com/R-Smith/supporting-docs/raw/master/vmPing/vmping02.png?raw=true "vmPing 2")

##### vmPing 3
![vmPing 3](https://github.com/R-Smith/supporting-docs/raw/master/vmPing/vmping03.png?raw=true "vmPing 3")

##### Trace Route
![Trace Route](https://github.com/R-Smith/supporting-docs/raw/master/vmPing/vmping04.png?raw=true "Trace Route")

##### Flood Host
![Flood Host](https://github.com/R-Smith/supporting-docs/raw/master/vmPing/vmping05.png?raw=true "Flood Host")

##### Popup Notification
![Popup Notification](https://github.com/R-Smith/supporting-docs/raw/master/vmPing/vmping06.png?raw=true "Popup Notification")
