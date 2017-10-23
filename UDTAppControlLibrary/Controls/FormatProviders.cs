using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace UDTAppControlLibrary.Controls
{
    public class DateTimeProvider : DcimalFromatProvider
    {
        public DateTimeProvider(DateTime maxNumber, DateTime minNumber)
            : base(maxNumber, minNumber)
        {
            prompt = "Enter a Date.";
            positiveNumberSymbol = new NumberSymbol("", "");
            negativeNumberSymbol = new NumberSymbol("", "");
        }

        public override void fromatNumberText(NumberText numberText)
        {
            if (!numberText.promptVisble)
            {
                // TBD: add date seperators
                //numberText.addCommas();
            }
        }

        public override void replacePromptText(NumberText numberText, TextCompositionEventArgs arg, char c)
        {
            numberText.clear();
            numberText.insertString("  /  /    ");
            numberText.selectionStart = 0;

            if (Char.IsDigit(c)) insertDigit(numberText, c);
            arg.Handled = true;
        }

        public override void deleteSelection(NumberText numberText)
        {
            int offset = numberText.selectionStart;
            while(offset < numberText.selectionStart + numberText.selectionLength)
            {
                if(numberText.numberString[offset] != '/')
                {
                    numberText.repalceChar(' ', offset);
                }
                offset++;
            }
            numberText.selectionLength = 0;
        }

        private void monthDigit (char c, int offset, NumberText numberText)
        {
            int val = c - '0';
            string txt = numberText.numberString.Split('/')[0];

            if(val > 2)
            {               
                numberText.repalceChar('0', 0);                
                numberText.repalceChar(c, 1);
                numberText.selectionStart = 3;
                return;
            }

            if (offset == 0 && val == 0)
            {
                numberText.repalceChar(c, 0);
                if (txt[1] == '0') 
                    numberText.repalceChar(' ', 1);
                numberText.selectionStart = 1;
            }
            if (offset == 0 && val == 1)
            {
                numberText.repalceChar(c, 0);
                if (txt[1] != ' ' && txt[1] - '0' > 2)
                    numberText.repalceChar(' ', 1);
                numberText.selectionStart = 1;
            }

            if (offset == 1)
            {
                if (val == 0 && txt[0] != '0')
                {
                    numberText.repalceChar(c, 1);
                    numberText.selectionStart = 3;
                }
                if (val == 1 || val == 2)
                {
                    numberText.repalceChar(c, 1);
                    if (txt[0] == ' ')
                        numberText.repalceChar('0', 0);
                    numberText.selectionStart = 3;
                }
            }
            // check if entry complete
            if(numberText.month != null)
            {
                numberText.selectionStart = 3;
            }
        }


        public override void insertDigit(NumberText numberText, char c)
        {
            // TBD: 1 - 12, 1 - 31 etc
            int digitVal = c - '0';
            if (numberText.selectionStart >= 0 && numberText.selectionStart < 2)
            {
                monthDigit(c, numberText.selectionStart, numberText);
            }
            else if (numberText.selectionStart >= 3 && numberText.selectionStart < 5)
            { }
            else if (numberText.selectionStart >= 6)
            { }

        }

        public override void insertDateSperator(NumberText numberText)
        {
            // TBD: move to next section if currect section is defined
            numberText.insertChar('/');
        }


        public override dynamic parseNumber(string numberTxt)
        {
            // TBD
            //return base.parseNumber(numberTxt) / 100;
            DateTime date;
            DateTime.TryParse("1/1/2001", out date);
            return date;
        }
    }

    public class PercentFromatProvider : DcimalFromatProvider
    {
        public PercentFromatProvider(Decimal maxNumber, Decimal minNumber) : base(maxNumber, minNumber)  
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

    public class NumberFromatProvider : DcimalFromatProvider
    {
        public NumberFromatProvider(Int32 maxNumber, Int32 minNumber)
            : base(maxNumber, minNumber)
        {
            prompt = "Enter Number.";
            positiveNumberSymbol = new NumberSymbol("", "");
            negativeNumberSymbol = new NumberSymbol("", "");
        }

        public override dynamic parseNumber(string numberTxt)
        {
            Int32? number = default(Int32?);

            if (emptyNumberText(numberTxt))
                return number;

            numberTxt = unFormatText(numberTxt);

            Int32 num;
            if (Int32.TryParse(numberTxt, out num))
            {
                number = num;
            }
            else if (numberTxt[0] == '-')
            {
                number = (Int32)numberMin ;
            }
            else
            {
                number = (Int32)numberMax;
            }

            return checkRange(number);
        }

        public override void insertDecimal(NumberText numberText, char c)
        {
            //base.insertDecimal(numberText, c);
        }

    }

    public class CurrencyFromatProvider : DcimalFromatProvider
    {
        public CurrencyFromatProvider(Decimal maxNumber, Decimal minNumber)
            : base(maxNumber, minNumber)
        {
            prompt = "Enter Amount.";
            positiveNumberSymbol = new NumberSymbol("$", "");
            negativeNumberSymbol = new NumberSymbol("${", ")");
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
            else numberText.deleteString();
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
        public DcimalFromatProvider(dynamic maxNumber, dynamic minNumber)
        {
            numberMax = maxNumber;
            numberMin = minNumber;

            prompt = "Enter Decimal.";
            positiveNumberSymbol = new NumberSymbol("", "");
            negativeNumberSymbol = new NumberSymbol("", "");
            
        }

        public string prompt;

        public dynamic numberMax;
        public dynamic numberMin; 

        public NumberSymbol positiveNumberSymbol;

        public NumberSymbol negativeNumberSymbol; 

        protected string unFormatText(string text)
        {
            text = text.Replace(",", "");
            return text;
        }

        protected dynamic checkRange(dynamic number)
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

        virtual public void insertDateSperator(NumberText numberText)
        {           
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

        virtual protected bool emptyNumberText(string numberTxt)
        {
            return (string.IsNullOrEmpty(numberTxt) || numberTxt == prompt || numberTxt == "."
                || numberTxt == "-" || numberTxt == "+");
        }

        virtual public dynamic parseNumber(string numberTxt)
        {
            Decimal? number = default(Decimal?);

            if (emptyNumberText(numberTxt))
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
