using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace UDTAppControlLibrary.Controls
{
    //public class MaskedDecimalProvider : MaskedNumberProvider
    //{
    //    public MaskedDecimalProvider(FormatType fmtType) : base(fmtType)
    //    {
    //        fromatProvider = new NumberFromatProvider<Decimal>(fmtType, "Enter a Decimal.");
    //    }

    //    //public Decimal? parseDecimal(string txtNum)
    //    //{
    //    //    Decimal? number = null;
    //    //    txtNum = fromatProvider.unFormatText(txtNum);
    //    //    if (string.IsNullOrEmpty(txtNum) || txtNum == fromatProvider.prompt)
    //    //        return number;

    //    //    Decimal num;
    //    //    if (Decimal.TryParse(txtNum, out num))
    //    //    {
    //    //        number = num;
    //    //    }
    //    //    else if (txtNum[0] == '-')
    //    //    {
    //    //        number = decimalMin;
    //    //    }
    //    //    else
    //    //    {
    //    //        number = decimalMax;
    //    //    }

    //    //    if (number > decimalMax)
    //    //        number = decimalMax;
    //    //    else if (number < decimalMin)
    //    //        number = decimalMin;

    //    //    return number;
    //    //}

    //    public override bool acceptChar(int postion, char c, string context)
    //    {
    //        bool haveDecPt = context.Contains(".");
    //        if (postion == 0)
    //        {
    //            return (Char.IsDigit(c) || c == '+' || c == '-' || (c == '.' && !haveDecPt));
    //        }
    //        else
    //        {
    //            return (Char.IsDigit(c) || (c == '.' && !haveDecPt));
    //        }
    //    }


    //    //public string getDecimalText(Decimal? num)
    //    //{
    //    //    string numTxt = "";
    //    //    if (num == null) return numTxt;

    //    //    numTxt = string.Format("{0}", num);
    //    //    int wholeNum = (int)num;
    //    //    string numTxtWhole = string.Format("{0:n0}", wholeNum);
    //    //    int decPos = numTxt.IndexOf(".");
    //    //    if (decPos > 0)
    //    //    {
    //    //        numTxt = numTxt.Substring(decPos);
    //    //        numTxt = string.Format("{0}{1}", numTxtWhole, numTxt);
    //    //    }
    //    //    else
    //    //        numTxt = numTxtWhole;

    //    //    numTxt = textFormat.formatText(numTxt);
    //    //    if (displayText.Length > 0 && displayText.Last() == '.')
    //    //        return numTxt + '.';
    //    //    else return numTxt;

    //    //}
    //}

    public class MaskedNumberProvider<T>
    {
        public MaskedNumberProvider(FormatType fmtType, T maxNumber, T minNumber)
        {
            fromatProvider = new NumberFromatProvider<T>(fmtType, maxNumber, minNumber);
        }

        //protected NumberFromatProvider<Int32> fromatProvider;
        public NumberFromatProvider<T> fromatProvider;

        private bool _numberComplete = false;
        public bool numberComplete
        {
            get { return _numberComplete; }
            set { _numberComplete = value; }
        }

        //private string _prompt = "";
        //public string prompt
        //{
        //    get { return _prompt; }
        //    set { _prompt = value; }
        //}

        public void setDisplayText(string ctrlText)
        {
            numberComplete = false;
            //if (ctrlText == prompt)
            //{
            //    displayText = ctrlText;
            //}
            //else if (string.IsNullOrEmpty(ctrlText))
            //{
            //    displayText = prompt;
            //}
            //else
            //{
            //    displayText = textFormat.formatText(ctrlText);
            //    numberComplete = canParse(displayText);
            //}
            displayText = fromatProvider.formatText(ctrlText);
            numberComplete = fromatProvider.canParse(displayText);

        }

        private string _displayText = "";
        public string displayText
        {
            get { return _displayText; }
            set { _displayText = value; }
        }


        //protected Int32? numberMin = Int32.MinValue;
        //protected Int32? numberMax = Int32.MaxValue;
        //protected Decimal? decimalMin = Decimal.MinValue;
        //protected Decimal? decimalMax = Decimal.MaxValue;

        //protected virtual bool canParse(string val)
        //{
        //    if (string.IsNullOrEmpty(val)) return true;
        //    if (val == fromatProvider.prompt) return true;
        //    bool retVal = false;
        //    val = fromatProvider.unFormatText(val);
        //    foreach (char c in val)
        //    {
        //        //if (!(c == '0' || c == '.' || c == '-'))
        //        if (!(c == '.' || c == '-'))
        //        {
        //            return true;
        //        }
        //    }
        //    return retVal;
        //}

        //public Int32? parseNumber(string txtNum)
        //{
        //    Int32? number = null;
        //    if (string.IsNullOrEmpty(txtNum) || txtNum == fromatProvider.prompt)
        //        return number;

        //    int num;
        //    txtNum = txtNum.Replace(",", "");
        //    if (Int32.TryParse(txtNum, out num))
        //        number = num;
        //    else if (txtNum[0] == '-')
        //    {
        //        number = numberMin;
        //    }
        //    else
        //    {
        //        number = numberMax;
        //    }

        //    if (number > numberMax)
        //        number = numberMax;
        //    else if (number < numberMin)
        //        number = numberMin;

        //    return number;
        //}

        //public virtual bool acceptChar(int postion, char c, string context)
        //{
        //    bool haveDecPt = true;
        //    if(fromatProvider.type != FormatType.Interger)
        //        haveDecPt = context.Contains(".");

        //    if (postion == 0)
        //    {
        //        return (Char.IsDigit(c) || c == '+' || c == '-' || (c == '.' && !haveDecPt));
        //    }
        //    else
        //    {
        //        return (Char.IsDigit(c) || (c == '.' && !haveDecPt));
        //    }
        //}

        //public virtual string getNumberText(Int32? num)
        //{
        //    if (num == null) return "";
        //    return string.Format("{0:n0}", num);
        //}

        public virtual void previewText(TextBox src, TextCompositionEventArgs arg)
        {
            if (!fromatProvider.acceptChar(src.CaretIndex, arg.Text[0], src.Text))
            {
                arg.Handled = true;
            }
            else if (src.Text == fromatProvider.prompt)
            {
                arg.Handled = true;
                src.Text = arg.Text;
                src.CaretIndex = 1;
            }
        }

        public virtual void textChanged(TextBox src, TextChangedEventArgs arg)
        {
            arg.Handled = true;
            int caretTmp = src.CaretIndex;
            setDisplayText(src.Text);
            src.FontWeight = FontWeights.Normal;
            if (displayText == fromatProvider.prompt)
            {
                src.FontWeight = FontWeights.UltraLight;
            }

            src.Text = displayText;
            src.CaretIndex = caretTmp;
        }

    }

    public struct FormatChar
    {
        public FormatChar(string fmtChar, int _offset)
        {
            fromatChar = fmtChar; 
            offset = _offset; 
        }
        public string fromatChar;
        // +1 is first pos, -1 is last position
        public int offset;

    }

    public enum FormatType { Currency, Percent, Interger, Decimal, Date}
    public class NumberFromatProvider<T>
    {
        public NumberFromatProvider(FormatType fmtType, T maxNumber, T minNumber)
        {
            type = fmtType;
            //numberMax = (T)Convert.ChangeType(maxNumber, typeof(T));
            //numberMin = (T)Convert.ChangeType(minNumber, typeof(T)); ;
            numberMax = maxNumber;
            numberMin = minNumber;
            if (type == FormatType.Currency)
            {
                prompt = "Enter $ Amount";
                formatChars.Add(new FormatChar("$", 1));
            }
            if (type == FormatType.Percent)
            {
                prompt = "Enter %";
                formatChars.Add(new FormatChar("%", -1));
            }
            if (type == FormatType.Interger)
            {
                prompt = "Enter Number.";
            } 
            if (type == FormatType.Decimal)
            {
                prompt = "Enter Decimal.";
            }
        }


        public string formatNumber(T number, string ctrlTxt)
        {
            string numTxt = "";
            if (number == null) return prompt;

            else if (type == FormatType.Decimal)
            {
                numTxt = string.Format("{0}", number);
                int wholeNum = (int)Convert.ChangeType(number, typeof(Int32)); ;

                string numTxtWhole = string.Format("{0:n0}", wholeNum);
                int decPos = numTxt.IndexOf(".");
                if (decPos > 0)
                {
                    numTxt = numTxt.Substring(decPos);
                    numTxt = string.Format("{0}{1}", numTxtWhole, numTxt);
                }
                else
                    numTxt = numTxtWhole;

                numTxt = formatText(numTxt);

                if (ctrlTxt.Length > 0 && ctrlTxt.Last() == '.')
                    numTxt = numTxt + '.';
            }

            else if(type == FormatType.Interger)
            {
                numTxt = string.Format("{0:n0}", number);
                numTxt = formatText(numTxt);
            }

            return numTxt;
        }

        public string formatText(string ctrlText)
        {
            string displayText = ctrlText;
            if (ctrlText == prompt)
            {
                return displayText;
            }
            else if (string.IsNullOrEmpty(ctrlText))
            {
                displayText = prompt;
                return displayText;
            }
            else
            {
                foreach (FormatChar fmtChar in formatChars)
                    displayText = insertFmtChar(fmtChar, displayText);
                return displayText;
            }

        }

        public string unFormatText(string text)
        {
            text = text.Replace(",", "");
            foreach (FormatChar fmtChar in formatChars)
                text = removeFmtChar(fmtChar, text);
            return text;
        }

        public bool acceptChar(int postion, char c, string context)
        {
            bool haveDecPt = true;
            if (type != FormatType.Interger)
                haveDecPt = context.Contains(".");

            if (postion == 0)
            {
                return (Char.IsDigit(c) || c == '+' || c == '-' || (c == '.' && !haveDecPt));
            }
            else
            {
                return (Char.IsDigit(c) || (c == '.' && !haveDecPt));
            }
        }

        public bool canParse(string val)
        {
            if (string.IsNullOrEmpty(val)) return true;
            if (val == prompt) return true;
            bool retVal = false;
            val = unFormatText(val);
            foreach (char c in val)
            {
                //if (!(c == '0' || c == '.' || c == '-'))
                if (!(c == '.' || c == '-'))
                {
                    return true;
                }
            }
            return retVal;
        }

        private T convertInt(Int32? num)
        {
            return (T)Convert.ChangeType(num, typeof(T));
        }

        private T convertDecimal(Decimal? num)
        {
            return (T)Convert.ChangeType(num, typeof(T));
        }

        private Int32? getIntValue(T number)
        {
            return (Int32)Convert.ChangeType(number, typeof(Int32?));
        }

        private Decimal? getDecimalValue(T number)
        {
            return (Int32)Convert.ChangeType(number, typeof(Decimal?));
        }

        private T checkRange(T number)
        {
            if (typeof(T) == typeof(Int32?))
            {
                if (ToInt(number) > ToInt(numberMax))
                    return numberMax;
                else if (ToInt(number) < ToInt(numberMin))
                    return numberMin;
            } 
            if (typeof(T) == typeof(Decimal?))
            {
                if (ToDecimal(number) > ToDecimal(numberMax))
                    return numberMax;
                else if (ToDecimal(number) < ToDecimal(numberMin))
                    return numberMin;
            }
            return number;
        }


        private T numberMax ;
        private T numberMin ;

        private Int32? ToInt(T value)
        {
            var t = typeof(Int32?);

            if (t.IsGenericType && t.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                if (value == null)
                {
                    return default(Int32?);
                }

                t = Nullable.GetUnderlyingType(t);
            }

            return (Int32?)Convert.ChangeType(value, t);
        }

        private Decimal? ToDecimal(T value)
        {
            var t = typeof(Decimal?);

            if (t.IsGenericType && t.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                if (value == null)
                {
                    return default(Int32?);
                }

                t = Nullable.GetUnderlyingType(t);
            }

            return (Decimal?)Convert.ChangeType(value, t);
        }

        private T ChangeType(object value)
        {
            var t = typeof(T);

            if (t.IsGenericType && t.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                if (value == null)
                {
                    return default(T);
                }

                t = Nullable.GetUnderlyingType(t);
            }

            return (T)Convert.ChangeType(value, t);
        }

        public T parseNumber(string numberTxt)
        {
            T number = default(T);

            if (string.IsNullOrEmpty(numberTxt) || numberTxt == prompt)
                return number; 

            numberTxt = unFormatText(numberTxt);

            if(type == FormatType.Decimal)
            {
                Decimal num;
                if (Decimal.TryParse(numberTxt, out num))
                {
                    number = ChangeType(num);
                }
                else if (numberTxt[0] == '-')
                {
                    number = numberMin;
                }
                else
                {
                    number = numberMax;
                }
            }

            else if (type == FormatType.Interger)
            { 
                int num;

                if (Int32.TryParse(numberTxt, out num))
                {
                    number = ChangeType(num);
                }
                else if (numberTxt[0] == '-')
                {
                    number = numberMin;
                }
                else
                {
                    number = numberMax;  
                }

            }

            return checkRange(number) ;

        }

        private FormatType type = FormatType.Interger;
        public string prompt = "";
        private List<FormatChar> formatChars = new List<FormatChar>();

        private string insertFmtChar(FormatChar formatChar, string text)
        {
            int charPos;
            if (formatChar.offset > 0)
            {
                charPos = formatChar.offset - 1;
            }
            else
            {
                charPos = text.Length + (formatChar.offset);
            }
            if(charPos < text.Length)
            {
                if (text[charPos] != formatChar.fromatChar[0])
                {
                    text = text.Insert(charPos, formatChar.fromatChar);
                }
            }
            return text;
        }

        private string removeFmtChar(FormatChar formatChar, string text)
        {
            int charPos;
            if (formatChar.offset > 0)
            {
                charPos = formatChar.offset - 1;
            }
            else
            {
                charPos = text.Length + (formatChar.offset);
            }
            if (charPos < text.Length)
            {
                if (text[charPos] == formatChar.fromatChar[0])
                {
                    text = text.Remove(charPos, 1);
                }
            }
            return text;
        }

    }
}
