using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using SimpleCalculator.Models;

namespace SimpleCalculator.ViewModels
{
    internal class MainWindowViewModel : INotifyPropertyChanged
    {
        private string input; // поле вода числа
        private string formula; // поле вывода формулы

        public event PropertyChangedEventHandler PropertyChanged;

        private Calculator calculator;

        public ICommand InputDigitCommand { get; }
        public ICommand AddSeparatorCommand { get; }
        public ICommand InverseSignCommand { get; }
        public ICommand BackspaceCommand { get; }
        public ICommand SetOperatorCommand { get; }
        public ICommand OneOperandCalculateCommand { get; }
        public ICommand OneDivisionCalculateCommand { get; }
        public ICommand SqrtCalculateCommand { get; }
        public ICommand PercentsCommand { get; }
        public ICommand EnterCommand { get; }
        public ICommand ClearEntryCommand { get; }
        public ICommand ClearCommand { get; }

        public MainWindowViewModel()
        {
            Input = "0";
            Formula = "";

            calculator = new Calculator();
            // при возникновении событий изменения вводимого операнда, формулы или результата сработают методы, которые
            // внесут изменения в соответствующие свойства (Input и Formula). Их изменение, в свою очередь, вызовет
            // событие OnPropertyChanged(), меняющее жлементы управления в окне
            calculator.Input.OnValueChanged += SetInput;
            calculator.OnFormulaChanged += SetFormula;
            calculator.OnResultChanged += SetInput;

            InputDigitCommand = new RelayCommand(OnInputDigitCommandExecute);
            AddSeparatorCommand = new RelayCommand(OnAddSeparatorCommandExecute, CanAddSeparatorCommandExecuted);

            InverseSignCommand = new RelayCommand(OnInverseSignCommandExecute, CanInverseSignCommandExecuted);
            BackspaceCommand = new RelayCommand(OnBackspaceCommandExecute, CanBackspaceCommandExecuted);

            SetOperatorCommand = new RelayCommand(OnSetOperatorCommandExecute, CanSetOperatorCommandExecuted);
            OneOperandCalculateCommand = new RelayCommand(OnOneOperandCalculateCommandExecute, CanSetOperatorCommandExecuted);
            OneDivisionCalculateCommand = new RelayCommand(OnOneOperandCalculateCommandExecute, CanOneDivisionCommandExecuted);
            SqrtCalculateCommand = new RelayCommand(OnOneOperandCalculateCommandExecute, CanSqrtCommandExecuted);
            PercentsCommand = new RelayCommand(OnPercentsCommandExecute, CanPercentsCommandExecuted);
            EnterCommand = new RelayCommand(OnEnterCommandExecute, CanEnterCommandExecuted);

            ClearCommand = new RelayCommand(OnClearCommandExecute);
            ClearEntryCommand = new RelayCommand(OnClearEntryCommandExecute);
        }

        void OnPropertyChanged([CallerMemberName] string PropertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }

        private void SetInput(string stringNumber) => Input = stringNumber;

        private void SetFormula(string formula) => Formula = formula;

        public string Input
        {
            get => input;
            set
            {
                input = value;
                OnPropertyChanged();
            }
        }

        public string Formula
        {
            get => formula;
            set
            {
                formula = value;
                OnPropertyChanged();
            }
        }

        // Добавление цифры в поле ввода операнда. Если ввод начинается сразу после завершения предыдущей операции,
        // это вызывает сброс предыдущих значений. В противном случае цифры добавляются к значению операнда.
        private void OnInputDigitCommandExecute(object p)
        {
            calculator.ResetIfIsFinishedCalculation();
            calculator.Input.AddDigit(p.ToString());
        }
        // Добавление дробного разделителя в поле ввода операнда. Если ввод начинается сразу после завершения предыдущей
        //  операции, это вызывает сброс предыдущих значений. В противном случае разделитель добавляется к значению
        //  операнда.
        private void OnAddSeparatorCommandExecute(object p)
        {
            calculator.ResetIfIsFinishedCalculation();
            calculator.Input.AddSeparator();
        }

        // Ввод дробного разделителя блокируется в интерфейсе, если разделитель уже имеется введён.
        private bool CanAddSeparatorCommandExecuted(object p) => !calculator.Input.IsFractional;

