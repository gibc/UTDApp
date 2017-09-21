using Microsoft.Win32;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Serialization;
using UDTApp.Models;
//using UDTApp.Models;

namespace UDTApp.ViewModels
{
    public class PageZeroViewModel : ValidatableBindableBase
    {
        public DelegateCommand<MouseEventArgs> MouseMoveCommand { get; set; }
        public DelegateCommand<DragEventArgs> DragEnterCommand { get; set; }
        public DelegateCommand<DragEventArgs> DragDropCommand { get; set; }
        public DelegateCommand<DragEventArgs> DragOverCommand { get; set; }
        public DelegateCommand SaveToXmlCommand { get; set; }
        public DelegateCommand ReadFromFileCommand { get; set; }
        public DelegateCommand CreateDataBaseCommand { get; set; }
        public DelegateCommand ReadDataBaseCommand { get; set; }
        public DelegateCommand WindowLoadedCommand { get; set; }


        public PageZeroViewModel()
        {
            MouseMoveCommand = new DelegateCommand<MouseEventArgs>(mouseMove);
            DragEnterCommand = new DelegateCommand<DragEventArgs>(dragEnter);
            DragDropCommand = new DelegateCommand<DragEventArgs>(dragDrop);
            DragOverCommand = new DelegateCommand<DragEventArgs>(dragOver);
            SaveToXmlCommand = new DelegateCommand(saveToXml);
            ReadFromFileCommand = new DelegateCommand(readFromXml);
            CreateDataBaseCommand = new DelegateCommand(createDatabase);
            ReadDataBaseCommand = new DelegateCommand(readDatabase);
            WindowLoadedCommand = new DelegateCommand(windowLoaded);

            viewModel = this;

        }

        public static PageZeroViewModel viewModel = null;
        
        private UDTBase _currentEditItem = null;
        public UDTBase currentEditItem 
        {
            get { return _currentEditItem; }
            set { SetProperty(ref _currentEditItem, value); }
        }

        private bool _anyErrors = false;
        public bool AnyErrors 
        { 
            get 
            {
                return _anyErrors; 
            }
            set { SetProperty(ref _anyErrors, value); }
        }

        
        private void setAnyErrors(bool value)
        {
            if (value)
                AdornerType = typeof(NoteAdorner);
            else
                AdornerType = null;
                //AdornerType = typeof(HideAdorer);
            
        }

        private Type _adornerType = typeof(NoteAdorner); 
        public Type AdornerType
        {
            get 
            {
                //return typeof(NoteAdorner); 
                return _adornerType;
            }
            set 
            { 
                SetProperty(ref _adornerType, value);
                if(value == null)
                    Debug.Write(string.Format("Set AdornerType NULL\r"));
                else
                    Debug.Write(string.Format("Set AdornerType {0}\r", value));

            }
        } 

        private void saveToXml()
        {
            //string xml = SerializeToString(SchemaList);
            ////SchemaList = readFromXml(xml);

            //SaveFileDialog saveFileDialog = new SaveFileDialog();
            //saveFileDialog.Filter = "Xml (*.xml)|*.xml";
            //if (saveFileDialog.ShowDialog().Value)
            //{
            //    FileStream xmlFile = File.Open(saveFileDialog.FileName, FileMode.OpenOrCreate);
            //    Byte[] info = new UTF8Encoding(true).GetBytes(xml);
            //    xmlFile.Write(info, 0, info.Length);
            //    xmlFile.Close();
            //}

            UDTXml.UDTXmlData.saveToXml(SchemaList);
        }

        void readFromXml()
        {
            //OpenFileDialog openFileDialog = new OpenFileDialog();
            //openFileDialog.Filter = "Xml (*.xml)|*.xml";
            //if (openFileDialog.ShowDialog().Value)
            //{
            //    StreamReader xmlFile = File.OpenText(openFileDialog.FileName);
            //    string xml = xmlFile.ReadToEnd();
            //    xmlFile.Close();

            //    List<UDTBase> schema = readFromXml(xml);
            //    SchemaList = schema;
            //}

            List<UDTBase> schema = UDTXml.UDTXmlData.readFromXml();
            if (schema != null) SchemaList = schema;
        }

        

        private static string SerializeToString(object obj)
        {
            XmlSerializer serializer = new XmlSerializer(obj.GetType());
 
            using (StringWriter writer = new StringWriter())
            {
                serializer.Serialize(writer, obj);
 
                return writer.ToString();
            }   
        }

        private List<UDTBase> readFromXml(string xml)
        {
            var serializer = new XmlSerializer(typeof(List<UDTBase>));

            List<UDTBase> result;

            using (TextReader reader = new StringReader(xml))
            {
                result = serializer.Deserialize(reader) as List<UDTBase>;
            }

            setParentRefs(result[0] as UDTData);

            return result;
        }

        //private void setParentRefs(UDTData dataItem)
        //{
        //    foreach (UDTBase child in dataItem.ChildData)
        //    {
        //        if (child.GetType() == typeof(UDTData))
        //        {
        //            //UDTData childData = child as UDTData;
        //            //childData.ParentColumnNames.Add(dataItem.Name);
        //            setParentRefs(child as UDTData);
        //        }
        //        child.parentObj = dataItem;
        //    }
        //}

