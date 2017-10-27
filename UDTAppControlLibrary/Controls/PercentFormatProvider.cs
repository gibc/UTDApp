using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace UDTAppControlLibrary.Controls
{
    public class PercentFromatProvider : DcimalFromatProvider
    {
        public PercentFromatProvider(Decimal maxNumber, Decimal minNumber)
            : base(maxNumber, minNumber)
        {
            prompt = "Enter Percent.";
            positiveNumberSymbol = new NumberSymbol("", " %");
            negativeNumberSymbol = new NumberSymbol("", " %");
        }

        public override dynamic parseNumber(string numberTxt)
        {
            return base.parseNumber(numberTxt) / 100;
        }
    }
}
