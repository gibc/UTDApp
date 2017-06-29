using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace UDTApp.Models
{
    public class DataSetRelation : ModelBase
    {
        public DataSetRelation()
        {
        }

        public DataSetRelation(string parent, string child)
        {
            ParentDateSet = parent;
            ChildDateSet = child;
        }

        //public string ParentDateSet { get; set; }
        private string _parentDataSet;
        public string ParentDateSet
        {
            get { return _parentDataSet; }
            set
            {
                SetProperty(ref _parentDataSet, value);
            }
        }
        //public string ChildDateSet { get; set; }
        private string _childDataSet;
        public string ChildDateSet
        {
            get { return _childDataSet; }
            set
            {
                SetProperty(ref _childDataSet, value);
            }
        }


    }
}
