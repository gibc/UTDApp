﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using UDTApp.ViewModels;
using UDTApp.Views;

namespace UDTApp.SetUp
{
    public class Install
    {
        public static bool checkInstallLocalDb()
        {
            if (localDbInstalled) return true;

            if (MessageBox.Show(
                @"The Sql Server local database component is not insalled. Download and install the local database server?", "Install Required.",
                MessageBoxButton.OKCancel, MessageBoxImage.Information) == MessageBoxResult.OK)
            {
                InstallView installView = new InstallView();
                InstallViewModel vm = installView.DataContext as InstallViewModel;
                vm.fileName = "SqlLocalDB.msi";
                vm.blobName = "local-db-msi";
                if(!installView.ShowDialog().Value)
                {
                    MessageBox.Show("Installation failed or cancled.  Installation failed.", "Install Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
                if (!localDbInstalled)
                {
                    MessageBox.Show("Cannot connect to local database server.  Installation failed.", "Install Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                MessageBox.Show("Local database server sucessfully installed.  Creating new Sql Server local database project.", "Install Complete", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            return true;
        }

        public static bool checkInstallSqlClient()
        {
            if (sqlClientInstalled) return true;

            if (MessageBox.Show(
                    @"The Sql Server client component is not insalled. Download and install the sql server client?", "Install Required.",
                MessageBoxButton.OKCancel, MessageBoxImage.Information) == MessageBoxResult.OK)
            {
                InstallView installView = new InstallView();
                InstallViewModel vm = installView.DataContext as InstallViewModel;
                string msiName = "sqlncli32.msi";
                if (Environment.Is64BitOperatingSystem)
                {
                    msiName = "sqlncli64.msi";
                }
                vm.fileName = msiName;
                vm.blobName = msiName;

                if (!installView.ShowDialog().Value)
                {
                    MessageBox.Show("Installation failed or cancled.  Installation failed.", "Install Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
                if (!localDbInstalled)
                {
                    MessageBox.Show("Cannot connect to local database server.  Installation failed.", "Install Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                MessageBox.Show("Local database server sucessfully installed.  Creating new Sql Server local database project.", "Install Complete", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            return true;
        }

        private static bool _localDbInstalled = false;
        public static bool localDbInstalled
        {
            get
            {
                if (_localDbInstalled) return true;

                bool retVal = true;
                using (SqlConnection sqlCon = new SqlConnection()) 
                {
                    sqlCon.ConnectionString = "Server = (localdb)\\MSSQLLocalDB; Integrated Security = true; Connection Timeout=30";

                    try
                    {
                        waitWin = new PleaseWaitView();
                        waitWin.Show();
                        sqlCon.Open();
                        _localDbInstalled = true;
                    }
                    catch (Exception ex)
                    {
                        retVal = false;
                        string msg = ex.Message;
                    }
                    finally
                    {
                        Thread.Sleep(1000);
                        waitWin.Close();
                        waitWin = null;
                    }
                }
                return retVal;
            }
        }

        static private PleaseWaitView waitWin = null;

        private static bool _sqlClientInstalled = false;
        public static bool sqlClientInstalled
        {
            get
            {
                if (_sqlClientInstalled) return true;

                bool retVal = true;
                using (SqlConnection sqlCon = new SqlConnection()) 
                {

                    sqlCon.ConnectionString =
                        @"Server = den1.mssql1.gear.host; User ID = testcon; Password = Dr14?_8DpG3u;  Connection Timeout = 30;";
                    try
                    {
                        waitWin = new PleaseWaitView();
                        waitWin.Show();
                        sqlCon.Open();
                        _sqlClientInstalled = true;
                    }
                    catch (Exception ex)
                    {
                        retVal = false;
                        string msg = ex.Message;
                    }
                    finally
                    {
                        Thread.Sleep(1000);
                        waitWin.Close();
                        waitWin = null;
                    }
                }
                return retVal;
            }
        }
    }
}
