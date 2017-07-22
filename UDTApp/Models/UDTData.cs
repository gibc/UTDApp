using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using UDTApp.ViewModels;

namespace UDTApp.Models
{
    public class UDTData : UDTBase //, UDTItem
    {
        public UDTData()
        {
            ChildData = new ObservableCollection<UDTBase>();
            TypeName = "Group";
            backgroundBrush = Brushes.MistyRose;
        }


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
        public ObservableCollection<UDTBase> ChildData { get; set; }
        // add this if we want children to have more than one parent
        public ObservableCollection<UDTParentColumn> ParentColumnNames { get; set; }
    }

    public class UDTBase : ValidatableBindableBase//: ValidatableBindableBase, INotifyDataErrorInfo
    {
        public UDTBase()
        {
            MouseMoveCommand = new DelegateCommand<MouseEventArgs>(mouseMove);
            DragEnterCommand = new DelegateCommand<DragEventArgs>(dragEnter);
            DragDropCommand = new DelegateCommand<DragEventArgs>(dragDrop);
            DragOverCommand = new DelegateCommand<DragEventArgs>(dragOver);

            objId = Guid.NewGuid();
            backgroundBrush = Brushes.Black;
        }

        public DelegateCommand<MouseEventArgs> MouseMoveCommand { get; set; }
        public DelegateCommand<DragEventArgs> DragEnterCommand { get; set; }
        public DelegateCommand<DragEventArgs> DragDropCommand { get; set; }
        public DelegateCommand<DragEventArgs> DragOverCommand { get; set; }

        public Guid objId;
        public Guid dragObjId;
        public SolidColorBrush backgroundBrush 
        {
            get; 
            set; 
        }

        public string Type { get; set; }
        private string _name = "";
        [Required]
        [StringLength(15, MinimumLength = 5, ErrorMessage = "Name must be between 5 and 15 characters.")]
        [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "Name can include only letter characters")]
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }
        public string TypeName { get; set; }


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
                UDTBase udtItem = getItemFromDragArgs(dragArgs);
                UDTBase udtBase = udtItem as UDTBase;

                // prevent copy to self
                if (udtBase.dragObjId == this.objId) return;

                udtItem.Name = "<Enter Name Here>";
                if (udtItem != null && col != null)
                    col.Add(udtItem);
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
                UDTBase udtItem = getItemFromDragArgs(dragArgs);

                _currentItem = udtItem;

            }
        }

        private bool inMove = false;
        private void mouseMove(MouseEventArgs data)
        {

            Button btn = data.Source as Button;
            ObservableCollection<UDTBase> col = Ex.GetSecurityId(btn);

            if (btn != null && data.LeftButton == MouseButtonState.Pressed && !inMove)
            {
                inMove = true;
                Debug.Write(string.Format(">>>Enter mouseMove\r", _currentItem));

                UDTBase udtItem = (UDTBase)Activator.CreateInstance(this.GetType());
                UDTBase udtBase = udtItem as UDTBase;
                udtBase.dragObjId = this.objId;

                if (udtItem != null)
                {
                    DragDrop.DoDragDrop(btn,
                      udtItem,
                      DragDropEffects.Copy);
                }

                Debug.Write(string.Format("<<<Exit mouseMove\r", _currentItem));
                data.Handled = true;
                inMove = false;
            }
        }

        private UDTBase getItemFromDragArgs(DragEventArgs dragArgs)
        {
            UDTBase udtItem = (UDTData)dragArgs.Data.GetData(typeof(UDTData));
            if (udtItem != null) return udtItem;
            udtItem = (UDTTxtItem)dragArgs.Data.GetData(typeof(UDTTxtItem));
            if (udtItem != null) return udtItem;
            udtItem = (UDTIntItem)dragArgs.Data.GetData(typeof(UDTIntItem));
            if (udtItem != null) return udtItem;
            udtItem = (UDTDecimalItem)dragArgs.Data.GetData(typeof(UDTDecimalItem));
            if (udtItem != null) return udtItem;
            udtItem = (UDTDateItem)dragArgs.Data.GetData(typeof(UDTDateItem));
            return udtItem;
        }

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

    public class UDTTxtItem : UDTBase //, UDTItem
    {
        public UDTTxtItem()
        { 
            Type = "[varchar](255) NULL ";
            TypeName = "Text";
            Name = "";
            backgroundBrush = Brushes.LightBlue;

        }

    }


    public class UDTIntItem : UDTBase//, UDTItem
    {
        public UDTIntItem()
        {
            Type = "[int] NULL ";
            TypeName = "Number";
            Name = "";
            backgroundBrush = Brushes.LightGreen;

        }    


    }
    public class UDTDecimalItem : UDTBase//, UDTItem
    {
        public UDTDecimalItem()
        {
            Type = "[decimal](10, 5) NULL ";
            TypeName = "Real";
            Name = "";
            backgroundBrush = Brushes.LightSalmon;
        }                
 

    }
    public class UDTDateItem : UDTBase//, UDTItem
    {
        public UDTDateItem()
        {
            Type = "[datetime] NULL";
            TypeName = "Date";
            Name = "";
            backgroundBrush = Brushes.LightYellow;
        }                
 
    }

    public class UDTItemList
    {
        private static Collection<UDTBase> _itemList = null;
        public static Collection<UDTBase> ItemList
        { 
            get
            {
                if (_itemList == null)
                {
                    _itemList = new Collection<UDTBase>();
                    _itemList.Add(new UDTData());
                    _itemList.Add(new UDTTxtItem());
                    _itemList.Add(new UDTIntItem());
                    _itemList.Add(new UDTDecimalItem());
                    _itemList.Add(new UDTDateItem());
                }
                return _itemList;
            }
        }
    }
}
