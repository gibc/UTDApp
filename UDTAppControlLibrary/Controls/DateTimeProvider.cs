using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UDTAppControlLibrary.Controls
{
using System;
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

            public override string getNumberText(dynamic number)
            {
                string numTxt = "";
                if (dateFormat == DateTimeFormat.Date_Only)
                {
                    numTxt = string.Format("{0:MM/dd/yyyy}", number);
                }
                else if (dateFormat == DateTimeFormat.Date_12_HourTime)
                {
                    numTxt = string.Format("{0:MM/dd/yyyy:hh:mm:tt}", number);
                }
                else if (dateFormat == DateTimeFormat.Date_24_HourTime)
                {
                    numTxt = string.Format("{0:MM/dd/yyyy:HH:mm}", number);
                }

                int offset = numTxt.IndexOf(':');
                if (offset > 0)
                {
                    numTxt = numTxt.Remove(offset, 1);
                    numTxt = numTxt.Insert(offset, "\n");
                }
                return numTxt;
            }

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
                if (arg != null) arg.Handled = true;
            }

            public override void deleteSelection(NumberText numberText)
            {
                int offset = numberText.selectionStart;
                while (offset < numberText.selectionStart + numberText.selectionLength)
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

            private void monthDigit(char c, int offset, NumberText numberText)
            {
                if (charVal(c) > 2)
                {
                    numberText.repalceChar('0', DateIndex.month);
                    numberText.repalceChar(c, DateIndex.month + 1);
                    numberText.selectionStart = DateIndex.day;
                    return;
                }

                if (offset == 0 && charVal(c) <= 1)
                {
                    numberText.repalceChar(c, DateIndex.month);
                    if (charVal(c) == 0)
                    {
                        if (charVal(numberText.monthTxt[DateIndex.month + 1]) == '0')
                            numberText.repalceChar(' ', DateIndex.month + 1);
                    }

                    if (charVal(c) == 1)
                    {
                        if (numberText.monthTxt[1] != ' ' && charVal(numberText.monthTxt[1]) > 2)
                            numberText.repalceChar(' ', DateIndex.month + 1);
                    }
                    numberText.selectionStart = DateIndex.month + 1;
                }

                if (offset == 1)
                {
                    if (charVal(c) >= 3)
                    {
                        numberText.repalceChar(c, DateIndex.month + 1);
                        numberText.repalceChar('0', DateIndex.month);
                        numberText.selectionStart = DateIndex.day;
                    }

                    else if (charVal(c) == 0)
                    {
                        numberText.repalceChar(c, DateIndex.month + 1);
                        numberText.selectionStart = DateIndex.day;
                        if (numberText.monthTxt[0] == '0')
                        {
                            numberText.repalceChar(' ', DateIndex.month);
                            numberText.selectionStart = DateIndex.month;
                        }
                    }

                    if (charVal(c) == 1 || charVal(c) == 2)
                    {
                        numberText.repalceChar(c, DateIndex.month + 1);
                        numberText.selectionStart = DateIndex.day;
                        if (numberText.monthTxt[0] == ' ')
                            numberText.repalceChar('0', DateIndex.month);
                    }
                }
            }

            private void adjustDay(NumberText numberText)
            {
                if (numberText.month != null)
                {
                    string maxDays = numberText.daysInMonth;
                    StringBuilder dayText = new StringBuilder(numberText.dayTxt);
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

            private void dayDigit(char c, int offset, NumberText numberText)
            {
                string maxDays = numberText.daysInMonth;

                if (offset == 0 && charVal(c) == 0)
                {
                    numberText.repalceChar(c, DateIndex.day);
                    if (numberText.dayTxt[1] == '0')
                        numberText.repalceChar(' ', numberText.selectionStart + 1);
                    numberText.selectionStart = DateIndex.day + 1;
                }

                if (offset == 0 && charVal(c) > 0 && charVal(c) <= 3)
                {
                    if (charVal(c) <= charVal(maxDays[0]))
                    {
                        numberText.repalceChar(c, DateIndex.day);
                        // if setting largest allowed 10s place then check max allowed 1s place
                        // and adj if needed
                        if (charVal(c) == charVal(maxDays[0])
                            && charVal(numberText.dayTxt[1]) > charVal(maxDays[1]))
                            numberText.repalceChar(maxDays[1], DateIndex.day + 1);

                        numberText.selectionStart = DateIndex.day + 1;
                    }
                }

                if (offset == 1)
                {
                    numberText.selectionStart = DateIndex.year;
                    if (charVal(c) == 0)
                    {
                        numberText.repalceChar(c, DateIndex.day + 1);
                        if (numberText.dayTxt[0] == '0')
                        {
                            numberText.repalceChar(' ', DateIndex.day);
                            numberText.selectionStart = DateIndex.day;
                        }
                    }
                    if (charVal(c) > 0)
                    {
                        // if 1s place is max
                        if (charVal(maxDays[0]) == charVal(numberText.dayTxt[0]))
                        {
                            // if char val is less than or equal 10s place max
                            if (charVal(c) <= charVal(maxDays[1]))
                            {
                                numberText.repalceChar(c, DateIndex.day + 1);
                            }
                            // else if char val is too high
                            else
                            {
                                numberText.repalceChar(c, DateIndex.day + 1);
                                numberText.repalceChar(' ', DateIndex.day);
                                numberText.selectionStart = DateIndex.day;
                            }
                        }
                        // else if 1s place is less than max
                        else
                            numberText.repalceChar(c, DateIndex.day + 1);
                    }
                }
            }

            private void yearDigit(char c, int offset, NumberText numberText)
            {
                if (offset < 4)
                {
                    numberText.repalceChar(c, DateIndex.year + offset);
                    if (offset < 3)
                        numberText.selectionStart = DateIndex.year + offset + 1;
                    else
                    {
                        if (dateFormat != DateTimeFormat.Date_Only)
                        {
                            if (numberText.year != null)
                                numberText.selectionStart = DateIndex.hour;
                        }
                        else
                            numberText.selectionStart = DateIndex.hour + 4;
                    }
                }
            }

            private int charVal(char? c)
            {
                return (char)c - '0';
            }

            private void hourDigit24(char c, int offset, NumberText numberText)
            {
                if (offset == 0)
                {
                    int maxDigitVal = 2;
                    if (charVal(c) <= maxDigitVal)
                    {
                        numberText.repalceChar(c, DateIndex.hour);
                        numberText.selectionStart = DateIndex.hour + 1;
                    }
                }
                if (offset == 1)
                {
                    numberText.repalceChar(c, DateIndex.hour + 1);
                    if (numberText.hour != null)
                        numberText.selectionStart = DateIndex.minute;
                }
            }

            private void hourDigit12(char c, int offset, NumberText numberText)
            {
                if (offset == 0)
                {
                    int maxDigitVal = 1;
                    if (charVal(c) <= maxDigitVal)
                    {
                        numberText.repalceChar(c, DateIndex.hour);
                        numberText.selectionStart++;
                        if (charVal(c) == 0)
                        {
                            if (charVal(numberText.hourTxt[1]) == 0)
                            {
                                numberText.repalceChar(' ', DateIndex.hour + 1);
                            }
                        }
                    }
                    else if (charVal(c) > maxDigitVal)
                    {
                        numberText.repalceChar('0', DateIndex.hour);
                        numberText.repalceChar(c, DateIndex.hour);
                        numberText.selectionStart = DateIndex.minute;
                    }
                }
                else if (offset == 1)
                {
                    if (charVal(c) == 0 && charVal(numberText.previousChar) == 0)
                    {
                        numberText.repalceChar(c, DateIndex.hour + 1);
                        numberText.repalceChar(' ', DateIndex.hour);
                        numberText.selectionStart = DateIndex.hour;
                    }
                    else
                    {
                        numberText.repalceChar(c, DateIndex.hour + 1);
                        if (numberText.hour != null)
                            numberText.selectionStart = DateIndex.minute;
                    }
                }
            }

            private void minuteDigit(char c, int offset, NumberText numberText)
            {
                if (offset == 0)
                {
                    if (charVal(c) <= 5)
                    {
                        numberText.repalceChar(c, numberText.selectionStart);
                        numberText.selectionStart = DateIndex.minute + 1;
                    }
                }
                else if (offset == 1)
                {
                    numberText.repalceChar(c, DateIndex.minute + 1);
                    if (numberText.minute != null)
                    {
                        if (dateFormat == DateTimeFormat.Date_12_HourTime)
                            numberText.selectionStart = DateIndex.meridiem;
                        else
                            numberText.selectionStart = DateIndex.minute + 1;
                    }
                }
            }

            public override void insertDigit(NumberText numberText, char c)
            {
                if (numberText.isMonthIndex)
                {
                    monthDigit(c, numberText.selectionStart - DateIndex.month, numberText);
                    if (numberText.month != null)
                    {
                        adjustDay(numberText);
                    }
                }
                else if (numberText.isDayIndex)
                {
                    dayDigit(c, numberText.selectionStart - DateIndex.day, numberText);
                }
                else if (numberText.isYearIndex)
                {
                    yearDigit(c, numberText.selectionStart - 6, numberText);
                    if (numberText.year != null)
                    {
                        adjustDay(numberText);
                    }
                }
                else if (numberText.isHourIndex)
                {
                    if (dateFormat == DateTimeFormat.Date_12_HourTime)
                        hourDigit12(c, numberText.selectionStart - DateIndex.hour, numberText);
                    if (dateFormat == DateTimeFormat.Date_24_HourTime)
                        hourDigit24(c, numberText.selectionStart - DateIndex.hour, numberText);
                }
                else if (numberText.isMinuteIndex)
                {
                    minuteDigit(c, numberText.selectionStart - DateIndex.minute, numberText);
                }

            }

            public override void insertLetter(NumberText numberText, char c)
            {
                if (numberText.isMeridiemIndex)
                {
                    if (c == 'a' || c == 'A')
                    {
                        numberText.repalceChar('A', DateIndex.meridiem);
                        numberText.repalceChar('M', DateIndex.meridiem + 1);
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
                        yearTxt.Substring(2, 2).All(Char.IsWhiteSpace))
                    {
                        if (!Char.IsWhiteSpace(yearTxt[0])) a = yearTxt[0];
                        if (!Char.IsWhiteSpace(yearTxt[1])) b = yearTxt[1];
                        numberText.repalceChar('2', DateIndex.year);
                        numberText.repalceChar('0', DateIndex.year + 1);
                        if (a == ' ')
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
                    return checkRange(date);
                return null;
            }
        }
    }
}
