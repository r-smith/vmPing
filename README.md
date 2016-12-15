vmPing
============

vmPing (Visual Multi Ping) is a graphical ping utility for monitoring multiple hosts.  Numerous host monitors can be added and removed, and each monitor dynamically resizes with the application window.  Color-coding allows you to tell at a glance the status of each host.  In addition to standard ICMP pings, you can also perform a TCP 'port ping', where the application continuously connects to a specified port and displays whether or not the port is open.  A fast trace route utility and a basic packet generator / stress tester is also included.

###### (Latest version is 1.1.4 released on December 14, 2016)
### [Click here to download the latest .exe](https://github.com/R-Smith/vmPing/releases/download/v1.1.4/vmPing.exe)
### [Click here to download the source](https://github.com/R-Smith/vmPing/archive/master.zip)

##### Notes
* There is no installer.  Just run the .exe.
* .NET 4.0 or greater is required.


Features
========
* Quickly and easily ping multiple hosts.
* Color coding allows you to instantly determine the status of each host.  Green means up.  Red means down.  Orange means error.
* Each host monitor dynamically resizes with the application's window.  Take up your whole screen or reduce everything to a tiny window.
* Probe options to configure the interval between each ping request (anything from 1 second to several hours) and how many seconds until it is considered timed out.
* Monitor TCP ports.  vmPing can continously connect to a given TCP port and will display whether the port is open or closed.  Enter HOSTNAME:PORT for the hostname.  For example, WebserverA:80
* Option to log everything to a text file.
* Option to send email alerts.  Each time a host goes up or down, you will receive an email.
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
