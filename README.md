# IpWatcherService
## Service that monitors the external IP address and sends an alert to the mail if it has changed. (Plus daily notice.)

INSTALLATION
------------
You need the InstallUtil.exe utility, located in the folder C:\Windows\Microsoft.NET\Framework\[version] or C:\Windows\Microsoft.NET\Framework64\[version].

Type in the command line to install: InstallUtil.exe [Disk and folders where your exe file is located]\IpWatcherService.exe

Type in the command line to uninstall: InstallUtil.exe /u [Disk and folders where your exe file is located]\IpWatcherService.exe

REQUIREMENTS
------------
Microsoft .NET Framework 2.0 or higher - must be installed on your system.

QUICK START
-----------
Start "IpWatcherService" in the Windows service.

The file "config.txt" will appear in the folder with "IpWatcherService.exe" file.

Enter the correct data in this file: email address, email parameters and etc according template.

DISPATCH_TIMER - This parameter will set the time of daily notification to the email address.

Restart "IpWatcherService" in the Windows service.

All actions will be written to a templog.txt file.
