//  _    _ _      _ _          _    _ _      _ _                        
// | |  | (_)    | | |        | |  | (_)    | | |                       
// | |__| |_  ___| | | _____  | |__| |_  ___| | | _____ _ __ ___   __ _ 
// |  __  | |/ _ \ | |/ / _ \ |  __  | |/ _ \ | |/ / _ \ '_ ` _ \ / _` |
// | |  | | |  __/ |   <  __/ | |  | | |  __/ |   <  __/ | | | | | (_| |
// |_|  |_|_|\___|_|_|\_\___| |_|  |_|_|\___|_|_|\_\___|_| |_| |_|\__,_|
// _____________________________________________________________________
//
// Filename: HxsExpressionOperator.cs
// Project: HXS Engine
// Created: 16-01-2014 (DD-MM-YYYY)
// Changed: 16-07-2014  (DD-MM-YYYY)
//
// (C) Hielke Hielkema - 2014
//
// Author: Hielke Hielkema
// Contact: HielkeHielkema93@gmail.com
//

using System;
using System.Linq;
using System.Threading.Tasks;

namespace HXS_Engine.Expressions.ExpressionTokens
{
    /// <summary>
    /// HXS Operator
    /// </summary>
    /// <author>Hielke Hielkema</author>
    public class HxsExpressionOperator : HxsExpressionToken
    {
        // Operator table:
        // WARNING:  operators with the same priority must have the same value for LeftToRight
        // [OPERATOR]   [PRIORITY]    [ARGS LEFT]     [ARGS RIGHT]   [LEFT TO RIGHT]    [ARG TYPE LEFT]         [ARG TYPE RIGHT]        [IMP STATE]
        //     +            3              1                1        true               int/double/string       int/double/string       Implemented
        //     -            3              1                1        true               int/double              int/double              Implemented
        //     *            2              1                1        true               int/double              int/double              Implemented
        //     /            2              1                1        true               int/double              int/double              Implemented
        //     %            2              1                1        true               int/double              int/double              Implemented
        //     ^            2              1                1        true               int/double              int/double              Implemented
        //
        //     &            6              1                1        true               int                     int                     Implemented
        //     |            7              1                1        true               int                     int                     Implemented
        //     >>           1              1                1        true               int                     int                     Implemented
        //     <<           1              1                1        true               int                     int                     Implemented
        //
        //     !            0              -                1        false              -                       int/double/bool         Implemented
        //     ~            0              -                1        false              -                       int                     Implemented
        //     ??           1              1                1        true               int/double/bool/string  int/double/bool/string  Implemented
        //
        //     &&           8              1                1        true               bool                    bool                    Implemented
        //     ||           9              1                1        true               bool                    bool                    Implemented
        //     XOR          9              1                1        true               bool/int                bool/int                Implemented
        //
        //     >=           4              1                1        true               int/double              int/double              Implemented
        //     <=           4              1                1        true               int/double              int/double              Implemented
        //     >            4              1                1        true               int/double              int/double              Implemented
        //     <            4              1                1        true               int/double              int/double              Implemented
        //     ==           5              1                1        true               int/double/bool/string  int/double/bool/string  Implemented
        //     !=           5              1                1        true               int/double/bool/string  int/double/bool/string  Implemented

        /// <summary>
        /// Gets or sets the operator
        /// </summary>
        /// <author>Hielke Hielkema</author>
        public string Operator { get; private set; }

        /// <summary>
        /// Gets or sets the operator priority
        /// </summary>
        /// <author>Hielke Hielkema</author>
        public int Priority { get; private set; }

        /// <summary>
        /// Accepts a parameter on the left
        /// </summary>
        /// <author>Hielke Hielkema</author>
        public bool ParamLeft { get; private set; }

        /// <summary>
        /// Accepts a parameter on the right
        /// </summary>
        /// <author>Hielke Hielkema</author>
        public bool ParamRight { get; private set; }

