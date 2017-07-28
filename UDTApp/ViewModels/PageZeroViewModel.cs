﻿using Prism.Commands;
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
using UDTApp.Models;

namespace UDTApp.ViewModels
{
    public class PageZeroViewModel : ValidatableBindableBase
    {
        public DelegateCommand<MouseEventArgs> MouseMoveCommand { get; set; }
        public DelegateCommand<DragEventArgs> DragEnterCommand { get; set; }
        public DelegateCommand<DragEventArgs> DragDropCommand { get; set; }
        public DelegateCommand<DragEventArgs> DragOverCommand { get; set; }

        public PageZeroViewModel()
        {
            MouseMoveCommand = new DelegateCommand<MouseEventArgs>(mouseMove);
            DragEnterCommand = new DelegateCommand<DragEventArgs>(dragEnter);
            DragDropCommand = new DelegateCommand<DragEventArgs>(dragDrop);
            DragOverCommand = new DelegateCommand<DragEventArgs>(dragOver);

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

        public Collection<UDTBase> UDTItems {
            get { return UDTItemList.ItemList; }
        }

        private ObservableCollection<UDTBase> DbSchema = new ObservableCollection<UDTBase>();
        public List<UDTBase> SchemaList { get; set; }

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
