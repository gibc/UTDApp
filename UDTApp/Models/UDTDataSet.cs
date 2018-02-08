using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using UDTApp.ViewModels;
using UDTAppControlLibrary.Controls;
using ADODB;
using UDTApp.DataBaseProvider;
using System.Data.Common;
using System.IO;
using UDTApp.ListManager;

namespace UDTApp.Models
{
    public class UDTDataSet : ValidatableBindableBase
    {
        public UDTDataSet()
        {

        }

        private static DbProvider _dbProvider = null;
        public static DbProvider dbProvider
        {
            get
            {
                if (_dbProvider == null)
                {
                    //_dbProvider = new DbProvider(DBType.sqlExpress);
                    _dbProvider = new DbProvider(DBType.sqlLite, "");
                }
                return _dbProvider;
            }

            set
            {
                _dbProvider = value;
            }
        }

        private static UDTDataSet _udtDataSet = null;
        public static UDTDataSet  udtDataSet
        {
            get
            {
                if (_udtDataSet == null)
                {
                    _udtDataSet = new UDTDataSet();
                }
                return _udtDataSet;
            }
        }

        public System.Data.DataSet DataSet
        {
            get;
            set;
        }

        private bool _isModified = false;        
        public bool IsModified
        {
            get { return _isModified; }
            set
            {
                bool isChanged = (value != _isModified);
                SetProperty(ref _isModified, value);
                if (dataChangeEvent != null && isChanged) dataChangeEvent();
            }
        }

        
        public void createDatabase(UDTData masterItem)
        {
            if(masterItem.dbType != DBType.none)
            {
                UDTDataSet.dbProvider = new DbProvider(masterItem.dbType, masterItem.serverName);
            }
            createSQLDatabase(masterItem.Name);
            //List<Guid> tableGuids = new List<Guid>();
            foreach(UDTData table in masterItem.tableData)
            {
                createDBTable(table, masterItem.Name /*, tableGuids*/);
            }
        }

        public bool dataBaseExists(DBType dbType, string dbName)
        {
            try
            {
                string dataFolder = "";
                if (dbType == DBType.sqlLite)
                {
                    string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                    dataFolder = path + "\\UdtApp";
                    if (Directory.Exists(dataFolder))
                    {
                        string filePath = string.Format("{0}\\{1}.db",
                            dataFolder, dbName);
                        if (File.Exists(filePath))
                            return true;
                    }
                    return false;
                }

                UDTData master = UDTXml.UDTXmlData.SchemaData[0] as UDTData;
                if (!string.IsNullOrEmpty(master.serverName)) return true;

                string dbPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                dataFolder = dbPath + "\\UdtLocalDb";
                if (Directory.Exists(dataFolder))
                {
                    string filePath = string.Format("{0}\\{1}.mdf",
                        dataFolder, dbName);
                    if (File.Exists(filePath))
                        return true;
                }

                return false;
            }
            catch
            {
                return false;
            }

        }

        public void deleteSQLDatabase(string dbName)
        {
            try
            {
                if (UDTDataSet.dbProvider.dbType == DBType.sqlLite)
                {
                    string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                    string dataFolder = path + "\\UdtApp";
                    if (Directory.Exists(dataFolder))
                    {
                        string filePath = string.Format("{0}\\{1}.db",
                            dataFolder, dbName);
                        if (File.Exists(filePath))
                            File.Delete(filePath);
                    }
                }
                else
                {
                    UDTData master = UDTXml.UDTXmlData.SchemaData[0] as UDTData;
                    if (!string.IsNullOrEmpty(master.serverName)) return;  // ignore request to delete remove db

                    string sqlTxt =
                        string.Format("ALTER DATABASE {0} SET SINGLE_USER WITH ROLLBACK IMMEDIATE", dbName);
                    if (!executeQuery(sqlTxt, true)) return;

                    sqlTxt = string.Format(@"DROP DATABASE {0}", dbName);
                    if (!executeQuery(sqlTxt, true)) return;
                }
            }
            catch (Exception ex)
            {
                string msg = string.Format("deleteSQLDatabase failed: {0}", ex.Message);
                UDTApp.Log.Log.LogMessage("msg");
                MessageBox.Show(msg);
            }
        }


