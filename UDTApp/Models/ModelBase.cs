using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UDTApp.Models
{
    public class ModelBase : BindableBase
    {
        public ModelBase() { }

        private int _id = -1;
        public int Id { get { return _id; } set { _id = value; } }

        private int _parentId = -1;
        public int ParentId { get { return _parentId; } set { _parentId = value; } }

        public static bool _tableUpdated = false;

        public object GetPropValue(string propName)
        {
            var val = this.GetType().GetProperty(propName).GetValue(this, null);
            if (val is string)
                val = string.Format("'{0}'", val);
            return val;
        }

        public void SetPropValue(string propName, object value)
        {
            this.GetType().GetProperty(propName).SetValue(this, value);
        }

        private Dictionary<string, string> typeDic = null;
        private string getDDLType(string key)
        {
            if (typeDic == null)
            {
                typeDic = new Dictionary<string, string>()
                {
                    { "String", "varchar(255)" },
                    { "Int32", "int" },
                };
            }
            if (typeDic.ContainsKey(key))
                return typeDic[key];
            return typeDic["String"];
        }


        public string CreateDDL()
        {
            //CREATE TABLE Persons (
            //   [RecordId] [int] IDENTITY(1,1) NOT NULL,
            //    PersonID int,
            //    LastName varchar(255),
            //    FirstName varchar(255),
            //    Address varchar(255),
            //    City varchar(255) 
            //);
            string tableName = this.GetType().Name;
            string ddl = string.Format(@"IF OBJECT_ID('{0}', 'U') IS NOT NULL DROP TABLE {0}; CREATE TABLE {0} (", tableName);
            PropertyInfo[] props = this.GetType().GetProperties();
            foreach (var prop in props)
            {
                if(prop.PropertyType.Name.Contains("Collection")) continue;
                ddl += string.Format("{0} ", prop.Name);
                var val = GetPropValue(prop.Name);
                ddl += string.Format("{0} ", getDDLType(prop.PropertyType.Name));
                if (prop.Name == "Id")
                    ddl += "IDENTITY(1,1) NOT NULL";
                if (prop != props.Last()) ddl += ", ";
            }
            ddl += ");";
            return ddl;
        }

        public void CreateTable()
        {
            using (SqlConnection conn = new SqlConnection())
            {
                if (_tableUpdated) return;

                conn.ConnectionString = ConfigurationManager.ConnectionStrings["conString"].ConnectionString;
                SqlCommand cmd = new SqlCommand(CreateDDL());

                cmd.Connection = conn;
                conn.Open();
                cmd.ExecuteNonQuery();

                _tableUpdated = true;

                // Data is accessible through the DataReader object here.      
                //try
                //{
                //    while (reader.Read())
                //    {
                //        _fieldName = reader["fieldName"].ToString();
                //        _fieldValue = (int)reader["fieldValue"];
                //    }
                //}
                //catch
                //{
                //    MessageBox.Show("Data read failed");
                //}
                //finally
                //{
                //    // Always call Close when done reading.
                //    reader.Close();
                //}
            }
        }

        //static private Type classType;

        static public ObservableCollection<object> ReadRecords(Type type, int parentId = -1)
        {
            ObservableCollection<object> records = new ObservableCollection<object>();

            PropertyInfo[] props = type.GetProperties();

            using (SqlConnection conn = new SqlConnection())
            {
                conn.ConnectionString = ConfigurationManager.ConnectionStrings["conString"].ConnectionString;
                SqlCommand cmd = new SqlCommand();
                SqlDataReader reader;

                if(parentId == -1) cmd.CommandText = string.Format("SELECT * FROM {0}", type.Name);
                else
                    cmd.CommandText = string.Format("SELECT * FROM {0} WHERE ParentId = {1}", type.Name, parentId);
                cmd.CommandType = CommandType.Text;
                cmd.Connection = conn;

                conn.Open();

                reader = cmd.ExecuteReader();
                // Data is accessible through the DataReader object here.      
                try
                {
                    while (reader.Read())
                    {
                        var instance = Activator.CreateInstance(type);
                        ModelBase basse = instance as ModelBase;

                        foreach(var prop in props)
                        {
                            if (prop.PropertyType.Name.Contains("Collection")) continue;
                            basse.SetPropValue(prop.Name, reader[prop.Name]);                            
                        }
                        records.Add(instance);
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
            return records;
        }

        public int CreateRecord()
        {

            //INSERT INTO table_name VALUES (value1, value2, value3, ...);
            //INSERT INTO Table1(col2,col4) VALUES (1,2)
            using (SqlConnection conn = new SqlConnection())
            {
                conn.ConnectionString = ConfigurationManager.ConnectionStrings["conString"].ConnectionString;
                SqlCommand cmd = new SqlCommand();

                string cmdText = string.Format("INSERT INTO {0} (", this.GetType().Name);
                PropertyInfo[] props = this.GetType().GetProperties();
                foreach (var prop in props)
                {
                    if (prop.Name == "Id" || prop.PropertyType.Name.Contains("Collection"))
                    {
                       if (prop == props.Last())
                       {
                           cmdText = cmdText.TrimEnd(' ');
                           cmdText = cmdText.TrimEnd(',');
                       }
                       continue;
                    }
                    cmdText += string.Format("{0}", prop.Name);
                    if (prop != props.Last()) cmdText += ", ";
                }
                cmdText += ") VALUES (";
                foreach (var prop in props)
                {
                    if(prop.Name == "Id" || prop.PropertyType.Name.Contains("Collection"))
                    {
                       if (prop == props.Last())
                       {
                           cmdText = cmdText.TrimEnd(' ');
                           cmdText = cmdText.TrimEnd(',');
                       }
                       continue;
                    }
                    cmdText += string.Format("{0}", GetPropValue(prop.Name));
                    if (prop != props.Last()) cmdText += ", ";
                }
                cmdText += "); SELECT SCOPE_IDENTITY();";

                cmd.CommandText = cmdText;
                cmd.CommandType = CommandType.Text;
                cmd.Connection = conn;

                conn.Open();

                //cmd.ExecuteNonQuery();
                decimal recId = (decimal)cmd.ExecuteScalar();

                // Data is accessible through the DataReader object here.      
                //try
                //{
                //    while (reader.Read())
                //    {
                //        //_fieldName = reader["fieldName"].ToString();
                //        //_fieldValue = (int)reader["fieldValue"];
                //    }
                //}
                //catch
                //{
                //    //MessageBox.Show("Data read failed");
                //}
                //finally
                //{
                //    // Always call Close when done reading.
                //    reader.Close();
                //}
                return Convert.ToInt32(recId);
            }
        }
    }
}
