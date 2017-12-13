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

namespace UDTApp.DataBaseProvider
{
    public enum DBType { sqlExpress, sqlLite}
    public class DbProvider
    {
        public DbProvider(DBType _dbType)
        {
            dbType = _dbType;
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
                    //return string.Format("Data Source=c:\\GibPCStuff\\UDTApp\\TestApp\\{0}.db;Version=3;",
                    //    DbName);
                    string conStr = string.Format("Data Source={0}\\{1}.db;Version=3;datetimeformat=CurrentCulture;",
                        dataFolder, DbName);
                    return conStr;
                }
                else
                    return ConfigurationManager.ConnectionStrings["conString"].ConnectionString;
            }
        }

        public string adjSQL(string sqltxt)
        {
            if (dbType == DBType.sqlLite)
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
