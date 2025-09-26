using System;

namespace Amanita.SaveSys
{
    public static class IntegerExtensions
    {
        public static string ToString(this int num, SlotNumFormat format)
        {
            string numStr = string.Empty;
            switch (format)
            {
                case SlotNumFormat.PaddedTwoDigits:
                    numStr = num.ToString("D2");
                    break;
                case SlotNumFormat.Ordinal:
                    numStr = num.ToString();
                    break;
                case SlotNumFormat.RomanNumeral:
                    numStr = RomanNumeralConverter.ToRoman(num);
                    break;
                case SlotNumFormat.Null:
                    throw new NotImplementedException("Null format is not a valid format.");
            }

            return numStr;
        }
    }
}