        private void createSQLDatabase(string DBName)
        {

            //using (SqlConnection conn = new SqlConnection())
            if (UDTDataSet.dbProvider.dbType == DBType.sqlLite) return;

            // for remote database to check if exists just try to connect so connection to 
            //      master DB not required (only connections to existing DBs allowed)

            using (DbConnection conn = UDTDataSet.dbProvider.Conection)
            {
                // if this is a remote db or if the database already exits we should be
                // able to connect to it but long time out on local DB
                // query on master is much faster
                UDTData mastr = UDTXml.UDTXmlData.SchemaData[0] as UDTData;
                if (!string.IsNullOrEmpty(mastr.serverName))
                {
                    conn.ConnectionString = UDTDataSet.dbProvider.ConnectionString;
                    try
                    {
                        conn.Open();
                        return;
                    }
                    catch
                    {
                        conn.Close();
                        throw new Exception("UDTDataSet::createSQLDatabase failed: Cannot connect to remote sql server database.");
                    }
                }
            
                // this must be database on localDb so get folder for mdf files
                string dbPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string dataFolder = dbPath + "\\UdtLocalDb";
                if (!Directory.Exists(dataFolder))
                {
                    Directory.CreateDirectory(dbPath);
                }

                conn.ConnectionString = UDTDataSet.dbProvider.MasterCatalogConnnectionString;
                DbCommand cmd = UDTDataSet.dbProvider.GetCommand(
                    string.Format("select count(*) from (select * from sys.databases where name = '{0}') rows", DBName)
                    );

                // query masted DB to see if database exits
                cmd.Connection = conn;
                conn.Open();
                try
                {
                    int dbCount = (int)cmd.ExecuteScalar();
                    if (dbCount < 1)
                    {
                        // if database does not exits, create in specified folder
                        cmd.CommandTimeout = 300; //create database foo on(name= 'foo', filename= 'c:\DBs\foo.mdf')
                        cmd.CommandText = string.Format("create database {1} on(name= '{1}', filename= '{0}')", 
                            string.Format("{0}\\{1}.mdf", dbPath, DBName), 
                            DBName); 
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                }
            }
        }

        public bool TableExists(string tableName, string dbName)
        {

            string sqlTxt = string.Format(@"select count(*) from 
                (SELECT * FROM {0}.dbo.sysobjects WHERE xtype = 'U' AND name = '{1}') rows",
                dbName, tableName);

            //select (select count() from XXX) as count
            if(UDTDataSet.dbProvider.dbType == DBType.sqlLite)
                sqlTxt = string.Format(@"SELECT name FROM sqlite_master WHERE type='table' AND name='{0}'",
                    tableName);

            bool retVal = true;
            //using (SqlConnection conn = new SqlConnection())
            using (DbConnection conn = UDTDataSet.dbProvider.Conection)
            {

                //conn.ConnectionString = ConfigurationManager.ConnectionStrings["conString"].ConnectionString;
                conn.ConnectionString = UDTDataSet.dbProvider.ConnectionString;

                //SqlCommand cmd = new SqlCommand(sqlTxt);
                DbCommand cmd = UDTDataSet.dbProvider.GetCommand(sqlTxt);

                cmd.Connection = conn;
                conn.Open();
                try
                {
                    if(UDTDataSet.dbProvider.dbType == DBType.sqlLite)
                    {
                        DbDataReader reader = UDTDataSet.dbProvider.Reader;
                        reader = cmd.ExecuteReader();
                        retVal = reader.HasRows;
                        reader.Close();
                    }
                    else
                    { 
                        int dbCount = (int)cmd.ExecuteScalar();
                        retVal = (dbCount >= 1);
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    conn.Close();
                }
            }
            return retVal;
        }

        private void createDBTable(UDTData dataItem, string dbName /*, List<Guid> tableGuids*/)
        {
            //if (dataItem.savTableData != null)
            //{
            //    foreach (UDTData item in dataItem.savTableData)
            //    {
            //        var c = dataItem.tableData.FirstOrDefault(p => p.Name == item.Name);
            //        if(c == null)
            //        {
            //            c = dataItem.tableData.FirstOrDefault(p => p.savName == item.Name);
            //            if(c == null)
            //            {
            //                // table has been deleted
            //                //delteDBTable(item, dbName, tableGuids);
            //            }
            //        }
            //    }
            //}

            if(dataItem.savTableData != null)
            {
                foreach(UDTData item in dataItem.savTableData)
                {
                    //if(item.isTableDeleted)
                    if(!TableDictionary.itemDic.ContainsKey(item.objId))
                    {
                        if (!isTableEmpty(dataItem, dbName))
                        {
                            MessageBox.Show("Error: Attempt to delete non-empty table", "Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                        dropTable(dataItem.Name, dbName);
                    }
                }
            }

            foreach (UDTData item in dataItem.tableData)
            {
                createDBTable(item, dbName/*, tableGuids*/);
            }

            //if (tableGuids.Contains(dataItem.objId)) return;


            if (!TableExists(dataItem.Name, dbName))
            {
                if (!string.IsNullOrEmpty(dataItem.savName) && TableExists(dataItem.savName, dbName))
                {
                    // table has a new name
                    string sqlTxt = string.Format("ALTER TABLE {1} RENAME TO {2}",
                        dbName, dataItem.savName, dataItem.Name);
                    if (!executeQuery(sqlTxt)) return;
                    dataItem.savName = dataItem.Name;  // remove table name mod from schema
                }
                else
                {
                    //tableGuids.Add(dataItem.objId);
                    createNewTable(dataItem, dataItem.Name, dbName);
                    return;

                    //string ddl;
                    //// create new table
                    //using (DbConnection conn = UDTDataSet.dbProvider.Conection)
                    //{
                    //    //ddl = UDTDataSet.dbProvider.adjSQL(string.Format("USE [{0}] CREATE TABLE {1} (", dbName, dataItem.Name));
                    //    ddl = string.Format("CREATE TABLE {0} (", dataItem.Name);
                    //    ddl += string.Format("[Id] [uniqueidentifier] NOT NULL, ");
                    //    foreach (UDTBase item in dataItem.columnData)
                    //    {
                    //        ddl += string.Format("{0} {1}, ", item.Name, item.Type);
                    //    }
                    //    foreach (string colName in dataItem.ParentColumnNames)
                    //    {
                    //        ddl += string.Format("{0} [uniqueidentifier], ", colName);
                    //    }
                    //    ddl = ddl.Substring(0, ddl.Length - 2);
                    //    ddl += "); ";

                    //    tableGuids.Add(dataItem.objId);

                    //    conn.ConnectionString = UDTDataSet.dbProvider.ConnectionString;
                    //    DbCommand cmd = UDTDataSet.dbProvider.GetCommand(ddl);


                    //    cmd.Connection = conn;
                    //    conn.Open();
                    //    try
                    //    {
                    //        cmd.ExecuteNonQuery();
                    //    }
                    //    catch (Exception ex)
                    //    {
                    //        MessageBox.Show(ex.Message);
                    //    }
                    //}
                    //return;
                }
            }

            // if table exits check for added, deleted, renamed columns
            if(dataItem.savColumnData != null)
            {
                if (!dataItem.isTableSchemsModified) return; 

                // if we have no data then just drop and recreate table with
                // all column mods
                if (isTableEmpty(dataItem, dbName))
                {
                    if (!dropTable(dataItem.Name, dbName)) return;

                    createNewTable(dataItem, dataItem.Name, dbName);
                    return;
                }

                // if we have data check for deleted columns
                if(dataItem.isColumnDeleted)
                {
                    
                    List<UDTBase> deleted = dataItem.savColumnData.ToList().FindAll
                        (p => dataItem.columnData.FirstOrDefault(a => a.savName == p.Name) == null);

                    foreach(UDTBase item in deleted)
                    {
                        if (!isColumnEmpty(dataItem.Name, item.Name, dbName))
                        {
                            MessageBox.Show("Error: Attempt to delete non-empty column",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }
                }

                if (!RenameColumns(dataItem, dbName)) return;

              }
        }

        private List<string> getColNameList(UDTData dataItem, bool savedCols = false)
        {
            List<string> colNameList = new List<string>();
            colNameList.Add("Id");

            // if col is presnet in current list then not deleted 
            // so this gets undeleted colums by new name
            // or by old name and not new columns
            foreach(UDTBase col in dataItem.columnData)
            {
                // skip new columns
                var c = dataItem.savColumnData.FirstOrDefault(p => p.Name == col.Name);
                if (c == null)
                    continue;

                if (savedCols)
                    colNameList.Add(col.savName);
                else
                    colNameList.Add(col.Name);
            }

            // add parent column names if they are not new
            foreach (string colName in dataItem.ParentColumnNames)
            {
                if(dataItem.savParentColumnNames.FirstOrDefault(p => p == colName) == null)
                    continue;

                colNameList.Add(string.Format("{0}", colName));
            }

            return colNameList;
        }

        private string getColSql(UDTData dataItem, bool savedCols = false)
        {
            string sqlTxt = "";
            List<string> colNameList = getColNameList(dataItem, savedCols);
            foreach(string colName in colNameList)
            {
                sqlTxt += colName;
                if (colName != colNameList.Last())
                    sqlTxt += ", ";
            }
            return sqlTxt;
        }

        public bool isColumnEmpty(string tableName, string columnName, string dbName)
        {
            bool retVal = true;
            //if (isTableEmpty(tableName, dbName)) return true;

            string sqlTxt = string.Format("SELECT {0} from {1} where {0} <> '' AND {0} IS NOT NULL", columnName, tableName);
            using (DbConnection conn = UDTDataSet.dbProvider.Conection)
            {
                conn.ConnectionString = UDTDataSet.dbProvider.ConnectionString;
                DbCommand cmd = UDTDataSet.dbProvider.GetCommand(sqlTxt);
                cmd.Connection = conn;
                conn.Open();
                try
                {
                    if (UDTDataSet.dbProvider.dbType == DBType.sqlLite)
                    {
                        DbDataReader reader = UDTDataSet.dbProvider.Reader;
                        reader = cmd.ExecuteReader();
                        retVal = !reader.HasRows;
                        reader.Close();
                    }
                    else
                    {
                        DbDataReader reader = UDTDataSet.dbProvider.Reader;
                        reader = cmd.ExecuteReader();
                        retVal = !reader.HasRows;
                        reader.Close();
                    }

                }
                catch (Exception ex)
                {
                    //MessageBox.Show(ex.Message);
                    retVal = true;
                }
                finally
                {
                    conn.Close();
                }
            }
            return retVal;

        }

        public bool isTableEmpty(UDTData dataItem, string dbName)
        {
            bool retVal = true;
            string sqlTxt = string.Format("SELECT ");
            foreach (UDTBase item in dataItem.columnData)
            {
                if (item.Name == "Id") continue;
                if (item.Name != item.savName) continue;
                sqlTxt += string.Format("{0}", item.Name);
                //if (item != dataItem.columnData.Last())
                    sqlTxt += ", ";
            }
            int off = sqlTxt.LastIndexOf(',');
            sqlTxt = sqlTxt.Substring(0, off);

            sqlTxt += string.Format(" FROM {0} WHERE ", dataItem.Name);
            foreach (UDTBase item in dataItem.columnData)
            {
                if (item.Name == "Id") continue;
                if (item.Name != item.savName) continue;
                sqlTxt += string.Format("{0} <> '' AND {0} IS NOT NULL", item.Name);
                //if (item != dataItem.columnData.Last())
                    sqlTxt += " OR ";
            }
            off = sqlTxt.LastIndexOf("OR");
            sqlTxt = sqlTxt.Substring(0, off);


            using (DbConnection conn = UDTDataSet.dbProvider.Conection)
            {
                conn.ConnectionString = UDTDataSet.dbProvider.ConnectionString;
                DbCommand cmd = UDTDataSet.dbProvider.GetCommand(sqlTxt);
                cmd.Connection = conn;
                conn.Open();
                try
                {
                    if (UDTDataSet.dbProvider.dbType == DBType.sqlLite)
                    {
                        DbDataReader reader = UDTDataSet.dbProvider.Reader;
                        reader = cmd.ExecuteReader();
                        retVal = !reader.HasRows;
                        reader.Close();
                    }
                    else
                    {
                        DbDataReader reader = UDTDataSet.dbProvider.Reader;
                        reader = cmd.ExecuteReader();
                        retVal = !reader.HasRows;
                        reader.Close();
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    conn.Close();
                }
            }
            return retVal;
        }

        private string unChangedColDefs(UDTData dataItem, List<string> unChangedColNames)
        {
            string colDef = "";
            foreach(string colName in unChangedColNames)
            {
                UDTBase item = dataItem.columnData.FirstOrDefault(p => p.Name == colName);
                if (item != null)
                {
                    if (!string.IsNullOrEmpty(colDef))
                        colDef += ", ";
                    colDef += string.Format("{0} {1}", item.Name, item.Type);
                }
            }
            colDef += string.Format("[Id] [uniqueidentifier] NOT NULL");
            return colDef;
        }

        private bool createNewTable(UDTData dataItem, string tableName, string dbName)
        {
            using (DbConnection conn = UDTDataSet.dbProvider.Conection)
            {
                //string ddl = UDTDataSet.dbProvider.adjSQL(string.Format("USE [{0}] CREATE TABLE {1} (", dbName, dataItem.Name));
                string ddl = string.Format("CREATE TABLE {1} (", dbName, dataItem.Name);
                ddl += string.Format("[Id] [uniqueidentifier] NOT NULL, ");
                foreach (UDTBase item in dataItem.columnData)
                {
                    ddl += string.Format("{0} {1}, ", item.Name, item.Type);
                }
                foreach (string colName in dataItem.ParentColumnNames)
                {
                    ddl += string.Format("{0} [uniqueidentifier], ", colName);
                }
                ddl = ddl.Substring(0, ddl.Length - 2);
                ddl += "); ";

                return executeQuery(ddl);
            }
        }

        //public bool dropColumn(string tableName, string colName)
        //{
        //    string sqlTxt = string.Format("ALTER TABLE {0} DROP COLUMN {1};",
        //        tableName, colName);
        //    return executeQuery(sqlTxt);
        //}

        public bool dropTable(string tableName, string dbName)
        {
            string sqlTxt =
                string.Format("DROP TABLE {0}", tableName);
            return executeQuery(sqlTxt);
        }

        private bool executeQuery(string sqlTxt, bool useMasterDb = false)
        {
            using (DbConnection conn = UDTDataSet.dbProvider.Conection)
            {
                if (useMasterDb)
                    conn.ConnectionString = UDTDataSet.dbProvider.MasterCatalogConnnectionString;
                else
                    conn.ConnectionString = UDTDataSet.dbProvider.ConnectionString;

                DbCommand cmd = UDTDataSet.dbProvider.GetCommand(sqlTxt);

                cmd.Connection = conn;
                conn.Open();
                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                { 
                    string msg = string.Format(@"executeQuery failed. Query: {0} Error: {1}", sqlTxt, ex.Message);
                    UDTApp.Log.Log.LogMessage("msg");
                    MessageBox.Show(msg);
                    return false;
                }
            }
            return true;
        }

        private List<string> GetColumns(string table, string dbName)
        {

            //string sqlTxt = UDTDataSet.dbProvider.adjSQL(string.Format(@"USE [{0}] SELECT * FROM INFORMATION_SCHEMA.COLUMNS
            //        WHERE TABLE_NAME = '{1}'
            //        ORDER BY ORDINAL_POSITION", dbName, table));
            string sqlTxt = string.Format(@"SELECT * FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_NAME = '{0}'
                    ORDER BY ORDINAL_POSITION", table);

            if (UDTDataSet.dbProvider.dbType == DBType.sqlLite)
                sqlTxt = string.Format("PRAGMA table_info({0})", table);

            List<string> colList = new List<string>();
            //using (SqlConnection conn = new SqlConnection())
            using (DbConnection conn = UDTDataSet.dbProvider.Conection)
            {

                //conn.ConnectionString = ConfigurationManager.ConnectionStrings["conString"].ConnectionString;
                conn.ConnectionString = UDTDataSet.dbProvider.ConnectionString;

                //SqlCommand cmd = new SqlCommand(sqlTxt);
                DbCommand cmd = UDTDataSet.dbProvider.GetCommand(sqlTxt);


                //SqlDataReader reader;
                DbDataReader reader = UDTDataSet.dbProvider.Reader;

                cmd.Connection = conn;
                conn.Open();
                reader = cmd.ExecuteReader();

                try
                {
                    while (reader.Read())
                    {
                        if (UDTDataSet.dbProvider.dbType == DBType.sqlLite)
                            colList.Add(reader.GetString(1));
                        else
                            colList.Add((string)reader["COLUMN_NAME"]);
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    reader.Close();
                }
            }
            return colList;
        }

        private bool RenameColumns(UDTData table, string dbName)
        {
            //First rename the old table:
            if (!renameTable(table.Name, string.Format("tmp_{0}", table.Name), dbName)) return false;

            // create the new table with new and renamed columns but without deleted columns
            createNewTable(table, table.Name, dbName);

            //copy the contents of renamed cols across from the original table.
            //string sqlTxt = UDTDataSet.dbProvider.adjSQL(
            //    string.Format(@"USE [{0}] INSERT INTO {1}({2}) SELECT {2} FROM tmp_{1}",
            //    dbName, table.Name, getColSql(table), getColSql(table, true)));
            string sqlTxt = string.Format(@"INSERT INTO {1}({2}) SELECT {2} FROM tmp_{1}",
                dbName, table.Name, getColSql(table), getColSql(table, true));
            if (!executeQuery(sqlTxt)) return false;

            // drop the old table.
            //sqlTxt = string.Format(@"DROP TABLE tmp_{0}", table.Name);
            if (!dropTable(string.Format("tmp_{0}", table.Name), table.Name)) return false;

            return true;
        }

        private bool renameTable(string oldName, string newName, string dbName)
        {
            string sqlTxt = "";
            if (TableExists(newName, dbName))
            {
                //sqlTxt = string.Format(@"DROP TABLE {0}", newName);
                if (!dropTable(newName, dbName)) return false;
            }
            if (UDTDataSet.dbProvider.dbType == DBType.sqlLite)
                sqlTxt = string.Format(@"ALTER TABLE {0} RENAME TO {1}",
                    oldName, newName);
            else if(UDTDataSet.dbProvider.dbType == DBType.sqlExpress)
            {
                //sqlTxt = UDTDataSet.dbProvider.adjSQL(string.Format(@"USE [{0}] EXEC sp_rename '{1}', '{2}'", dbName, oldName, newName));
                sqlTxt = string.Format(@"EXEC sp_rename '{0}', '{1}'", oldName, newName);
            }
            if (!executeQuery(sqlTxt)) return false;

            return true;

        }

        private void AddColumn(string table, UDTBase udtItem, string dbName)
        {
            //ALTER TABLE table_name ADD column_name datatype;

            //string sqlTxt = UDTDataSet.dbProvider.adjSQL(string.Format(@"USE [{0}] ALTER TABLE {1}", dbName, table));
            string sqlTxt = string.Format(@"ALTER TABLE {0}", table);
            sqlTxt += string.Format(" ADD {0} ", udtItem.Name);
            sqlTxt += string.Format("{0} ", udtItem.Type);

            //using (SqlConnection conn = new SqlConnection())
            using (DbConnection conn = UDTDataSet.dbProvider.Conection)
            {

                //conn.ConnectionString = ConfigurationManager.ConnectionStrings["conString"].ConnectionString;
                conn.ConnectionString = UDTDataSet.dbProvider.ConnectionString;

                //SqlCommand cmd = new SqlCommand(sqlTxt);
                DbCommand cmd = UDTDataSet.dbProvider.GetCommand(sqlTxt);

                cmd.Connection = conn;
                conn.Open();
                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {

                }
            }
        }


        public void readDatabase(UDTData masterItem)
        {
            DataSet = new System.Data.DataSet(masterItem.Name);
            DataSet.EnforceConstraints = true;
            UDTDataSet.dbProvider = new DbProvider(masterItem.dbType, masterItem.serverName);
            foreach(UDTData table in masterItem.tableData)
            {
                readTable(DataSet, table, masterItem.Name);
            }
            RaisePropertyChanged("udtDataSet");
            IsModified = false;
        }

        public delegate void dataChangeDel();
        public event dataChangeDel dataChangeEvent;

        private void rowChanged(object sender, DataRowChangeEventArgs e)
        {
            IsModified = true;
            if (dataChangeEvent != null) dataChangeEvent();
        }

        private void columnChanged(object sender, DataColumnChangeEventArgs e)
        {
            IsModified = true;
            if (dataChangeEvent != null) dataChangeEvent();
        }

        // links data edit class to file menu class
        // edit class sets the error state by calling validationChange
        // file menu class subscribes to validationChangedEvent
        // on validation changed event, menu class set file can save state
        public bool HasEditErrors { get; set; }
        public delegate void validationChangedDel();
        public event validationChangedDel validationChangedEvent;
        public void validationChange(bool hasErrors)
        {
            HasEditErrors = hasErrors;
            if (validationChangedEvent != null) validationChangedEvent();
        }


        DataTable createDataTable(UDTData dataItem)
        {
            DataTable tbl = new DataTable(dataItem.Name);
            //foreach (UDTBase item in dataItem.ChildData)
            foreach (UDTBase item in dataItem.columnData)
            {
                //if (item.GetType() != typeof(UDTData))
                //{
                    DataColumn col = new DataColumn();
                    col.ColumnName = item.Name;
                    col.DataType = typeof(int);
                    if (item.GetType() == typeof(UDTTxtItem))
                    {
                        col.DataType = typeof(string);
                    }
                    else if (item.GetType() == typeof(UDTDateItem))
                    {
                        col.DataType = typeof(DateTime);
                        UDTDateItem decimalItem = item as UDTDateItem;
                        UDTDateEditProps props = decimalItem.editProps as UDTDateEditProps;
                        if (props.dateFormat == DateTimeFormat.Date_12_HourTime)
                        {
                            col.ExtendedProperties.Add("fmt", "{0:MM/dd/yyyy:hh:mm:tt}");
                        }
                        else if (props.dateFormat == DateTimeFormat.Date_24_HourTime)
                        {
                            col.ExtendedProperties.Add("fmt", "{0:MM/dd/yyyy:HH:mm}");
                        }
                        else if (props.dateFormat == DateTimeFormat.Date_Only)
                        {
                            col.ExtendedProperties.Add("fmt", "{0:MM/dd/yyyy}");
                        }
                    }
                    else if (item.GetType() == typeof(UDTDecimalItem))
                    {
                        col.DataType = typeof(decimal);
                        UDTDecimalItem decimalItem = item as UDTDecimalItem;
                        UDTDecimalEditProps props = decimalItem.editProps as UDTDecimalEditProps;
                        if (props.decimalFormat == DecimalFormatType.Currency)
                            col.ExtendedProperties.Add("fmt", "{0:c}");
                        else if (props.decimalFormat == DecimalFormatType.Percent)
                            col.ExtendedProperties.Add("fmt", "{0:P2}");
                        else if (props.decimalFormat == DecimalFormatType.Decimal)
                            col.ExtendedProperties.Add("fmt", "{0}");
                    }
                    else if (item.GetType() == typeof(UDTIntItem))
                    {
                        col.DataType = typeof(int);
                    }
                    tbl.Columns.Add(col);
                //}
            }
            foreach (string colName in dataItem.ParentColumnNames)
            {
                DataColumn col = new DataColumn();
                col.ColumnName = colName;
                col.DataType = typeof(Guid);
                col.AllowDBNull = true;
                tbl.Columns.Add(col);
            }
            DataColumn idCol = new DataColumn();
            //idCol.AutoIncrement = true;
            idCol.ColumnName = "Id";
            idCol.DataType = typeof(Guid);
            tbl.Columns.Add(idCol);

            tbl.RowChanged += new DataRowChangeEventHandler(rowChanged);
            tbl.RowDeleted += rowDeleted;
            tbl.ColumnChanged += new DataColumnChangeEventHandler(columnChanged);

            return tbl;
        }

        private void rowDeleted(object sender, DataRowChangeEventArgs e)
        {
            IsModified = true;
            if (dataChangeEvent != null) dataChangeEvent();
        }

        private void addConstraint(DataTable tbl, string parentCol)
        {
            if (!string.IsNullOrEmpty(parentCol))
            {
                string fkName = string.Format("{0}{1}", parentCol, tbl.TableName);
                DataColumn pCol = DataSet.Tables[parentCol].Columns["Id"];
                ForeignKeyConstraint fKConstrint = new ForeignKeyConstraint(
                      fkName,
                      pCol,  // parent col
                      tbl.Columns[parentCol]); // child column
                fKConstrint.DeleteRule = Rule.Cascade;
                if (!tbl.Constraints.Contains(fKConstrint.ConstraintName))
                    tbl.Constraints.Add(fKConstrint);
            }
        }

        private void readTable(System.Data.DataSet dataSet, UDTData dataItem, string dbName, string parentColName = "")
        {
            DataTable tbl;
            if (dataSet.Tables.Contains(dataItem.Name))
            {
                tbl = dataSet.Tables[dataItem.Name];
                addConstraint(tbl, parentColName);
                return;  // create and read table only once
            }

            tbl = createDataTable(dataItem);

            dataSet.Tables.Add(tbl);

            //if (parentColName != "")
            //{
            addConstraint(tbl, parentColName);
            //    string colName = parentColName;
            //    string fkName = string.Format("{0}{1}", colName, dataItem.Name);
            //    DataColumn pCol = DataSet.Tables[colName].Columns["Id"];
            //    ForeignKeyConstraint fKConstrint = new ForeignKeyConstraint(
            //          fkName,
            //          pCol,  // parent col
            //          tbl.Columns[colName]); // child column
            //    fKConstrint.DeleteRule = Rule.Cascade; 
            //    if(!tbl.Constraints.Contains(fKConstrint.ConstraintName))
            //        tbl.Constraints.Add(fKConstrint);
            //}

            DataTable dataTable = dataSet.Tables[dataItem.Name];

            using (DbConnection conn = UDTDataSet.dbProvider.Conection)
            {

                conn.ConnectionString = UDTDataSet.dbProvider.ConnectionString;
                DbCommand cmd = UDTDataSet.dbProvider.Command;
                DbDataReader reader = UDTDataSet.dbProvider.Reader;


                // read all records in table on first call and only call
                string sqlTxt;
                sqlTxt = string.Format("select * from {0} ", dataItem.Name);

                cmd.CommandText = sqlTxt;
                cmd.CommandType = CommandType.Text;
                cmd.Connection = conn;

                try
                {
                    conn.Open();
                    reader = cmd.ExecuteReader();
                    dataTable.Load(reader);
                    foreach (UDTData childItem in dataItem.tableData)
                    {
                        readTable(dataSet, childItem, dbName, dataItem.Name);
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    if(reader != null) reader.Close();
                }
            }
        }

        public void saveDataset()
        {
            foreach(DataTable tbl in DataSet.Tables)
            {
                foreach(DataRow row in tbl.Rows)
                {
                    if(row.RowState == DataRowState.Added)
                    {
                        addRow(row);
                    }
                    else if (row.RowState == DataRowState.Deleted)
                    {
                        deleteRow(row);
                    }
                    else if (row.RowState == DataRowState.Modified)
                    {
                        updateRow(row);
                    }
                }
            }

            DataSet.AcceptChanges();
            IsModified = false;
        }

        private void deleteRow(DataRow row)
        {
            //DELETE FROM table_name
            //WHERE condition;
            Guid id = (Guid)row["Id", DataRowVersion.Original];
            //string sqlTxt = UDTDataSet.dbProvider.adjSQL(string.Format("USE [{0}] delete from {1} where Id = '{2}'", 
            //    DataSet.DataSetName, row.Table.TableName, id));
            string sqlTxt = string.Format("delete from {1} where Id = '{2}'",
               DataSet.DataSetName, row.Table.TableName, id);
            //using (SqlConnection conn = new SqlConnection())
            using (DbConnection conn = UDTDataSet.dbProvider.Conection)
            {

                //conn.ConnectionString = ConfigurationManager.ConnectionStrings["conString"].ConnectionString;
                conn.ConnectionString = UDTDataSet.dbProvider.ConnectionString;
                //SqlCommand cmd = new SqlCommand(sqlTxt);
                DbCommand cmd = UDTDataSet.dbProvider.GetCommand(sqlTxt);

                cmd.Connection = conn;
                conn.Open();
                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                }
            }
        }

        private void addRow(DataRow row)
        {
            //INSERT INTO table_name (column1, column2, column3, ...)
            //VALUES (value1, value2, value3, ...);
            //string sqlTxt = UDTDataSet.dbProvider.adjSQL(string.Format("USE [{0}] insert into {1} (",
            //    DataSet.DataSetName, row.Table.TableName));
            string sqlTxt = string.Format("insert into {1} (",
                DataSet.DataSetName, row.Table.TableName);
            foreach (DataColumn col in row.Table.Columns)
            {
                if(row[col.ColumnName] != DBNull.Value)
                    sqlTxt += string.Format("{0}, ", col.ColumnName);
            }
            sqlTxt = sqlTxt.Substring(0, sqlTxt.Length - 2);
            sqlTxt += ") values (";
            foreach (DataColumn col in row.Table.Columns)
            {
                if (row[col.ColumnName] == DBNull.Value)
                    continue;

                if (col.DataType == typeof(String))
                    sqlTxt += string.Format("'{0}', ", row[col.ColumnName]);
                else if (col.DataType == typeof(decimal) || col.DataType == typeof(int))
                    sqlTxt += string.Format("{0}, ", row[col.ColumnName]);
                else if (col.DataType == typeof(DateTime))
                    sqlTxt += string.Format("'{0}', ", row[col.ColumnName]);
                else if (col.DataType == typeof(Guid))
                {
                    var id = row[col.ColumnName];
                    if(id.GetType() == typeof(Guid))
                        sqlTxt += string.Format("'{0}', ", row[col.ColumnName]);
                    else
                        sqlTxt += string.Format("NULL, ");
                }
            }
            sqlTxt = sqlTxt.Substring(0, sqlTxt.Length - 2);
            sqlTxt += ")";
            //using (SqlConnection conn = new SqlConnection())
            using (DbConnection conn = UDTDataSet.dbProvider.Conection)
            {

                //conn.ConnectionString = ConfigurationManager.ConnectionStrings["conString"].ConnectionString;
                conn.ConnectionString = UDTDataSet.dbProvider.ConnectionString;
                //SqlCommand cmd = new SqlCommand(sqlTxt);
                DbCommand cmd = UDTDataSet.dbProvider.GetCommand(sqlTxt);

                cmd.Connection = conn;
                conn.Open();
                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format("addRow failed: {0}", ex.Message));
                }
                finally
                {
                }
            }
        }

        private void updateRow(DataRow row)
        {
            //UPDATE table_name
            //SET column1 = value1, column2 = value2, ...
            //WHERE condition;
            //string sqlTxt = UDTDataSet.dbProvider.adjSQL(string.Format("USE [{0}] update {1} set ", DataSet.DataSetName, row.Table.TableName));
            string sqlTxt = string.Format("update {1} set ", 
                DataSet.DataSetName, row.Table.TableName);
            sqlTxt += " ";
            foreach(DataColumn col in row.Table.Columns)
            {
                if (row[col.ColumnName] == DBNull.Value)
                    sqlTxt += string.Format("{0}={1}, ", col.ColumnName, "NULL");

                else if (col.DataType == typeof(String))
                    sqlTxt += string.Format("{0}='{1}', ", col.ColumnName, row[col.ColumnName]);
                else if (col.DataType == typeof(decimal) || col.DataType == typeof(int))
                    sqlTxt += string.Format("{0}={1}, ", col.ColumnName, row[col.ColumnName]);
                else if (col.DataType == typeof(DateTime))
                    sqlTxt += string.Format("{0}='{1}', ", col.ColumnName, row[col.ColumnName]);
                else if (col.DataType == typeof(Guid))
                {
                    var id = row[col.ColumnName];
                    if (id.GetType() == typeof(Guid))
                        sqlTxt += string.Format("{0}='{1}', ", col.ColumnName, row[col.ColumnName]);
                    else
                        sqlTxt += string.Format("{0}=NULL, ", col.ColumnName);
                }
                
            }
            sqlTxt = sqlTxt.Substring(0, sqlTxt.Length - 2);
            sqlTxt += string.Format(" where Id = '{0}' ", row["Id"]);

            //using (SqlConnection conn = new SqlConnection())
            using (DbConnection conn = UDTDataSet.dbProvider.Conection)
            {

                //conn.ConnectionString = ConfigurationManager.ConnectionStrings["conString"].ConnectionString;
                conn.ConnectionString = UDTDataSet.dbProvider.ConnectionString;
                //SqlCommand cmd = new SqlCommand(sqlTxt);
                DbCommand cmd = UDTDataSet.dbProvider.GetCommand(sqlTxt);

                cmd.Connection = conn;
                conn.Open();
                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format("updateRow failed: {0}",ex.Message));
                }
                finally
                {
                }
            }
        }
    }
}
