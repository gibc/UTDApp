using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UDTAppControlLibrary.Controls
{
    public class FormatProviderBase
    {
        public FormatProviderBase() { }
    }

    public class NumberProviderBase<T> : FormatProviderBase
    {
        public NumberProviderBase() { }

        public FormatType type = FormatType.Decimal;
        public string prompt = "";

        public T numberMax;
        public T numberMin;

        public NumberSymbol positiveNumberSymbol
        {
            get;
            set;
        }
        public NumberSymbol negativeNumberSymbol
        {
            get;
            set;
        }

    }

    public class PercentFromatProvider : DcimalFromatProvider
    {
        public PercentFromatProvider(Decimal maxNumber, Decimal minNumber) : base(maxNumber, minNumber)  
        {
            basePovider.prompt = "Enter Percent.";
            basePovider.positiveNumberSymbol = new NumberSymbol("", " %");
            basePovider.negativeNumberSymbol = new NumberSymbol("", " %");
        }
    }

    public class CurrencyFromatProvider : DcimalFromatProvider
    {
        public CurrencyFromatProvider(Decimal maxNumber, Decimal minNumber)
            : base(maxNumber, minNumber)
        {
            basePovider.prompt = "Enter Amount.";
            basePovider.positiveNumberSymbol = new NumberSymbol("$", "");
            basePovider.negativeNumberSymbol = new NumberSymbol("${", ")");
        }
    }


    public class DcimalFromatProvider 
    {
        public DcimalFromatProvider(Decimal maxNumber, Decimal minNumber)
        {
            basePovider.numberMax = maxNumber;
            basePovider.numberMin = minNumber;

            basePovider.prompt = "Enter Decimal.";
            basePovider.positiveNumberSymbol = new NumberSymbol("", "");
            basePovider.negativeNumberSymbol = new NumberSymbol("", "");
            
        }

        protected NumberProviderBase<Decimal?> basePovider = new NumberProviderBase<Decimal?>();

        public FormatType type = FormatType.Decimal;

        public string prompt{ get{ return basePovider.prompt;}}

        public Decimal? numberMax { get { return basePovider.numberMax; } }
        public Decimal? numberMin { get { return basePovider.numberMin; } }

        public NumberSymbol positiveNumberSymbol { get { return basePovider.positiveNumberSymbol; } }

        public NumberSymbol negativeNumberSymbol { get { return basePovider.negativeNumberSymbol; } }

        private string unFormatText(string text)
        {
            text = text.Replace(",", "");
            return text;
        }

        private Decimal? checkRange(Decimal? number)
        {

            if (number > numberMax)
                return numberMax;
            else if (number < numberMin)
                return numberMin;
            
            return number;
        }

        virtual public Decimal? parseNumber(string numberTxt)
        {
            Decimal? number = default(Decimal?);

            if (string.IsNullOrEmpty(numberTxt) || numberTxt == prompt || numberTxt == "."
                || numberTxt == "-" || numberTxt == "+")
                return number;

            numberTxt = unFormatText(numberTxt);

            Decimal num;
            if (Decimal.TryParse(numberTxt, out num))
            {
                number = num;
            }
            else if (numberTxt[0] == '-')
            {
                number = numberMin;
            }
            else
            {
                number = numberMax;
            }

            return checkRange(number);

        }

    }
}
