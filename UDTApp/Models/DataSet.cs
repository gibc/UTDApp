using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UDTApp.Models
{
    public class DataSet : BindableBase
    {
        public DataSet()
        {

        }

        public DataSet(string name, string description, ObservableCollection<DataItem> dataItems)
        {
            Name = name;
            Description = description;
            DataItems = dataItems;
        }

        private ObservableCollection<DataItem> _dataItems;
        public  ObservableCollection<DataItem> DataItems
        {
            get { return _dataItems; }
            set
            {
                SetProperty(ref _dataItems, value);
            }
        }

        private string _name;
        public string Name 
        {
            get { return _name; }
            set 
            {
                SetProperty(ref _name, value);
            }
        }

        private string _description;
        public string Description 
        {
            get { return _description; }
            set
            {
                SetProperty(ref _description, value);
            }
        }

        private string _title = "title test";
        public string Title
        {
            get { return _title; }
            set
            {
                SetProperty(ref _title, value);
            }
        }

        private int _id = -1;
        public int ID
        {
            get { return _id; }
            set
            {
                SetProperty(ref _id, value);
            }
        }
    }
}
