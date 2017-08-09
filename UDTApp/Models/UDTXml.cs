using Microsoft.Win32;
using System;
using System.Collections.Generic;
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

        public void saveToXml(List<UDTBase> SchemaList)
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
            }
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

        private void setParentRefs(UDTData dataItem)
        {
            foreach (UDTBase child in dataItem.ChildData)
            {
                if (child.GetType() == typeof(UDTData))
                {
                    //UDTData childData = child as UDTData;
                    //childData.ParentColumnNames.Add(dataItem.Name);
                    setParentRefs(child as UDTData);
                }
                child.parentObj = dataItem;

            }
        }
    }

}
