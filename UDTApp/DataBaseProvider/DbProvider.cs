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
using UDTApp.Models;

// map to slqlite or sql sever ado classes
namespace UDTApp.DataBaseProvider
{
    public enum DBType { sqlExpress, sqlLite, none}
    public class DbProvider
    {
        public DbProvider(DBType _dbType, string remoteConString)
        {
            dbType = _dbType;
            remoteConnectionString = remoteConString;
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

        private string _DbName;
        public string DbName
        {
            get
            {
                UDTData udtData = UDTXml.UDTXmlData.SchemaData[0] as UDTData;
                return udtData.Name;
            }
        }

        private List<string> conStrings;
        private string remoteConnectionString
        {
            get
            {
                if (conStrings == null) return "";
                if(!string.IsNullOrEmpty(initialCatalog))
                {
                    conStrings.Add(initialCatalog);
                }
                string retString = string.Join("; ", conStrings);
                if (!string.IsNullOrEmpty(initialCatalog))
                {
                    conStrings.Remove(initialCatalog);
                }
                return retString;
            }
            set
            {
                conStrings = value.Split(';').Where(x => !string.IsNullOrWhiteSpace(x)).Select(p => p.Trim()).ToList();
                string catalog = conStrings.FirstOrDefault(p => p.Contains("Catalog"));
                if (catalog != null)
                {
                    initialCatalog = catalog;
                    conStrings.Remove(catalog);
                }
            }
        }

        public string initialCatalog
        {
            get;
            set;
        }

        public string ConnectionString
        {
            get
            {
                string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string dataFolder = path + "\\UdtApp";
                if (!Directory.Exists(dataFolder))
                    Directory.CreateDirectory(dataFolder);
                if (dbType == DBType.sqlLite)
                {
                    string conStr = string.Format("Data Source={0}\\{1}.db;Version=3;datetimeformat=CurrentCulture;",
                        dataFolder, DbName);
                    return conStr;
                }
                else if (dbType == DBType.sqlExpress)
                {
                    if (!string.IsNullOrEmpty(remoteConnectionString)) return remoteConnectionString;
                    return ConfigurationManager.ConnectionStrings["conString"].ConnectionString;
                }
                return "";
            }
        }

        public string adjSQL(string sqltxt)
        {
            if (dbType == DBType.sqlLite || !string.IsNullOrEmpty(remoteConnectionString))
            {
                if(sqltxt.ToUpper().Contains("USE"))
                {
                    int off = sqltxt.IndexOf(']');
                    if (off > 0)
                        sqltxt = sqltxt.Substring(off + 1).Trim();
                }
            }
            return sqltxt;
        }

        public DBType dbType = DBType.sqlExpress;

    }
}
