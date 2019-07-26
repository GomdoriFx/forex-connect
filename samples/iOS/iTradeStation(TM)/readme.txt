iTradeStation(TM)


Brief
===============================================================================
This sample application shows how to display trading tables and create an order.

The application functions as follows:
1) The application prompts the user to enter the login parameters: Login, Password, Connection, Host and the trading session ID.
2) The user enters the login parameters and taps "Connect".
3) After the successful login, the application shows a list of available Tables: Offers, Orders, Trades, and Accounts.
4) The user taps a table.
5) The application shows the details for that table.
6) The user taps the Offers table.
7) The application shows a list of available Symbols with their Bid and Ask prices.
8) The user taps a symbol.
9) The application prompts the user to enter the order parameters for that symbol: Sell/Buy, Amount, Rate, and TrueMarket/EntryStop/EntryLimit.
10) The user enters the order parameters and taps "OK".
11) The application creates the order for that symbol.


Building and running the application
===============================================================================
With ForexConnect API installed,

1. Open the ForexConnectAPI_1.6.0-iOS directory in Finder, then open the iOS sample
and double-click the .xcodeproj file to open the project in Xcode application.

2. Click the "Build and Run button" to build and execute the sample application
in the simulator.


Building the iOS sample using Xcode 10.*
===============================================================================
To build the iOS sample using Xcode 10.* you should perform the following steps:

- Open the iOS sample project via Xcode 10.*.
- On the File menu, click Project Settings, and the click Build System.
- In the Build system list from Per-User Project Settings section, select Legacy Build System and click Done.
- Build project.


See readme.txt for more information about ForexConnect API.