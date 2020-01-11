//  _    _ _      _ _          _    _ _      _ _                        
// | |  | (_)    | | |        | |  | (_)    | | |                       
// | |__| |_  ___| | | _____  | |__| |_  ___| | | _____ _ __ ___   __ _ 
// |  __  | |/ _ \ | |/ / _ \ |  __  | |/ _ \ | |/ / _ \ '_ ` _ \ / _` |
// | |  | | |  __/ |   <  __/ | |  | | |  __/ |   <  __/ | | | | | (_| |
// |_|  |_|_|\___|_|_|\_\___| |_|  |_|_|\___|_|_|\_\___|_| |_| |_|\__,_|
// _____________________________________________________________________
//
// Filename: HxsExpression.cs
// Project: HXS Engine
// Created: 16-01-2014 (DD-MM-YYYY)
// Changed: 16-07-2014  (DD-MM-YYYY)
//
// (C) Hielke Hielkema - 2014
//
// Author: Hielke Hielkema
// Contact: HielkeHielkema93@gmail.com
//

// todo:
// optimizer improven

// extra features:
// lists

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HXS_Engine.Expressions.ExpressionTokens
{
    /// <summary>
    /// HXS Expression
    /// </summary>
    public sealed class HxsExpression : HxsExpressionToken, IExternals, IExecutableToken
    {
        /// <summary>
        /// Tokens
        /// </summary>
        public List<HxsExpressionToken> Tokens { get; private set; }

        // Private fields
        private bool _inverted = false;

        // private enums
        private enum ParseStates { None, Name, Number, String }

        /// <summary>
        /// Constructor
        /// 
        /// throws:
        /// - HxsException
        /// - HxsParseException
        /// 
        /// </summary>
        /// <author>Hielke Hielkema</author>
        /// <param name="source">
        ///     expression string.
        ///     for example: "img1.x + 58 - math.abs(img1.w * 2)"
        /// </param>
        public HxsExpression(string source, bool optimize, IHxsFunctionExecuter functionExecuter = null)
        {
            // Convert the expression into tokens
            ParseTokens(source, functionExecuter);

            // Check if the expression is not empty
            if (Tokens.Count == 0)
                throw new HxsException("Empty (sub-)Expression.");

            // Apply (-) on numbers, functions and expression
            FixNegativeNumbers();

            if (optimize)
                Optimize(functionExecuter);
        }

        /// <summary>
        /// Constructor for sub expressions
        /// 
        /// throws:
        /// - HxsParseException - unexpected '-' operator
        /// 
        /// </summary>
        /// <author>Hielke Hielkema</author>
        /// <param name="tokens">source tokens</param>
        private HxsExpression(List<HxsExpressionToken> tokens)
        {
            // Set the tokens list
            Tokens = tokens;

            // Apply (-) on numbers, functions and expression
            FixNegativeNumbers();
        }

        /// <summary>
        /// Execute the expression
        /// 
        /// throws
        /// - HxsException
        /// 
        /// </summary>
        /// <author>Hielke Hielkema</author>
        /// <param name="variableProvider">variable provider</param>
        /// <param name="functionExecuter">function executer</param>
        /// <returns>execute result</returns>
        public object Execute(IHxsVariableProvider variableProvider, IHxsFunctionExecuter functionExecuter)
        {
            object result = null;
            List<HxsExpressionToken> tokens = Tokens.ToList(); // Copy list
            if (tokens.Count == 0)
                throw new HxsException("Empty (sub-)Expression.");
            if (tokens.Count == 1)
            {
                if (tokens[0] is IExecutableToken)
                    result = (tokens[0] as IExecutableToken).Execute(variableProvider, functionExecuter);
                else
                    throw new HxsException("(sub-)Expression does not return a result.");
            }
            else
            {
                int maxPriority = 0;
                bool leftToRight = true;
                for (int i = 0; i < tokens.Count; i++)
                {
                    if (tokens[i] is HxsExpressionOperator && (tokens[i] as HxsExpressionOperator).Priority > maxPriority)
                    {
                        maxPriority = (tokens[i] as HxsExpressionOperator).Priority;
                        leftToRight = (tokens[i] as HxsExpressionOperator).LeftToRight;
                    }
                }
                if (maxPriority == int.MaxValue)
                    throw new HxsException("Missing an operator");
                if (leftToRight)
                {
                    for (int i = Tokens.Count - 1; i >= 0; i--)
                    {
                        if (tokens[i] is HxsExpressionOperator && (tokens[i] as HxsExpressionOperator).Priority == maxPriority)
                        {
                            List<HxsExpressionToken> left = tokens.GetRange(0, i);
                            List<HxsExpressionToken> right = tokens.GetRange(i + 1, tokens.Count - i - 1);
                            result = (tokens[i] as HxsExpressionOperator).Execute(new HxsExpression(left), new HxsExpression(right), variableProvider, functionExecuter);
                            break;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < tokens.Count; i++)
                    {
                        if (tokens[i] is HxsExpressionOperator && (tokens[i] as HxsExpressionOperator).Priority == maxPriority)
                        {
                            List<HxsExpressionToken> left = tokens.GetRange(0, i);
                            List<HxsExpressionToken> right = tokens.GetRange(i + 1, tokens.Count - i - 1);
                            result = (tokens[i] as HxsExpressionOperator).Execute(new HxsExpression(left), new HxsExpression(right), variableProvider, functionExecuter);
                            break;
                        }
                    }
                }
            }
            return _inverted ? invertObject(result) : result;
        }

        /// <summary>
        /// Optimize the expression (recursive)
        /// 
        /// Remarks: 
        /// - use Execute(..) if Externals equals false because executing is faster then optimizing before executing.
        /// 
        /// Optimalizations:
        /// - PI -> 3.14...
        /// - (((x))) -> x
        /// - 5*5 -> 25
        /// - (5*5)+x -> 25+x
        /// - abs(abs(-55)+4+x) -> abs(59+x)
        /// 
        /// </summary>
        /// <author>Hielke Hielkema</author>
        public void Optimize(IHxsFunctionExecuter functionExecuter)
        {
            // First use recursion to optimize sub expressions
            for (int i = 0; i < Tokens.Count; i++)
            {
                if (Tokens[i] is HxsExpression)
                {
                    // Run the optimizer on the expression
                    (Tokens[i] as HxsExpression).Optimize(functionExecuter);

                    // Check for 1 item expressions
                    if ((Tokens[i] as HxsExpression).Tokens.Count == 1)
                        Tokens[i] = (Tokens[i] as HxsExpression).Unpack();
                }
                else if (Tokens[i] is HxsExpressionFunction)
                    (Tokens[i] as HxsExpressionFunction).Optimize(functionExecuter);
            }

            // Replace predefined variable by there value if possible
            for (int i = 0; i < Tokens.Count; i++)
            {
                if (Tokens[i] is HxsExpressionVariable && (Tokens[i] as HxsExpressionVariable).CanBeConvertedToAHxsConstant())
                    Tokens[i] = new HxsExpressionConstant((Tokens[i] as HxsExpressionVariable).GetValue(null) + "");
            }

            // Solve parts with no externals
            if ((this as IExternals).GetExternalsCount() == 0)
            {
                HxsExpressionConstant constant = new HxsExpressionConstant(Execute(null, functionExecuter));
                Tokens.Clear();
                Tokens.Add(constant);
            }
            else if (Tokens.Count > 1) // Execute as far as possible
            {
                int maxPriority = 0;
                bool leftToRight = true;
                for (int i = 0; i < Tokens.Count; i++)
                {
                    if (Tokens[i] is HxsExpressionOperator && (Tokens[i] as HxsExpressionOperator).Priority > maxPriority)
                    {
                        maxPriority = (Tokens[i] as HxsExpressionOperator).Priority;
                        leftToRight = (Tokens[i] as HxsExpressionOperator).LeftToRight;
                    }
                }
                if (maxPriority == int.MaxValue)
                    throw new HxsException("Missing an operator");
                if (leftToRight)
                {
                    for (int i = Tokens.Count - 1; i >= 0; i--)
                    {
                        if (Tokens[i] is HxsExpressionOperator && (Tokens[i] as HxsExpressionOperator).Priority == maxPriority)
                        {
                            HxsExpressionOperator op = Tokens[i] as HxsExpressionOperator;
                            HxsExpression left = new HxsExpression(Tokens.GetRange(0, i));
                            HxsExpression right = new HxsExpression(Tokens.GetRange(i + 1, Tokens.Count - i - 1));

                            Parallel.Invoke(() => left.Optimize(functionExecuter),
                                            () => right.Optimize(functionExecuter));

                            Tokens.Clear();
                            Tokens.AddRange(left.Tokens);
                            Tokens.Add(op);
                            Tokens.AddRange(right.Tokens);

                            break;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < Tokens.Count; i++)
                    {
                        if (Tokens[i] is HxsExpressionOperator && (Tokens[i] as HxsExpressionOperator).Priority == maxPriority)
                        {
                            HxsExpressionOperator op = Tokens[i] as HxsExpressionOperator;
                            HxsExpression left = new HxsExpression(Tokens.GetRange(0, i));
                            HxsExpression right = new HxsExpression(Tokens.GetRange(i + 1, Tokens.Count - i - 1));

                            Parallel.Invoke(() => left.Optimize(functionExecuter),
                                            () => right.Optimize(functionExecuter));

                            Tokens.Clear();
                            Tokens.AddRange(left.Tokens);
                            Tokens.Add(op);
                            Tokens.AddRange(right.Tokens);

                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// An expression can contain 1 item, this method gets that item.
        /// The (-) operator is also applied.
        /// </summary>
        /// <author>Hielke Hielkema</author>
        /// <returns>first item in the exception if the item count is 1</returns>
        private HxsExpressionToken Unpack()
        {
            if (Tokens.Count == 1)
            {
                if (_inverted)
                {
                    HxsExpressionToken token = Tokens[0];
                    if (token.IsInvertable())
                    {
                        token.Invert();
                        return token;
                    }
                    else
                        return this;
                }
                else
                    return Tokens[0];
            }
            else
                return this;
        }

        /// <summary>
        /// Parse the expression string
        /// 
        /// throws:
        /// - HxsException - parse error
        /// - HxsUnexpectedCharException - unexpected charachter
        /// 
        /// </summary>
        /// <author>Hielke Hielkema</author>
        /// <param name="source">
        ///     expression string.
        ///     for example: "img1.x + 58 - math.abs(img1.w * 2)"
        /// </param>
        private void ParseTokens(string source, IHxsFunctionExecuter functionExecuter)
        {
            Tokens = new List<HxsExpressionToken>();
            ParseStates state = ParseStates.None;
            string name = "";
            bool escaped = false;
            for (int i = 0; i < source.Length; i++)
            {
                char ch = source[i];
                if (state == ParseStates.None)
                {
                    if (ch == ' ') { /* skip */ }
                    else if (HxsExpressionOperator.MatchOperator(source.Substring(i)) > 0)
                    {
                        int opSize = HxsExpressionOperator.MatchOperator(source.Substring(i));
                        Tokens.Add(new HxsExpressionOperator(source.Substring(i, opSize)));
                        i += opSize - 1;
                    }
                    else if (Char.IsNumber(ch))
                    {
                        name = ch + "";
                        state = ParseStates.Number;
                    }
                    else if (Char.IsLetter(ch))
                    {
                        name = ch + "";
                        state = ParseStates.Name;
                    }
                    else if (ch == '(')
                    {
                        int endBracketIndex = FindEndBracket(source, i);
                        Tokens.Add(new HxsExpression(source.Substring(i + 1, endBracketIndex - i - 1), false));
                        i = endBracketIndex;
                    }
                    else if (ch == '"')
                    {
                        name = ch + "";
                        state = ParseStates.String;
                    }
                    else if (ch == '?')
                    {
                        HxsExpression condition = new HxsExpression(Tokens.ToList());
                        string rightSide = source.Substring(i + 1);
                        int splitterPostion = FindSplitter(rightSide);
                        HxsExpression first = new HxsExpression(rightSide.Substring(0, splitterPostion), false, functionExecuter);
                        HxsExpression second = new HxsExpression(rightSide.Substring(splitterPostion + 1), false, functionExecuter);
                        Tokens.Clear();
                        Tokens.Add(new HxsExpressionConditionalOperator(condition, first, second));
                        break; // break to end, state is already none
                    }
                    else
                        throw new HxsUnexpectedCharException(ch);
                }
                else if (state == ParseStates.Name)
                {
                    if (Char.IsLetter(ch) || Char.IsNumber(ch) || ch == '.' || ch == '_')
                        name += ch; // continue in same state
                    else if (ch == '(')
                    {
                        int endBracketIndex = FindEndBracket(source, i);
                        Tokens.Add(new HxsExpressionFunction(name, source.Substring(i + 1, endBracketIndex - i - 1), functionExecuter));
                        i = endBracketIndex;
                        state = ParseStates.None;
                    }
                    else
                    {
                        i--;
                        if (name.ToLower() == "true" || name.ToLower() == "false")
                            Tokens.Add(new HxsExpressionConstant(name));
                        else
                            Tokens.Add(new HxsExpressionVariable(name));
                        state = ParseStates.None;
                    }
                }
                else if (state == ParseStates.Number)
                {
                    if (Char.IsNumber(ch))
                        name += ch; // continue in same state
                    else if (!name.Contains(".") && ch == '.')
                        name += ch;
                    else
                    {
                        i--;
                        Tokens.Add(new HxsExpressionConstant(name));
                        state = ParseStates.None;
                    }
                }
                else if (state == ParseStates.String)
                {
                    if (escaped)
                    {
                        // ' " \ n r t 
                        if (ch == 'n')
                            ch = '\n';
                        else if (ch == 'r')
                            ch = '\r';
                        else if (ch == 't')
                            ch = '\t';
                        name += ch;
                        escaped = false;
                    }
                    else if (ch == '\\')
                        escaped = true;
                    else if (ch == '"')
                    {
                        name += ch;
                        Tokens.Add(new HxsExpressionConstant(name));
                        state = ParseStates.None;
                    }
                    else
                        name += ch;
                }
            }
            if (state == ParseStates.Name)
            {
                if (name.ToLower() == "true" || name.ToLower() == "false")
                    Tokens.Add(new HxsExpressionConstant(name));
                else
                    Tokens.Add(new HxsExpressionVariable(name));
            }
            else if (state == ParseStates.Number)
                Tokens.Add(new HxsExpressionConstant(name));
            else if (state == ParseStates.String)
                throw new HxsException("String with no ending");
        }

        /// <summary>
        /// A - before a number or expression can may be added to invert the value and not as an operator
        /// This method will check for '-' operator tokens that are actualy added to invert a value.
        /// 
        /// throws:
        /// - HxsParseException - unexpected '-' operator
        /// 
        /// </summary>
        /// <author>Hielke Hielkema</author>
        private void FixNegativeNumbers()
        {
            for (int i = Tokens.Count - 2; i >= 0; i--)
            {
                // if first token
                // or token before is operator
                // and token after is inversable
                if (Tokens[i] is HxsExpressionOperator && (Tokens[i] as HxsExpressionOperator).Operator == "-" && (i == 0 || Tokens[i - 1] is HxsExpressionOperator))
                {
                    if (Tokens[i + 1].IsInvertable())
                    {
                        Tokens[i + 1].Invert();
                        Tokens.RemoveAt(i++);
                    }
                    else
                        throw new HxsParseException("invalid '-'");
                }
            }
        }

        /// <summary>
        /// Get a string representation of the token
        /// </summary>
        /// <author>Hielke Hielkema</author>
        /// <returns>string representation of the token</returns>
        public override string ToExpString()
        {
            string str = "";
            foreach (HxsExpressionToken token in Tokens)
            {
                string tokenStr = token.ToExpString();
                if (token is HxsExpression)
                    tokenStr = "(" + tokenStr + ")";
                str += tokenStr;
            }
            return _inverted ? string.Format("-({0})", str) : str;
        }

        /// <summary>
        /// Help parse function to find the ending bracket
        /// 
        /// throws:
        /// - HxsParseException - No ending bracket found
        /// 
        /// </summary>
        /// <author>Hielke Hielkema</author>
        /// <param name="str">expression string</param>
        /// <param name="start">search start point</param>
        /// <returns>end bracket index</returns>
        private int FindEndBracket(string str, int start)
        {
            bool isstr = false;
            for (int i = start + 1, level = 0, len = str.Length; i < len; i++)
            {
                if (isstr)
                {
                    if (str[i] == '\"')
                        isstr = false;
                }
                else
                {
                    if (str[i] == '(') 
                        level++;
                    else if (str[i] == '\"') 
                        isstr = true;
                    else if (str[i] == ')')
                    {
                        if (level-- == 0)
                            return i;
                        else if (level < 0)
                            throw new HxsParseException("No ending bracket found");
                    }
                }
            }
            throw new HxsParseException("No ending bracket found");
        }

        /// <summary>
        /// Find the splitting : char in a string and return the result
        /// Ignores sub ?: expressions
        /// </summary>
        /// <author>Hielke Hielkema</author>
        /// <param name="str">expression string</param>
        /// <returns>position of splitter, -1 if not found</returns>
        private int FindSplitter(string str)
        {
            bool isstr = false;
            for (int i = 0, level = 0, len = str.Length; i < len; i++)
            {
                if (isstr)
                {
                    if (str[i] == '\"')
                        isstr = false;
                }
                else
                {
                    if (level == 0 && str[i] == ':') return i;
                    else if (str[i] == '\"') isstr = true;
                    else if (str[i] == '(') level++;
                    else if (str[i] == ')') level--;
                }
            }
            return -1;
        }

        /// <summary>
        /// Invert an object
        /// 
        /// throws:
        /// - HxsException - Object not invertable
        /// 
        /// </summary>
        /// <author>Hielke Hielkema</author>
        /// <param name="value">input object</param>
        /// <returns>inverted object</returns>
        private object invertObject(object value)
        {
            if (value is int)
                return -(int)value;
            else if (value is double)
                return -(double)value;
            else
                throw new HxsException("Value can not be inverted");
        }

        /// <summary>
        /// Toggle the invert value
        /// </summary>
        /// <author>Hielke Hielkema</author>
        public override void Invert()
        {
            _inverted = !_inverted;
        }

        /// <summary>
        /// Get if the value of the token is invertable
        /// </summary>
        /// <author>Hielke Hielkema</author>
        /// <returns>if the value of the token is invertable</returns>
        public override bool IsInvertable()
        {
            return true;
        }

        /// <summary>
        /// Rename a HXS object.
        /// For example component_1hb3.x to column1.x
        /// </summary>
        /// <author>Hielke Hielkema</author>
        /// <param name="oldName">the old object name</param>
        /// <param name="newName">the new object name</param>
        public override void RenameObject(string oldName, string newName)
        {
            Tokens.ForEach(t => t.RenameObject(oldName, newName));
        }

        #region IExternals

        /// <summary>
        /// Get the number of external variables and functions
        /// </summary>
        /// <author>Hielke Hielkema</author>
        /// <returns>number of external variables and functions</returns>
        int IExternals.GetExternalsCount()
        {
            return Tokens.Where(x => x is IExternals).Select(x => (x as IExternals).GetExternalsCount()).Aggregate(0, (a, b) => a + b);
        }

        /// <summary>
        /// Get the number of external variables
        /// </summary>
        /// <author>Hielke Hielkema</author>
        /// <returns>number of external variables</returns>
        int IExternals.GetExternalVariableCount()
        {
            return Tokens.Where(x => x is IExternals).Select(x => (x as IExternals).GetExternalVariableCount()).Aggregate(0, (a, b) => a + b);
        }

        /// <summary>
        /// Get the number of external functions
        /// </summary>
        /// <author>Hielke Hielkema</author>
        /// <returns>number of external functions</returns>
        int IExternals.GetExternalFunctionCount()
        {
            return Tokens.Where(x => x is IExternals).Select(x => (x as IExternals).GetExternalFunctionCount()).Aggregate(0, (a, b) => a + b);
        }

        /// <summary>
        /// Get the external variables
        /// </summary>
        /// <author>Hielke Hielkema</author>
        /// <returns>the external variables</returns>
        List<string> IExternals.GetExternalVariables()
        {
            return Tokens.Where(x => x is IExternals).Select(x => (x as IExternals).GetExternalVariables()).Aggregate(new List<string>(), (a, b) => a.Union(b).ToList());
        }

        /// <summary>
        /// Get the external functions
        /// </summary>
        /// <author>Hielke Hielkema</author>
        /// <returns>the external functions</returns>
        List<string> IExternals.GetExternalFunctions()
        {
            return Tokens.Where(x => x is IExternals).Select(x => (x as IExternals).GetExternalFunctions()).Aggregate(new List<string>(), (a, b) => a.Union(b).ToList());
        }

        #endregion
    }



}
