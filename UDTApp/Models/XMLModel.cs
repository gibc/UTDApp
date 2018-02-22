using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Xml.Serialization;
using UDTApp.DataBaseProvider;
using UDTApp.ListManager;
using UDTApp.SchemaModels;
using UDTApp.Settings;

namespace UDTApp.Models
{
    public class XMLModel
    {
        public XMLModel()
        {
            TableDictionary.itemDic = new Dictionary<Guid, TableRef>();
            Service = this;
        }

        public static XMLModel Service = null;

        private UDTData _dbSchema = null;
        public UDTData dbSchema
        {
            get { return _dbSchema; }
            set
            {              
                _dbSchema = value;
                if (_dbSchema != null)
                {
                    new DBModel(_dbSchema.dbType, _dbSchema.serverName);
                }
            }
        }

        public bool saveToXml(List<UDTBase> SchemaList)
        {
            string xml = SerializeToString(SchemaList);

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Xml (*.xml)|*.xml";
            if (saveFileDialog.ShowDialog().Value)
            {
                FileStream xmlFile = File.Open(saveFileDialog.FileName, FileMode.Create);
                Byte[] info = new UTF8Encoding(true).GetBytes(xml);
                xmlFile.Write(info, 0, info.Length);
                xmlFile.Close();
                dbSchema = SchemaList[0] as UDTData;
                return true;
            }
            return false;
        }

        //public bool saveToXml(List<UDTBase> SchemaList, string filePath)        
        public bool saveToXml(UDTData _dbSchema, string filePath)
        {
            try
            {
                //(SchemaList[0] as UDTData).schemaVersion = Int32.Parse(ConfigurationManager.AppSettings["SchemaVersion"]);
                _dbSchema.schemaVersion = Int32.Parse(ConfigurationManager.AppSettings["SchemaVersion"]);
                List<UDTBase> schemaList = new List<UDTBase>();
                schemaList.Add(_dbSchema);
                //string xml = SerializeToString(SchemaList);
                string xml = SerializeToString(schemaList);
                FileStream xmlFile = File.Open(filePath, FileMode.Create);
                Byte[] info = new UTF8Encoding(true).GetBytes(xml);
                xmlFile.Write(info, 0, info.Length);
                xmlFile.Close();
                //dbSchema = SchemaList[0] as UDTData;
                dbSchema = _dbSchema;
                return true;
            }
            catch (Exception ex)
            {
                string msg = string.Format("saveToXml failed: {0}", ex.Message);
                UDTApp.Log.Log.LogMessage(msg);
                MessageBox.Show(msg);
                return false;
            }
        }

        static public void newProject(string ProjectName, DBType dbType, string serverName)
        {
            new XMLModel();
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
            XMLModel.Service.dbSchema = baseObj;
        }

        public List<UDTBase> readFromXml()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Xml (*.xml)|*.xml";
            if (openFileDialog.ShowDialog().Value)
            {
                AppSettings.appSettings.addFile(openFileDialog.FileName);
                return openProject(openFileDialog.FileName);
            }
            return null;
        }

        public static List<UDTBase> openProject(string filePath)
        {
            StreamReader xmlFile = File.OpenText(filePath);
            string xml = xmlFile.ReadToEnd();
            xmlFile.Close();
            List<UDTBase> schema = null;
            try
            {
                new XMLModel();
                schema = XMLModel.Service.readFromXml(xml);
                XMLModel.Service.dbSchema = schema[0] as UDTData;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: not a valid UDT project file.", "Invalid File",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                FileSetting fileSetting = AppSettings.appSettings.fileSettings.FirstOrDefault(p => p.filePath == filePath);
                if (fileSetting != null)
                {
                    AppSettings.appSettings.fileSettings.Remove(fileSetting);
                }
                XMLModel.Service = null;
            }
            return schema;
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

            // After Version 0 database ref in table dic via TypeName property setting
            // so parentObj references will return
            // master item and eliminate the need for parentObj fix up
            //if((result[0] as UDTData).schemaVersion < 0)
            //    setParentRefs(result[0] as UDTData);

            UDTData schema = result[0] as UDTData;
            schema.setAllSavedProps();

            return result;
        }
    }
}
