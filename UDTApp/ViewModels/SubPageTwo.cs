using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UDTApp.Models;

namespace UDTApp.ViewModels
{
    //public class SubPageTwo : ISubPage
    //{
    //    public SubPageTwo(PageTwoViewModel _page)
    //    {
    //        page = _page;
    //    }

    //    PageTwoViewModel page;

    //    //public dynamic DataItems { get { return page.DataItems; } set { page.DataItems = value; } }
    //    public dynamic SelectedItem { get { return page.ChildSelectedItem; } set { page.ChildSelectedItem = value; } }
    //    public int SelectedIndex { get { return page.ChildSelectedIndex; } set { page.ChildSelectedIndex = value; } }
    //    public bool IsPropertyEdited
    //    {
    //        get
    //        {
    //            return page.DataItems[page.ChildSelectedIndex].Name != page.ChildName ||
    //                page.DataItems[page.ChildSelectedIndex].Type.ToString() != page.Type;
    //        }
    //    }

    //    public void SetTextProps(dynamic dataSet, string value = "")
    //    {
    //        page.ChildName = value;
    //        page.Type = value;
    //        if (dataSet != null)
    //        {
    //            page.ChildName = dataSet.Name;
    //            page.Type = dataSet.Type.ToString();
    //        }

    //    }
    //    public void LoadTextPops(dynamic dataSet) 
    //    {
    //        dataSet.Name = page.ChildName;
    //        int ty = -1;
    //        Int32.TryParse(page.Type, out ty);
    //        dataSet.Type = ty;
    //    }
    //    public dynamic CreateNewDataSet()
    //    {
    //        return new DataItem("", -1);
    //    }

    //    public dynamic SetChildDataSet(dynamic DataSets, int selectedIndex)
    //    {
    //        return DataSets[selectedIndex].DataItems;
    //    }
    //}
}
