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
    public class MaskedDecimalProvider : MaskedNumberProvider
    {
        public MaskedDecimalProvider() { }

        public Decimal? parseDecimal(string txtNum)
        {
            Decimal? number = null;
            if (string.IsNullOrEmpty(txtNum) || txtNum == prompt)
                return number;

            Decimal num;
            txtNum = txtNum.Replace(",", "");

            if (Decimal.TryParse(txtNum, out num))
            {
                number = num;
            }
            else if (txtNum[0] == '-')
            {
                number = decimalMin;
            }
            else
            {
                number = decimalMax;
            }

            if (number > decimalMax)
                number = decimalMax;
            else if (number < decimalMin)
                number = decimalMin;

            return number;
        }

        public override bool acceptChar(int postion, char c, string context)
        {
            bool haveDecPt = context.Contains(".");
            if (postion == 0)
            {
                return (Char.IsDigit(c) || c == '+' || c == '-' || (c == '.' && !haveDecPt));
            }
            else
            {
                return (Char.IsDigit(c) || (c == '.' && !haveDecPt));
            }
        }


        public string getDecimalText(Decimal? num)
        {
            string numTxt = "";
            if (num == null) return numTxt;

            numTxt = string.Format("{0}", num);
            if (displayText.Length > 0 && displayText.Last() == '.')
                return numTxt + '.';
            else return numTxt;

        }
    }

    public class MaskedNumberProvider
    {
        public MaskedNumberProvider()
        {

        }

        private bool _numberComplete = false;
        public bool numberComplete
        {
            get { return _numberComplete; }
            set { _numberComplete = value; }
        }

        private string _prompt = "";
        public string prompt
        {
            get { return _prompt; }
            set { _prompt = value; }
        }

        public void setDisplayText(string ctrlText)
        {
            numberComplete = false;
            if (string.IsNullOrEmpty(ctrlText))
            {
                displayText = prompt;
            }
            else
            {
                displayText = ctrlText;
                numberComplete = canParse(displayText);
            }
        }

        private string _displayText = "";
        public string displayText
        {
            get { return _displayText; }
            set { _displayText = value; }
        }


        protected Int32? numberMin = Int32.MinValue;
        protected Int32? numberMax = Int32.MaxValue;
        protected Decimal? decimalMin = Decimal.MinValue;
        protected Decimal? decimalMax = Decimal.MaxValue;

        protected virtual bool canParse(string val)
        {
            if (string.IsNullOrEmpty(val)) return true;
            if (val == prompt) return true;
            bool retVal = false;
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

        public Int32? parseNumber(string txtNum)
        {
            Int32? number = null;
            if (string.IsNullOrEmpty(txtNum) || txtNum == prompt)
                return number;

            int num;
            txtNum = txtNum.Replace(",", "");
            if (Int32.TryParse(txtNum, out num))
                number = num;
            else if (txtNum[0] == '-')
            {
                number = numberMin;
            }
            else
            {
                number = numberMax;
            }

            if (number > numberMax)
                number = numberMax;
            else if (number < numberMin)
                number = numberMin;

            return number;
        }

        public virtual bool acceptChar(int postion, char c, string context)
        {
            if (postion == 0)
            {
                return (c != '0' && Char.IsDigit(c) || c == '+' || c == '-');
            }
            else
            {
                return Char.IsDigit(c);
            }
        }

        public virtual string getNumberText(Int32? num)
        {
            if (num == null) return "";
            return string.Format("{0:n0}", num);
        }

        public virtual void previewText(TextBox src, TextCompositionEventArgs arg)
        {
            if (!acceptChar(src.CaretIndex, arg.Text[0], src.Text))
            {
                arg.Handled = true;
            }
            else if (src.Text == prompt)
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
            if (displayText == prompt)
            {
                src.FontWeight = FontWeights.UltraLight;
            }

            src.Text = displayText;
            src.CaretIndex = caretTmp;
        }

    }
}
