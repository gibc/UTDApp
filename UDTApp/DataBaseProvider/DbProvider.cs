using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using UDTApp.Models;
using UDTApp.Settings;

// map to slqlite or sql sever ado classes
namespace UDTApp.DataBaseProvider
{
    public enum DBType { sqlExpress, sqlLite, none}
    public class DbProvider
    {
        public DbProvider(DBType _dbType, string serverName, string userId = "", string password = "")
        {
            dbType = _dbType;

            if (dbType == DBType.none)
            {
                dbType = DBType.sqlExpress;
                string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string dataFolder = path + "\\UdtApp";
                if (Directory.Exists(dataFolder))
                {
                    var fl = Directory.GetFiles(dataFolder, string.Format("{0}.db", DbName)).ToList();
                    if (fl != null && fl.Count > 0)
                    {
                        dbType = DBType.sqlLite;
                    }
                }
            }

            if (dbType == DBType.sqlExpress && string.IsNullOrEmpty(serverName))
            {
                //Server = (localdb)\\MSSQLLocalDB; Integrated Security = true; Connection Timeout = 30
                //ConnectionString = "Data Source=.\\SQLEXPRESS; Integrated Security=True";
                ConnectionString = "Server = (localdb)\\MSSQLLocalDB; Integrated Security = true; Connection Timeout = 30";
            }

            if (dbType == DBType.sqlExpress && !string.IsNullOrEmpty(serverName))
            {
                ServerSetting svr = AppSettings.appSettings.getServer(serverName);
                if(svr == null)
                {
                    if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(password))
                    {
                        svr = new ServerSetting() { serverName = serverName, pwd = password, userId = userId };
                    }
                    else
                    {
                        MessageBox.Show("DbProvider: login not saved in settings");
                        return;
                    }
                }

                //Server = tcp:metric.database.windows.net,1433; Initial Catalog = MetricDB; Persist Security Info = False; User ID = { your_username }; Password ={ your_password}; MultipleActiveResultSets = False; Encrypt = True; TrustServerCertificate = False; Connection Timeout = 30;
                string remoteConSrg =
                    string.Format("Server = {0}; Initial Catalog = Master; Persist Security Info = False; User ID = {1}; Password = {2}; MultipleActiveResultSets = False; Encrypt = True; TrustServerCertificate = False; Connection Timeout = 30;", 
                    svr.serverName, svr.userId, svr.pwd);
                ConnectionString = remoteConSrg;
            }

        }

        public DbConnection Conection 
        {
            get 
            {
                if (dbType == DBType.sqlLite)
                    return new SQLiteConnection();
                else
                    return new SqlConnection();
            }
        }

        public DbCommand Command
        {
            get
            {
                if (dbType == DBType.sqlLite)
                    return new SQLiteCommand();
                else
                    return new SqlCommand();
            }
        }

        public DbCommand GetCommand(string sqltxt = "")
        {
            if (dbType == DBType.sqlLite)
                return new SQLiteCommand(sqltxt);
            else
                return new SqlCommand(sqltxt);
        }

        private SqlDataReader reader = null;
        private SQLiteDataReader liteReader = null;
        public DbDataReader Reader
        {
            get
            {
                if (dbType == DBType.sqlLite)
                    return liteReader;
                else
                    return reader;
            }
        }

        public string DbName
        {
            get
            {
                if (UDTXml.UDTXmlData.SchemaData == null) return "";
                UDTData udtData = UDTXml.UDTXmlData.SchemaData[0] as UDTData;
                return udtData.Name;
            }
        }

        //private string remoteConnectionString
        //{
        //    get
        //    {
        //        if (conStrings == null) return "";
        //        if(!string.IsNullOrEmpty(initialCatalog))
        //        {
        //            conStrings.Add(initialCatalog);
        //        }
        //        string retString = string.Join("; ", conStrings);
        //        if (!string.IsNullOrEmpty(initialCatalog))
        //        {
        //            conStrings.Remove(initialCatalog);
        //        }
        //        return retString;
        //    }
        //    set
        //    {
        //        conStrings = value.Split(';').Where(x => !string.IsNullOrWhiteSpace(x)).Select(p => p.Trim()).ToList();
        //        string catalog = conStrings.FirstOrDefault(p => p.ToUpper().Contains("CATALOG"));
        //        if (catalog != null)
        //        {
        //            //initialCatalog = catalog;
        //            conStrings.Remove(catalog);
        //        }
        //    }
        //}

        private string initialCatalog
        {
            get
            {
                return string.Format("Initial Catalog = {0};", DbName);
            }
        }

        public string MasterCatalogConnnectionString
        {
            get
            {
                string conStr = "";
                if (dbType == DBType.sqlLite)
                    conStr = sqlLiteConnectionString;
                else
                {
                    if (conStrings == null) return "";
                    string masterCat = string.Format("Initial Catalog = {0};", "MASTER");
                    conStrings.Add(masterCat);               
                    conStr = string.Join("; ", conStrings);
                    conStrings.Remove(masterCat);
                }
                return conStr;
            }
        }

        private List<string> conStrings;
        public string ConnectionString
        {
            set
            {
                conStrings = value.Split(';').Where(x => !string.IsNullOrWhiteSpace(x)).Select(p => p.Trim()).ToList();
                string catalog = conStrings.FirstOrDefault(p => p.ToUpper().Contains("CATALOG"));
                if (catalog != null)
                {
                    conStrings.Remove(catalog);
                }
            }
            get
            {
                string conStr = "";
                if (dbType == DBType.sqlLite)
                {
                    return sqlLiteConnectionString;
                }
                else if (dbType == DBType.sqlExpress)
                {
                    if (conStrings == null) return "";
                    if (!string.IsNullOrEmpty(initialCatalog))
                    {
                        conStrings.Add(initialCatalog);
                    }
                    conStr = string.Join("; ", conStrings);
                    if (!string.IsNullOrEmpty(initialCatalog))
                    {
                        conStrings.Remove(initialCatalog);
                    }
                }
                return conStr;
            }
        }

        private string sqlLiteConnectionString
        {
            get
            {
                string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string dataFolder = path + "\\UdtApp";
                if (!Directory.Exists(dataFolder))
                    Directory.CreateDirectory(dataFolder);

                return string.Format("Data Source={0}\\{1}.db;Version=3;datetimeformat=CurrentCulture;",
                    dataFolder, DbName);
            }
        }


        //public string adjSQL(string sqltxt)
        //{
        //    if (dbType == DBType.sqlLite )
        //    {
        //        if(sqltxt.ToUpper().Contains("USE"))
        //        {
        //            int off = sqltxt.IndexOf(']');
        //            if (off > 0)
        //                sqltxt = sqltxt.Substring(off + 1).Trim();
        //        }
        //    }
        //    return sqltxt;
        //}

        public DBType dbType = DBType.sqlExpress;

    }
}
