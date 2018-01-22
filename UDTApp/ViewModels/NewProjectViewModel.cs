using Prism.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using UDTApp.DataBaseProvider;
using UDTApp.Models;
using System.Windows.Controls;
using System.Windows.Media;
using UDTApp.BlobStorage;
using System.Data.Common;
using System.Linq;

namespace UDTApp.ViewModels
{
    public class NewProjectViewModel : ValidatableBindableBase
    {
        public DelegateCommand OkCommand { get; set; }
        public DelegateCommand CancelCommand { get; set; }
        public DelegateCommand<Window> WindowLoadedCommand { get; set; }

        public NewProjectViewModel()
        {
            OkCommand = new DelegateCommand(okCmd, canOk);
            CancelCommand = new DelegateCommand(cancelCmd);
            WindowLoadedCommand = new DelegateCommand<Window>(winLoaded);
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

        public DBType dbType
        {
            get;
            set;
        }

        private string _sqlServerUrl = "";
        public string sqlServerUrl
        {
            get { return _sqlServerUrl; }
            set
            {
                SetProperty(ref _sqlServerUrl, value);
            }
        }

        private string _sqlUser = "";
        public string sqlUser
        {
            get { return _sqlUser; }
            set
            {
                SetProperty(ref _sqlUser, value);
            }
        }

        public string sqlPassword
        {
            get { return passwordBox.Password; }
            set { passwordBox.Password = value; }
        }

        private string _sqlConnString = "";
        public string sqlConnString
        {
            get { return _sqlConnString; }
            set
            {
                SetProperty(ref _sqlConnString, value);
            }
        }

        //public string connectionString
        //{
        //    get;
        //    set;
        //}

        private List<string> currentDBs
        {
            get;
            set;
        }

        private void okCmd()
        {
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

        //    Server = tcp:metric.database.windows.net,1433; 
        //    User ID = gcard;  
        //    Password = dbpassme789!; 

        private Window newPrjView { get; set; }
        private Action closeAction { get; set; }
        private void winLoaded(Window window)
        {
            closeAction = new Action(window.Close);
            newPrjView = window;
            ProjectName = "";
            currentDBs = getDbList(dbType, sqlServerUrl, sqlUser, sqlPassword);
        }

        private PasswordBox passwordBox = new PasswordBox();

        public List<string> getDbList(DBType dbType, string serverName, string userId, string password)
        {
            List<string> dbList = new List<string>();
            if (dbType == DBType.sqlLite)
            {
                string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string dataFolder = path + "\\UdtApp";
                if (Directory.Exists(dataFolder))
                {
                    Directory.GetFiles(dataFolder, "*.db").ToList().
                        ForEach(p => dbList.Add(Path.GetFileNameWithoutExtension(p)));
                }
            }
            else if (dbType == DBType.sqlExpress)
            {
                DbProvider dbProvider = null;
                if (string.IsNullOrEmpty(serverName))
                    dbProvider = new DbProvider(dbType, "");
                else
                    dbProvider = new DbProvider(dbType, serverName, userId, password);

                using (DbConnection conn = dbProvider.Conection)
                {
                    conn.ConnectionString = dbProvider.MasterCatalogConnnectionString;
                    DbCommand cmd = dbProvider.GetCommand(
                        "select * from sys.databases");

                    cmd.Connection = conn;
                    DbDataReader reader = dbProvider.Reader;
                    conn.Open();
                    try
                    {
                        reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            dbList.Add((string)reader["name"]);
                        }
                    }
                    catch (Exception ex)
                    {
                        string msg = string.Format("getDbList failed: {0}", ex.Message);
                        MessageBox.Show(msg);
                        UDTApp.Log.Log.LogMessage("msg");
                    }
                    finally
                    {
                        reader.Close();
                    }
                }
            }
            return dbList;
        }


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
