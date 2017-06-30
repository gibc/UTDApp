using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UDTApp.ViewModels
{
    public interface ISubPage
    {
        //dynamic DataItems { get; set; }
        dynamic SelectedItem { get; set; }
        int SelectedIndex { get; set; }
        bool IsPropertyEdited { get; }
        void SetTextProps(dynamic dataSet, string value = "");
        void LoadTextPops(dynamic dataSet);
        dynamic CreateNewDataSet();
        dynamic SetChildDataSet(dynamic DataSets, int selectedIndex);
    }
}
