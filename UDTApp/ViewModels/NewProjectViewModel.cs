using Prism.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Sql;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
//using System.Windows.Forms;
using UDTApp.DataBaseProvider;
using UDTApp.Models;
using UDTApp.Settings;
using Microsoft.WindowsAzure; // Namespace for CloudConfigurationManager
using Microsoft.WindowsAzure.Storage; // Namespace for CloudStorageAccount
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.Azure;
using System.Configuration;
using System.Threading;
using System.Windows.Threading;
using System.Windows.Controls;
using System.Windows.Media;

namespace UDTApp.ViewModels
{
    public class NewProjectViewModel : ValidatableBindableBase
    {
        public DelegateCommand OkCommand { get; set; }
        public DelegateCommand CancelCommand { get; set; }
        public DelegateCommand<Window> WindowLoadedCommand { get; set; }
        public DelegateCommand SqliteCommand { get; set; }
        public DelegateCommand SqlServerCommand { get; set; }
        public DelegateCommand LocalCommand { get; set; }
        public DelegateCommand RemoteCommand { get; set; }

        public NewProjectViewModel()
        {
            OkCommand = new DelegateCommand(okCmd, canOk);
            CancelCommand = new DelegateCommand(cancelCmd);
            SqliteCommand = new DelegateCommand(sqliteCmd);
            SqlServerCommand = new DelegateCommand(sqlServerCmd);
            LocalCommand = new DelegateCommand(localCmd);
            RemoteCommand = new DelegateCommand(remoteCmd);
            WindowLoadedCommand = new DelegateCommand<Window>(winLoaded);
            currentDBs = UDTDataSet.udtDataSet.getDbList();
            dbType = DBType.sqlLite;
        }

        private string _projectName = null;
        [Required(ErrorMessage = "Project Name is required.")]
        [StringLength(15, MinimumLength = 5, ErrorMessage = "Project Name must be between 5 and 15 characters.")]
        [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "Project Name can include only letter characters")]
        [CustomValidation(typeof(NewProjectViewModel), "CheckDuplicateName")]
        [CustomValidation(typeof(NewProjectViewModel), "CheckSqlWord")]
        public string ProjectName
        {
            get { return _projectName; }
            set
            {
                SetProperty(ref _projectName, value);
                OkCommand.RaiseCanExecuteChanged();
            }
        }

        private bool _sqliteDb = true;
        public bool sqliteDb
        {
            get { return _sqliteDb; }
            set { SetProperty(ref _sqliteDb, value); }
        }

        private bool _sqlServerDb = false;
        public bool sqlServerDb
        {
            get { return _sqlServerDb; }
            set { SetProperty(ref _sqlServerDb, value); }
        }

        private bool _localDb = true;
        public bool localDb
        {
            get { return _localDb; }
            set { SetProperty(ref _localDb, value); }
        }

        private bool _remoteDb = false;
        public bool remoteDb
        {
            get { return _remoteDb; }
            set { SetProperty(ref _remoteDb, value); }
        }

        private Visibility _conStrVisible = Visibility.Collapsed;
        public Visibility conStrVisible
        {
            get { return _conStrVisible; }
            set { SetProperty(ref _conStrVisible, value); }
        }

        public DBType dbType
        {
            get;
            set;
        }

        public string connectionString
        {
            get;
            set;
        }

        private List<string> currentDBs
        {
            get;
            set;
        }

