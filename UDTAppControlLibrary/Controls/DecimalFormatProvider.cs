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

        public NumberSymbol positiveNumberSymbol;

        public NumberSymbol negativeNumberSymbol;

        protected string unFormatText(string text)
        {
            text = text.Replace(",", "");
            return text;
        }

        protected dynamic checkRange(dynamic number)
        {

            if (number >= numberMax)
                return numberMax;
            else if (number <= numberMin)
                return numberMin;

            return number;
        }

        public bool outOfRange(dynamic number)
        {
            if (number >= numberMax)
                return true;
            else if (number <= numberMin)
                return true;

            return false;

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
