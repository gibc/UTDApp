using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UDTApp.ViewModels
{
    public interface ISubPage
    {
        dynamic SelectedItem { get; set; }
        int SelectedIndex { get; set; }
        bool IsPropertyEdited { get; }
        void SetTextProps(dynamic dataSet, string value = "");
        void LoadTextPops(dynamic dataSet);
        dynamic CreateNewDataSet();
        dynamic SetChildDataSet(dynamic dataSets, int selectedIndex);
    }
}
