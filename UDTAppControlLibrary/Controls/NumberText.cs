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
                string yearText = numberString.Split('/')[2];
                if (!yearText.Any(Char.IsWhiteSpace))
                { 
                    Int32 year;
                    if (Int32.TryParse(yearText, out year))
                        return year;
                }
                return null;
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
