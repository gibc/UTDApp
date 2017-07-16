using Prism.Mvvm;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;

namespace UDTApp.Models
{
    public enum ObjectState { New, Updated, Dirty };

    public class ModelBase : BindableBase
    {
        public ModelBase() { }

        private int _id = -1;
        public int Id { get { return _id; } set { _id = value; } }

        private int _parentId = -1;
        public int ParentId { get { return _parentId; } set { _parentId = value; } }

        private ObjectState _state = ObjectState.New;
        public ObjectState State { get { return _state; } set { _state = value; } }

        public static bool _tableUpdated = false;

        public void GetData(object vmObj)
        {
            List<PropertyInfo> modelPropsList = new List<PropertyInfo>(this.GetType().GetProperties());
            List<PropertyInfo> vmPropsList = new List<PropertyInfo>(vmObj.GetType().GetProperties());
            foreach (PropertyInfo modelProp in modelPropsList)
            {
                if (IsRecordProperty(modelProp))
                {
                    PropertyInfo vmProp = vmPropsList.Find(p => p.Name == modelProp.Name);
                    vmProp.SetValue(vmObj, modelProp.GetValue(this));
                }
            }
            this.State = ObjectState.Updated;
        }

        public void PutData(object vmObj)
        {
            List<PropertyInfo> modelPropsList = new List<PropertyInfo>(this.GetType().GetProperties());
            List<PropertyInfo> vmPropsList = new List<PropertyInfo>(vmObj.GetType().GetProperties());
            foreach (PropertyInfo modelProp in modelPropsList)
            {
                if (IsRecordProperty(modelProp))
                {
                    PropertyInfo vmProp = vmPropsList.Find(p => p.Name == modelProp.Name);
                    SetPropValue(modelProp.Name, vmProp.GetValue(vmObj, null));
                }
            }
            this.State = ObjectState.Dirty;
        }

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

        static List<PropertyInfo> _propList = null;
        static private List<PropertyInfo> basePropList {
            get
            {
                if (_propList == null)
                {
                    Type baseType = typeof(ModelBase);
                    PropertyInfo[] baseProps = baseType.GetProperties();
                    _propList = new List<PropertyInfo>(baseProps);
                }
                return _propList;
            }
        } 

        static public bool IsRecordProperty(PropertyInfo prop)
        {
            if (prop.PropertyType.Name.Contains("Collection")) return false;
            return !(basePropList.Any(p => p.Name == prop.Name) &&
                prop.Name != "ParentId");
        }

        static public bool IsRecordProperty(DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.PropertyType.Name.Contains("Collection")) return false;
            return !basePropList.Any(p => p.Name == e.PropertyName); 
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
                if(!IsRecordProperty(prop)) continue;
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

        static public void SaveObjects<T>(ObservableCollection<T> objCol)
        {
            List<T> objList = objCol.ToList();
            SaveRecords(objList);
        }

        static private void SaveRecords(IList objList, int parentId = -1)
        {
            int recId = -1;
            foreach(object obj in objList)
            {
                Type type = obj.GetType();
                ModelBase modelBase = obj as ModelBase;
                if (modelBase.State == ObjectState.New) recId = modelBase.CreateRecord(parentId);
                else if (modelBase.State == ObjectState.Dirty) recId = modelBase.UpdateRecord();

                PropertyInfo[] props = type.GetProperties();
                foreach (var prop in props)
                {
                    if (prop.PropertyType.Name.Contains("Collection"))
                    {
                        Type genericListType = typeof(List<>);
                        Type[] typeArgs = { prop.PropertyType.GenericTypeArguments[0] };
                        Type constructed = genericListType.MakeGenericType(typeArgs);

                        var col = modelBase.GetPropValue(prop.Name);

                        IList customListInstance = (IList)Activator.CreateInstance(constructed, col);
                        SaveRecords(customListInstance, modelBase.Id);
                    }
                }
            }
        }

        static public void DeleteRecords(IList objList)
        {
            foreach (object obj in objList)
            {
                Type type = obj.GetType();
                ModelBase modelBase = obj as ModelBase;
                if (modelBase.State != ObjectState.New) modelBase.DeleteRecord();

                PropertyInfo[] props = type.GetProperties();
                foreach (var prop in props)
                {
                    if (prop.PropertyType.Name.Contains("Collection"))
                    {
                        Type genericListType = typeof(List<>);
                        Type[] typeArgs = { prop.PropertyType.GenericTypeArguments[0] };
                        Type constructed = genericListType.MakeGenericType(typeArgs);

                        var col = modelBase.GetPropValue(prop.Name);

                        IList customListInstance = (IList)Activator.CreateInstance(constructed, col);
                        DeleteRecords(customListInstance);
                    }
                }
            }
        }

