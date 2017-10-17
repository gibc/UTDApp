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

        public void insertChar(char c, int offset)
        {
            numberString = numberString.Insert(offset, Char.ToString(c));
            if (offset < selectionStart) selectionStart++;
        }

        public void deleteChar()
        {
            numberString = numberString.Remove(selectionStart, 1);
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
                if (selectionStart > 0)
                    return numberString[selectionStart];
                else
                    return null;
            }
        }

        public void addCommas()
        {
            numberString = numberString.Replace(",", "");
            int offset = numberString.IndexOf(".");
            if (offset < 0) offset = numberString.Length;
            offset -= 3;
            while (offset > 0)
            {
                insertChar(',', offset);
                offset -= 3;
            }           
        }
    }
}