        private void setParentRefs(UDTData dataItem)
        {
            foreach (UDTBase child in dataItem.columnData)
            {
                child.parentObj = dataItem;
            }

            foreach(UDTData child in dataItem.tableData)
            {
                setParentRefs(child);
                child.parentObj = dataItem;
            }
        }

        private void createDatabase()
        {
            //createSQLDatabase(SchemaList[0].Name);
            //List<Guid> tableGuids = new List<Guid>();
            //createDBTable(SchemaList[0] as UDTData, SchemaList[0].Name, tableGuids);

            UDTDataSet.udtDataSet.createDatabase(SchemaList[0] as UDTData);
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

        public bool TableExists(string tableName)
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

            if(!TableExists(dataItem.Name))
            {
                using (SqlConnection conn = new SqlConnection())
                {
                    ddl = string.Format("USE [{0}] CREATE TABLE {1} (", dbName, dataItem.Name);
                    ddl += string.Format("[Id] [int] IDENTITY(1,1) NOT NULL, ");
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

            //foreach (UDTBase item in dataItem.ChildData)
            foreach (UDTData item in dataItem.tableData)
            {
                //if (item.GetType() == typeof(UDTData))
                //{
                    createDBTable(item as UDTData, dbName, tableGuids);                  
                //}
            }
        }

        private void windowLoaded()
        {
            SchemaList = UDTXml.UDTXmlData.SchemaData;
        }

        private void readDatabase()
        {
            //System.Data.DataSet dataSet = new System.Data.DataSet(SchemaList[0].Name);
            //readTable(dataSet, SchemaList[0] as UDTData, SchemaList[0].Name);

            UDTDataSet.udtDataSet.readDatabase(SchemaList[0] as UDTData);
        }

        DataTable createDataTable(UDTData dataItem)
        {
            DataTable tbl = new DataTable(dataItem.Name);
            //foreach (UDTBase item in dataItem.ChildData)
            foreach(UDTBase item in dataItem.columnData)
            {
                //if(item.GetType() != typeof(UDTData))
                //{
                    DataColumn col = new DataColumn();
                    col.ColumnName = item.Name;
                    col.DataType = typeof(int);
                    if(item.GetType() == typeof(UDTTxtItem))
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
                //}
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
            if(!dataSet.Tables.Contains(dataItem.Name))
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
                    //foreach (UDTBase childItem in dataItem.ChildData)
                    foreach (UDTData childItem in dataItem.tableData)
                    {
                        //if (childItem.GetType() == typeof(UDTData))
                        //{
                            foreach(DataRow row in dataTable.Rows)
                            {
                                readTable(dataSet, childItem as UDTData, dbName, dataItem.Name, (int)row["Id"]);
                            }
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

        public Collection<UDTBase> UDTItems {
            get { return UDTItemList.ItemList; }
        }

        private ObservableCollection<UDTBase> DbSchema = new ObservableCollection<UDTBase>();

        public static List<UDTBase> _schemaList = new List<UDTBase>();
        public List<UDTBase> SchemaList {
            get { return _schemaList; }
            set
            {
                SetProperty(ref _schemaList, value);
            }
        }

        private void dragOver(DragEventArgs dragArgs)
        {
            Button btn = dragArgs.Source as Button;
            dragArgs.Effects = DragDropEffects.Copy;
        }

        //private void sizeChange(EventArgs sizeArgs)
        //{

        //}

        private void dragDrop(DragEventArgs dragArgs)
        {
            Button btn = dragArgs.Source as Button;
            if (!dragArgs.Handled && btn != null)
            {
                ObservableCollection<UDTBase> col = Ex.GetSecurityId(btn);
                UDTData dataItem = (UDTData)dragArgs.Data.GetData(typeof(UDTData));
                col.Add(dataItem);
                dragArgs.Handled = true;
                _currentItem = null;
            }
        }


        private UDTBase _currentItem = null;
        private void dragEnter(DragEventArgs dragArgs)
        {
            Button btn = dragArgs.Source as Button;
            if (btn != null)
            {
 
                string[] frmts = dragArgs.Data.GetFormats();
                if (dragArgs.Data.GetDataPresent(typeof(UDTData)))
                {
                    UDTData dataItem = (UDTData)dragArgs.Data.GetData(typeof(UDTData));
                    _currentItem = dataItem as UDTBase;

 
                }
            }
        }

        private bool inMove = false;
        private void mouseMove(MouseEventArgs data)
        {

            Button btn = data.Source as Button;
            ObservableCollection<UDTBase> col = Ex.GetSecurityId(btn);
            //ObservableCollection<UDTData> col = UTDDataColProp.GetDataCol(btn);

            if (btn != null && data.LeftButton == MouseButtonState.Pressed && !inMove)
            {
                inMove = true;
                Debug.Write(string.Format(">>>Enter mouseMove\r", _currentItem));

                DragDrop.DoDragDrop(btn,
                                 new UDTData(),
                                 DragDropEffects.Copy);

                Debug.Write(string.Format("<<<Exit mouseMove\r", _currentItem));
                data.Handled = true;
                inMove = false;
            }
        }
    }
}
