using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace UDTAppControlLibrary.Controls
{
    public class NumberBox : NumberBoxBase
    {

        static NumberBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NumberBox), new FrameworkPropertyMetadata(typeof(NumberBox)));
        }

        public NumberBox()
        {
            fromatProvider = new NumberFromatProvider(Int32.MaxValue, Int32.MinValue);
        }

        Int32? parsedNumber = null;
        override protected void setParsedNumber(dynamic value)
        {
            // TBD:  set bound number property if changed
            parsedNumber = value; 
        }


   }
}
