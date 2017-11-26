using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace UDTAppControlLibrary.Controls
{
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
        public bool isMax = false;
        public bool isMin = false;

        public NumberSymbol positiveNumberSymbol;

        public NumberSymbol negativeNumberSymbol;

        public virtual string getNumberText(dynamic number)
        {
            //string numTxt = "";
            //if (fromat == DecimalFormatType.Decimal)
            //{
            //    numTxt = string.Format("{0}", number);
            //}
            //else if (fromat == DecimalFormatType.Currency)
            //{
            //    numTxt = string.Format("{0:n2}", number);
            //}
            //else if (fromat == DecimalFormatType.Percent)
            //{
            //    numTxt = string.Format("{0:n2}", 100 * number);
            //}
            return string.Format("{0}", number);
        }

        protected string unFormatText(string text)
        {
            text = text.Replace(",", "");
            return text;
        }

        protected dynamic checkRange(dynamic number)
        {
            isMax = isMin = false;
            if (number >= numberMax)
            {
                isMax = true;
                return numberMax;
            }
            else if (number <= numberMin)
            {
                isMin = true;
                return numberMin;
            }
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

        protected dynamic parseNumberTxt(string numberTxt)
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
                number = Decimal.MinValue;
            }
            else
            {
                number = Decimal.MaxValue; 
            }
            return number;
        }

        virtual public dynamic parseNumber(string numberTxt)
        {
            Decimal? number = parseNumberTxt(numberTxt);
            return checkRange(number);
        }

    }
}
