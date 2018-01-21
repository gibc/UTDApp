using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using UDTApp.Views;

namespace UDTApp.ViewModels
{
    public class LoginViewModel : ValidatableBindableBase
    {
        public DelegateCommand OkCommand { get; set; }
        public DelegateCommand CancelCommand { get; set; }
        public DelegateCommand TestLoginCommand { get; set; }
        public DelegateCommand<Window> WindowLoadedCommand { get; set; }

        public LoginViewModel()
        {
            OkCommand = new DelegateCommand(okCmd, canOk);
            CancelCommand = new DelegateCommand(cancelCmd);
            TestLoginCommand = new DelegateCommand(testConnection, canTestConnection);
            WindowLoadedCommand = new DelegateCommand<Window>(winLoaded);
        }

        private string _sqlServerUrl = "";
        public string sqlServerUrl
        {
            get { return _sqlServerUrl; }
            set
            {
                SetProperty(ref _sqlServerUrl, value);
                TestLoginCommand.RaiseCanExecuteChanged();
            }
        }

        private string _sqlUser = "";
        public string sqlUser
        {
            get { return _sqlUser; }
            set
            {
                SetProperty(ref _sqlUser, value);
                TestLoginCommand.RaiseCanExecuteChanged();
            }
        }

        public string sqlPassword
        {
            get { return passwordBox.Password; }
        }

        private PasswordBox passwordBox;
        private LoginView loginView { get; set; }
        private void winLoaded(Window window)
        {
            loginView = window as LoginView;
            //loadCountTxtBlock = newPrjView.FindName("loadCount") as TextBlock;
            passwordBox = loginView.FindName("pwdBox") as PasswordBox;
            passwordBox.PasswordChanged += PasswordBox_PasswordChanged;
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            TestLoginCommand.RaiseCanExecuteChanged();
        }

        private void okCmd()
        {
            loginView.DialogResult = true;
            loginView.Close();
        }

        private bool canOk()
        {
            return loginTestOk;
        }

        private void cancelCmd()
        {
            loginView.DialogResult = false;
            loginView.Close();
        }

        private bool _loginTestOk = false;
        private bool loginTestOk
        {
            get { return _loginTestOk; }
            set
            {
                _loginTestOk = value;
                OkCommand.RaiseCanExecuteChanged();
            }
        }

        private void testConnection()
        {
            string remoteConStr = "";
 
            if (!string.IsNullOrEmpty(sqlServerUrl) && !string.IsNullOrEmpty(sqlUser)
                && !string.IsNullOrEmpty(sqlPassword))
            {
                remoteConStr = string.Format(
               "Server = {0}; Initial Catalog = Master; Persist Security Info = False; User ID = {1}; Password = {2}; Connection Timeout = 10;",
                    sqlServerUrl, sqlUser, sqlPassword);
            }

            using (SqlConnection sqlCon = new SqlConnection())
            {
                sqlCon.ConnectionString = remoteConStr;
                try
                {
                    sqlCon.Open();
                    loginTestOk = true;
                    MessageBox.Show("Successfully connected to remote server!", "Connection Test",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch
                {
                    loginTestOk = false;
                    MessageBox.Show("Connection to remote server failed!", "Connection Test",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool canTestConnection()
        {
            if (!string.IsNullOrEmpty(sqlServerUrl) && !string.IsNullOrEmpty(sqlUser)
                && !string.IsNullOrEmpty(sqlPassword)) return true;
            return false;
        }
    }
}