        /// <summary>
        /// Gets or set if the Associativity is left to rights
        /// </summary>
        /// <author>Hielke Hielkema</author>
        public bool LeftToRight { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="op">operator</param>
        public HxsExpressionOperator(string op)
        {
            Operator = op.ToLower();
            ParamLeft = Operator != "!" && Operator != "~";
            ParamRight = true;
            LeftToRight = Operator != "!" && Operator != "~";

            // Determine the priority
            Priority = -1;
            string[][] priorityTable = new string[][]
            {
                new string[] { "!", "~" },
                new string[] { ">>", "<<", "??" },
                new string[] { "*", "/", "%", "^" },
                new string[] { "+", "-" },
                new string[] { "<", "<=", ">", ">=" },
                new string[] { "==", "!=" },
                new string[] { "&" },
                new string[] { "|" },
                new string[] { "&&" },
                new string[] { "||", "xor" },
            };
            for (int p = 0; p < priorityTable.Length; p++)
            {
                if (priorityTable[p].Contains(op.ToLower()))
                {
                    Priority = p + 5; // 5 ofset
                    break;
                }
            }
            if (Priority == -1)
                throw new HxsException("Unknown operator");
        }

        /// <summary>
        /// Execute the operator
        /// </summary>
        /// <param name="left">left argument</param>
        /// <param name="right">right argument</param>
        /// <param name="variableProvider">variable provider</param>
        /// <param name="functionExecuter">function executer</param>
        /// <returns>execution result</returns>
        public object Execute(HxsExpression left, HxsExpression right, IHxsVariableProvider variableProvider, IHxsFunctionExecuter functionExecuter)
        {
            object valueLeft = null;
            object valueRight = null;
            if (ParamLeft && ParamRight)
            {
                // Parallel execution of both sides
                Parallel.Invoke(() => valueLeft = left.Execute(variableProvider, functionExecuter),
                                () => valueRight = right.Execute(variableProvider, functionExecuter));
            }
            else if (ParamLeft)
                valueLeft = left.Execute(variableProvider, functionExecuter);
            else if (ParamRight)
                valueRight = right.Execute(variableProvider, functionExecuter);

            #region +, -, *, /, %, ^
            if (Operator == "+")
            {
                if (valueLeft is int && valueRight is int)
                    return (int)valueLeft + (int)valueRight;
                if ((valueLeft is double || valueLeft is int) && (valueRight is double || valueRight is int))
                    return Convert.ToDouble(valueLeft) + Convert.ToDouble(valueRight);
                if (valueLeft is string && valueRight is string)
                    return (string)valueLeft + (string)valueRight;
                if (valueLeft is string && (valueRight is double || valueRight is int | valueRight is bool))
                    return (string)valueLeft + Convert.ToString(valueRight);
                if ((valueLeft is double || valueLeft is int | valueLeft is bool) && valueRight is string)
                    return Convert.ToString(valueLeft) + (string)valueRight;
                throw new HxsException("Invalid parameters");
            }
            if (Operator == "-")
            {
                if (valueLeft is int && valueRight is int)
                    return (int)valueLeft - (int)valueRight;
                if ((valueLeft is double || valueLeft is int) && (valueRight is double || valueRight is int))
                    return Convert.ToDouble(valueLeft) - Convert.ToDouble(valueRight);
                throw new HxsException("Invalid parameters");
            }
            if (Operator == "*")
            {
                if (valueLeft is int && valueRight is int)
                    return (int)valueLeft * (int)valueRight;
                if ((valueLeft is double || valueLeft is int) && (valueRight is double || valueRight is int))
                    return Convert.ToDouble(valueLeft) * Convert.ToDouble(valueRight);
                throw new HxsException("Invalid parameters");
            }
            if (Operator == "/")
            {
                if (valueLeft is int && valueRight is int)
                    return (int)valueLeft / (int)valueRight;
                if ((valueLeft is double || valueLeft is int) && (valueRight is double || valueRight is int))
                    return Convert.ToDouble(valueLeft) / Convert.ToDouble(valueRight);
                throw new HxsException("Invalid parameters");
            }
            if (Operator == "%")
            {
                if (valueLeft is int && valueRight is int)
                    return (int)valueLeft % (int)valueRight;
                if ((valueLeft is double || valueLeft is int) && (valueRight is double || valueRight is int))
                    return Convert.ToDouble(valueLeft) % Convert.ToDouble(valueRight);
                throw new HxsException("Invalid parameters");
            }
            if (Operator == "^")
            {
                if (valueLeft is int && valueRight is int)
                    return IntPow((int)valueLeft, (int)valueRight);
                if ((valueLeft is double || valueLeft is int) && (valueRight is double || valueRight is int))
                    return Math.Pow(Convert.ToDouble(valueLeft), Convert.ToDouble(valueRight));
                throw new HxsException("Invalid parameters");
            }
            #endregion
            #region !, ~, ??

            if (Operator == "!")
            {
                if (valueRight is bool)
                    return !(bool)valueRight;
                if (valueRight is int)
                    return (int)valueRight == 0;
                if (valueRight is double)
                    return (double)valueRight == 0;
                return (valueRight == null);
            }
            if (Operator == "~")
            {
                if (valueRight is int)
                    return ~(int)valueRight;

                throw new HxsException("Invalid parameters");
            }
            if (Operator == "??")
            {
                return valueLeft ?? valueRight;
            }

            #endregion
            #region >, >=, <, <=
            if (Operator == ">")
            {
                if (valueLeft is int && valueRight is int)
                    return (int)valueLeft > (int)valueRight;
                if ((valueLeft is double || valueLeft is int) && (valueRight is double || valueRight is int))
                    return Convert.ToDouble(valueLeft) > Convert.ToDouble(valueRight);
                throw new HxsException("Invalid parameters");
            }
            if (Operator == ">=")
            {
                if (valueLeft is int && valueRight is int)
                    return (int)valueLeft >= (int)valueRight;
                if ((valueLeft is double || valueLeft is int) && (valueRight is double || valueRight is int))
                    return Convert.ToDouble(valueLeft) >= Convert.ToDouble(valueRight);
                throw new HxsException("Invalid parameters");
            }
            if (Operator == "<=")
            {
                if (valueLeft is int && valueRight is int)
                    return (int)valueLeft <= (int)valueRight;
                if ((valueLeft is double || valueLeft is int) && (valueRight is double || valueRight is int))
                    return Convert.ToDouble(valueLeft) <= Convert.ToDouble(valueRight);
                throw new HxsException("Invalid parameters");
            }
            if (Operator == "<")
            {
                if (valueLeft is int && valueRight is int)
                    return (int)valueLeft < (int)valueRight;
                if ((valueLeft is double || valueLeft is int) && (valueRight is double || valueRight is int))
                    return Convert.ToDouble(valueLeft) < Convert.ToDouble(valueRight);
                throw new HxsException("Invalid parameters");
            }
            #endregion
            #region ==, !==
            if (Operator == "==")
            {
                if (valueLeft is int && valueRight is int)
                    return (int)valueLeft == (int)valueRight;
                if (valueLeft is bool && valueRight is bool)
                    return (bool)valueLeft == (bool)valueRight;
                if (valueLeft is string && valueRight is string)
                    return (string)valueLeft == (string)valueRight;
                if ((valueLeft is double || valueLeft is int) && (valueRight is double || valueRight is int))
                    return Convert.ToDouble(valueLeft) == Convert.ToDouble(valueRight);
                throw new HxsException("Invalid parameters");
            }
            if (Operator == "!=")
            {
                if (valueLeft is int && valueRight is int)
                    return (int)valueLeft != (int)valueRight;
                if (valueLeft is bool && valueRight is bool)
                    return (bool)valueLeft != (bool)valueRight;
                if (valueLeft is string && valueRight is string)
                    return (string)valueLeft != (string)valueRight;
                if ((valueLeft is double || valueLeft is int) && (valueRight is double || valueRight is int))
                    return Convert.ToDouble(valueLeft) != Convert.ToDouble(valueRight);
                throw new HxsException("Invalid parameters");
            }
            #endregion
            #region &, |, >>, <<
            if (Operator == "&")
            {
                if (valueLeft is int && valueRight is int)
                    return (int)valueLeft & (int)valueRight;
                throw new HxsException("Invalid parameters");
            }
            if (Operator == "|")
            {
                if (valueLeft is int && valueRight is int)
                    return (int)valueLeft | (int)valueRight;
                throw new HxsException("Invalid parameters");
            }
            if (Operator == ">>")
            {
                if (valueLeft is int && valueRight is int)
                    return (int)valueLeft >> (int)valueRight;
                throw new HxsException("Invalid parameters");
            }
            if (Operator == "<<")
            {
                if (valueLeft is int && valueRight is int)
                    return (int)valueLeft << (int)valueRight;
                throw new HxsException("Invalid parameters");
            }
            #endregion
            #region &&, ||, XOR
            if (Operator == "&&")
            {
                if (valueLeft is bool && valueRight is bool)
                    return (bool)valueLeft && (bool)valueRight;
                throw new HxsException("Invalid parameters");
            }
            if (Operator == "||")
            {
                if (valueLeft is bool && valueRight is bool)
                    return (bool)valueLeft || (bool)valueRight;
                throw new HxsException("Invalid parameters");
            }
            if (Operator == "xor")
            {
                if (valueLeft is bool && valueRight is bool)
                    return (bool)valueLeft ^ (bool)valueRight;
                if (valueLeft is int && valueRight is int)
                    return (int)valueLeft ^ (int)valueRight;
                throw new HxsException("Invalid parameters");
            }
            #endregion

            throw new HxsException("Unknown operator");
        }

        /// <summary>
        /// Check if a string matches a operator and returns the lenght
        /// 0 means no match
        /// </summary>
        /// <param name="str">string to match</param>
        /// <returns>length of the match</returns>
        public static int MatchOperator(string str)
        {
            // Operators:
            // +, -, *, /, !, ^, %
            // &&, ||
            // &, |
            // >=, <=, ==, !=, >, <

            // Operator list (lower case)
            string[] ops = { 
                             "+", "-", "*", "/", "%", "^", "~", "!",
                             "??", "&&", "&", "||", "|" , "xor", 
                             ">=", "<=", "==", "!=", ">>", "<<", ">", "<"
                           };

            // Try to match the operator
            for (int i = 0; i < ops.Length; i++)
            {
                if (str.Length >= ops[i].Length)
                {
                    if (str.Substring(0, ops[i].Length).ToLower() == ops[i])
                        return ops[i].Length;
                }
            }
            return 0;
        }

        /// <summary>
        /// Convert the token back to an expression string
        /// </summary>
        /// <author>Hielke Hielkema</author>
        /// <returns>expression string</returns>
        public override string ToExpString()
        {
            return Operator;
        }

        /// <summary>
        /// A power function for integers
        /// </summary>
        /// <author>Hielke Hielkema</author>
        /// <param name="number">number</param>
        /// <param name="power">power</param>
        /// <returns>number^power</returns>
        private static int IntPow(int number, int power)
        {
            int result = 1;
            for (int i = 0; i < power; i++)
            {
                result *= number;
            }
            return result;
        }
    }
}
