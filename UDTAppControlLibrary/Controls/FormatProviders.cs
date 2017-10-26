﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace UDTAppControlLibrary.Controls
{
    public class DateTimeProvider : DcimalFromatProvider
    {
        public DateTimeProvider(DateTimeFormat format, DateTime maxNumber, DateTime minNumber)
            : base(maxNumber, minNumber)
        {
            dateFormat = format;
            prompt = "Enter a Date.";
            positiveNumberSymbol = new NumberSymbol("", "");
            negativeNumberSymbol = new NumberSymbol("", "");
        }


        private DateTimeFormat dateFormat;

        public override void replacePromptText(NumberText numberText, TextCompositionEventArgs arg, char c)
        {
            numberText.clear();
            if (dateFormat == DateTimeFormat.Date_Only)
                numberText.insertString("  /  /    ");
            if (dateFormat == DateTimeFormat.Date_12_HourTime)
                numberText.insertString("  /  /    " + "\n" + DateTime.Now.ToString("hh:mm:tt"));
            if (dateFormat == DateTimeFormat.Date_24_HourTime)
                numberText.insertString("  /  /    " + "\n" + DateTime.Now.ToString("HH:mm"));

            numberText.selectionStart = 0;

            if (Char.IsDigit(c)) insertDigit(numberText, c);
            if(arg != null) arg.Handled = true;
        }

        public override void deleteSelection(NumberText numberText)
        {
            int offset = numberText.selectionStart;
            while(offset < numberText.selectionStart + numberText.selectionLength)
            {
                if (numberText.numberString[offset] != '/' && numberText.numberString[offset] != ':'
                    && numberText.numberString[offset] != 'M' && numberText.numberString[offset] != '\n')
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
                numberText.selectionStart = DateIndex.day;// numberText.dayIndex;
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
                    numberText.selectionStart = DateIndex.day;// numberText.dayIndex;
                }
                if (dayText[0] == maxDays[0] && (dayText[1] - '0') > (maxDays[1] - '0'))
                {
                    dayText[1] = ' ';
                    numberText.selectionStart = DateIndex.day + 1;
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

        private void hourDigit(char c, int offset, NumberText numberText)
        {
            if (offset == 0)
            {
                int maxDigitVal = 2;
                if (dateFormat == DateTimeFormat.Date_12_HourTime) maxDigitVal = 1;
                if (c - '0' <= maxDigitVal)
                { 
                    numberText.repalceChar(c, numberText.selectionStart);
                    numberText.selectionStart++;
                }
            }
            else if (offset == 1)
            {
                //if (c - '0' <= 2)
                {
                    numberText.repalceChar(c, numberText.selectionStart);
                    numberText.selectionStart = DateIndex.minute;
                }
            }
        }

        private void minuteDigit(char c, int offset, NumberText numberText)
        {
            if (offset == 0)
            {
                if (c - '0' <= 5)
                {
                    numberText.repalceChar(c, numberText.selectionStart);
                    numberText.selectionStart++;
                }
            }
            else if (offset == 1)
            {
                //if (c - '0' <= 2)
                {
                    numberText.repalceChar(c, numberText.selectionStart);
                    numberText.selectionStart = DateIndex.meridiem;
                }
            }
        }

        public override void insertDigit(NumberText numberText, char c)
        {
            int digitVal = c - '0';
            if (numberText.isMonthIndex )
            {
                monthDigit(c, numberText.selectionStart-DateIndex.month, numberText);
                if (numberText.month != null)
                {
                    numberText.selectionStart = DateIndex.day;// numberText.dayIndex;
                    adjustDay(numberText);
                }
            }
            else if (numberText.isDayIndex)
            {
                dayDigit(c, numberText.selectionStart - DateIndex.day, numberText);
                if (numberText.day != null)
                    numberText.selectionStart = DateIndex.year;// numberText.yearIndex;
            }
            else if (numberText.isYearIndex)
            {
                yearDigit(c, numberText.selectionStart - 6, numberText);
                if (numberText.year != null)
                {
                    numberText.selectionStart = DateIndex.year + 4;// numberText.yearIndex + 4;
                    adjustDay(numberText);
                }
            }
            else if(numberText.isHourIndex)
            {
                hourDigit(c, numberText.selectionStart - DateIndex.hour, numberText);
            }
            else if(numberText.isMinuteIndex)
            {
                minuteDigit(c, numberText.selectionStart - DateIndex.minute, numberText);
            }

        }

        public override void insertLetter(NumberText numberText, char c)
        {
            if(numberText.isMeridiemIndex)
            {
                if (c == 'a' || c == 'A' )
                {
                    numberText.repalceChar('A', DateIndex.meridiem);
                    numberText.repalceChar('M', DateIndex.meridiem+1);
                }
                if (c == 'p' || c == 'P')
                {
                    numberText.repalceChar('P', DateIndex.meridiem);
                    numberText.repalceChar('M', DateIndex.meridiem + 1);
                }

            }
        }

        public override void insertDateSperator(NumberText numberText)
        {
            char c = ' ';
            if (numberText.isMonthIndex)
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
                    numberText.repalceChar('0', DateIndex.month);
                    numberText.repalceChar(c, DateIndex.month + 1);
                }
                numberText.selectionStart = DateIndex.day;// numberText.dayIndex;
            }
            if (numberText.isDayIndex)
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
                    numberText.repalceChar('0', DateIndex.day);
                    numberText.repalceChar(c, DateIndex.day + 1);
                }
                numberText.selectionStart = DateIndex.year;// numberText.yearIndex;
            }
            if (numberText.isYearIndex)
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
                    numberText.repalceChar('2', DateIndex.year );
                    numberText.repalceChar('0', DateIndex.year + 1);
                    if(a == ' ')
                    {
                        numberText.repalceChar('0', DateIndex.year + 2);
                        numberText.repalceChar(b, DateIndex.year + 3);
                    }
                    if (b == ' ')
                    {
                        numberText.repalceChar('0', DateIndex.year + 2);
                        numberText.repalceChar(a, DateIndex.year + 3);
                    }
                    else
                    {
                        numberText.repalceChar(a, DateIndex.year + 2);
                        numberText.repalceChar(b, DateIndex.year + 3);
                    }
                }
                //if (!yearTxt.Any(Char.IsWhiteSpace))
                numberText.selectionStart = DateIndex.year + 4;
            }
        }

        public override dynamic parseNumber(string numberTxt)
        {
            numberTxt = numberTxt.Replace("\n", ":");
            DateTime date;
            string fmt = "MM/dd/yyyy";
            if (dateFormat == DateTimeFormat.Date_12_HourTime)
                fmt = "MM/dd/yyyy:hh:mm:tt";
            if (dateFormat == DateTimeFormat.Date_24_HourTime)
                fmt = "MM/dd/yyyy:HH:mm";
            if (DateTime.TryParseExact(numberTxt, fmt, CultureInfo.InvariantCulture,
                DateTimeStyles.None, out date))
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

        virtual public void insertLetter(NumberText numberText, char c)
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