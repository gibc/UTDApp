﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

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
                FileStream xmlFile = File.Open(saveFileDialog.FileName, FileMode.OpenOrCreate);
                Byte[] info = new UTF8Encoding(true).GetBytes(xml);
                xmlFile.Write(info, 0, info.Length);
                xmlFile.Close();
                SchemaData = SchemaList;
                return true;
            }
            return false;
        }

        public List<UDTBase> newProject(string ProjectName)
        {
            SchemaData = new List<UDTBase>();
            UDTData baseObj = new UDTData();
            baseObj.ToolBoxItem = false;
            baseObj.Name = ProjectName;
            baseObj.parentObj = new UDTData();
            baseObj.AnyErrors = false;
            baseObj.EditBoxEnabled = true;
            SchemaData.Add(baseObj);
            return SchemaData;
        }

        public List<UDTBase> readFromXml()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Xml (*.xml)|*.xml";
            if (openFileDialog.ShowDialog().Value)
            {
                StreamReader xmlFile = File.OpenText(openFileDialog.FileName);
                string xml = xmlFile.ReadToEnd();
                xmlFile.Close();

                List<UDTBase> schema = readFromXml(xml);
                SchemaData = schema;
                return SchemaData;
            }
            return null;
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
            foreach(UDTBase child in dataItem.columnData)
            {
                child.parentObj = dataItem;
            }

            foreach (UDTData child in dataItem.tableData)
            {

                setParentRefs(child as UDTData);
               
                child.parentObj = dataItem;

            }



        }
    }

}
