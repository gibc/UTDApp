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

        //public override void fromatNumberText(NumberText numberText)
        //{
        //    if (!numberText.promptVisble)
        //    {
        //        // TBD: add date seperators
        //        //numberText.addCommas();
        //    }
        //}


        public override void replacePromptText(NumberText numberText, TextCompositionEventArgs arg, char c, dynamic extra = null)
        {
            numberText.clear();
            DateTimeFormat fmt = (DateTimeFormat)extra;
            if (fmt == DateTimeFormat.Date_Only)
                numberText.insertString("  /  /    ");
            if (fmt == DateTimeFormat.Date_12_HourTime)
                numberText.insertString("  /  /    " + "\n" + DateTime.Now.ToString("hh:mm:tt"));
            if (fmt == DateTimeFormat.Date_24_HourTime)
                numberText.insertString("  /  /    " + "\n" + DateTime.Now.ToString("HH:mm:tt"));

            numberText.selectionStart = 0;

            if (Char.IsDigit(c)) insertDigit(numberText, c);
            if(arg != null) arg.Handled = true;
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

            if (val > 2)
            {
                numberText.repalceChar('0', 0);
                numberText.repalceChar(c, 1);
                numberText.selectionStart = numberText.dayIndex;
                return;
            }

            if (offset == 0 && val == 0)
            {
                numberText.repalceChar(c, numberText.selectionStart);
                if (txt[1] == '0')
                    numberText.repalceChar(' ', numberText.selectionStart+1);
                numberText.selectionStart++;
            }
            else if (offset == 0 && val == 1)
            {
                numberText.repalceChar(c, numberText.selectionStart);
                if (txt[1] != ' ' && txt[1] - '0' > 2)
                    numberText.repalceChar(' ', numberText.selectionStart+1);
                numberText.selectionStart++;
            }

            if (offset == 1)
            {
                if (val == 0)
                {
                    numberText.repalceChar(c, numberText.selectionStart);
                    if (txt[0] == '0')
                    {
                        numberText.repalceChar(' ', numberText.selectionStart - 1);
                    }
                    else if (txt[0] == ' ')
                    {
                        numberText.repalceChar('0', numberText.selectionStart - 1);
                    }
                }
                if (val == 1 || val == 2)
                {
                    numberText.repalceChar(c, numberText.selectionStart);
                    if (txt[0] == ' ')
                        numberText.repalceChar('0', numberText.selectionStart - 1);
                }
            }
        }

        private void adjustDay(NumberText numberText)
        {
            if(numberText.month != null)
            {
                string maxDays = numberText.daysInMonth;
                StringBuilder dayText = new  StringBuilder(numberText.dayTxt);
                if ((dayText[0] - '0') > (maxDays[0] - '0'))
                {
                    dayText[0] = ' ';
                    numberText.selectionStart = numberText.dayIndex;
                }
                if (dayText[0] == maxDays[0] && (dayText[1] - '0') > (maxDays[1] - '0'))
                {
                    dayText[1] = ' ';
                    numberText.selectionStart = numberText.dayIndex+1;
                }
                numberText.dayTxt = dayText.ToString();
            }
        }

        private void dayDigit (char c, int offset, NumberText numberText)
        {
            int val = c - '0';
            string txt = numberText.numberString.Split('/')[1];
            string maxDays = numberText.daysInMonth;

            if (offset == 0 && val == 0)
            {
                numberText.repalceChar(c, numberText.selectionStart);
                if (txt[1] == '0')
                    numberText.repalceChar(' ', numberText.selectionStart+1);
                numberText.selectionStart++;
            }

            if (offset == 0 && val > 0 && val <= 3)
            {
                if (val <= maxDays[0] - '0')
                {
                    numberText.repalceChar(c, numberText.selectionStart);
                    // if setting largest allow 10s place the check max allowed 1s place
                    // and adj if needed
                    if((val == maxDays[0]- '0') && (txt[1] - '0') > (maxDays[1] - '0'))
                        numberText.repalceChar(maxDays[1], numberText.selectionStart + 1);

                    numberText.selectionStart++;
                }
            }

            if (offset == 1)
            {
                if (val == 0 )
                {
                    numberText.repalceChar(c, numberText.selectionStart);
                    if(txt[0] == '0')
                    { 
                        numberText.repalceChar(' ', numberText.selectionStart-1);
                    }
                }
                if (val > 0)
                {
                    if((maxDays[0] - '0') == (txt[0] - '0'))
                    {
                        if(val <= maxDays[1] -'0')
                        {
                            numberText.repalceChar(c, numberText.selectionStart);
                        }
                        else
                        {
                            numberText.repalceChar(c, numberText.selectionStart);
                            numberText.repalceChar(' ', numberText.selectionStart - 1);
                        }
                    }
                    else
                        numberText.repalceChar(c, numberText.selectionStart);
                }
            }
       }

        private void yearDigit (char c, int offset, NumberText numberText)
        {
            if(offset < 4)
            { 
                numberText.repalceChar(c, numberText.selectionStart);
                if (numberText.selectionStart < 10)
                    numberText.selectionStart++;
            }
        }

        public override void insertDigit(NumberText numberText, char c)
        {
            int digitVal = c - '0';
            if (numberText.selectionStart >= numberText.monthIndex
                && numberText.selectionStart < numberText.monthIndex+2)
            {
                monthDigit(c, numberText.selectionStart, numberText);
                if (numberText.month != null)
                {
                    numberText.selectionStart = numberText.dayIndex;
                    adjustDay(numberText);
                }
                if (numberText.day != null)
                    numberText.selectionStart = numberText.monthIndex;
            }
            else if (numberText.selectionStart >= numberText.dayIndex
                && numberText.selectionStart < numberText.dayIndex+2)
            {
                dayDigit(c, numberText.selectionStart - 3, numberText);
                if (numberText.day != null)
                    numberText.selectionStart = numberText.yearIndex;
            }
            else if (numberText.selectionStart >= numberText.yearIndex)
            {
                yearDigit(c, numberText.selectionStart - 6, numberText);
                if (numberText.year != null)
                { 
                    numberText.selectionStart = numberText.yearIndex+4;
                    adjustDay(numberText);
                }
            }

        }

        public override void insertDateSperator(NumberText numberText)
        {
            char c = ' ';
            int offset = numberText.selectionStart;
            if (offset >= numberText.monthIndex && offset <= numberText.monthIndex + 1)
            {
                string moTxt = numberText.monthTxt;
                if (moTxt.All(Char.IsWhiteSpace))
                {
                    DateTime now = DateTime.Now;
                    numberText.monthTxt = now.Month.ToString();
                }
                else if (moTxt.Any(Char.IsWhiteSpace))
                {
                    if (!Char.IsWhiteSpace(moTxt[0])) c = moTxt[0]; 
                    if (!Char.IsWhiteSpace(moTxt[1])) c = moTxt[1];
                    numberText.repalceChar('0', numberText.monthIndex);
                    numberText.repalceChar(c, numberText.monthIndex+1);
                }
                numberText.selectionStart = numberText.dayIndex;
            }
            if (offset >= numberText.dayIndex && offset <= numberText.dayIndex + 1)
            {
                string dayTxt = numberText.dayTxt;
                if (dayTxt.All(Char.IsWhiteSpace))
                {
                    DateTime now = DateTime.Now;
                    numberText.dayTxt = now.Day.ToString();
                }
                else if (dayTxt.Any(Char.IsWhiteSpace))
                {
                    if (!Char.IsWhiteSpace(dayTxt[0])) c = dayTxt[0];
                    if (!Char.IsWhiteSpace(dayTxt[1])) c = dayTxt[1];
                    numberText.repalceChar('0', numberText.dayIndex);
                    numberText.repalceChar(c, numberText.dayIndex + 1);
                }
                numberText.selectionStart = numberText.yearIndex;
            }
            if (offset >= numberText.yearIndex && offset <= numberText.yearIndex + 3)
            {
                char a = ' ';
                char b = ' ';
                string yearTxt = numberText.yearTxt;
                if (yearTxt.All(Char.IsWhiteSpace))
                {
                    DateTime now = DateTime.Now;
                    numberText.yearTxt = now.Year.ToString();
                }
                else if (!yearTxt.Substring(0, 2).All(Char.IsWhiteSpace) && 
                    yearTxt.Substring(2,2).All(Char.IsWhiteSpace))
                {
                    if (!Char.IsWhiteSpace(yearTxt[0])) a = yearTxt[0];
                    if (!Char.IsWhiteSpace(yearTxt[1])) b = yearTxt[1];
                    numberText.repalceChar('2', numberText.yearIndex);
                    numberText.repalceChar('0', numberText.yearIndex + 1);
                    if(a == ' ')
                    {
                        numberText.repalceChar('0', numberText.yearIndex + 2);
                        numberText.repalceChar(b, numberText.yearIndex + 3);
                    }
                    if (b == ' ')
                    {
                        numberText.repalceChar('0', numberText.yearIndex + 2);
                        numberText.repalceChar(a, numberText.yearIndex + 3);
                    }
                    else
                    { 
                        numberText.repalceChar(a, numberText.yearIndex + 2);
                        numberText.repalceChar(b, numberText.yearIndex + 3);
                    }
                }
                //if (!yearTxt.Any(Char.IsWhiteSpace))
                    numberText.selectionStart = numberText.yearIndex + 4;
            }
        }

        public override dynamic parseNumber(string numberTxt)
        {
            DateTime date;
            if(DateTime.TryParse(numberTxt, out date))
                return date;
            return null;
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

        override public void replacePromptText(NumberText numberText, TextCompositionEventArgs arg, char c, dynamic extra = null)
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


        virtual public void replacePromptText(NumberText numberText, TextCompositionEventArgs arg, char c, dynamic extra = null)
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
