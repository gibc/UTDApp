using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomControlTest.ViewModel
{
    public class MainViewModel
    {
        public MainViewModel()
        {

        }

        public string _maskedText = "";
        public string maskedText 
        {
            get { return _maskedText; }
            set { _maskedText = value; }
        }
    }
}
