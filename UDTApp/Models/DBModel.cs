using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Windows;
using UDTApp.DataBaseProvider;
using UDTApp.ListManager;
using UDTApp.SchemaModels;
using UDTApp.ViewModels;
using UDTAppControlLibrary.Controls;

namespace UDTApp.Models
{
    public class DBModel : ValidatableBindableBase
    {
        //public DBModel(UDTData _dbSchema) 
        public DBModel(DBType dbType, string serverName)
        {
            //dbSchema = _dbSchema;
            //dbProvider = new DbProvider(_dbSchema.dbType, _dbSchema.serverName);
            dbProvider = new DbProvider(dbType, serverName);
            Service = this;
        }

        public static DBModel Service;

        //public UDTData dbSchema;
        private DbProvider dbProvider;

        #region SqlCode

        public void createDatabase()
        {
            createSQLDatabase(XMLModel.Service.dbSchema.Name);
            List<Guid> tableGuids = new List<Guid>();
            foreach (UDTData table in XMLModel.Service.dbSchema.tableData)
            {
                createDBTable(table, XMLModel.Service.dbSchema.Name, tableGuids);
            }
        }

        private void createSQLDatabase(string DBName)
        {
            if (dbProvider.dbType == DBType.sqlLite) return;

            using (DbConnection conn = dbProvider.Conection)
            {
                //UDTData mastr = UDTXml.UDTXmlData.SchemaData[0] as UDTData;
                // if remote db must exits before project is created
                if (!string.IsNullOrEmpty(XMLModel.Service.dbSchema.serverName))
                {
                    conn.ConnectionString = dbProvider.ConnectionString;
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

                // must be sql server database on localDb so get folder for mdf files
                string dataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\UdtLocalDb";
                if (!Directory.Exists(dataFolder))
                {
                    Directory.CreateDirectory(dataFolder);
                }

                // test if db exits and create in db folder if not
                conn.ConnectionString = dbProvider.MasterCatalogConnnectionString;
                DbCommand cmd = dbProvider.GetCommand(
                    string.Format("select count(*) from (select * from sys.databases where name = '{0}') rows", DBName)
                    );

                // query master DB to see if database exits
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
                            string.Format("{0}\\{1}.mdf", dataFolder, DBName),
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

        public void deleteSQLDatabase(string dbName)
        {
            try
            {
                if (dbProvider.dbType == DBType.sqlLite)
                {
                    string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                    string dataFolder = path + "\\UdtApp";
                    if (Directory.Exists(dataFolder))
                    {
                        // TBD: does this fix sqlite file not closed when connection closed;
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                        string filePath = string.Format("{0}\\{1}.db",
                            dataFolder, dbName);
                        if (File.Exists(filePath))
                            File.Delete(filePath);
                    }
                }
                else
                {
                    //UDTData master = UDTXml.UDTXmlData.SchemaData[0] as UDTData;
                    if (!string.IsNullOrEmpty(XMLModel.Service.dbSchema.serverName)) return;  // ignore request to delete remove db

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

        private void createDBTable(UDTData dataItem, string dbName, List<Guid> tableGuids)
        {
            if (tableGuids.Contains(dataItem.objId)) return;

            if (dataItem.savTableData != null)
            {
                foreach (UDTData item in dataItem.savTableData)
                {
                    // table is deleleted when last reference is removed from item dictionary
                    if (!TableDictionary.itemDic.ContainsKey(item.objId))
                    {
                        if (!isTableEmpty(item, dbName))
                        {
                            MessageBox.Show("Error: Attempt to delete non-empty table", "Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                        dropTable(item.Name, dbName);
                    }
                }
                tableGuids.Add(dataItem.dragObjId);
            }

            foreach (UDTData item in dataItem.tableData)
            {
                createDBTable(item, dbName, tableGuids);
            }

            if (!TableExists(dataItem.Name, dbName))
            {
                if (!string.IsNullOrEmpty(dataItem.savName) && TableExists(dataItem.savName, dbName))
                {
                    // table is renamed
                    if (!renameTable(dataItem.savName, dataItem.Name, dbName)) return;
                    dataItem.savName = dataItem.Name;  // remove table name mod from schema
                }
                else
                {
                    createNewTable(dataItem, dataItem.Name, dbName);
                    return;
                }
            }

            // if table exits check for added, deleted, renamed columns
            if (dataItem.savColumnData != null)
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
                if (dataItem.isColumnDeleted)
                {

                    List<UDTBase> deleted = dataItem.savColumnData.ToList().FindAll
                        (p => dataItem.columnData.FirstOrDefault(a => a.savName == p.Name) == null);

                    foreach (UDTBase item in deleted)
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

        public bool isDatabaseEmpty()
        {
            return dbEmpty(XMLModel.Service.dbSchema, XMLModel.Service.dbSchema.Name);
        }

        private bool dbEmpty(UDTData table, string dbName)
        {
            foreach (UDTData chTbl in table.tableData)
            {
                if (!dbEmpty(chTbl, dbName)) return false;
            }

            if (!isTableEmpty(table, dbName))
                return false;
            else
                return true;
        }

        private bool isTableEmpty(UDTData dataItem, string dbName)
        {
            if (dataItem.savColumnData == null || dataItem.savColumnData.Count <= 0) return true;

            bool retVal = true;
            List<string> colList = new List<string>();
            foreach (UDTBase item in dataItem.savColumnData)
            {
                colList.Add(item.Name);
            }

            string sqlTxt = string.Format("SELECT ");
            foreach (string colName in colList)
            {
                sqlTxt += string.Format("{0}", colName);
                if (colName != colList.Last())
                    sqlTxt += ", ";
            }

            sqlTxt += string.Format(" FROM {0} WHERE ", dataItem.unEditedName);
            foreach (string colName in colList)
            {
                sqlTxt += string.Format("{0} <> '' AND {0} IS NOT NULL", colName);
                if (colName != colList.Last())
                    sqlTxt += " OR ";
            }

            using (DbConnection conn = dbProvider.Conection)
            {
                conn.ConnectionString = dbProvider.ConnectionString;
                DbCommand cmd = dbProvider.GetCommand(sqlTxt);
                cmd.Connection = conn;
                conn.Open();
                try
                {
 
                    {
                        DbDataReader reader = dbProvider.Reader;
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

            // if data rows are not empty, check fornien key (child) rows if this is child table
            if (!retVal)
                return childRowsEmpty(dataItem);
            return retVal;
        }

        private bool dropTable(string tableName, string dbName)
        {
            string sqlTxt =
                string.Format("DROP TABLE {0}", tableName);
            return executeQuery(sqlTxt);
        }

        private bool createNewTable(UDTData dataItem, string tableName, string dbName)
        {
            using (DbConnection conn = dbProvider.Conection)
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

        private bool TableExists(string tableName, string dbName)
        {

            string sqlTxt = string.Format(@"select count(*) from 
                (SELECT * FROM {0}.dbo.sysobjects WHERE xtype = 'U' AND name = '{1}') rows",
                dbName, tableName);

            //select (select count() from XXX) as count
            if (dbProvider.dbType == DBType.sqlLite)
                sqlTxt = string.Format(@"SELECT name FROM sqlite_master WHERE type='table' AND name='{0}'",
                    tableName);

            bool retVal = true;
            //using (SqlConnection conn = new SqlConnection())
            using (DbConnection conn = dbProvider.Conection)
            {

                //conn.ConnectionString = ConfigurationManager.ConnectionStrings["conString"].ConnectionString;
                conn.ConnectionString = dbProvider.ConnectionString;

                //SqlCommand cmd = new SqlCommand(sqlTxt);
                DbCommand cmd = dbProvider.GetCommand(sqlTxt);

                cmd.Connection = conn;
                conn.Open();
                try
                {
                    if (dbProvider.dbType == DBType.sqlLite)
                    {
                        DbDataReader reader = dbProvider.Reader;
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

        private bool renameTable(string oldName, string newName, string dbName)
        {
            string sqlTxt = "";
            if (TableExists(newName, dbName))
            {
                //sqlTxt = string.Format(@"DROP TABLE {0}", newName);
                if (!dropTable(newName, dbName)) return false;
            }
            if (dbProvider.dbType == DBType.sqlLite)
                sqlTxt = string.Format(@"ALTER TABLE {0} RENAME TO {1}",
                    oldName, newName);
            else if (dbProvider.dbType == DBType.sqlExpress)
            {
                //sqlTxt = UDTDataSet.dbProvider.adjSQL(string.Format(@"USE [{0}] EXEC sp_rename '{1}', '{2}'", dbName, oldName, newName));
                sqlTxt = string.Format(@"EXEC sp_rename '{0}', '{1}'", oldName, newName);
            }
            if (!executeQuery(sqlTxt)) return false;

            return true;

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

        private bool isColumnEmpty(string tableName, string columnName, string dbName)
        {
            bool retVal = true;
            //if (isTableEmpty(tableName, dbName)) return true;

            string sqlTxt = string.Format("SELECT {0} from {1} where {0} <> '' AND {0} IS NOT NULL", columnName, tableName);
            using (DbConnection conn = dbProvider.Conection)
            {
                conn.ConnectionString = dbProvider.ConnectionString;
                DbCommand cmd = dbProvider.GetCommand(sqlTxt);
                cmd.Connection = conn;
                conn.Open();
                try
                {
                    if (dbProvider.dbType == DBType.sqlLite)
                    {
                        DbDataReader reader = dbProvider.Reader;
                        reader = cmd.ExecuteReader();
                        retVal = !reader.HasRows;
                        reader.Close();
                    }
                    else
                    {
                        DbDataReader reader = dbProvider.Reader;
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

        private bool childRowsEmpty(UDTData dataItem)
        {
            if (dataItem.ParentColumnNames == null || dataItem.ParentColumnNames.Count <= 0) return false;

            bool retVal = true;
            string sqlTxt = string.Format("SELECT {0} FROM {1} WHERE {0} IS NOT NULL",
                dataItem.parentObj.unEditedName, dataItem.unEditedName);

            using (DbConnection conn = dbProvider.Conection)
            {
                conn.ConnectionString = dbProvider.ConnectionString;
                DbCommand cmd = dbProvider.GetCommand(sqlTxt);
                cmd.Connection = conn;
                conn.Open();
                try
                {
                    DbDataReader reader = dbProvider.Reader;
                    reader = cmd.ExecuteReader();
                    retVal = !reader.HasRows;
                    reader.Close();
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

        private string getColSql(UDTData dataItem, bool savedCols = false)
        {
            string sqlTxt = "";
            List<string> colNameList = getColNameList(dataItem, savedCols);
            foreach (string colName in colNameList)
            {
                sqlTxt += colName;
                if (colName != colNameList.Last())
                    sqlTxt += ", ";
            }
            return sqlTxt;
        }

        private List<string> getColNameList(UDTData dataItem, bool savedCols = false)
        {
            List<string> colNameList = new List<string>();
            colNameList.Add("Id");

            // if col is presnet in current list then not deleted 
            // so this gets undeleted colums by new name
            // or by old name and not new columns
            foreach (UDTBase col in dataItem.columnData)
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
                if (dataItem.savParentColumnNames.FirstOrDefault(p => p == colName) == null)
                    continue;

                colNameList.Add(string.Format("{0}", colName));
            }

            return colNameList;
        }


        private bool executeQuery(string sqlTxt, bool useMasterDb = false)
        {
            using (DbConnection conn = dbProvider.Conection)
            {
                if (useMasterDb)
                    conn.ConnectionString = dbProvider.MasterCatalogConnnectionString;
                else
                    conn.ConnectionString = dbProvider.ConnectionString;

                DbCommand cmd = dbProvider.GetCommand(sqlTxt);

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

        private bool dataBaseExists(DBType dbType, string dbName)
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

                // remote db must exist before project in created
                //UDTData master = UDTXml.UDTXmlData.SchemaData[0] as UDTData;
                if (!string.IsNullOrEmpty(XMLModel.Service.dbSchema.serverName)) return true;

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

        #endregion

        #region DataSetCode

        private System.Data.DataSet _DataSet = null;
        public System.Data.DataSet DataSet
        {
            get { return _DataSet; }
            set { _DataSet = value; }
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

        public delegate void dataChangeDel();
        public event dataChangeDel dataChangeEvent;

        public bool HasEditErrors { get; set; }
        public delegate void validationChangedDel();
        public event validationChangedDel validationChangedEvent;
        public void validationChange(bool hasErrors)
        {
            HasEditErrors = hasErrors;
            if (validationChangedEvent != null) validationChangedEvent();
        }

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

        private void rowDeleted(object sender, DataRowChangeEventArgs e)
        {
            IsModified = true;
            if (dataChangeEvent != null) dataChangeEvent();
        }

        public void readDatabase(UDTData masterItem)
        {
            DataSet = new System.Data.DataSet(masterItem.Name);
            DataSet.EnforceConstraints = true;
            dbProvider = new DbProvider(masterItem.dbType, masterItem.serverName);
            foreach (UDTData table in masterItem.tableData)
            {
                readTable(DataSet, table, masterItem.Name);
            }
            RaisePropertyChanged("udtDataSet");
            IsModified = false;
        }

        public void saveDataset()
        {
            foreach (DataTable tbl in DataSet.Tables)
            {
                foreach (DataRow row in tbl.Rows)
                {
                    if (row.RowState == DataRowState.Added)
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

            addConstraint(tbl, parentColName);

            DataTable dataTable = dataSet.Tables[dataItem.Name];

            using (DbConnection conn = dbProvider.Conection)
            {

                conn.ConnectionString = dbProvider.ConnectionString;
                DbCommand cmd = dbProvider.Command;
                DbDataReader reader = dbProvider.Reader;


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
                    dataTable.RowChanged -= rowChanged;
                    dataTable.Load(reader);
                    dataTable.RowChanged += rowChanged;
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
                    if (reader != null) reader.Close();
                }
            }
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

        private DataTable createDataTable(UDTData dataItem)
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
                if (row[col.ColumnName] != DBNull.Value)
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
                    if (id.GetType() == typeof(Guid))
                        sqlTxt += string.Format("'{0}', ", row[col.ColumnName]);
                    else
                        sqlTxt += string.Format("NULL, ");
                }
            }
            sqlTxt = sqlTxt.Substring(0, sqlTxt.Length - 2);
            sqlTxt += ")";
            //using (SqlConnection conn = new SqlConnection())
            using (DbConnection conn = dbProvider.Conection)
            {

                //conn.ConnectionString = ConfigurationManager.ConnectionStrings["conString"].ConnectionString;
                conn.ConnectionString = dbProvider.ConnectionString;
                //SqlCommand cmd = new SqlCommand(sqlTxt);
                DbCommand cmd = dbProvider.GetCommand(sqlTxt);

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
            string sqlTxt = string.Format("update {1} set ",
                DataSet.DataSetName, row.Table.TableName);
            sqlTxt += " ";
            foreach (DataColumn col in row.Table.Columns)
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

            using (DbConnection conn = dbProvider.Conection)
            {

                conn.ConnectionString = dbProvider.ConnectionString;
                DbCommand cmd = dbProvider.GetCommand(sqlTxt);

                cmd.Connection = conn;
                conn.Open();
                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format("updateRow failed: {0}", ex.Message));
                }
                finally
                {
                }
            }
        }

        private void deleteRow(DataRow row)
        {
            //DELETE FROM table_name
            //WHERE condition;
            Guid id = (Guid)row["Id", DataRowVersion.Original];
            string sqlTxt = string.Format("delete from {1} where Id = '{2}'",
               DataSet.DataSetName, row.Table.TableName, id);
            using (DbConnection conn = dbProvider.Conection)
            {

                //conn.ConnectionString = ConfigurationManager.ConnectionStrings["conString"].ConnectionString;
                conn.ConnectionString = dbProvider.ConnectionString;
                //SqlCommand cmd = new SqlCommand(sqlTxt);
                DbCommand cmd = dbProvider.GetCommand(sqlTxt);

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

        public bool canRemoveTable(UDTData masterItem, UDTData dataItem)
        {
            if (string.IsNullOrEmpty(dataItem.savName)) return true;
            if (!dataBaseExists(masterItem.dbType, masterItem.Name)) return true;
            if (DataSet != null)
            {
                return isDataSetTableEmpty(masterItem, dataItem);
            }
            else
            {
                dbProvider = new DbProvider(masterItem.dbType, masterItem.serverName);
                return isTableEmpty(dataItem, masterItem.Name);
            }
        }

        public bool canRemoveCol(UDTData masterItem, UDTData tableItem, UDTBase colItem)
        {
            if (string.IsNullOrEmpty(tableItem.savName)) return true;
            if (DataSet != null)
            {
                return isDataSetColumnEmpty(tableItem, colItem);
            }
            else
            {
                //dbProvider = new DbProvider(masterItem.dbType, masterItem.serverName);
                return isColumnEmpty(tableItem.Name, colItem.Name, masterItem.Name);
            }
        }

        private bool isDataSetColumnEmpty(UDTData tableItem, UDTBase colItem)
        {
            DataTable tb = DataSet.Tables[tableItem.Name];
            if (!tb.Columns.Contains(colItem.Name)) return true;
            if (colItem.TypeName == UDTTypeName.Text)
            {
                EnumerableRowCollection<DataRow> rows = tb.AsEnumerable().Where(r => r[colItem.Name] != DBNull.Value);
                if (rows.Any())
                    rows = rows.Where(r => !string.IsNullOrEmpty((string)r[colItem.Name]));
                return !rows.Any();
            }
            else
            {
                EnumerableRowCollection<DataRow> rows = tb.AsEnumerable().Where(r => r[colItem.Name] != DBNull.Value);
                return !rows.Any();
            }
        }

        private bool isDataSetTableEmpty(UDTData masterItem, UDTData dataItem)
        {
            if (!DataSet.Tables.Contains(dataItem.unEditedName)) return true;
            if (dataItem.savColumnData == null || dataItem.savColumnData.Count <= 0) return true;

            DataTable tb = DataSet.Tables[dataItem.unEditedName];
            if (tb.Rows.Count <= 0) return true;

            // table is not empty if any data cols not null AND if id cols for this parent table are not null

            // check all data cols
            foreach (UDTBase col in dataItem.savColumnData)
            {
                if (!tb.Columns.Contains(col.Name)) continue;

                EnumerableRowCollection<DataRow> rows = tb.AsEnumerable().
                    Where(r => r[col.Name] != DBNull.Value);
                if (rows.Any() && col.TypeName == UDTTypeName.Text)
                {
                    rows = rows.Where(r => !string.IsNullOrEmpty((string)r[col.unEditedName]));
                }
                // if we have non null data column, check if this is child table and
                // any parent column id field not null
                if (rows.Any() && dataItem.ParentColumnNames != null
                    && dataItem.ParentColumnNames.Count > 0)
                {
                    rows = rows.Where(r => r[dataItem.parentObj.unEditedName] != DBNull.Value);
                    return !rows.Any();
                }
            }
            // if all data columns are null table is empty
            return true;
        }

        #endregion
    }
}
