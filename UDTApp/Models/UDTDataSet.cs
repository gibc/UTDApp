using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using UDTApp.ViewModels;

namespace UDTApp.Models
{
    public class UDTDataSet : ValidatableBindableBase
    {
        public UDTDataSet()
        {

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

        public void createDatabase(UDTData masterItem)
        {
            createSQLDatabase(masterItem.Name);
            List<Guid> tableGuids = new List<Guid>();
            createDBTable(masterItem, masterItem.Name, tableGuids);
        }



        //private void addParentColumns(UDTData dataItem)
        //{
        //    foreach (UDTBase item in dataItem.ChildData)
        //    {
        //        if (item.GetType() == typeof(UDTData))
        //        {
        //            UDTData childItem = item as UDTData;
        //            UDTParentColumn pc = new UDTParentColumn();
        //            pc.ParentColumnName = dataItem.Name;
        //            childItem.ParentColumnNames.Add(pc);
        //            addParentColumns(childItem);
        //        }
        //    }
        //}

        private void createSQLDatabase(string DBName)
        {
            using (SqlConnection conn = new SqlConnection())
            {

                conn.ConnectionString = ConfigurationManager.ConnectionStrings["conString"].ConnectionString;
                SqlCommand cmd = new SqlCommand(
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

        private bool TableExists(string tableName)
        {

            string sqlTxt = string.Format(@"select count(*) from 
                (SELECT * FROM UDTUser.dbo.sysobjects WHERE xtype = 'U' AND name = '{0}') rows",
                tableName);

            bool retVal = true;
            using (SqlConnection conn = new SqlConnection())
            {

                conn.ConnectionString = ConfigurationManager.ConnectionStrings["conString"].ConnectionString;

                SqlCommand cmd = new SqlCommand(sqlTxt);

                cmd.Connection = conn;
                conn.Open();
                try
                {
                    int dbCount = (int)cmd.ExecuteScalar();
                    retVal = (dbCount >= 1);

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {

                }
            }
            return retVal;
        }

        private void createDBTable(UDTData dataItem, string dbName, List<Guid> tableGuids)
        {
            string ddl;

            if (tableGuids.Contains(dataItem.objId)) return;

            if (!TableExists(dataItem.Name))
            {
                using (SqlConnection conn = new SqlConnection())
                {
                    ddl = string.Format("USE [{0}] CREATE TABLE {1} (", dbName, dataItem.Name);
                    ddl += string.Format("[Id] [int] IDENTITY(1,1) NOT NULL, ");
                    foreach (UDTBase item in dataItem.ChildData)
                    {
                        if (item.GetType() != typeof(UDTData))
                        {
                            ddl += string.Format("{0} {1}, ", item.Name, item.Type);
                        }
                    }
                    foreach (string colName in dataItem.ParentColumnNames)
                    {
                        ddl += string.Format("{0} int, ", colName);
                    }
                    ddl = ddl.Substring(0, ddl.Length - 2);
                    ddl += "); ";

                    tableGuids.Add(dataItem.objId);

                    conn.ConnectionString = ConfigurationManager.ConnectionStrings["conString"].ConnectionString;
                    SqlCommand cmd = new SqlCommand(ddl);

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

            foreach (UDTBase item in dataItem.ChildData)
            {
                if (item.GetType() == typeof(UDTData))
                {
                    createDBTable(item as UDTData, dbName, tableGuids);
                }
            }
        }

        public void readDatabase(UDTData masterItem)
        {
            DataSet = new System.Data.DataSet(masterItem.Name);
            readTable(DataSet, masterItem, masterItem.Name);
            RaisePropertyChanged("udtDataSet");
        }

        DataTable createDataTable(UDTData dataItem)
        {
            DataTable tbl = new DataTable(dataItem.Name);
            foreach (UDTBase item in dataItem.ChildData)
            {
                if (item.GetType() != typeof(UDTData))
                {
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
                    }
                    else if (item.GetType() == typeof(UDTDecimalItem))
                    {
                        col.DataType = typeof(decimal);
                    }
                    else if (item.GetType() == typeof(UDTIntItem))
                    {
                        col.DataType = typeof(int);
                    }
                    tbl.Columns.Add(col);
                }
            }
            foreach (string colName in dataItem.ParentColumnNames)
            {
                DataColumn col = new DataColumn();
                col.ColumnName = colName;
                col.DataType = typeof(int);
                tbl.Columns.Add(col);
            }
            DataColumn idCol = new DataColumn();
            idCol.ColumnName = "Id";
            idCol.DataType = typeof(int);
            tbl.Columns.Add(idCol);

            return tbl;
        }

        private void readTable(System.Data.DataSet dataSet, UDTData dataItem, string dbName, string parentColName = "", int parentId = -1)
        {
            if (!dataSet.Tables.Contains(dataItem.Name))
                dataSet.Tables.Add(createDataTable(dataItem));
            DataTable dataTable = dataSet.Tables[dataItem.Name];
            using (SqlConnection conn = new SqlConnection())
            {
                conn.ConnectionString = ConfigurationManager.ConnectionStrings["conString"].ConnectionString;
                SqlCommand cmd = new SqlCommand();
                SqlDataReader reader;

                string sqlTxt;
                if (parentId == -1)
                    sqlTxt = string.Format("USE [{0}] select * from {1} ", dbName, dataItem.Name);
                else
                    sqlTxt = string.Format("USE [{0}] select * from {1} where {2} = {3} ", dbName,
                        dataItem.Name, parentColName, parentId);

                cmd.CommandText = sqlTxt;
                cmd.CommandType = CommandType.Text;
                cmd.Connection = conn;
                conn.Open();

                reader = cmd.ExecuteReader();
                // Data is accessible through the DataReader object here.      
                try
                {
                    dataTable.Load(reader);
                    foreach (UDTBase childItem in dataItem.ChildData)
                    {
                        if (childItem.GetType() == typeof(UDTData))
                        {
                            foreach (DataRow row in dataTable.Rows)
                            {
                                readTable(dataSet, childItem as UDTData, dbName, dataItem.Name, (int)row["Id"]);
                            }
                        }
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
    }
}
