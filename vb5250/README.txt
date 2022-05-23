Revision History:

1.1.1.0
-------
-Added support for 5250 protocol SAVE_PARTIAL_SCREEN and RESTORE_PARTIAL_SCREEN commands.

1.1.0.0
-------
-Fixed failure to draw background color for plain text having a Reverse color attribute.
-Fixed 'StartIndex cannot be less than zero' when evaluating signed numeric field contents.
-Added support for 5250 protocol REMOVE GUI WINDOW and REMOVE ALL GUI CONSTRUCTS commands.
-Improved print screen portrait/landscape handling.
-Fixed focus bug making Emulator form unresponsive.
-Added IBM iSeries LIPI server preauthentication & password changes.

1.0.10.0
-------
-Added user selectable font.

1.0.9.1
-------
-Added configurable codepage/character set/keyboard support.
-Added confidentiality flag to Kerberos ticket request.
-Fixed a drag & drop bug in the Connection Manager tree.
-Added fallback to Lucida Console font on systems where Consolas is not installed (Windows XP).

1.0.8.0
-------
-Fixed Telnet connection and disconnection bugs.  
-Added informative error message translations for some common Kerberos errors.
-Fixed failure to disconnect after receiving an iSeries connection error code.
-Standardized logging for all projects.

1.0.7.0
-Added kerberos support. The hostname you specify must be the FQDN used by the host in its kerberos configuration.

1.0.6.0
-Some work on Telnet connection bugs.

1.0.5.0
-Added code to check for non-null hostname and port before attempting to connect.
-Added a possible workaround for an exception I can't reproduce when reading the .Connected property of a TCPClient.

1.0.4.0
-Changed some connection logic that caused an infinite wait under certain circumstances when connecting with SSL.
-Added password encryption for ini file.
-Emulator form is now displayed prior to attempting connection.

1.0.3.0
-Added SSL support and various minor improvements.
