using UnityEngine;
using System;

namespace Amanita.UI
{
    [CreateAssetMenu(fileName = "IntegerFormatter", menuName = "Amanita/UI/Formatters/IntegerFormatter")]
    public class IntegerFormatter : TextFormatter
    {
        protected override string DefaultFormat => "D2";

        protected override bool CanWorkWith(object toFormat)
        {
            return toFormat is int;
        }

        protected override string FormatAsAppropriate(object toFormat)
        {
            return FormatInteger((int)toFormat);
        }

        public virtual string FormatInteger(int number)
        {
            string formattedNumber;
            if (SetToConvertToRomanNumerals)
            {
                formattedNumber = RomanNumeralConverter.ToRoman(number);
            }
            else
            {
                formattedNumber = number.ToString(FormatString);
            }
            string result = $"{Prefix}{formattedNumber}{Postfix}";
            return result;
        }

        protected virtual bool SetToConvertToRomanNumerals
        {
            get => !string.IsNullOrEmpty(FormatString) && FormatString.ToLower() == "roman";
        }
        
        protected override void OnValidate()
        {
            base.OnValidate();
            ValidateIntegerFormat();
            void ValidateIntegerFormat()
            {
                if (SetToConvertToRomanNumerals)
                {
                    // If we are set to convert to Roman numerals, we don't need to validate the format string.
                    return;
                }

                try
                {
                    int sample = 1; // Just a sample integer to test the format
                    string formattedSample = sample.ToString(FormatString);
                    // ^Throws if format is totally invalid
                }
                catch (FormatException)
                {
                    Debug.LogWarning($"Invalid integer format: {FormatString}. Using default format: {DefaultFormat}.");
                    FormatString = DefaultFormat;
                }
            }
        }
    }
}