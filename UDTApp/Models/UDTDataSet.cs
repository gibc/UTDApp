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
            List<Guid> tableGuids = new List<Guid>();
            foreach(UDTData table in masterItem.tableData)
            {
                createDBTable(table, masterItem.Name, tableGuids);
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
                    string sqlTxt = string.Format(@"DROP DATABASE {0}", dbName);
                    if (!executeQuery(sqlTxt)) return;
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

            using (DbConnection conn = UDTDataSet.dbProvider.Conection)
            {
                conn.ConnectionString = UDTDataSet.dbProvider.MasterCatalogConnnectionString;
                DbCommand cmd = UDTDataSet.dbProvider.GetCommand(
                    string.Format("select count(*) from (select * from sys.databases where name = '{0}') rows", DBName)
                    ); 

                cmd.Connection = conn;
                conn.Open();
                try
                {
                    int dbCount = (int)cmd.ExecuteScalar();
                    if (dbCount < 1)
                    {
                        cmd.CommandTimeout = 300;
                        cmd.CommandText = string.Format("CREATE DATABASE {0} (EDITION = 'basic')", DBName); //CREATE DATABASE TestDB2 (EDITION = 'standard');
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

        private bool TableExists(string tableName, string dbName)
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

        private void createDBTable(UDTData dataItem, string dbName, List<Guid> tableGuids)
        {
            if (dataItem.savTableData != null)
            {
                foreach (UDTData item in dataItem.savTableData)
                {
                    var c = dataItem.tableData.FirstOrDefault(p => p.Name == item.Name);
                    if(c == null)
                    {
                        c = dataItem.tableData.FirstOrDefault(p => p.savName == item.Name);
                        if(c == null)
                        {
                            // table has been deleted
                            //delteDBTable(item, dbName, tableGuids);
                        }
                    }
                }
            }

            foreach (UDTData item in dataItem.tableData)
            {
                createDBTable(item, dbName, tableGuids);
            }

            string ddl;

            if (tableGuids.Contains(dataItem.objId)) return;

            if (!TableExists(dataItem.Name, dbName))
            {
                if (!string.IsNullOrEmpty(dataItem.savName) && TableExists(dataItem.savName, dbName))
                {
                    // table has a new name
                    //string sqlTxt = UDTDataSet.dbProvider.adjSQL(string.Format("USE [{0}]  ALTER TABLE {1} RENAME TO {2}",
                    //    dbName, dataItem.savName, dataItem.Name));
                    string sqlTxt = string.Format("ALTER TABLE {1} RENAME TO {2}",
                        dbName, dataItem.savName, dataItem.Name);
                    if (!executeQuery(sqlTxt)) return;
                    dataItem.savName = dataItem.Name;  // remove table name mod from schema
                }
                else
                {
                    // create new table
                    using (DbConnection conn = UDTDataSet.dbProvider.Conection)
                    {
                        //ddl = UDTDataSet.dbProvider.adjSQL(string.Format("USE [{0}] CREATE TABLE {1} (", dbName, dataItem.Name));
                        ddl = string.Format("CREATE TABLE {0} (", dataItem.Name);
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

                        tableGuids.Add(dataItem.objId);

                        conn.ConnectionString = UDTDataSet.dbProvider.ConnectionString;
                        DbCommand cmd = UDTDataSet.dbProvider.GetCommand(ddl);


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
                    }
                    return;
                }
            }

            // if table exits check for added, deleted, renamed columns
            if(dataItem.savColumnData != null)
            {
                //if (!dataItem.isModified) return; 
                if (!dataItem.isTableSchemsModified) return; 

                // if we have no data then just drop and recreate table with
                // all column mods
                if (isTableEmpty(dataItem.Name, dbName))
                {
                    //string sqlTxt = string.Format(@"DROP TABLE {0}", dataItem.Name);
                    if (!dropTable(dataItem.Name, dbName)) return;

                    createNewTable(dataItem, dataItem.Name, dbName);
                    return;
                }

                // if we have data check for deleted columns and
                // save to backup table
                if(dataItem.isColumnDeleted)
                { 
                    UDTData dropedColTable = new UDTData();
                    dropedColTable.Name = dataItem.Name += "DropedCols";

                    List<UDTBase> deleted = dataItem.savColumnData.ToList().FindAll
                        (p => dataItem.columnData.FirstOrDefault(a => a.savName == p.Name) == null);
                    dropedColTable.columnData = 
                        new System.Collections.ObjectModel.ObservableCollection<UDTBase>(deleted);

                    //createNewTable(dropedColTable, dropedColTable.Name, dbName);
                    //// copy data from current table to new table SELECT
                    //string sqlTxt = string.Format(@"INSERT INTO {0}({2}) SELECT {2} FROM {1}",
                    //    dropedColTable.Name, dataItem.Name,
                    //    getColSql(dropedColTable));
                    //if (!executeQuery(sqlTxt)) return;

                }

                // add new cols and renamed cols and
                // drop any deleted cols and
                // copy over data to renamed cols
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

            // TBD: need saved parent column names
            foreach (string colName in dataItem.ParentColumnNames)
            {
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

        bool isTableEmpty(string tableName, string dbName)
        {
            bool retVal = true;
            //string sqlTxt = UDTDataSet.dbProvider.adjSQL(string.Format("USE [{1}] SELECT * from {0}", tableName, dbName));
            string sqlTxt = string.Format("SELECT * from {0}", tableName);
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

        public bool dropTable(string tableName, string dbName)
        {
            string sqlTxt =
               // UDTDataSet.dbProvider.adjSQL(string.Format("USE [{0}] DROP TABLE {1}", dbName, tableName));
                string.Format("DROP TABLE {0}", tableName);
            return executeQuery(sqlTxt);
        }

        private bool executeQuery(string sqlTxt)
        {
            using (DbConnection conn = UDTDataSet.dbProvider.Conection)
            {
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
            tbl.ColumnChanged += new DataColumnChangeEventHandler(columnChanged);

            return tbl;
        }

        private void readTable(System.Data.DataSet dataSet, UDTData dataItem, string dbName, string parentColName = "")
        {
            if (dataSet.Tables.Contains(dataItem.Name)) return;  // read table only once

            DataTable tbl = createDataTable(dataItem);

            dataSet.Tables.Add(tbl);

            foreach (string colName in dataItem.ParentColumnNames)
            {
                string fkName = string.Format("{0}{1}", colName, dataItem.Name);
                DataColumn pCol = DataSet.Tables[colName].Columns["Id"];
                ForeignKeyConstraint fKConstrint = new ForeignKeyConstraint(
                      fkName,
                      pCol,  // parent col
                      tbl.Columns[colName]); // child column
                fKConstrint.DeleteRule = Rule.Cascade; 
                if(!tbl.Constraints.Contains(fKConstrint.ConstraintName))
                    tbl.Constraints.Add(fKConstrint);
            }

            DataTable dataTable = dataSet.Tables[dataItem.Name];
            //using (SqlConnection conn = new SqlConnection())
            using (DbConnection conn = UDTDataSet.dbProvider.Conection)
            {

                //conn.ConnectionString = ConfigurationManager.ConnectionStrings["conString"].ConnectionString;
                conn.ConnectionString = UDTDataSet.dbProvider.ConnectionString;
                //SqlCommand cmd = new SqlCommand();
                DbCommand cmd = UDTDataSet.dbProvider.Command;
                //SqlDataReader reader;
                DbDataReader reader = UDTDataSet.dbProvider.Reader;


                // read all records in table on first call and only call
                string sqlTxt;
                //if (parentId == -1)
                //sqlTxt = UDTDataSet.dbProvider.adjSQL(string.Format("USE [{0}] select * from {1} ", dbName, dataItem.Name));
                sqlTxt = string.Format("select * from {0} ", dataItem.Name);

                cmd.CommandText = sqlTxt;
                cmd.CommandType = CommandType.Text;
                cmd.Connection = conn;
                conn.Open();

                //reader = cmd.ExecuteReader();
                try
                {
                    reader = cmd.ExecuteReader();
                    dataTable.Load(reader);
                    //foreach (UDTBase childItem in dataItem.ChildData)
                    foreach (UDTBase childItem in dataItem.tableData)
                    {
                        //if (childItem.GetType() == typeof(UDTData))
                        //{
                            // why per row read?? 
                            //foreach (DataRow row in dataTable.Rows)
                            //{
                                readTable(dataSet, childItem as UDTData, dbName, dataItem.Name);
                            //}
                        //}
                    }

                    //while (reader.Read())
                    //{
                    //    DataRow dataRow = dataTable.NewRow();
                    //    //recId = (int)reader["Id"];
                    //    dataRow["Id"] = (int)reader["Id"];
                    //    foreach (UDTBase childItem in dataItem.ChildData)
                    //    {
                    //        if (childItem.GetType() != typeof(UDTData))
                    //        {
                    //            var data = reader[childItem.Name];
                    //            dataRow[childItem.Name] = reader[childItem.Name];
                    //        }
                    //        else
                    //        {
                    //            readTable(dataSet, childItem as UDTData, dbName, dataItem.Name, (int)dataRow["Id"]);
                    //        }
                    //    }
                    //    foreach (string colName in dataItem.ParentColumnNames)
                    //    {
                    //        dataRow[colName] = reader[colName];
                    //    }
                    //    dataTable.Rows.Add(dataRow);
                    //}
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
                if (row[col.ColumnName] == DBNull.Value) continue;
                
                if (col.DataType == typeof(String))
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
