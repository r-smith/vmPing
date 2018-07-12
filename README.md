vmPing
============

vmPing (Visual Multi Ping) is a graphical ping utility for monitoring multiple hosts.  Numerous host monitors can be added and removed, and each monitor dynamically resizes with the application window.  Color-coding allows you to tell at a glance the status of each host.  In addition to standard ICMP pings, you can also perform a TCP 'port ping', where the application continuously connects to a specified port and displays whether or not the port is open.  A fast trace route utility and a basic packet generator / stress tester is also included.

###### (Latest version is 1.2.1 released on July 11, 2018)
### [Click here to download the latest .exe](https://github.com/R-Smith/vmPing/releases/download/v1.2.1/vmPing.exe)
### [Click here to download the source](https://github.com/R-Smith/vmPing/archive/master.zip)

##### Notes
* There is no installer.  Just run the .exe.
* .NET 4.0 or greater is required.

[![Join the chat at https://gitter.im/vmPing/Lobby](https://badges.gitter.im/vmPing/Lobby.svg)](https://gitter.im/vmPing/Lobby?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

### Changes for v1.2.1
* New option for aliases - Display a custom title for each ping window.  To use: Hover over an active ping monitor to display the icon row.  Click the pencil icon and enter an alias.
* TODO: Persistent aliases - Save aliases to disk so they are remembered after closing the application. Include the ability to view/recall/edit saved aliases.
* TODO: Alternate compact view.
* TODO: Alternate historical up/down view (similar to the popup notification window).


### Changes for v1.2.0
* New isolated views - Open any ping monitor in it's own dedicated window.  To access: Hover over an active ping monitor to display the icon row.  The icon row currently consists of an 'X' (close) button and a 'rectangle' (isolated view) button.


### Changes for v1.1.15
* Added SMTP authentication options for email alerts (Contributed by @Nirad)
* Latency for TCP 'pings' are now displayed.  The value, in milliseconds, is the total time taken to establish and close a TCP connection on the given port.
* Bug fix for an issue introduced in v1.1.14 that would cause the application to hang when an invalid hostname was entered.


### Older changes
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
