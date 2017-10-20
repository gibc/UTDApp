using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

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

        public override dynamic parseNumber(string numberTxt)
        {
            return base.parseNumber(numberTxt) / 100;
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

        override public void insertDecimal(NumberText numberText, char c)
        { }

        override public void deleteSelection(NumberText numberText)
        {
            int offset = numberText.numberString.IndexOf('.');
            if (offset >= numberText.selectionStart &&
                offset <= numberText.selectionStart + numberText.selectionLength)
            {
                numberText.deleteString();
                numberText.insertChar('.');
                numberText.selectionStart--;
            }
        }

        override public void insertDigit(NumberText numberText, char c)
        {
            if (!isMaxDecimalDigits(numberText))
                numberText.deleteChar(numberText.numberString.Length - 1);

            numberText.insertChar(c);
        }

        override public void replacePromptText(NumberText numberText, TextCompositionEventArgs arg, char c)
        {
            numberText.clear();
            numberText.insertString(".00");
            numberText.selectionStart = 0;

            if (c == '+' || c == '-') numberText.insertChar(c);
            if (Char.IsDigit(c)) numberText.insertChar(c);
            arg.Handled = true;
        }

        override public void fromatNumberText(NumberText numberText)
        {
            if (!numberText.promptVisble)
            {
                numberText.addCommas();
                numberText.limitDecimaDigits(2);
            }
        }


        private bool isMaxDecimalDigits(NumberText numberText)
        {
            int ptOffset = numberText.numberString.IndexOf('.');
            if (numberText.selectionStart > ptOffset + 2)
            {
                return true;
            }
            return false;
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

        protected FormatType type = FormatType.Decimal;

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

        virtual public void deleteSelection(NumberText numberText)
        {
            numberText.deleteString();
        }

        virtual public void insertDecimal(NumberText numberText, char c)
        {
            int ptOffset = numberText.numberString.IndexOf(c);
            if (ptOffset >= 0)
            {
                numberText.deleteChar(ptOffset);
            }
            numberText.insertChar(c);
        }

        virtual public void insertDigit(NumberText numberText, char c)
        {
            numberText.insertChar(c);
        }

        virtual public void insertSign(NumberText numberText, char c)
        {
            numberText.insertChar(c);
        }

        virtual public void replacePromptText(NumberText numberText, TextCompositionEventArgs arg, char c)
        {
            numberText.clear();
        }

        virtual public void fromatNumberText(NumberText numberText)
        {
            if (!numberText.promptVisble)
            {
                numberText.addCommas();
            }
        }

        virtual public dynamic parseNumber(string numberTxt)
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
