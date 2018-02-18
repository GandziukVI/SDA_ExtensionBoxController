using Keithley26xx;
using SourceMeterUnit;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MCBJ.Experiments
{
    [ValueConversion(typeof(int), typeof(SMUSourceMode))]
    public class SourceModeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var mode = (SMUSourceMode)value;

            switch (mode)
            {
                case SMUSourceMode.Voltage:
                    return 0;
                case SMUSourceMode.Current:
                    return 1;
                case SMUSourceMode.ModeNotSet:
                    return 2;
                default:
                    throw new ArgumentException("Not able to convert units.");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var index = (int)value;

            switch (index)
            {
                case 0:
                    return SMUSourceMode.Voltage;
                case 1:
                    return SMUSourceMode.Current;
                case 2:
                    return SMUSourceMode.ModeNotSet;
                default:
                    throw new ArgumentException("Not able to convert units.");
            }
        }
    }

    [ValueConversion(typeof(int), typeof(string))]
    public class SourceFunctionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var mode = (SMUSourceMode)value;

            switch (mode)
            {
                case SMUSourceMode.Voltage:
                    return "V";
                case SMUSourceMode.Current:
                    return "A";
                case SMUSourceMode.ModeNotSet:
                    return "";
                default:
                    throw new ArgumentException("Not able to convert units.");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var index = (string)value;

            switch (index)
            {
                case "V":
                    return SMUSourceMode.Voltage;
                case "A":
                    return SMUSourceMode.Current;
                case "":
                    return SMUSourceMode.ModeNotSet;
                default:
                    throw new ArgumentException("Not able to convert units.");
            }
        }
    }

    [ValueConversion(typeof(int), typeof(string))]
    public class ComplianceFunctionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var mode = (SMUSourceMode)value;

            switch (mode)
            {
                case SMUSourceMode.Voltage:
                    return "A";
                case SMUSourceMode.Current:
                    return "V";
                case SMUSourceMode.ModeNotSet:
                    return "";
                default:
                    throw new ArgumentException("Not able to convert units.");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var index = (string)value;

            switch (index)
            {
                case "V":
                    return SMUSourceMode.Current;
                case "A":
                    return SMUSourceMode.Voltage;
                case "":
                    return SMUSourceMode.ModeNotSet;
                default:
                    throw new ArgumentException("Not able to convert units.");
            }
        }
    }

    [ValueConversion(typeof(int), typeof(Keithley26xxB_Channels))]
    public class ChannelNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var channel = (Keithley26xxB_Channels)value;

            switch (channel)
            {
                case Keithley26xxB_Channels.Channel_A:
                    return 0;
                case Keithley26xxB_Channels.Channel_B:
                    return 1;
                default:
                    throw new ArgumentException();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var index = (int)value;

            if (index == 0)
                return Keithley26xxB_Channels.Channel_A;
            else if (index == 1)
                return Keithley26xxB_Channels.Channel_B;
            else
                throw new ArgumentException();
        }
    }

    [ValueConversion(typeof(int), typeof(Keithley26xxB_Channels))]
    public class Ke26xxBChannelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var channel = (Keithley26xxB_Channels)value;

            switch (channel)
            {
                case Keithley26xxB_Channels.Channel_A:
                    return 9;
                case Keithley26xxB_Channels.Channel_B:
                    return 1;
                default:
                    throw new ArgumentException("Not able to convert units.");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var index = (int)value;

            switch (index)
            {
                case 0:
                    return Keithley26xxB_Channels.Channel_A;
                case 1:
                    return Keithley26xxB_Channels.Channel_B;
                default:
                    throw new ArgumentException("Not able to convert units.");
            }
        }
    }

    [ValueConversion(typeof(bool), typeof(bool))]
    public class InverseBooleanConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(bool))
                throw new InvalidOperationException("The target must be a boolean");

            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(bool))
                throw new InvalidOperationException("The target must be a boolean");

            return !(bool)value;
        }

        #endregion
    }

    [ValueConversion(typeof(int), typeof(double))]
    public class MultiplierConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var multiplier = 1.0;
            var conversionSuccess = false;
            conversionSuccess = double.TryParse(System.Convert.ToString(value, NumberFormatInfo.InvariantInfo), NumberStyles.Float, NumberFormatInfo.InvariantInfo, out multiplier);

            if (conversionSuccess)
            {
                if (multiplier == 1.0)
                    return 0;
                else if (multiplier == 1e-3)
                    return 1;
                else if (multiplier == 1e-6)
                    return 2;
                else if (multiplier == 1e-9)
                    return 3;
                else
                    return -1;
            }
            else
                return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var index = 0;
            var conversionSuccess = false;
            conversionSuccess = int.TryParse(System.Convert.ToString(value, NumberFormatInfo.InvariantInfo), out index);

            if (conversionSuccess)
            {
                if (index == 0)
                    return 1.0;
                else if (index == 1)
                    return 1e-3;
                else if (index == 2)
                    return 1e-6;
                else if (index == 3)
                    return 1e-9;
                else
                    return double.NaN;
            }
            else
                return 1.0;
        }
    }

    [ValueConversion(typeof(string), typeof(double[]))]
    public class ValueCollectionConverter : IValueConverter
    {
        char[] separators = "[], ".ToCharArray();

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var values = (double[])value;

            var sb = new StringBuilder();
            sb.Append("[ ");

            for (int i = 0; i < values.Length; i++)
                sb.AppendFormat("{0}, ", values[i].ToString("0.000", NumberFormatInfo.InvariantInfo));

            sb.Append("]");

            return sb.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var arrayString = (string)value;

            var query = from item in arrayString.Split(separators, StringSplitOptions.RemoveEmptyEntries)
                        select double.Parse(item, NumberFormatInfo.InvariantInfo);

            return query.ToArray();
        }
    }

    // Does a math equation on the bound value.
    // Use @VALUE in your mathEquation as a substitute for bound value
    // Operator order is parenthesis first, then Left-To-Right (no operator precedence)
    public class MathConverter : IValueConverter
    {
        private static readonly char[] _allOperators = new[] { '+', '-', '*', '/', '%', '(', ')' };

        private static readonly List<string> _grouping = new List<string> { "(", ")" };
        private static readonly List<string> _operators = new List<string> { "+", "-", "*", "/", "%" };

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Parse value into equation and remove spaces
            var mathEquation = parameter as string;
            mathEquation = mathEquation.Replace(" ", "");
            mathEquation = mathEquation.Replace("@VALUE", value.ToString());

            // Validate values and get list of numbers in equation
            var numbers = new List<double>();
            double tmp;

            var eqnSplit = mathEquation.Split(_allOperators);
            for (int i = 0; i < eqnSplit.Length; i++)
            {
                var s = eqnSplit[i];
                if (s != string.Empty)
                {
                    if (double.TryParse(s, out tmp))
                    {
                        numbers.Add(tmp);
                    }
                    else
                    {
                        // Handle Error - Some non-numeric, operator, or grouping character found in string
                        throw new InvalidCastException();
                    }
                }
            }

            // Begin parsing method
            EvaluateMathString(ref mathEquation, ref numbers, 0);

            // After parsing the numbers list should only have one value - the total
            return numbers[0];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion

        // Evaluates a mathematical string and keeps track of the results in a List<double> of numbers
        private void EvaluateMathString(ref string mathEquation, ref List<double> numbers, int index)
        {
            // Loop through each mathemtaical token in the equation
            string token = GetNextToken(mathEquation);

            while (token != string.Empty)
            {
                // Remove token from mathEquation
                mathEquation = mathEquation.Remove(0, token.Length);

                // If token is a grouping character, it affects program flow
                if (_grouping.Contains(token))
                {
                    switch (token)
                    {
                        case "(":
                            EvaluateMathString(ref mathEquation, ref numbers, index);
                            break;

                        case ")":
                            return;
                    }
                }

                // If token is an operator, do requested operation
                if (_operators.Contains(token))
                {
                    // If next token after operator is a parenthesis, call method recursively
                    string nextToken = GetNextToken(mathEquation);
                    if (nextToken == "(")
                    {
                        EvaluateMathString(ref mathEquation, ref numbers, index + 1);
                    }

                    // Verify that enough numbers exist in the List<double> to complete the operation
                    // and that the next token is either the number expected, or it was a ( meaning
                    // that this was called recursively and that the number changed
                    if (numbers.Count > (index + 1) &&
                        (double.Parse(nextToken) == numbers[index + 1] || nextToken == "("))
                    {
                        switch (token)
                        {
                            case "+":
                                numbers[index] = numbers[index] + numbers[index + 1];
                                break;
                            case "-":
                                numbers[index] = numbers[index] - numbers[index + 1];
                                break;
                            case "*":
                                numbers[index] = numbers[index] * numbers[index + 1];
                                break;
                            case "/":
                                numbers[index] = numbers[index] / numbers[index + 1];
                                break;
                            case "%":
                                numbers[index] = numbers[index] % numbers[index + 1];
                                break;
                        }
                        numbers.RemoveAt(index + 1);
                    }
                    else
                    {
                        // Handle Error - Next token is not the expected number
                        throw new FormatException("Next token is not the expected number");
                    }
                }

                token = GetNextToken(mathEquation);
            }
        }

        // Gets the next mathematical token in the equation
        private string GetNextToken(string mathEquation)
        {
            // If we're at the end of the equation, return string.empty
            if (mathEquation == string.Empty)
            {
                return string.Empty;
            }

            // Get next operator or numeric value in equation and return it
            string tmp = "";
            foreach (char c in mathEquation)
            {
                if (_allOperators.Contains(c))
                {
                    return (tmp == "" ? c.ToString() : tmp);
                }
                else
                {
                    tmp += c;
                }
            }

            return tmp;
        }
    }
}
