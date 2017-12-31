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
                    _dbProvider = new DbProvider(DBType.sqlLite);
                }
                return _dbProvider;
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

        
        //private string getDbName(UDTData udtData)
        //{
        //    return udtData.Name + "_db";
        //}


        public void createDatabase(UDTData masterItem)
        {
            //createSQLDatabase(getDbName(masterItem));
            createSQLDatabase(masterItem.Name);
            List<Guid> tableGuids = new List<Guid>();
            //createDBTable(masterItem, getDbName(masterItem), tableGuids);
            foreach(UDTData table in masterItem.tableData)
            {
                createDBTable(table, masterItem.Name, tableGuids);
            }
        }

        private void createSQLDatabase(string DBName)
        {
            //using (SqlConnection conn = new SqlConnection())
            if (UDTDataSet.dbProvider.dbType == DBType.sqlLite) return;

            using (DbConnection conn = UDTDataSet.dbProvider.Conection)
            {

                //conn.ConnectionString = ConfigurationManager.ConnectionStrings["conString"].ConnectionString;
                conn.ConnectionString = UDTDataSet.dbProvider.ConnectionString;
                //SqlCommand cmd = new SqlCommand(
                //    string.Format("select count(*) from (select * from sys.databases where name = '{0}') rows", DBName)
                //    );
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
                        cmd.CommandText = string.Format("CREATE DATABASE {0} ", DBName);
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
            string ddl;

            if (tableGuids.Contains(dataItem.objId)) return;

            if (!TableExists(dataItem.Name, dbName))
            {
                //using (SqlConnection conn = new SqlConnection())
                using (DbConnection conn = UDTDataSet.dbProvider.Conection)
                {
                    ddl = UDTDataSet.dbProvider.adjSQL(string.Format("USE [{0}] CREATE TABLE {1} (", dbName, dataItem.Name));
                    ddl += string.Format("[Id] [uniqueidentifier] NOT NULL, ");
                    //foreach (UDTBase item in dataItem.ChildData)
                    foreach (UDTBase item in dataItem.columnData)
                    {
                        //if (item.GetType() != typeof(UDTData))
                        //{
                            ddl += string.Format("{0} {1}, ", item.Name, item.Type);
                        //}
                    }
                    foreach (string colName in dataItem.ParentColumnNames)
                    {
                        ddl += string.Format("{0} [uniqueidentifier], ", colName);
                    }
                    ddl = ddl.Substring(0, ddl.Length - 2);
                    ddl += "); ";

                    tableGuids.Add(dataItem.objId);

                    //conn.ConnectionString = ConfigurationManager.ConnectionStrings["conString"].ConnectionString;
                    conn.ConnectionString = UDTDataSet.dbProvider.ConnectionString;
                    //SqlCommand cmd = new SqlCommand(ddl);
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
            }
            // if table exits check for and add any new columns
            else
            {
                List<string> colList = GetColumns(dataItem.Name, dbName);

                //foreach (UDTBase item in dataItem.ChildData)
                foreach (UDTBase item in dataItem.columnData)
                {
                    //if (item.GetType() != typeof(UDTData) && !colList.Contains(item.Name))
                    if (!colList.Contains(item.Name))
                    {
                        AddColumn(dataItem.Name, item, dbName);
                    }
                }
            }

            //foreach (UDTBase item in dataItem.ChildData)
            foreach (UDTData item in dataItem.tableData)
            {
                //if (item.GetType() == typeof(UDTData))
                //{
                    createDBTable(item as UDTData, dbName, tableGuids);
                //}
            }
        }

        private List<string> GetColumns(string table, string dbName)
        {

            string sqlTxt = UDTDataSet.dbProvider.adjSQL(string.Format(@"USE [{0}] SELECT * FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_NAME = '{1}'
                    ORDER BY ORDINAL_POSITION", dbName, table));

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

        private void AddColumn(string table, UDTBase udtItem, string dbName)
        {
            //ALTER TABLE table_name ADD column_name datatype;

            string sqlTxt = UDTDataSet.dbProvider.adjSQL(string.Format(@"USE [{0}] ALTER TABLE {1}", dbName, table));
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
            //DataSet = new System.Data.DataSet(getDbName(masterItem));
            DataSet = new System.Data.DataSet(masterItem.Name);
            DataSet.EnforceConstraints = true;
            //readTable(DataSet, masterItem, getDbName(masterItem));
            //readTable(DataSet, masterItem, masterItem.Name);
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
                sqlTxt = UDTDataSet.dbProvider.adjSQL(string.Format("USE [{0}] select * from {1} ", dbName, dataItem.Name));

                cmd.CommandText = sqlTxt;
                cmd.CommandType = CommandType.Text;
                cmd.Connection = conn;
                conn.Open();

                reader = cmd.ExecuteReader();
                try
                {
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
                    reader.Close();
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
            string sqlTxt = UDTDataSet.dbProvider.adjSQL(string.Format("USE [{0}] delete from {1} where Id = '{2}'", 
                DataSet.DataSetName, row.Table.TableName, id));
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
            string sqlTxt = UDTDataSet.dbProvider.adjSQL(string.Format("USE [{0}] insert into {1} (", 
                DataSet.DataSetName, row.Table.TableName));
            foreach (DataColumn col in row.Table.Columns)
            {
                sqlTxt += string.Format("{0}, ", col.ColumnName);
            }
            sqlTxt = sqlTxt.Substring(0, sqlTxt.Length - 2);
            sqlTxt += ") values (";
            foreach (DataColumn col in row.Table.Columns)
            {

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
                    MessageBox.Show(ex.Message);
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
            string sqlTxt = UDTDataSet.dbProvider.adjSQL(string.Format("USE [{0}] update {1} set ", DataSet.DataSetName, row.Table.TableName));
            foreach(DataColumn col in row.Table.Columns)
            {
                //if(col.ColumnName != "Id")
                {
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
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                }
            }
        }
    }
}
