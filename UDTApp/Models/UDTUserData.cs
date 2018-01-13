using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data.Common;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace UDTApp.Models
{
    public class UDTUserData
    {
        ObservableCollection<DataSet> DataDef;
        public UDTUserData(ObservableCollection<DataSet> dataDef)
        {
            DataDef = dataDef;
            CreateDatabase();
            foreach(DataSet dataSet in dataDef)
            {
                CreateTable(dataSet);
            }
            // for each DataSet create DB table
            // for each DataSet do db read operation where
            //  DataSet name = table name 
            //  each DataItem name is column name
            // select * from <TableName>
            //  for ecah read op
            //    for each child DataItem
            //    obj.<IDataItem.Name> = reader[DataItem.Name]
            //  end loop
            //  add obj to UDTData.<TableName> list
            //
            //  each recrod is ExpandoObject with DataItem properties
            //  each ExpandoObject is added to Expando table object colletion
            //
            // UDTData.<TableName> = List<ExpandoObject> -- list of records for <TableName>
            // List<ExpandoObject> UDTDataList == list of UDTData.<TableName> objects
            // 
        }

        private void CreateDatabase()
        {
            //using (SqlConnection conn = new SqlConnection())
            using (DbConnection conn = UDTDataSet.dbProvider.Conection)
            {

                //conn.ConnectionString = ConfigurationManager.ConnectionStrings["conString"].ConnectionString;
                conn.ConnectionString = UDTDataSet.dbProvider.ConnectionString;
                //SqlCommand cmd = new SqlCommand("select count(*) from (select * from sys.databases where name = 'UDTUser') rows");
                DbCommand cmd = UDTDataSet.dbProvider.GetCommand("select count(*) from (select * from sys.databases where name = 'UDTUser') rows");
                cmd.Connection = conn;
                conn.Open();
                try
                {
                    int dbCount = (int)cmd.ExecuteScalar();
                    if(dbCount < 1)
                    {
                        cmd.CommandText = "CREATE DATABASE UDTUser";
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

        public bool TableExists(DataSet tableDef)
        {

            string sqlTxt = string.Format(@"select count(*) from 
                (SELECT * FROM UDTUser.dbo.sysobjects WHERE xtype = 'U' AND name = '{0}') rows", 
                tableDef.Name);

            bool retVal = true; 
            using (SqlConnection conn = new SqlConnection())
            {

                //conn.ConnectionString = ConfigurationManager.ConnectionStrings["conString"].ConnectionString;
                conn.ConnectionString = UDTDataSet.dbProvider.ConnectionString;
                //SqlCommand cmd = new SqlCommand(sqlTxt);
                DbCommand cmd = UDTDataSet.dbProvider.GetCommand(sqlTxt);

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

        private string TableDDL(DataSet tableDef)
        {
            string ddl = string.Format("CREATE TABLE UDTUser.dbo.[{0}] ( ", tableDef.Name);
            ddl += "[Id] [int] IDENTITY(1,1) NOT NULL, [ParentId] [int] NULL, ";
            foreach(DataItem item in tableDef.DataItems)
            {
                ddl += string.Format("{0} ", item.Name);
                ddl += GetDBType(item.Type);
                if (item != tableDef.DataItems.Last()) ddl += ", ";
            }
            ddl += ") ";
            return ddl;
        }

        private List<string> GetColumns(string table)
        {

            string sqlTxt = UDTDataSet.dbProvider.adjSQL(string.Format(@"USE [UDTUser] SELECT * FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_NAME = '{0}'
                    ORDER BY ORDINAL_POSITION", table));

            List<string> colList = new List<string>();
            using (SqlConnection conn = new SqlConnection())
            {

                conn.ConnectionString = ConfigurationManager.ConnectionStrings["conString"].ConnectionString;

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

        private string GetDBType(int type)
        {
            switch (type)
            {
                case 1:
                    return "[varchar](255) NULL ";
                case 2:
                    return "[int] NULL ";
                case 3:
                    return "[decimal](10, 5) NULL ";
            }
            return "[varchar](255) NULL ";
        }

        private void AddColumn(string table, DataItem itemDef)
        {
            //ALTER TABLE table_name ADD column_name datatype;

            string sqlTxt = UDTDataSet.dbProvider.adjSQL(string.Format(@"USE [UDTUser] ALTER TABLE {0} ", table));
            sqlTxt += string.Format("ADD {0} ", itemDef.Name);
            sqlTxt += string.Format("{0} ", GetDBType(itemDef.Type));

            using (SqlConnection conn = new SqlConnection())
            {

                conn.ConnectionString = ConfigurationManager.ConnectionStrings["conString"].ConnectionString;

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

        private void CreateTable(DataSet tableDef)
        {
            if (!TableExists(tableDef))
            {
                using (SqlConnection conn = new SqlConnection())
                {

                    conn.ConnectionString = ConfigurationManager.ConnectionStrings["conString"].ConnectionString;

                    //SqlCommand cmd = new SqlCommand(TableDDL(tableDef));
                    DbCommand cmd = UDTDataSet.dbProvider.GetCommand(TableDDL(tableDef));

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
            // if table exits check for and add any new columns
            else
            {
                List<string> colList = GetColumns(tableDef.Name);

                foreach(DataItem dataItem in tableDef.DataItems)
                { 
                    if(!colList.Contains(dataItem.Name))
                    {
                        AddColumn(tableDef.Name, dataItem);
                    }
                }
            }
        }

        private List<ExpandoObject> UDTDataList = new List<ExpandoObject>();
    }
}