        /* Метод обрабатывает нажатие кнопки +/-
         * 
         * (Разделение логики управления вынесено в этот метод из модели, поскольку в другой реализации интерфейса
         * действия могут быть закреплены за разными командами)
         */
        private void OnInverseSignCommandExecute(object p)
        {
            // Если команда поступила во время ввода значения, меняется знак вводимого числа
            if (!calculator.IsFinishedCalculation)
            {
                calculator.Input.InverseSign();
            }
            // Если команда поступила после вычисления результата, инверсируется знак результата
            else
            {
                calculator.CalculateAtOneOperand("+/-");
            }
        }

        // Смена знака блокируется в интерфейсе, если операнд 0
        private bool CanInverseSignCommandExecuted(object p) => (calculator.IsFinishedCalculation && !calculator.Result.IsZero)
        || (!calculator.IsFinishedCalculation && !calculator.Input.IsZero);

        /* Метод обрабатывает команду Backspace
         * 
         * (Разделение логики управления вынесено в этот метод из модели, поскольку в другой реализации интерфейса
         * действия могут быть закреплены за разными командами)
         */
        private void OnBackspaceCommandExecute(object p)
        {
            // Если команда поступила во время ввода значения, редактируется вводимое число
            if (!calculator.IsFinishedCalculation)
            {
                calculator.Input.Backspace();
            }
            // Если команда поступила после вычисления результата, выполняетя сброс калькулятора
            else
            {
                calculator.Reset();
            }
        }

        // Backspace блокируется в интерфейсе, если в операнде только 0, однако разблокируется, если только что
        // было завершено вычисление (в этом случае он может очистить калькулятор)
        private bool CanBackspaceCommandExecuted(object p) =>
            !(calculator.Input.IsZero && !calculator.Input.IsFractional) || calculator.IsFinishedCalculation;

        private void OnSetOperatorCommandExecute(object p) => calculator.SetOperator(p.ToString());

        // Ввод оператора в интерфейсе разблокирован, если в процессе подготовки к вычислению ещё не был введён другой
        // оператор или если вычисление было выполнено и операцию можно произвести над его результатом
        private bool CanSetOperatorCommandExecuted(object p)
        {
            return (!calculator.IsSpecifiedCalcOperator && !calculator.IsFinishedCalculation)
                || (calculator.IsFinishedCalculation);
        }

        private void OnOneOperandCalculateCommandExecute(object p) => calculator.CalculateAtOneOperand(p.ToString());

        // Команда 1/x разблокируется в интерфейсе, если операнд не равен 0
        private bool CanOneDivisionCommandExecuted(object p) => CanSetOperatorCommandExecuted(p)
            && ((calculator.IsFinishedCalculation && !calculator.Result.IsZero)
                || (!calculator.IsFinishedCalculation && !calculator.Input.IsZero));

        // Команда извлечения корня разблокируется в интерфейсе, если операнд не отрицательный
        private bool CanSqrtCommandExecuted(object p) => CanSetOperatorCommandExecuted(p)
            && ((calculator.IsFinishedCalculation && !calculator.Result.IsNegative)
                || (!calculator.IsFinishedCalculation && !calculator.Input.IsNegative));

        private void OnPercentsCommandExecute(object p) => calculator.Percents();

        // Команда на работу с процентами разблокируется только на стадии ввода второго операнда
        private bool CanPercentsCommandExecuted(object p) => calculator.IsSpecifiedCalcOperator && !calculator.IsFinishedCalculation;

        private void OnClearEntryCommandExecute(object p) 
            {
                calculator.Input.Reset();
                calculator.Result.Reset();
            }

        private void OnClearCommandExecute(object p) => calculator.Reset();

        private void OnEnterCommandExecute(object p) => calculator.Calculate();

        // Команда на вычисление разблокируется в интерфейсе, если все данные подготовлены к вычислению
        // При этом она блокируется, если предполагается деление на 0
        private bool CanEnterCommandExecuted(object p) => calculator.IsReadyToCalculate
            && !(calculator.Input.IsZero && calculator.CalcOperatorKey.Equals("÷"));
    }
}

