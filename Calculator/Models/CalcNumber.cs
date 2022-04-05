using System;
using System.Linq;
using System.Windows;

namespace SimpleCalculator.Models
{
    //Класс реализует объект-число, с которым опереирует калькулятор
    // Помимо числового значения типа decimal объект содержит его строковое представление(ограниченное заданным
    // количеством символов).
    // Уменьшение точности строкового представления числа по сравнению с его decimal значением
    // позволяет не показывать пользователю неточности вычислений типа x = 1 / 6, x*6 != 1
    // Класс также содержит свойства, возвращающие различные характеристики числа и его строкового представления
    // А также методы, позволяющие изменять число с учётом типичного интерфейса калькулятора (добавление цифры,
    // удаление последнего символа, инверсия знака).
    internal class CalcNumber
    {
        private const int limitCountOfDigits = 29; // Предельное количество значимых цифр в числе типа decimal
        private const char separatorChar = ','; // Символ-разделитель

        private decimal decimalValue;
        private string stringValue;
        private readonly int maxCountOfDigits;

        public delegate void InputNumberHandler(string stringNumber);
        public event InputNumberHandler OnValueChanged; // Событие - изменение значения числа

        // Изменеие строкового представления происходит только изнутри класса
        private string StringValue
        {
            get => stringValue;
            set
            {
                stringValue = value;
                // При изменении строкового представления вызывается событие изменения значения
                OnValueChanged?.Invoke(StringValue);
            }
        }

        public decimal Value
        {
            get => decimalValue;

            // Через это свойство можно непосредственно установить значение числа извне
            set
            {
                decimalValue = value;
                StringValue = value.ToString();
                // После конвертации в строку вызывается метод, нормализующий строковое представление числа
                NormalizeStringValue();
            }
        }

        public bool IsZero => StringValue.Trim(separatorChar).Equals("0");
        public bool IsNegative => Value < 0;
        public bool IsPositive => Value > 0;
        public bool IsFractional => StringValue.Contains(separatorChar);
        public int CountOfDigits => StringValue.Length - (IsNegative ? 1 : 0) - (IsFractional ? 1 : 0);
        public int MaxCountOfDigits => maxCountOfDigits;
        public int CountOfIntegerDigits
        {
            get => Math.Abs(Math.Round(Value)).ToString().Length;
        }
        public int CountOfFractionalDigits => CountOfDigits - CountOfIntegerDigits;

        // Конструктор может принимать аргумент - количество цифр в строковом представлении числа.
        // (по-умолчанию - Предельное количество значимых цифр в числе типа decimal)
        public CalcNumber(int maxCountOfDigits = limitCountOfDigits)
        {
            this.maxCountOfDigits =
                (maxCountOfDigits > 0 && maxCountOfDigits < limitCountOfDigits) ? maxCountOfDigits : limitCountOfDigits;
            Reset();
        }

        // Сброс числа (в ноль)
        public void Reset() => Value = 0;

        // Метод нормализует строковое представление числа, приводя его к максимальному количеству цифр, заданному
        // в свойстве MaxCountOfDigits. Если целая часть числа настолько велика, что превышает это количество, выбрасывается
        // исключение. В противном случае обрезается лишнее количество цифр в дробной части.
        // Также от конца числа обрезаются "висящие" нули
        public void NormalizeStringValue()
        {
            if (CountOfDigits > MaxCountOfDigits)
            {
                if (CountOfIntegerDigits <= MaxCountOfDigits)
                {
                    int newCountOfFractionalDigits = CountOfFractionalDigits - (CountOfDigits - MaxCountOfDigits);
                    StringValue = Value.ToString($"F{newCountOfFractionalDigits}");
                }
                else
                {
                    Value = 0;
                    throw new OverflowException("Слишком большое число");
                }
            }
            if (!StringValue.Equals("0"))
            {
            StringValue = StringValue.TrimEnd('0').TrimEnd(',');
            }
        }

        public void AddDigit(byte digit)
        {
            if (digit <= 9 && CountOfDigits < MaxCountOfDigits)
            {
                if (IsZero && !IsFractional)
                {
                    StringValue = digit.ToString();
                }
                else
                {
                    StringValue += digit.ToString();
                }
                decimalValue = Convert.ToDecimal(StringValue);
            }
        }

        public void AddDigit(string stringDigit)
        {
            try
            {
                byte digit = Convert.ToByte(stringDigit);
                AddDigit(digit);
            }
            catch (Exception) { }
        }

        public void AddSeparator()
        {
            if (!IsFractional)
            {
                StringValue += separatorChar;
            }
        }

        public void Backspace()
        {
            if (CountOfDigits > 1 || IsFractional)
            {
                StringValue = StringValue.Remove(StringValue.Length - 1);
            }
            else
            {
                Reset();
            }
            decimalValue = Convert.ToDecimal(StringValue);
        }

        public void InverseSign()
        {
            if (IsPositive)
            {
                StringValue = "-" + StringValue;
            }
            else if (IsNegative)
            {
                StringValue = StringValue.Remove(0, 1);
            }
            decimalValue = -decimalValue;
        }

        public override string ToString() => StringValue;
    }
}