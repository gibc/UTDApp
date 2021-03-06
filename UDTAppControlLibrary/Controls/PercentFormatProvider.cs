﻿using System;
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
            if (maxNumber >= Decimal.MaxValue / 100m)
            {
                Decimal? nullDec = Decimal.MaxValue;
                numberMax = nullDec / 100m;
            }
            if (minNumber <= Decimal.MinValue / 100m)
            {
                Decimal? nullDec = Decimal.MinValue;
                numberMin = nullDec / 100m;
            }

            prompt = "Enter Percent.";
            positiveNumberSymbol = new NumberSymbol("", " %");
            negativeNumberSymbol = new NumberSymbol("", " %");
        }

        public override string getNumberText(dynamic number)
        {
            return string.Format("{0:n2}", 100 * number);
        }

        public override dynamic parseNumber(string numberTxt)
        {
            Decimal? number = parseNumberTxt(numberTxt);
            number = number / 100m;
            return checkRange(number);
        }
    }
}
