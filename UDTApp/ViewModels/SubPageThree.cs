using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UDTApp.Models;

namespace UDTApp.ViewModels
{
    public class SubPageThree : ISubPage
    {
        public SubPageThree(PageThreeViewModel _page)
        {
            page = _page;
        }

        PageThreeViewModel page;

        //public dynamic DataItems { get { return page.DataRelationItems; } set { page.DataRelationItems = value; } }
        public dynamic SelectedItem { get { return page.ChildSelectedItem; } set { page.ChildSelectedItem = value; } }
        public int SelectedIndex { get { return page.ChildSelectedIndex; } set { page.ChildSelectedIndex = value; } }
        public bool IsPropertyEdited
        {
            get
            {
                //return page.DataItems[page.ChildSelectedIndex].ChildDateSet != page.SelectedChild ||
                //    page.DataItems[page.ChildSelectedIndex].ParentDateSet != page.ParentDataSet;
                return true;
            }
        }

        public void SetTextProps(dynamic dataSet, string value = "")
        {
            page.SelectedChild = value;
            //page.ParentDataSet = value;
            if (dataSet != null)
            {
                page.SelectedChild = dataSet.ChildDateSet;
                //page.ParentDataSet = dataSet.ParentDateSet;
            }

        }
        public void LoadTextPops(dynamic dataSet)
        {
            dataSet.ChildDateSet = page.SelectedChild;
            dataSet.ParentDateSet = page.ParentDataSet;
        }
        public dynamic CreateNewDataSet()
        {
            var parentName = "";
            if (page.SelectedIndex > -1)
            { 
                parentName = page.DataSets[page.SelectedIndex].Name;
                page.ChildOptions = page.getChildOptions(parentName, "");
                // set combo box to "unselected" so first selection
                // will fire data validation event
                page.ComboIndex = -1;

            }
            return new DataSetRelation(parentName, "");
        }

        public dynamic SetChildDataSet(dynamic DataSets, int selectedIndex)
        {
            return DataSets[selectedIndex].DataSetRelations;
        }
    }
}
