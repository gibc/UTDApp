using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
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

        public static bool localDbInstalled
        {
            get
            {
                bool retVal = true;
                using (SqlConnection sqlCon = new SqlConnection()) 
                {
                    sqlCon.ConnectionString = "Server = (localdb)\\MSSQLLocalDB; Integrated Security = true; Connection Timeout=30";
                    //sqlCon.ConnectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=master;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";

                    try
                    {
                        sqlCon.Open();
                    }
                    catch (Exception ex)
                    {
                        retVal = false;
                        string msg = ex.Message;
                    }
                }
                return retVal;
            }
        }

        public static bool sqlClientInstalled
        {
            get
            {
                bool retVal = true;
                using (SqlConnection sqlCon = new SqlConnection()) 
                {
                    sqlCon.ConnectionString =
                        @"Server = tcp:metric.database.windows.net,1433; Initial Catalog = udtConTest; User ID = udtUser; Password = ConTester567!;  Connection Timeout = 10;";
                    try
                    {
                        sqlCon.Open();
                    }
                    catch (Exception ex)
                    {
                        retVal = false;
                        string msg = ex.Message;
                    }
                }
                return retVal;
            }
        }
    }
}
