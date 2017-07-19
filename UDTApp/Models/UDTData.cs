using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using UDTApp.ViewModels;

namespace UDTApp.Models
{
    public class UDTData : UDTItem
    {
        public UDTData()
        {
            ChildData = new ObservableCollection<UDTData>();
            MouseMoveCommand = new DelegateCommand<MouseEventArgs>(mouseMove);
            DragEnterCommand = new DelegateCommand<DragEventArgs>(dragEnter);
            DragDropCommand = new DelegateCommand<DragEventArgs>(dragDrop);
            DragOverCommand = new DelegateCommand<DragEventArgs>(dragOver);
        }

        public DelegateCommand<MouseEventArgs> MouseMoveCommand { get; set; }
        public DelegateCommand<DragEventArgs> DragEnterCommand { get; set; }
        public DelegateCommand<DragEventArgs> DragDropCommand { get; set; }
        public DelegateCommand<DragEventArgs> DragOverCommand { get; set; }

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
                ObservableCollection<UDTData> col = Ex.GetSecurityId(btn);
                UDTData dataItem = (UDTData)dragArgs.Data.GetData(typeof(UDTData));
                col.Add(dataItem);
                dragArgs.Handled = true;
                _currentItem = null;
            }
        }


        private UDTItem _currentItem = null;
        private void dragEnter(DragEventArgs dragArgs)
        {
            Button btn = dragArgs.Source as Button;
            if (btn != null)
            {

                string[] frmts = dragArgs.Data.GetFormats();
                if (dragArgs.Data.GetDataPresent(typeof(UDTData)))
                {
                    UDTData dataItem = (UDTData)dragArgs.Data.GetData(typeof(UDTData));
                    _currentItem = dataItem as UDTItem;


                }
            }
        }

        private bool inMove = false;
        private void mouseMove(MouseEventArgs data)
        {

            Button btn = data.Source as Button;
            ObservableCollection<UDTData> col = Ex.GetSecurityId(btn);
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

        public string Type { get; set; }
        public string Name { get; set; }
        public string TypeName { get { return "Item Group"; } set{} }
        // on write child data insert or update UDTRelation record
        //   where 
        //     parent and child names are parent and child colleciton name
        //     child column name is pareent collection name 
        // on read UDT data set do select * from DataRelation where ParentName = [Name]
        //  for ecah relation
        //     create UDTData collection
        //     select * from [ChildName] where [ChildColumnName] = [Id of this record]
        //        for each record add record to collection
        // To create CRUD UI
        //   find UDTData collectons with ChildData.count = 0;
        //     create display and edit page for each UDTData item
        public ObservableCollection<UDTData> ChildData { get; set; }
        public ObservableCollection<UDTItem> DataItems { get; set; }
        // add this if we want children to have more than on parent
        public ObservableCollection<UDTParentColumn> ParentColumnNames { get; set; }
    }

    public class UDTRelation
    {
        public string ParentTableName { get; set; }
        public string ChildTableName { get; set; }
        public string ChildColumnName { get; set; }
    }

    public class UDTParentColumn
    {
        public string ParentColumnName { get; set; }
    }

    public interface UDTItem
    {
        string Name { get; set; }
        string Type { get; set; }
        string TypeName { get; set; }
    }

    public class UDTTxtItem : UDTItem
    {
        public UDTTxtItem(string name)
        { 
            Type = "[varchar](255) NULL ";
            TypeName = "Text Item";
            Name = name;
        }
        public string Type { get; set; }
        public string Name { get; set; }
        public string TypeName { get; set; }
    }


    public class UDTIntItem : UDTItem
    {
        public UDTIntItem(string name)
        {
            Type = "[int] NULL ";
            TypeName = "Number Item";
            Name = name;
        }        
        public string Type { get; set; }
        public string Name { get; set; }
        public string TypeName { get; set; }

    }
    public class UDTDecimalItem : UDTItem
    {
        public UDTDecimalItem(string name)
        {
            Type = "[decimal](10, 5) NULL ";
            TypeName = "Real Number Item";
            Name = name;
        }                
        public string Type { get; set; }
        public string Name { get; set; }
        public string TypeName { get; set; }

    }
    public class UDTDateItem : UDTItem
    {
        public UDTDateItem(string name)
        {
            Type = "[datetime] NULL";
            TypeName = "DateTime Item";
            Name = name;
        }                
        public string Type { get; set; }
        public string Name { get; set; }
        public string TypeName { get; set; }
    }

    public class UDTItemList
    {
        private static Collection<UDTItem> _itemList = null;
        public static Collection<UDTItem> ItemList { 
            get
            {
                if (_itemList == null)
                {
                    _itemList = new Collection<UDTItem>();
                    _itemList.Add(new UDTData());
                    _itemList.Add(new UDTTxtItem(""));
                    _itemList.Add(new UDTIntItem(""));
                    _itemList.Add(new UDTDecimalItem(""));
                    _itemList.Add(new UDTDateItem(""));
                }
                return _itemList;
            }
        }
    }
}