        private async Task<bool> installLocalDb()
        {
            string cons = @"DefaultEndpointsProtocol=https;AccountName=udtapp;AccountKey=bdQ/RekPpoYC0RPCtYhOYM4A8mo5Wy3j/0gF4TuuaKrOwTY7rBzL5jGIJsMuprULG9iF4l0UqJKLmaQrhPy8jQ==;";
            cons = cons.Trim();
            string blobCon = ConfigurationManager.AppSettings["StorageConnectionString"];
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(cons);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve a reference to a container.
            CloudBlobContainer container = blobClient.GetContainerReference("install-downloads");

            // Create the container if it doesn't already exist.
            container.CreateIfNotExists();

            // Retrieve reference to a blob named "local-db-msi".
            CloudBlockBlob blockBlob = container.GetBlockBlobReference("local-db-msi");

            // Create or overwrite the "local-db-msi" blob with contents from a local file.
            //using (var fileStream = System.IO.File.OpenRead(@"C:\GibPCStuff\UDTApp\TestApp\SqlLocalDB.msi"))
            //{
            //    blockBlob.UploadFromStream(fileStream);
            //    Stream blobStream = blockBlob.OpenRead();
            //}

            blockBlob.FetchAttributes();
            long size = blockBlob.Properties.Length;
            bool done = false;
            downloadMsiFile(blockBlob, () => done = true);

            progressMsgVisable = Visibility.Visible;
            while (!done)
            {
                if(loadCountTxtBlock.Foreground == Brushes.DarkBlue)
                    loadCountTxtBlock.Foreground = Brushes.DarkGreen;
                else
                    loadCountTxtBlock.Foreground = Brushes.DarkBlue;

                await Task.Delay(1000);
                loadCountTxtBlock.Text = String.Format("{0:n0} of {1:n0} bytes", downloadCount, size);
                loadCountTxtBlock.UpdateLayout();
            }
            progressMsgVisable = Visibility.Collapsed;
            if (downloadCount < 0) return false;


            return true;
        }

        int downloadCount = 0;

