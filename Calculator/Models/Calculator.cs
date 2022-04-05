using System;

namespace SimpleCalculator.Models
{
    // Класс реализует типичную логику калькулятора, оперирующего двумя операндами
    internal class Calculator
    {
        private const int maxCountOfDigits = 16; // максимальное количество цифр в числе, передаваемых в интерфейс
        private string calcOperatorKey; // символ-ключ текущего оператора
        private bool isFinishedCalculation; // признак того, что очередная операция вычисления завершена и не начат
                                            // ввод новых операндов или операторов

        public delegate void FormulaHandler(string formula);
        public event FormulaHandler OnFormulaChanged; // событие на изменение формулы
        public delegate void ResultHandler(string result);
        public event ResultHandler OnResultChanged; // событие на изменение результата вычисления

        public CalcNumber Input { get; set; } // объект-число, над вводом которого в текущий момент работает пользователь
        public CalcNumber Result { get; set; } // объект-число - результат вычислений
        public CalcNumber OperandA { get; set; } // объект-число - уже введённый операнд
        public CalcNumber OperandB  // объект-число - второй операнд для вычислений (возвращает объект-число,
        {                           // которое вводится последним, предварительно нормализуя его строковое представление)
            get
            {
                Input.NormalizeStringValue();
                return Input;
            }
        } 
        public string CalcOperatorKey => calcOperatorKey; // символ-ключ текущего оператора

        public bool IsFinishedCalculation => isFinishedCalculation; // признак того, что очередная операция вычисления 
                                                                    // завершена и не начат новый ввод
        public bool IsSpecifiedCalcOperator => !CalcOperatorKey.Equals(""); // признак, что оператор уже задан
        public bool IsReadyToCalculate => IsSpecifiedCalcOperator && !IsFinishedCalculation; // оператор задан, но вычисление ещё не выполнялось

        // Свойство возвращает символьное представление выполненной или выполняющейся операции
        public string Formula
        {
            get
            {
                string formula = "";
                // Если операция завершена, то метод для формирования формулы берётся из коллекции операции по ключу
                if (IsFinishedCalculation)
                {
                    formula = CalcOpertatons.Items[CalcOperatorKey].Formula(OperandA.ToString(), OperandB.ToString());
                }
                // Еслди операция в проессе выполнеия, что формула формируется из первого операнда и оператора
                else if (IsSpecifiedCalcOperator)
                {
                    formula = OperandA.ToString() + CalcOperatorKey;
                }
                return formula;
            }
        }

        public Calculator()
        {
            OperandA = new CalcNumber(maxCountOfDigits);
            Input = new CalcNumber(maxCountOfDigits);
            Result = new CalcNumber(maxCountOfDigits);
            Reset();
        }

        // Сброс калькулятора (операндов, оператора, результата)
        public void Reset()
        {
            OperandA.Reset();
            calcOperatorKey = "";
            Input.Reset();
            Result.Reset();
            isFinishedCalculation = false;
            OnFormulaChanged?.Invoke("");
        }

        // Метод выполняет вычисление с обработкой возможных исключений
        public void Calculate()
        {
            if (IsReadyToCalculate)
            {
                isFinishedCalculation = true;
                OnFormulaChanged?.Invoke(Formula);
                try
                {
                    // Метод для вычисления берётся из коллекции операции по ключу
                    Result.Value = CalcOpertatons.Items[CalcOperatorKey].Operation(OperandA.Value, OperandB.Value);
                    OnResultChanged?.Invoke(Result.ToString());
                }
                catch (Exception)
                {
                    OnResultChanged?.Invoke("Ошибка");
                }
                
            }
        }

        // Метод инициирует операцию вычисления для случая, когда операция возможна с одним операндом
        public void CalculateAtOneOperand(string calcOperatorKey)
        {
            if (IsFinishedCalculation)
            {
                SetResultAsOperand();
            } else
            {
                OperandA.Value = OperandB.Value;
            }
            this.calcOperatorKey = calcOperatorKey;
            Calculate();
        }

        public void ResetIfIsFinishedCalculation()
        {
            if (isFinishedCalculation)
            {
                Reset();
            }
        }

        // Метод устанавливает оператор для текущей операции
        public void SetOperator(string calcOperatorKey)
        {
            if (!IsFinishedCalculation && !IsSpecifiedCalcOperator)
            {
                OperandA.Value = Input.Value;
                Input.Reset();
                this.calcOperatorKey = calcOperatorKey;
            }
            else if (IsFinishedCalculation)
            {
                SetResultAsOperand();
                this.calcOperatorKey = calcOperatorKey;
            }
            OnFormulaChanged?.Invoke(Formula);
        }

        // Метод позволяет продолжить вычисления, устанавливая предыдущий результат в качестве операнда
        private void SetResultAsOperand()
        {
            OperandA.Value = Result.Value;
            Result.Reset();
            Input.Reset();
            isFinishedCalculation = false;
        }

        // Метод реализует логику вычисления процентов, преобразуя в процентное значение второй операнд
        public void Percents()
        {
            if (!IsFinishedCalculation)
            {
                try
                {
                    // Для операция умножения и деления второй операнд интерпретируется как проценты от 100
                    if (CalcOperatorKey == "×" || CalcOperatorKey == "÷")
                    {
                        Input.Value = Input.Value / 100;
                    }
                    // Для операция сложения и вычитания второй операнд интерпретируется как проценты от первого операнда
                    else if (CalcOperatorKey == "+" || CalcOperatorKey == "–")
                    {
                        Input.Value = OperandA.Value * Input.Value / 100;
                    }
                }
                catch (Exception)
                {
                    OnResultChanged?.Invoke("Ошибка");
                }
            }
        }
    }
}
