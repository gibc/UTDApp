using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using UDTApp.Models;

namespace UDTApp.ViewModels
{
    public class PageZeroViewModel
    {
        public DelegateCommand<MouseEventArgs> MouseMoveCommand { get; set; }

        public PageZeroViewModel()
        {
            MouseMoveCommand = new DelegateCommand<MouseEventArgs>(mouseMove);
        }

        public Collection<UDTItem> UDTItems {
            get { return UDTItemList.ItemList; }
        }

        private void mouseMove(MouseEventArgs data)
        {
            Button btn = data.Source as Button;
            ObservableCollection<UDTData> col = Ex.GetSecurityId(btn);
            //ObservableCollection<UDTData> col = UTDDataColProp.GetDataCol(btn);

            if (btn != null && data.LeftButton == MouseButtonState.Pressed)
            {
                DragDrop.DoDragDrop(btn,
                                 new UDTData(),
                                 DragDropEffects.Copy);
            }
        }
    }
}