        private void downloadMsiFile(CloudBlockBlob blockBlob, Action callback)
        {
            new Thread(() =>
            {
                try
                {
                    Thread.CurrentThread.IsBackground = true;
                    int count = 50000;
                    byte[] buff = new byte[count];
                    string tempFolder = Path.GetTempPath();
                    tempFolder += "udtsetup";
                    if (!Directory.Exists(tempFolder))
                        Directory.CreateDirectory(tempFolder);
                    Directory.CreateDirectory(tempFolder);
                    string msiFile = tempFolder + "\\SqlLocalDB.msi";
                    using (var fileStream = System.IO.File.OpenWrite(msiFile))
                    {
                        using (Stream blobStream = blockBlob.OpenRead())
                        {
                            while (count > 0)
                            {
                                count = blobStream.Read(buff, 0, 50000);
                                fileStream.Write(buff, 0, count);
                                downloadCount += count;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format("Insatll package download failed: {0}", ex.Message), 
                        "Download Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    downloadCount = -1;
                }
                finally
                {
                    callback();
                }
            }).Start();
        }

        private string _downLoadCountStr = "";      
        public string downLoadCountStr
        {
            get { return _downLoadCountStr; }
            set { SetProperty(ref _downLoadCountStr, value); }
        }

        private Visibility _progressMsgVisable = Visibility.Collapsed;
        public Visibility progressMsgVisable
        {
            get { return _progressMsgVisable; }
            set { SetProperty(ref _progressMsgVisable, value); }
        }

        private bool testRemoteConn()
        {
            return false;
        }

        private void okCmd()
        {
            dbType = DBType.sqlLite;
            if (sqlServerDb && remoteDb)
            {
                if (!testRemoteConn()) return;
            }
            if (sqlServerDb && !remoteDb)
            { 
                if(!localDbInstalled)
                {
                    if(MessageBox.Show(@"The Sql Server local database component is not insalled. Download and install the local database server?", "Install Required.", 
                            MessageBoxButton.OKCancel, MessageBoxImage.Information) == MessageBoxResult.OK)
                    {
                        //if (!installLocalDb()) return;
                        installLocalDb(); return;
                    }
                    else return;
                }
                dbType = DBType.sqlExpress;
                connectionString =
                    "Server = (localdb)\\MSSQLLocalDB; Integrated Security = true;";
            }

            newPrjView.DialogResult = true;
            closeAction();
        }

        private bool canOk()
        {
            return !HasErrors;
        }

        private void cancelCmd()
        {
            newPrjView.DialogResult = false;
            closeAction();
        }

        private void sqliteCmd()
        {
            sqlServerDb = !sqliteDb;
        }

        private void localCmd()
        {
            remoteDb = !localDb;
            if (localDb)
                conStrVisible = Visibility.Collapsed;
            else
                conStrVisible = Visibility.Visible;
        }

        private void remoteCmd()
        {
            localDb = !remoteDb;
            if (remoteDb)
                conStrVisible = Visibility.Visible;
            else
                conStrVisible = Visibility.Collapsed;
        }


        private void sqlServerCmd()
        {
            // TBD: chech if sql server installed
            //SqlDataSourceEnumerator instance =
            //        SqlDataSourceEnumerator.Instance;
            //System.Data.DataTable table = instance.GetDataSources();
            //foreach (System.Data.DataRow row in table.Rows)
            //{
            //    foreach (System.Data.DataColumn col in table.Columns)
            //    {
            //        string svrInfo = string.Format("{0} = {1}", col.ColumnName, row[col]);
            //    }
            //}

            //SqlConnection sqlCon = new SqlConnection();
            //sqlCon.ConnectionString = "Server = (localdb)\\MSSQLLocalDB; Integrated Security = true";
            ////sqlCon.ConnectionString = "Data Source=.\\SQLEXPRESS;Integrated Security=True";
            //try
            //{ 
            //    sqlCon.Open();
            //}
            //catch(Exception ex)
            //{
            //    string msg = ex.Message;
            //}

            //using (SqlConnection sqlCon = new SqlConnection())
            //{
            //    string folder = Environment.ExpandEnvironmentVariables("%systemroot%") + "\\system32\\";
            //    string[] sysFiles = Directory.GetFiles(folder, "sqlncli*");
            //    string conStr = @"Server = tcp:metric.database.windows.net,1433; Initial Catalog = MetricDB; 
            //                    Persist Security Info = False; User ID = gcard; 
            //                    Password = dbpassme789!; MultipleActiveResultSets = False; Encrypt = True; 
            //                    TrustServerCertificate = False; Connection Timeout = 30;";
            //    sqlCon.ConnectionString = conStr;
            //    try
            //    {
            //        sqlCon.Open();
            //        connectionString = conStr;
            //    }
            //    catch (Exception ex)
            //    {
            //        string msg = ex.Message;
            //    }
            //}

            sqliteDb = !sqlServerDb;
        }

        private bool localDbInstalled
        {
            get
            {
                return false;
                bool retVal = true;
                using (SqlConnection sqlCon = new SqlConnection())
                {
                    sqlCon.ConnectionString = "Server = (localdb)\\MSSQLLocalDB; Integrated Security = true; Connection Timeout=5";
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

        private Window newPrjView { get; set; }
        private Action closeAction { get; set; }
        private void winLoaded(Window window)
        {
            closeAction = new Action(window.Close);
            newPrjView = window;
            loadCountTxtBlock = newPrjView.FindName("loadCount") as TextBlock;
            ProjectName = "";
        }

        private TextBlock loadCountTxtBlock;

        public static System.ComponentModel.DataAnnotations.ValidationResult CheckDuplicateName(string name, ValidationContext context)
        {
            NewProjectViewModel dataObj = context.ObjectInstance as NewProjectViewModel;
            if (dataObj.currentDBs != null)
            {
                if (dataObj.currentDBs.Contains(dataObj.ProjectName))
                {
                    return new System.ComponentModel.DataAnnotations.ValidationResult("A project already exits with this name. Select another name.");
                }
            }
            return System.ComponentModel.DataAnnotations.ValidationResult.Success;
        }
        public static System.ComponentModel.DataAnnotations.ValidationResult CheckSqlWord(string name, ValidationContext context)
        {
            NewProjectViewModel dataObj = context.ObjectInstance as NewProjectViewModel;
            if (dataObj != null)
            {
                if (UDTBase.sqlWordList.Contains(name.ToUpper()))
                {
                    return new System.ComponentModel.DataAnnotations.ValidationResult("Error: Reserved SQL key word.");
                }
            }

            return System.ComponentModel.DataAnnotations.ValidationResult.Success;

        }

    }
}
