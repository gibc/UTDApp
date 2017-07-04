using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UDTApp.Models;

namespace UDTApp.ViewModels
{
    //public class SubPageOne : ISubPage
    //{
    //    public SubPageOne(PageOneViewModel _page)
    //    {
    //        page = _page;
    //    }

    //    PageOneViewModel page;

    //    //public dynamic DataItems { get { return page.DataItems; } set { page.DataItems = value; } }
    //    //public dynamic SelectedItem { get { return page.SelectedItem; } set { page.SelectedItem = value; } }
    //    public int SelectedIndex { get { return page.SelectedIndex; } set { page.SelectedIndex = value; } }
    //    public bool IsPropertyEdited
    //    { 
    //        get 
    //        {
    //            return page.DataSets[page.SelectedIndex].Name != page.Name || 
    //                page.DataSets[page.SelectedIndex].Description != page.Description; 
    //        } 
    //    }
    //    public void SetTextProps(dynamic dataSet, string value = "")
    //    {
    //        page.Name = value;
    //        page.Description = value;
    //        if (dataSet != null)
    //        {
    //            page.Name = dataSet.Name;
    //            page.Description = dataSet.Description;
    //        }

    //    }
    //    public void LoadTextPops(dynamic dataSet) 
    //    {
    //        dataSet.Name = page.Name;
    //        dataSet.Description = page.Description;
    //    }


    //    // C is for create type
    //    // D is for detial type
    //    // master is fixed always DataSet
    //    // one: C = DataSet
    //    // two: C = DataItem
    //    // three: C = DataRelation
    //    public dynamic CreateNewDataSet()
    //    {
    //        return new DataSet("", "", new ObservableCollection<DataItem>(), new ObservableCollection<DataSetRelation>());
    //    }

    //    // one: D = DataItem
    //    // two: D = DataItem
    //    // three: D = DataRelation
    //    public dynamic SetChildDataSet(dynamic DataSets, int selectedIndex)
    //    {
    //        return DataSets[selectedIndex].DataItems;
    //    }
    //}
}
