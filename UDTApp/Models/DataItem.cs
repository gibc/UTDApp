using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UDTApp.Models
{
    public class DataItem : BindableBase
    {
        public DataItem(string name, int type)
        {
            _name = name;
            _type = type;
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

        private int _type;
        public int Type
        {
            get { return _type; }
            set
            {
                SetProperty(ref _type, value);
            }
        }
    }
}
