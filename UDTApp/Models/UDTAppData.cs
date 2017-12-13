using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace UDTApp.Models
{
    public class UDTAppData
    {
        private string _fieldName;
        private int _fieldValue;

        public UDTAppData()
        {
            //using (SqlConnection conn = new SqlConnection())
            using (DbConnection conn = UDTDataSet.dbProvider.Conection)
            {
                conn.ConnectionString = ConfigurationManager.ConnectionStrings["conString"].ConnectionString;
                //SqlCommand cmd = new SqlCommand();
                DbCommand cmd = UDTDataSet.dbProvider.Command;
                //SqlDataReader reader;
                DbDataReader reader = UDTDataSet.dbProvider.Reader;

                cmd.CommandText = "SELECT * FROM TableOne";
                cmd.CommandType = CommandType.Text;
                cmd.Connection = conn;

                conn.Open();

                reader = cmd.ExecuteReader();
                // Data is accessible through the DataReader object here.      
                try
                {
                    while (reader.Read())
                    {
                        _fieldName = reader["fieldName"].ToString();
                        _fieldValue = (int)reader["fieldValue"];
                    }
                }
                catch
                {
                    MessageBox.Show("Data read failed");
                }
                finally
                {
                    // Always call Close when done reading.
                    reader.Close();
                }
            }


        }

        public void SaveData()
        {
            //using (SqlConnection conn = new SqlConnection())
            using (DbConnection conn = UDTDataSet.dbProvider.Conection)
            {
                conn.ConnectionString = ConfigurationManager.ConnectionStrings["conString"].ConnectionString;
                //SqlCommand cmd = new SqlCommand();
                DbCommand cmd = UDTDataSet.dbProvider.Command;


                cmd.CommandText = string.Format("UPDATE TableOne set fieldName = '{0}'", _fieldName);
                cmd.CommandType = CommandType.Text;
                cmd.Connection = conn;

                conn.Open();

                try
                {
                    int rows = cmd.ExecuteNonQuery();
                }
                catch
                {
                    MessageBox.Show("Data save failed");
                }
                finally
                {
                }
            }

        }

        public string FieldName
        {
            get { return _fieldName; }
            set { _fieldName = value ; }
        }

        public string FieldValue
        {
            get { return _fieldValue.ToString(); }
            set { _fieldValue = Int32.Parse(value); }
        }
    }
}
