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
            //displayText = fromatProvider.formatText(ctrlText);
            displayText = ctrlText;
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

            if (!fromatProvider.acceptChar(src.CaretIndex, arg.Text[0], src))
            {
                arg.Handled = true;
            }
            else if (src.Text == fromatProvider.prompt)
            {
                //arg.Handled = true;
                src.Text = "";
                //src.CaretIndex = 1;
            }
        }

        public virtual void textChanged(TextBox src, TextChangedEventArgs arg)
        {
            arg.Handled = true;
            int caretTmp = src.CaretIndex;
            int lengthTmp = src.Text.Length;
            setDisplayText(src.Text);
            src.FontWeight = FontWeights.Normal;
            if (displayText == fromatProvider.prompt)
            {
                src.FontWeight = FontWeights.UltraLight;
            }

            if (displayText.Length != lengthTmp)
            {
                caretTmp = caretTmp + (displayText.Length - lengthTmp);
            }

            //if (fromatProvider.caretOverride != null)
            //{
            //    caretTmp = (int)fromatProvider.caretOverride;
            //    fromatProvider.caretOverride = null;
            //}

            src.Text = displayText;
            src.CaretIndex = caretTmp;
        }

    }

    public struct FormatString
    {
        public FormatString(string fmtString, int _offset)
        {
            fromatChars = fmtString; 
            offset = _offset; 
        }
        public string fromatChars;
        // +1 is prepend, -1 is append
        public int offset;

    }

    public class NumberSymbol
    {
        public NumberSymbol(string _pre, string _post) { pre = _pre; post = _post; }
        public string pre = "";
        public string post = "";
    }

    public enum FormatType { Percent = 1, Currency, Interger, Decimal, Date };
    public class NumberFromatProvider<T>
    {
        public NumberFromatProvider(FormatType fmtType, T maxNumber, T minNumber)
        {
            type = fmtType;
            numberMax = maxNumber;
            numberMin = minNumber;
            if (type == FormatType.Currency)
            {
                prompt = "Enter Amount";
                //formatChars.Add(new FormatString("$", 1));
                positiveNumberSymbol = new NumberSymbol("$", "");
                negativeNumberSymbol = new NumberSymbol("$(", ")");
            }
            if (type == FormatType.Percent)
            {
                prompt = "Enter Percent";
                //formatChars.Add(new FormatString(" %", -2));
                positiveNumberSymbol = new NumberSymbol("", " %");
                negativeNumberSymbol = new NumberSymbol("", " %");
            }
            if (type == FormatType.Interger)
            {
                prompt = "Enter Number.";
                positiveNumberSymbol = new NumberSymbol("", "");
                negativeNumberSymbol = new NumberSymbol("", "");
            } 
            if (type == FormatType.Decimal)
            {
                prompt = "Enter Decimal.";
                positiveNumberSymbol = new NumberSymbol("", "");
                negativeNumberSymbol = new NumberSymbol("", "");
            }
        }

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

        private int fromatPerpendLength 
        {
            get
            {
                int len = 0;
                foreach(FormatString str in formatChars)
                {
                    if (str.offset > 0)
                        len += str.fromatChars.Length;
                }
                return len;
            }
        }

        private int fromatAppendLength
        {
            get
            {
                int len = 0;
                foreach(FormatString str in formatChars)
                {
                    if (str.offset < 0)
                        len += str.fromatChars.Length;
                }
                return len;
            }
        }

        public int? caretOverride = null;
        public int adjustCaret(int caretPos, string text)
        {
            int textLength = text.Length;
            if (caretPos < fromatPerpendLength)
                caretPos = fromatPerpendLength;
            if (caretPos > textLength - fromatAppendLength)
                caretPos = textLength - fromatAppendLength;
            if (caretPos > textLength)
                caretPos = textLength;
            if (caretPos < 0)
                caretPos = 0;


            return caretPos;
        }

        public int getCaretPos(int curPos, int oldTextLength, int newTextLenght)
        {
            int caretPos = 0;
            if (caretOverride != null)
            {
                caretPos = fromatPerpendLength + (int)caretOverride;
                caretOverride = null;
            }
            else
            {
                caretPos = curPos + (newTextLenght - oldTextLength);
            }
            return caretPos;
        }

        public string formatNumber(T number, string ctrlTxt)
        {
            string numTxt = "";
            if (number == null) return prompt;

            else if (type == FormatType.Currency)
            {
                numTxt = string.Format("{0:n2}", number);
                //numTxt = formatText(numTxt);
            }
            else if (type == FormatType.Percent)
            {
                Decimal? percent = ToDecimal(number) * 100;
                numTxt = string.Format("{0}", percent);
                if(numTxt.Contains("."))
                    numTxt = numTxt.TrimEnd('0');
            }
            else if (type == FormatType.Decimal)
            {
                numTxt = string.Format("{0}", number);
                //Decimal? decNum = (Decimal?)ToDecimal(number); 
                //int wholeNum = (int)decNum;

                //string numTxtWhole = string.Format("{0:n0}", wholeNum);
                //int decPos = numTxt.IndexOf(".");
                //if (decPos > 0)
                //{
                //    numTxt = numTxt.Substring(decPos);
                //    numTxt = string.Format("{0}{1}", numTxtWhole, numTxt);
                //}
                //else
                //    numTxt = numTxtWhole;

                //numTxt = formatText(numTxt);

                if (!numTxt.Contains(".") && ctrlTxt.Last() == '.')
                    numTxt = numTxt + '.';
            }

            else if(type == FormatType.Interger)
            {
                numTxt = string.Format("{0:n0}", number);
                //numTxt = formatText(numTxt);
            }

            return formatText(numTxt); 
        }

        private string addCommas(string text)
        {
            if(type != FormatType.Interger)
            {
                text = text.Replace(",", "");
                int offset = text.IndexOf(".");
                if(offset < 0) offset = text.Length;
                offset -= 3;
                while(offset > 0)
                {
                    text = text.Insert(offset, ",");
                    offset -= 3;
                }
            }
            return text;
        }

        private string formatText(string ctrlText)
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
            else if (string.IsNullOrEmpty(unFormatText(ctrlText)))
            {
                displayText = prompt;
                return displayText;
            }
            else
            {
                foreach (FormatString fmtStr in formatChars)
                {
                    displayText = removeFmtChar(fmtStr, displayText);
                }
                displayText = addCommas(displayText);
                foreach (FormatString fmtStr in formatChars)
                {
                    displayText = insertFmtChar(fmtStr, displayText);
                }
                //if (type != FormatType.Interger && !displayText.Contains("."))
                //    displayText = displayText + ".";
                return displayText;
            }

        }

        public string unFormatText(string text)
        {
            text = text.Replace(",", "");
            foreach (FormatString fmtChar in formatChars)
                text = removeFmtChar(fmtChar, text);
            return text;
        }

        public bool acceptChar(int postion, char c, TextBox src)
        {
            bool haveDecPt = true;
            bool promptVisable = src.Text == prompt;
            if (type != FormatType.Interger && !promptVisable)
            {
                haveDecPt = src.Text.Contains(".");
                if (c == '.' && haveDecPt)
                {
                    // move decimal pt to new postion
                    int caretTmp = src.CaretIndex;
                    src.Text = src.Text.Replace(".", "");
                    //src.Text = src.Text.Insert(caretTmp, ".");
                    src.CaretIndex = caretTmp+1;
                    return true;
                }
            }

            if(promptVisable)
            {
                //return (Char.IsDigit(c) || c == '+' || c == '-' || (c == '.'));
                if ((Char.IsDigit(c) || c == '+' || c == '-' || (c == '.')))
                {
                    if(Char.IsDigit(c)) caretOverride = 1;
                    return true;
                }
                return false;
            }
            else if (postion == 0)
            {
                //return (Char.IsDigit(c) || c == '+' || c == '-' || (c == '.' && !haveDecPt));
                return (Char.IsDigit(c) || c == '+' || c == '-');
            }
            else
            {
                //return (Char.IsDigit(c) || (c == '.' && !haveDecPt));
                return (Char.IsDigit(c) || c == '.');
            }
        }

        public void previewKeyDown(TextBox src, KeyEventArgs arg)
        {
            if (src.Text != prompt)
            {
                if (arg.Key == Key.Back)
                {
                    if (src.SelectionStart > 0 && src.Text[src.SelectionStart - 1] == '.')
                    { 
                        src.SelectionStart = src.SelectionStart - 1;
                        arg.Handled = true;
                    }
                }
                if (arg.Key == Key.Delete)
                {
                    if (src.SelectionStart < src.Text.Length && src.Text[src.SelectionStart] == '.')
                    {
                        if (src.SelectionStart < src.Text.Length-1)
                            src.SelectionStart = src.SelectionStart + 1;
                        arg.Handled = true;
                    }
                }

                return;
            }

            if (arg.Key == Key.Space)
            {
                arg.Handled = true;
            }
            if (arg.Key == Key.Back)
            {
                arg.Handled = true;
            }
            if (arg.Key == Key.Delete)
            {
                arg.Handled = true;
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

            if (string.IsNullOrEmpty(numberTxt) || numberTxt == prompt || numberTxt == "."
                || numberTxt == "-" || numberTxt == "+")
                return number; 

            numberTxt = unFormatText(numberTxt);

            if (type == FormatType.Decimal || type == FormatType.Currency || type == FormatType.Percent)
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
                if (type == FormatType.Percent)
                { 
                    Decimal? decNum = ToDecimal(number) / 100;
                    number = ChangeType(decNum);
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

        public FormatType type = FormatType.Interger;
        public string prompt = "";
        private List<FormatString> formatChars = new List<FormatString>();

        private string insertFmtChar(FormatString formatStr, string text)
        {
            if (formatStr.offset > 0)
            {
                text = formatStr.fromatChars + text;
            }
            else
            { 
                text = text + formatStr.fromatChars;
            }
            return text;
        }

        private string removeFmtChar(FormatString formatString, string text)
        {
            foreach(char c in formatString.fromatChars)
                text = text.Replace(c.ToString(), "");
            return text;
        }

    }
}
