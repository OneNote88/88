using System;
using System.Windows;
using System.Windows.Controls;
using System.Data;
using System.Text;

namespace ScientificCalculator
{
    public partial class MainWindow : Window
    {
        private bool isNewCalculation = true;

        public MainWindow()
        {
            InitializeComponent();
            ClearAll();
        }

        private void OnButtonClick(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null) return;

            string buttonContent = button.Content.ToString();

            switch (buttonContent)
            {
                case "C":
                    ClearAll();
                    break;
                case "⌫":
                    Backspace();
                    break;
                case "=":
                    CalculateResult();
                    break;
                case "π":
                    AppendToExpression(Math.PI.ToString());
                    break;
                case "e":
                    AppendToExpression(Math.E.ToString());
                    break;
                case "x²":
                    AppendToExpression("²");
                    break;
                case "√":
                    AppendToExpression("√(");
                    break;
                case "n!":
                    AppendToExpression("!");
                    break;
                case "|x|":
                    AppendToExpression("abs(");
                    break;
                case "log":
                    AppendToExpression("log(");
                    break;
                case "ln":
                    AppendToExpression("ln(");
                    break;
                case "10^x":
                    AppendToExpression("10^(");
                    break;
                case "sin":
                    AppendToExpression("sin(");
                    break;
                case "cos":
                    AppendToExpression("cos(");
                    break;
                case "tan":
                    AppendToExpression("tan(");
                    break;
                case "1/x":
                    AppendToExpression("1/(");
                    break;
                case "%":
                    AppendToExpression("%");
                    break;
                default:
                    AppendToExpression(buttonContent);
                    break;
            }
        }

        private void AppendToExpression(string value)
        {
            if (isNewCalculation && value != "(" && value != ")" && value != "⌫" && value != "C")
            {
                ExpressionTextBox.Text = "";
                isNewCalculation = false;
            }

            ExpressionTextBox.Text += value;
            TryEvaluateExpression();
        }

        private void TryEvaluateExpression()
        {
            try
            {
                string expression = ExpressionTextBox.Text;
                if (string.IsNullOrEmpty(expression))
                {
                    ResultTextBox.Text = "0";
                    return;
                }

                // Заменяем специальные символы и функции на понятные для вычисления
                string evalExpression = expression
                    .Replace("²", "^2")
                    .Replace("√", "sqrt")
                    .Replace("abs", "Abs")
                    .Replace("log", "Log10")
                    .Replace("ln", "Log")
                    .Replace("π", "PI")
                    .Replace("e", "E")
                    .Replace("%", "/100");

                // Обработка факториала
                evalExpression = ProcessFactorial(evalExpression);

                // Вычисление выражения
                double result = Evaluate(evalExpression);
                ResultTextBox.Text = result.ToString();
            }
            catch
            {
                ResultTextBox.Text = "Ошибка";
            }
        }

        private string ProcessFactorial(string expression)
        {
            int factIndex = expression.IndexOf('!');
            while (factIndex != -1)
            {
                int numStart = factIndex - 1;
                while (numStart >= 0 && (char.IsDigit(expression[numStart]) || expression[numStart] == '.'))
                {
                    numStart--;
                }
                numStart++;

                string numStr = expression.Substring(numStart, factIndex - numStart);
                if (double.TryParse(numStr, out double number))
                {
                    int integerNumber = (int)number;
                    if (integerNumber == number && integerNumber >= 0)
                    {
                        long factorial = CalculateFactorial(integerNumber);
                        expression = expression.Remove(numStart, factIndex - numStart + 1).Insert(numStart, factorial.ToString());
                    }
                    else
                    {
                        throw new Exception("Факториал определен только для целых неотрицательных чисел");
                    }
                }
                else
                {
                    throw new Exception("Неверный формат числа для факториала");
                }

                factIndex = expression.IndexOf('!');
            }
            return expression;
        }

        private long CalculateFactorial(int n)
        {
            if (n < 0) throw new ArgumentException("Факториал отрицательного числа не определен");
            if (n == 0) return 1;

            long result = 1;
            for (int i = 1; i <= n; i++)
            {
                result *= i;
            }
            return result;
        }

        private double Evaluate(string expression)
        {
            if (string.IsNullOrEmpty(expression)) return 0;

            // Используем DataTable для вычисления выражений
            using (DataTable dt = new DataTable())
            {
                // Добавляем поддержку математических функций
                expression = expression.Replace("sin(", "Math.Sin(")
                                      .Replace("cos(", "Math.Cos(")
                                      .Replace("tan(", "Math.Tan(")
                                      .Replace("sqrt(", "Math.Sqrt(")
                                      .Replace("Abs(", "Math.Abs(")
                                      .Replace("Log10(", "Math.Log10(")
                                      .Replace("Log(", "Math.Log(")
                                      .Replace("PI", Math.PI.ToString())
                                      .Replace("E", Math.E.ToString());

                // Проверяем сбалансированность скобок
                if (!AreParenthesesBalanced(expression))
                {
                    throw new Exception("Несбалансированные скобки");
                }

                var result = dt.Compute(expression, null);
                return Convert.ToDouble(result);
            }
        }

        private bool AreParenthesesBalanced(string expression)
        {
            int balance = 0;
            foreach (char c in expression)
            {
                if (c == '(') balance++;
                if (c == ')') balance--;
                if (balance < 0) return false;
            }
            return balance == 0;
        }

        private void CalculateResult()
        {
            if (string.IsNullOrEmpty(ExpressionTextBox.Text)) return;

            try
            {
                string expression = ExpressionTextBox.Text;
                string result = ResultTextBox.Text;

                if (result == "Ошибка")
                {
                    return;
                }

                ExpressionTextBox.Text = result;
                ResultTextBox.Text = "0";
                isNewCalculation = true;
            }
            catch
            {
                ResultTextBox.Text = "Ошибка";
            }
        }

        private void Backspace()
        {
            if (ExpressionTextBox.Text.Length > 0)
            {
                ExpressionTextBox.Text = ExpressionTextBox.Text.Substring(0, ExpressionTextBox.Text.Length - 1);
                TryEvaluateExpression();
            }

            if (ExpressionTextBox.Text.Length == 0)
            {
                ClearAll();
            }
        }

        private void ClearAll()
        {
            ExpressionTextBox.Text = "";
            ResultTextBox.Text = "0";
            isNewCalculation = true;
        }
    }
}