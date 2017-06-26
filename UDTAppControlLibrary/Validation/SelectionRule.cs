using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace UDTAppControlLibrary.Validation
{
    public class SelectionRule : ValidationRule
    {
        public SelectionRule()
        {

        }

        public override System.Windows.Controls.ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string val = value as string;
            if (val != null && val.Length == 0)
                return new ValidationResult(false, "Please select a child dataset from the drop-down list.");
            return new ValidationResult(true, null);
        }
    }
}
