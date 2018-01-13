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
using System.Windows.Forms;
using UDTApp.DataBaseProvider;
using UDTApp.Models;
using UDTApp.Settings;

namespace UDTApp.ViewModels
{
    public class NewProjectViewModel : ValidatableBindableBase
    {
        public DelegateCommand OkCommand { get; set; }
        public DelegateCommand CancelCommand { get; set; }
        public DelegateCommand<Window> WindowLoadedCommand { get; set; }
        public DelegateCommand SqliteCommand { get; set; }
        public DelegateCommand SqlServerCommand { get; set; }

        public NewProjectViewModel()
        {
            OkCommand = new DelegateCommand(okCmd, canOk);
            CancelCommand = new DelegateCommand(cancelCmd);
            SqliteCommand = new DelegateCommand(sqliteCmd);
            SqlServerCommand = new DelegateCommand(sqlServerCmd);
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

        private void okCmd()
        {
            dbType = DBType.sqlExpress;
            if (sqlServerDb)
                dbType = DBType.sqlExpress;
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

            using (SqlConnection sqlCon = new SqlConnection())
            {
                string conStr = @"Server = tcp:metric.database.windows.net,1433; Initial Catalog = MetricDB; 
                                Persist Security Info = False; User ID = gcard; 
                                Password = dbpassme789!; MultipleActiveResultSets = False; Encrypt = True; 
                                TrustServerCertificate = False; Connection Timeout = 30;";
                sqlCon.ConnectionString = conStr;
                try
                {
                    sqlCon.Open();
                    connectionString = conStr;
                }
                catch (Exception ex)
                {
                    string msg = ex.Message;
                }
            }

            sqliteDb = !sqlServerDb;
        }

        private Window newPrjView { get; set; }
        private Action closeAction { get; set; }
        private void winLoaded(Window window)
        {
            closeAction = new Action(window.Close);
            newPrjView = window;
            ProjectName = "";
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
