using Microsoft.Win32;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        public PageZeroViewModel()
        {
            MouseMoveCommand = new DelegateCommand<MouseEventArgs>(mouseMove);
            DragEnterCommand = new DelegateCommand<DragEventArgs>(dragEnter);
            DragDropCommand = new DelegateCommand<DragEventArgs>(dragDrop);
            DragOverCommand = new DelegateCommand<DragEventArgs>(dragOver);
            SaveToXmlCommand = new DelegateCommand(saveToXml);
            ReadFromFileCommand = new DelegateCommand(readFromXml);

            SchemaList = new List<UDTBase>();
            UDTData baseObj = new UDTData();
            baseObj.ChildData = DbSchema;
            baseObj.ToolBoxItem = false;
            baseObj.Name = "Master";
            baseObj.parentObj = new UDTData();
            baseObj.setAnyError = setAnyErrors;
            SchemaList.Add(baseObj);

        }

        private UDTData masterItem = null;

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
            string xml = SerializeToString(SchemaList);
            //SchemaList = readFromXml(xml);

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Xml (*.xml)|*.xml";
            if (saveFileDialog.ShowDialog().Value)
            {
                FileStream xmlFile = File.Open(saveFileDialog.FileName, FileMode.OpenOrCreate);
                Byte[] info = new UTF8Encoding(true).GetBytes(xml);
                xmlFile.Write(info, 0, info.Length);
                xmlFile.Close();
            }
        }

        void readFromXml()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Xml (*.xml)|*.xml";
            if (openFileDialog.ShowDialog().Value)
            {
                StreamReader xmlFile = File.OpenText(openFileDialog.FileName);
                string xml = xmlFile.ReadToEnd();
                xmlFile.Close();

                List<UDTBase> schema = readFromXml(xml);
                SchemaList = schema;
            }

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
                    setParentRefs(child as UDTData);
                child.parentObj = dataItem;
                child.createCommnadDelegates();

            }
        }

        public Collection<UDTBase> UDTItems {
            get { return UDTItemList.ItemList; }
        }

        private ObservableCollection<UDTBase> DbSchema = new ObservableCollection<UDTBase>();

        private List<UDTBase> _schemaList = null; 
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
