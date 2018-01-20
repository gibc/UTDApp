using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;
using UDTApp.DataBaseProvider;
using UDTApp.Settings;

namespace UDTApp.Models
{
    public class UDTXml
    {
        private UDTXml()
        {
            SchemaData = new List<UDTBase>();
        }

        private static UDTXml _UDTXmlData = null;
        public static UDTXml UDTXmlData
        {
            get
            {
                if(_UDTXmlData == null)
                {
                    _UDTXmlData = new UDTXml();
                }
                return _UDTXmlData;
            }
        }

        public List<UDTBase> SchemaData
        {
            get;
            set;
        }

        public bool saveToXml(List<UDTBase> SchemaList)
        {
            string xml = SerializeToString(SchemaList);

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Xml (*.xml)|*.xml";
            if (saveFileDialog.ShowDialog().Value)
            {
                //FileStream xmlFile = File.Open(saveFileDialog.FileName, FileMode.OpenOrCreate);
                FileStream xmlFile = File.Open(saveFileDialog.FileName, FileMode.Create);
                Byte[] info = new UTF8Encoding(true).GetBytes(xml);
                xmlFile.Write(info, 0, info.Length);
                xmlFile.Close();
                SchemaData = SchemaList;
                return true;
            }
            return false;
        }

        public bool saveToXml(List<UDTBase> SchemaList, string filePath)
        {
            try
            { 
                string xml = SerializeToString(SchemaList);
                FileStream xmlFile = File.Open(filePath, FileMode.Create);
                Byte[] info = new UTF8Encoding(true).GetBytes(xml);
                xmlFile.Write(info, 0, info.Length);
                xmlFile.Close();
                SchemaData = SchemaList;
                return true;
            }
            catch(Exception ex)
            {
                string msg = string.Format("saveToXml failed: {0}", ex.Message);
                UDTApp.Log.Log.LogMessage(msg);
                MessageBox.Show(msg);
                return false;
            }
        }


        public List<UDTBase> newProject(string ProjectName, DBType dbType, string serverName)
        {
            SchemaData = new List<UDTBase>();
            UDTData baseObj = new UDTData();
            baseObj.dbType = dbType;
            baseObj.serverName = serverName;
            baseObj.ToolBoxItem = false;
            baseObj.Name = ProjectName;
            baseObj.parentObj = null;
            baseObj.AnyErrors = false;
            baseObj.EditBoxEnabled = true;
            baseObj.TypeName = UDTTypeName.DataBase;
            baseObj.ValidateProperty("Name");
            //baseObj.PopUpOpen = false;
            SchemaData.Add(baseObj);
            return SchemaData;
        }

        public List<UDTBase> readFromXml()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Xml (*.xml)|*.xml";
            if (openFileDialog.ShowDialog().Value)
            {
                AppSettings.appSettings.addFile(openFileDialog.FileName);
                //StreamReader xmlFile = File.OpenText(openFileDialog.FileName);
                //string xml = xmlFile.ReadToEnd();
                //xmlFile.Close();

                //List<UDTBase> schema = readFromXml(xml);
                //SchemaData = schema;
                //return SchemaData;
                return openProject(openFileDialog.FileName);
            }
            return null;
        }

        public List<UDTBase> openProject(string filePath)
        {
            StreamReader xmlFile = File.OpenText(filePath);
            string xml = xmlFile.ReadToEnd();
            xmlFile.Close();
            List<UDTBase> schema = null;
            try
            {
                schema = readFromXml(xml);
                SchemaData = schema;
            }
            catch(Exception ex)
            {
                MessageBox.Show("Error: not a valid UDT project file.", "Invalid File", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                FileSetting fileSetting = AppSettings.appSettings.fileSettings.FirstOrDefault(p => p.filePath == filePath);
                if(fileSetting != null)
                {
                    AppSettings.appSettings.fileSettings.Remove(fileSetting);
                }

            }
            //SchemaData = schema;
            return SchemaData;
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

            UDTData schema = result[0] as UDTData;
            schema.setAllSavedProps();

            return result;
        }

        //private void setParentRefs(UDTData dataItem)
        //{
        //    //dataItem.tableData.Add(new UDTTableView(dataItem.ChildData));
          
        //    foreach (UDTBase child in dataItem.ChildData)
        //    {
        //        if (child.GetType() == typeof(UDTData))
        //        {
        //            //UDTData childData = child as UDTData;
        //            //childData.ParentColumnNames.Add(dataItem.Name);
        //            dataItem.tableData.Add(child as UDTData);
        //            setParentRefs(child as UDTData);
        //        }
        //        else
        //            dataItem.columnData.Add(child);

        //        child.parentObj = dataItem;

        //    }

        //    List<UDTBase> childList = dataItem.ChildData.ToList();
        //    childList.Sort((x, y) => x.sortOrder.CompareTo(y.sortOrder));
        //    dataItem.ChildData = new ObservableCollection<UDTBase>(childList);

            
        //}

        private void setParentRefs(UDTData dataItem)
        {
            // TBD: load backup savName, savColumnData, savTableData
            //dataItem.savName = dataItem.Name;
            //dataItem.savColumnData = new ObservableCollection<UDTBase>(dataItem.columnData);
            //dataItem.savTableData = new ObservableCollection<UDTData>(dataItem.tableData);
            foreach (UDTBase child in dataItem.columnData)
            {
                child.parentObj = dataItem;
                //child.savName = child.Name;
            }

            foreach (UDTData child in dataItem.tableData)
            {

                setParentRefs(child as UDTData);
               
                child.parentObj = dataItem;

            }

        }
    }

}