        static public ObservableCollection<T> LoadObjects<T>(int parentId = -1)
        {
            List<T> dataList = new List<T>();
            ReadRecords(dataList, typeof(T), parentId);
            return new ObservableCollection<T>(dataList);
        }

        static private void ReadRecords(IList recs, Type type, int parentId = -1)
        {
            PropertyInfo[] props = type.GetProperties();

            using (SqlConnection conn = new SqlConnection())
            {
                conn.ConnectionString = ConfigurationManager.ConnectionStrings["conString"].ConnectionString;
                SqlCommand cmd = new SqlCommand();
                SqlDataReader reader;

                if(parentId == -1) 
                    cmd.CommandText = string.Format("SELECT * FROM {0}", type.Name);
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
                        ModelBase modelBase = instance as ModelBase;
                        modelBase.State = ObjectState.Updated;

                        foreach(var prop in props)
                        {
                            if (prop.PropertyType.Name.Contains("Collection"))
                            {
                                Type genericListType = typeof(List<>);
                                Type[] typeArgs = { prop.PropertyType.GenericTypeArguments[0] };
                                Type constructed = genericListType.MakeGenericType(typeArgs);

                                IList customListInstance = (IList)Activator.CreateInstance(constructed);
                                ReadRecords(customListInstance, prop.PropertyType.GenericTypeArguments[0], (int)reader["Id"]);

                                Type genericCollectionType = typeof(ObservableCollection<>);
                                constructed = genericCollectionType.MakeGenericType(typeArgs);
                                var customCollectionInstance = Activator.CreateInstance(constructed, customListInstance);

                                modelBase.SetPropValue(prop.Name, customCollectionInstance); 
                                continue;
                            }
                            if(IsRecordProperty(prop))
                                modelBase.SetPropValue(prop.Name, reader[prop.Name]); 
                            if(prop.Name == "Id")
                                modelBase.SetPropValue(prop.Name, reader[prop.Name]); 

                        }
                        recs.Add(instance);
                    }
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    reader.Close();
                }
            }
        }

        public void DeleteRecord()
        {
            //DELETE FROM table_name
            //WHERE condition;
            using (SqlConnection conn = new SqlConnection())
            {
                conn.ConnectionString = ConfigurationManager.ConnectionStrings["conString"].ConnectionString;
                SqlCommand cmd = new SqlCommand();

                string cmdText = string.Format("DELETE FROM {0} WHERE Id = {1} ", 
                    this.GetType().Name, this.Id);

                cmd.CommandText = cmdText;
                cmd.CommandType = CommandType.Text;
                cmd.Connection = conn;

                conn.Open();

                cmd.ExecuteNonQuery();

            }
        }

        public int UpdateRecord()
        {
            //UPDATE table_name
            //SET column1 = value1, column2 = value2, ...
            //WHERE condition;
            using (SqlConnection conn = new SqlConnection())
            {
                conn.ConnectionString = ConfigurationManager.ConnectionStrings["conString"].ConnectionString;
                SqlCommand cmd = new SqlCommand();

                string cmdText = string.Format("UPDATE {0} SET ", this.GetType().Name);
                PropertyInfo[] props = this.GetType().GetProperties();
                foreach (var prop in props)
                {
                    if (!IsRecordProperty(prop))
                    {
                        if (prop == props.Last())
                        {
                            cmdText = cmdText.TrimEnd(' ');
                            cmdText = cmdText.TrimEnd(',');
                        }
                        continue;
                    }
                    cmdText += string.Format("{0} = {1}", prop.Name, GetPropValue(prop.Name));
                    if (prop != props.Last()) cmdText += ", ";
                }
                cmdText += string.Format(" WHERE Id = {0}", Id);

                cmd.CommandText = cmdText;
                cmd.CommandType = CommandType.Text;
                cmd.Connection = conn;

                conn.Open();

                cmd.ExecuteNonQuery();

                State = ObjectState.Updated;

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
                return Id;
            }
        }

        public int CreateRecord(int parentID)
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
                    if (!IsRecordProperty(prop))
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
                    if (!IsRecordProperty(prop))
                    {
                       if (prop == props.Last())
                       {
                           cmdText = cmdText.TrimEnd(' ');
                           cmdText = cmdText.TrimEnd(',');
                       }
                       continue;
                    }
                    if(prop.Name == "ParentId")
                        cmdText += string.Format("{0}", parentID);
                    else
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

                State = ObjectState.Updated;

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
