using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace UDTAppControlLibrary.Controls
{
    public class NumberText
    {
        public NumberText() 
        {
        }

        public void insertChar(char c)
        {
            numberString = numberString.Insert(selectionStart, Char.ToString(c));
            selectionStart++;
        }

        public void repalceChar(char c, int offset)
        {
            deleteChar(offset);
            insertChar(c, offset);
        }

        public void insertChar(char c, int offset)
        {
            numberString = numberString.Insert(offset, Char.ToString(c));
            if (offset < selectionStart) 
                selectionStart++;
        }

        public void deleteChar()
        {
            if (selectionStart < numberString.Length)
                numberString = numberString.Remove(selectionStart, 1);
        }

        public void deleteChar(int offset)
        {
            numberString = numberString.Remove(offset, 1);
            if (offset < selectionStart)
                selectionStart--;
        }

        public void insertString(string str)
        {
            numberString = numberString.Insert(selectionStart, str);
            selectionStart += str.Length;
        }

        private bool _promptVisble = false;
        public bool promptVisble 
        {
            get { return _promptVisble; }
            set { _promptVisble = value; }
        }

        public void setPrompt(string str)
        {
            numberString = str;
            selectionStart = 0;
            selectionLength = 0;
            promptVisble = true;
        }

        public void deleteString()
        {
            numberString = numberString.Remove(selectionStart, selectionLength);
            selectionLength = 0;
        }

        public void clear()
        {
            numberString = "";
            selectionStart = 0;
            selectionLength = 0;
            promptVisble = false;
        }

        private int _selectionStart = 0;
        public int selectionStart
        {
            get { return _selectionStart; }
            set 
            {
                if (value < 0) value = 0;
                if (value > numberString.Length) value = numberString.Length;
                _selectionStart = value; 
            }
        }

        private int _selectionLength = 0;         
        public int selectionLength 
        {
            get { return _selectionLength; }
            set { _selectionLength = value; }
        }

        private string _numberString = "";        
        public string numberString
        {
            get { return _numberString; }
            private set { _numberString = value; }
        }

        public char? currentChar 
        {
            get 
            {
                if (selectionStart < numberString.Length)
                    return numberString[selectionStart];
                return null;
            }
        }

        public char? previousChar
        {
            get 
            {
                if (selectionStart > 1)
                    return numberString[selectionStart-1];
                else
                    return null;
            }
        }

        public string dayTxt
        {
            get { return numberString.Substring(DateIndex.day, 2); }
            set 
            {
                if (value != numberString.Substring(DateIndex.day, 2))
                {
                    numberString = numberString.Remove(DateIndex.day, 2);
                    numberString = numberString.Insert(DateIndex.day, value);
                }
            }
        }

        public bool isDayIndex { get { return selectionStart >= DateIndex.day && selectionStart < DateIndex.day + 2; } }
        public bool isMonthIndex { get { return selectionStart >= DateIndex.month && selectionStart < DateIndex.month + 2; } }
        public bool isYearIndex { get { return selectionStart >= DateIndex.year && selectionStart < DateIndex.year + 4; } }
        public bool isHourIndex { get { return selectionStart >= DateIndex.hour && selectionStart < DateIndex.hour + 2; } }
        public bool isMinuteIndex { get { return selectionStart >= DateIndex.minute && selectionStart < DateIndex.minute + 2; } }
        public bool isMeridiemIndex { get { return selectionStart >= DateIndex.meridiem && selectionStart < DateIndex.meridiem + 2; } }

        public Int32? day
        {
            get
            {
                if (promptVisble) return null;
                string dayText = numberString.Split('/')[1];
                if (!dayText.Any(Char.IsWhiteSpace))
                { 
                    Int32 day;
                    if (Int32.TryParse(dayText, out day))
                    {
                        if(day < 31)
                            return day;
                    }
                }
                return null;
            }
        }

        public Int32? year
        {
            get
            {
                if (promptVisble) return null;
                char[] splitChars = {'/', '\n', ':'};
                string yearText = numberString.Split(splitChars)[2];
                if (!yearText.Any(Char.IsWhiteSpace))
                { 
                    Int32 year;
                    if (Int32.TryParse(yearText, out year))
                        return year;
                }
                return null;
            }
        }

        public string yearTxt
        {
            get { return numberString.Substring(DateIndex.year, 4); }
            set
            {
                if (value != numberString.Substring(DateIndex.year, 4))
                {
                    numberString = numberString.Remove(DateIndex.year, 4);
                    numberString = numberString.Insert(DateIndex.year, value);
                }
            }
        }


        public string monthTxt
        {
            get { return numberString.Substring(DateIndex.month, 2); }
            set
            {
                numberString = numberString.Remove(DateIndex.month, 2);
                numberString = numberString.Insert(DateIndex.month, value);
            }
        }

        public Int32? month
        {
            get
            {
                if (promptVisble) return null;
                string monthText = numberString.Split('/')[0];
                if (!monthText.Any(Char.IsWhiteSpace))
                {
                    Int32 month;
                    if (Int32.TryParse(monthText, out month))
                    {
                        if (month > 0 && month <= 12)
                            return month;
                    }
                }
                return null;
            }
        }

        public string daysInMonth
        {
            get
            {
                string maxDays = "31";
                int? mo = month;
                if(mo != null)
                {
                    if(mo == 9 || mo == 4 || mo == 6|| mo == 11)
                    {
                        maxDays = "30";
                    }
                    if(mo == 2)
                    {
                        maxDays = "28"; 
                        if(year != null)
                        { 
                            if ((year % 4 == 0) && (year % 100 != 0)
                                || (year % 400 == 0))
                                maxDays = "29";
                        }
                    }
                }
                return maxDays;
            }
        }

        public void limitDecimaDigits(int maxDecmialDigits)
        {
            int offset = numberString.IndexOf(".");
            if(offset >= 0)
            {
                int strLen = numberString.Length-1;
                int digitCount = strLen - (offset);
                while (digitCount > maxDecmialDigits)
                {
                    deleteChar(digitCount + offset);
                    digitCount--;
                }
            }
        }

        public void addCommas()
        {
            removeCommas();
            int offset = numberString.IndexOf(".");
            if (offset < 0) offset = numberString.Length;
            offset -= 3;
            int endOffset = 0;
            if (numberString[0] == '-' || numberString[0] == '+')
                endOffset = 1;
            while (offset > endOffset)
            {
                insertChar(',', offset);
                offset -= 3;
            }           
        }

        private void removeCommas()
        {
            int offset = numberString.Length-1;
            while (offset >= 0)
            {
                if (numberString[offset] == ',')
                    deleteChar(offset);
                offset -= 1;
            }
        }

    }
}
