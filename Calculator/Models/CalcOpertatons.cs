using System;
using System.Collections.Generic;

namespace SimpleCalculator.Models
{
    public class CalcOperation
    {

        public Func<decimal, decimal, decimal> Operation { get; set; }
        public Func<string, string, string> Formula { get; set; }
    }

    // Класс предоставляет доступ к коллекции методов математических операций и репрезентующих их строковых формул
    // (с типом делегата) по ключу с обозначением операции.
    // Для унификации все методы принимают два аргумента, но в некоторых операция выполняется только над первым из них

    public static class CalcOpertatons
    {
        private static Dictionary<string, CalcOperation> operators = new Dictionary<string, CalcOperation>()
        {
            {
                "+", new CalcOperation()
                {
                    Operation = (a, b) => a + b,
                    Formula = (a, b) => $"{a}+{BracketNegativeNumber(b)}="
                }
            },
            {
                "–", new CalcOperation()
                {
                    Operation = (a, b) => a - b,
                    Formula = (a, b) => $"{a}–{BracketNegativeNumber(b)}="
                }
            },
            {
                "×", new CalcOperation()
                {
                    Operation = (a, b) => a * b,
                    Formula = (a, b) => $"{a}×{BracketNegativeNumber(b)}="
                }
            },
            {
                "÷", new CalcOperation()
                {
                    Operation = (a, b) => a / b,
                    Formula = (a, b) => $"{a}÷{BracketNegativeNumber(b)}="
                }
            },
            {
                "x²", new CalcOperation()
                {
                    Operation = (a, b) => a * a,
                    Formula = (a, b) => $"{BracketNegativeNumber(a)}²="
                }
            },
            {
                "√x", new CalcOperation()
                {
                    Operation = (a, b) => Sqrt(a),
                    Formula = (a, b) => $"√{BracketNegativeNumber(a)}="
                }
            },
            {
                "1/x", new CalcOperation()
                {
                    Operation = (a, b) => 1 / a,
                    Formula = (a, b) => $"1/{BracketNegativeNumber(a)}="
                }
            },
            {
                "+/-", new CalcOperation()
                {
                    Operation = (a, b) => -a,
                    Formula = (a, b) => $"–{BracketNegativeNumber(a)}="
                }
            }
        };

        public static Dictionary<string, CalcOperation> Items { get => operators; }

        // Метод обрамляет скобками переданную строку, если она содержит отрицательное число
        private static string BracketNegativeNumber(String number)
        {
            try
            {
                return (Convert.ToDecimal(number) < 0) ? $"({number})" : $"{number}";
            }
            catch (Exception)
            {
                return number;
            }
        }

        // Метод предназначен для вычисления квадратного корня из числа типа decimal
        public static decimal Sqrt(decimal x, decimal epsilon = 0.0M)
        {
            if (x < 0) throw new OverflowException("Невозможно вычислить корень из отрицательного числа");

            decimal current = (decimal)Math.Sqrt((double)x), previous;
            do
            {
                previous = current;
                if (previous == 0.0M) return 0;
                current = (previous + x / previous) / 2;
            }
            while (Math.Abs(previous - current) > epsilon);
            return current;
        }
    }
}