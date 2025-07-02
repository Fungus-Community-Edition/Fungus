using System;
using System.Text;
using System.Collections.Generic;

namespace Amanita
{
    public static class RomanNumeralConverter
    {
        private static IDictionary<int, string> numeralMainLetters = new Dictionary<int, string>()
        {
            { 1000, "M" }, { 900, "CM" }, { 500, "D" },
            { 400, "CD" }, { 100, "C" },  { 90, "XC" },
            { 50, "L" },  { 40, "XL" }, { 10, "X" },
            { 9, "IX" },  { 5, "V" },   { 4, "IV" },
            { 1, "I" }
        };

        public static string ToRoman(int number)
        {
            if (number < min || number > max)
            {
                string errorMessage = $"Value {number} is invalid. We need it to be between {min} and {max}.";
                throw new ArgumentOutOfRangeException(nameof(number), errorMessage);
            }

            var result = new StringBuilder();
            foreach (var (value, numeral) in numeralMainLetters)
            {
                while (number >= value)
                {
                    result.Append(numeral);
                    number -= value;
                }
            }
            return result.ToString();
        }

        private static readonly int min = 1;
        private static readonly int max = 3999;
    }
}