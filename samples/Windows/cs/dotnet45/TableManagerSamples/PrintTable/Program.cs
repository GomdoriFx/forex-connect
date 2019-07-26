using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Threading;
using System.Configuration;
using fxcore2;

namespace PrintTable
{
    class Program
    {
        static SessionStatusListener statusListener = null;

        static void Main(string[] args)
        {
            O2GSession session = null;

            try
            {
                LoginParams loginParams = new LoginParams(ConfigurationManager.AppSettings);
                SampleParams sampleParams = new SampleParams(ConfigurationManager.AppSettings);

                PrintSampleParams("PrintTable", loginParams, sampleParams);

                session = O2GTransport.createSession();
                session.useTableManager(O2GTableManagerMode.Yes, null);
                statusListener = new SessionStatusListener(session, loginParams.SessionID, loginParams.Pin);
                session.subscribeSessionStatus(statusListener);
                statusListener.Reset();
                session.login(loginParams.Login, loginParams.Password, loginParams.URL, loginParams.Connection);
                if (statusListener.WaitEvents() && statusListener.Connected)
                {
                    O2GTableManager tableManager = session.getTableManager();
                    O2GTableManagerStatus managerStatus = tableManager.getStatus();
                    while (managerStatus == O2GTableManagerStatus.TablesLoading)
                    {
                        Thread.Sleep(50);
                        managerStatus = tableManager.getStatus();
                    }

                    if (managerStatus == O2GTableManagerStatus.TablesLoadFailed)
                    {
                        throw new Exception("Cannot refresh all tables of table manager");
                    }

                    O2GAccountRow account = GetAccount(tableManager);
                    if (account == null)
                        throw new Exception("No valid accounts");

                    O2GResponseType responseType = string.Equals(sampleParams.TableType, SampleParams.OrdersTable) == true ?
                                O2GResponseType.GetOrders : O2GResponseType.GetTrades;

                    if (responseType == O2GResponseType.GetOrders)
                    {
                        PrintOrders(tableManager, account.AccountID);
                    }
                    else
                    {
                        PrintTrades(tableManager, account.AccountID);
                    }

                    Console.WriteLine("Done!");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: {0}", e.ToString());
            }
            finally
            {
                if (session != null)
                {
                    if (statusListener.Connected)
                    {
                        statusListener.Reset();
                        session.logout();
                        statusListener.WaitEvents();
                    }
                    session.unsubscribeSessionStatus(statusListener);
                    session.Dispose();
                }
            }
        }

        /// <summary>
        /// Print accounts and get the first account
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
        private static O2GAccountRow GetAccount(O2GTableManager tableManager)
        {
            O2GAccountsTable accountsTable = (O2GAccountsTable)tableManager.getTable(O2GTableType.Accounts);
            O2GTableIterator accountsIterator = new O2GTableIterator();
            O2GAccountTableRow accountRow = null;
            accountsTable.getNextRow(accountsIterator, out accountRow);
            while (accountRow != null)
            {
                Console.WriteLine("AccountID: {0}, Balance: {1}", accountRow.AccountID, accountRow.Balance);
                accountsTable.getNextRow(accountsIterator, out accountRow);
            }
            return accountsTable.getRow(0);
        }

        // Print orders table using IO2GEachRowListener
        public static void PrintOrders(O2GTableManager tableManager, string sAccountID)
        {
            O2GOrdersTable ordersTable = (O2GOrdersTable)tableManager.getTable(O2GTableType.Orders);
            if (ordersTable.Count == 0)
            {
                Console.WriteLine("Table is empty!");
            }
            else
            {
                ordersTable.forEachRow(new EachRowListener(sAccountID));
            }
        }

        // Print orders table using IO2GEachRowListener
        public static void PrintTrades(O2GTableManager tableManager, string sAccountID)
        {
            O2GTradesTable tradesTable = (O2GTradesTable)tableManager.getTable(O2GTableType.Trades);
            if (tradesTable.Count == 0)
            {
                Console.WriteLine("Table is empty!");
            }
            else
            {
                tradesTable.forEachRow(new EachRowListener(sAccountID));
            }
        }

        /// <summary>
        /// Print process name and sample parameters
        /// </summary>
        /// <param name="procName"></param>
        /// <param name="loginPrm"></param>
        /// <param name="prm"></param>
        private static void PrintSampleParams(string procName, LoginParams loginPrm, SampleParams prm)
        {
            Console.WriteLine("{0} : Table='{1}'", procName, prm.TableType);
        }

        class LoginParams
        {
            public string Login
            {
                get
                {
                    return mLogin;
                }
            }
            private string mLogin;

            public string Password
            {
                get
                {
                    return mPassword;
                }
            }
            private string mPassword;

            public string URL
            {
                get
                {
                    return mURL;
                }
            }
            private string mURL;

            public string Connection
            {
                get
                {
                    return mConnection;
                }
            }
            private string mConnection;

            public string SessionID
            {
                get
                {
                    return mSessionID;
                }
            }
            private string mSessionID;

            public string Pin
            {
                get
                {
                    return mPin;
                }
            }
            private string mPin;

            /// <summary>
            /// ctor
            /// </summary>
            /// <param name="args"></param>
            public LoginParams(NameValueCollection args)
            {
                mLogin = GetRequiredArgument(args, "Login");
                mPassword = GetRequiredArgument(args, "Password");
                mURL = GetRequiredArgument(args, "URL");
                if (!string.IsNullOrEmpty(mURL))
                {
                    if (!mURL.EndsWith("Hosts.jsp", StringComparison.OrdinalIgnoreCase))
                    {
                        mURL += "/Hosts.jsp";
                    }
                }
                mConnection = GetRequiredArgument(args, "Connection");
                mSessionID = args["SessionID"];
                mPin = args["Pin"];
            }

            /// <summary>
            /// Get required argument from configuration file
            /// </summary>
            /// <param name="args">Configuration file key-value collection</param>
            /// <param name="sArgumentName">Argument name (key) from configuration file</param>
            /// <returns>Argument value</returns>
            private string GetRequiredArgument(NameValueCollection args, string sArgumentName)
            {
                string sArgument = args[sArgumentName];
                if (!string.IsNullOrEmpty(sArgument))
                {
                    sArgument = sArgument.Trim();
                }
                if (string.IsNullOrEmpty(sArgument))
                {
                    throw new Exception(string.Format("Please provide {0} in configuration file", sArgumentName));
                }
                return sArgument;
            }
        }
        class SampleParams
        {
            public static readonly string OrdersTable = "orders";
            public static readonly string TradesTable = "trades";

            public string TableType
            {
                get
                {
                    return mTableType;
                }
            }
            private string mTableType;

            /// <summary>
            /// ctor
            /// </summary>
            /// <param name="args"></param>
            public SampleParams(NameValueCollection args)
            {
                mTableType = args["Table"];
                if (string.IsNullOrEmpty(mTableType) ||
                    !mTableType.Equals(OrdersTable) &&
                    !mTableType.Equals(TradesTable))
                {
                    mTableType = TradesTable; // default
                }
            }
        }
    }
}
