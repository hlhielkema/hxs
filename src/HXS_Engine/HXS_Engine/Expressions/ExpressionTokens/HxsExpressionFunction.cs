//  _    _ _      _ _          _    _ _      _ _                        
// | |  | (_)    | | |        | |  | (_)    | | |                       
// | |__| |_  ___| | | _____  | |__| |_  ___| | | _____ _ __ ___   __ _ 
// |  __  | |/ _ \ | |/ / _ \ |  __  | |/ _ \ | |/ / _ \ '_ ` _ \ / _` |
// | |  | | |  __/ |   <  __/ | |  | | |  __/ |   <  __/ | | | | | (_| |
// |_|  |_|_|\___|_|_|\_\___| |_|  |_|_|\___|_|_|\_\___|_| |_| |_|\__,_|
// _____________________________________________________________________
//
// Filename: HxsExpressionFunction.cs
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
using System.Collections.Generic;
using System.Linq;

namespace HXS_Engine.Expressions.ExpressionTokens
{
    /// <summary>
    /// HXS function
    /// </summary>
    /// <author>Hielke Hielkema</author>
    public class HxsExpressionFunction : HxsExpressionToken, IExternals, IExecutableToken
    {
        // Predefined function list: (a * means double/int/string/bool)
        //
        // -Math:
        //   [NAME]          [RETURN VALUE]           [ARGUMENTS]                  [DESCRIPTION]
        //   abs             double/int               double/int                   Returns the absolute value of a number.
        //   ceil            double/int               double/int                   Returns the smallest integral value that is greater than or equal to the specified decimal number.
        //   floor           double/int               double/int                   Returns the largest integer less than or equal to the specified decimal number.
        //   round           double/int               double/int                   Rounds a decimal value to the nearest integral value.
        //   truncate        double/int               double/int                   Calculates the integral part of a specified double-precision floating-point number. 
        //   sign            double/int               double/int                   Returns a value indicating the sign of a double-precision floating-point number.

        //   exp             double/int               double                       Returns e raised to the specified power.
        //   pow             double/int               double/int, double/int       Returns a specified number raised to the specified power.
        //   log             double/int               double                       Returns the natural (base e) logarithm of a specified number.
        //   log10           double/int               double                       Returns the base 10 logarithm of a specified number.
        //   sqrt            double/int               double                       Returns the square root of a specified number.

        //   max             double/int, double/int   double/int                   Returns the larger of two decimal numbers.
        //   min             double/int, double/int   double/int                   Returns the larger of two decimal numbers.

        //   cos             double/int               double                       Returns the cosine of the specified angle.
        //   cosh            double/int               double                       Returns the hyperbolic cosine of the specified angle.
        //   acos            double/int               double                       Returns the angle whose cosine is the specified number.
        //   sin             double/int               double                       Returns the sine of the specified angle.
        //   sinh            double/int               double                       Returns the hyperbolic sine of the specified angle.
        //   asin            double/int               double                       Returns the angle whose sine is the specified number.
        //   tan             double/int               double                       Returns the tangent of the specified angle.
        //   tanh            double/int               double                       Returns the hyperbolic tangent of the specified angle.
        //   atan            double/int               double                       Returns the angle whose tangent is the specified number.
        //   atan2           double/int,double/int    double                       Returns the angle whose tangent is the quotient of two specified numbers.
        
        //   isnull          bool                     *                            Returns if a value equals null (same as '== null')
        //   isbool          bool                     *                            Returns if a value is a boolean
        //   isint           bool                     *                            Returns if a value is an integer
        //   isdouble        bool                     *                            Returns if a value is a double
        //   isstring        bool                     *                            Returns if a value is a string
        
        //   canparseint     bool                     *                            Returns if a value can be parsed into an integer
        //   canparsedouble  bool                     *                            Returns if a value can be parsed into a double
        //   canparsebool    bool                     *                            Returns if a value can be parsed into a boolean
        
        //   tobool          bool                     double/int/string            Converts a double, integer or string to a boolean
        //   toint           int                      int/double                   Converts an integer or double to an integer
        //   todouble        double                   int/double                   Converts an integer or double to a double
        //   tostring        string                   *                            Convert a value to an object

        // Contants
        private static string[] PREDEFINDED_FUNTIONS = new string[] { 
                                                                        "abs", "ceil", "floor", "round", "truncate", "sign",
                                                                        "exp", "pow", "log", "log10", "sqrt",
                                                                        "max", "min",
                                                                        "cos", "cosh", "acos", "sin", "sinh", "asin", "tan", "tanh", "atan", "atan2",
                                                                        "isnull", "isbool", "isint", "isdouble", "isstring",
                                                                        "canparseint", "canparsedouble", "canparsebool",
                                                                        "tobool", "toint", "todouble", "tostring"
                                                                    }; 

        /// <summary>
        /// Function name
        /// </summary>
        /// <author>Hielke Hielkema</author>
        public string Name { get; private set; }

        /// <summary>
        /// Gets if the function is a build in function
        /// </summary>
        /// <author>Hielke Hielkema</author>
        public bool BuildInFunction { get; private set; }

        /// <summary>
        /// Function parameters
        /// </summary>
        /// <author>Hielke Hielkema</author>
        public HxsExpression[] Parameters { get; private set; }

        // Private fields
        private bool _inverted = false;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <author>Hielke Hielkema</author>
        /// <param name="function">function name</param>
        /// <param name="arguments">arguments</param>
        public HxsExpressionFunction(string function, string arguments, IHxsFunctionExecuter functionProvider)
        {
            // Split the function name
            Name = function.ToLower();

            // Check if the function is build in
            BuildInFunction = IsBuildInFunction(function);

            // Split the arguments
            Parameters = SplitArguments(arguments);
        }

        /// <summary>
        /// Execute the function
        /// </summary>
        /// <param name="variableProvider">variable provider</param>
        /// <returns>function result</returns>
        public object Execute(IHxsVariableProvider variableProvider, IHxsFunctionExecuter functionExecuter)
        {
            object value = null;
            if (BuildInFunction)
                value = ExecuteBuildInFunction(Name, Parameters.Select(x => x.Execute(variableProvider, functionExecuter)).ToArray());
            else if (functionExecuter != null && functionExecuter.FunctionExists(Name))
                value =  functionExecuter.ExecuteFunction(Name, Parameters.Select(x => x.Execute(variableProvider, functionExecuter)).ToArray());
            else
                throw new HxsUndefinedFunctionException(Name);
            return _inverted ? InvertObject(value) : value;
        }

        /// <summary>
        /// Invert an object
        /// 
        /// throws:
        /// - HxsException - object not invertable
        /// </summary>
        /// <author>Hielke Hielkema</author>
        /// <param name="value">object to invert</param>
        /// <returns>inverted object</returns>
        private object InvertObject(object value)
        {
            if (value is int)
                return -(int)value;
            else if (value is double)
                return -(double)value;
            else
                throw new HxsException("Value can not be inverted");
        }

        /// <summary>
        /// Split the function parameters
        /// </summary>
        /// <author>Hielke Hielkema</author>
        /// <param name="str">string to parse</param>
        /// <returns>array of expressions</returns>
        private HxsExpression[] SplitArguments(string str)
        {
            List<string> arguments = new List<string>();
            int level = 0;
            int ofset = 0;
            for (int i = 0; i < str.Length; i++)
            {
                if ((str[i] == ',') && level == 0)
                {
                    arguments.Add(str.Substring(ofset, i - ofset));
                    ofset = i + 1;
                }
                else if (str[i] == '(') level++;
                else if (str[i] == ')') level--;
            }
            if (ofset < str.Length)
                arguments.Add(str.Substring(ofset));

            return arguments.Select(x => new HxsExpression(x.Trim(), false)).ToArray();
        }

        /// <summary>
        /// Get is a function is a build-in function
        /// </summary>
        /// <author>Hielke Hielkema</author>
        /// <param name="name">name of the function</param>
        /// <returns>true=build-in function, not a build-in function</returns>
        private bool IsBuildInFunction(string name)
        {
            return PREDEFINDED_FUNTIONS.Contains(name.ToLower());
        }

        /// <summary>
        /// Execute a build-in function
        /// 
        /// throws:
        /// - HxsException invalid types
        /// - HxsInvalidParameterCountException invalid parameter count
        /// </summary>
        /// <author>Hielke Hielkema</author>
        /// <param name="name">name of the function</param>
        /// <param name="parameters">parameters</param>
        /// <returns>function results</returns>
        private object ExecuteBuildInFunction(string name, object[] parameters)
        {
            // --- MATH FUNCTIONS ---

            //   abs       double/int               double/int                   Returns the absolute value of a number.
            //   ceil      double/int               double/int                   Returns the smallest integral value that is greater than or equal to the specified decimal number.
            //   floor     double/int               double/int                   Returns the largest integer less than or equal to the specified decimal number.
            //   round     double/int               double/int                   Rounds a decimal value to the nearest integral value.
            //   truncate  double/int               double/int                   Calculates the integral part of a specified double-precision floating-point number. 
            //   sign      double/int               double/int                   Returns a value indicating the sign of a double-precision floating-point number.
            #region abs, ceil, floor, round, truncate, sign
            if (name == "abs")
            {
                if (parameters.Length == 1)
                {
                    if (parameters[0] is double)
                        return Math.Abs(Convert.ToDouble(parameters[0]));
                    if (parameters[0] is int)
                        return (int)Math.Abs(Convert.ToInt32(parameters[0]));
                    throw new HxsException("Invalid parameters");
                }
                else
                    throw new HxsInvalidParameterCountException(name, parameters.Length, 1);
            }
            else if (name == "ceil")
            {
                if (parameters.Length == 1)
                {
                    if (parameters[0] is double)
                        return Math.Ceiling(Convert.ToDouble(parameters[0]));
                    if (parameters[0] is int)
                        return parameters[0];
                    throw new HxsException("Invalid parameters");
                }
                else
                    throw new HxsInvalidParameterCountException(name, parameters.Length, 1);
            }
            else if (name == "floor")
            {
                if (parameters.Length == 1)
                {
                    if (parameters[0] is double)
                        return Math.Floor(Convert.ToDouble(parameters[0]));
                    if (parameters[0] is int)
                        return parameters[0];
                    throw new HxsException("Invalid parameters");
                }
                else
                    throw new HxsInvalidParameterCountException(name, parameters.Length, 1);
            }
            else if (name == "round")
            {
                if (parameters.Length == 1)
                {
                    if (parameters[0] is double)
                        return Math.Round(Convert.ToDouble(parameters[0]));
                    if (parameters[0] is int)
                        return parameters[0];
                    throw new HxsException("Invalid parameters");
                }
                else
                    throw new HxsInvalidParameterCountException(name, parameters.Length, 1);
            }
            else if (name == "truncate")
            {
                if (parameters.Length == 1)
                {
                    if (parameters[0] is double)
                        return Math.Truncate(Convert.ToDouble(parameters[0]));
                    if (parameters[0] is int)
                        return parameters[0];
                    throw new HxsException("Invalid parameters");
                }
                else
                    throw new HxsInvalidParameterCountException(name, parameters.Length, 1);
            }
            else if (name == "sign")
            {
                if (parameters.Length == 1)
                {
                    if (parameters[0] is double || parameters[0] is int)
                        return Math.Sign(Convert.ToDouble(parameters[0]));
                    throw new HxsException("Invalid parameters");
                }
                else
                    throw new HxsInvalidParameterCountException(name, parameters.Length, 2);
            }
            #endregion

            //   exp       double/int               double                       Returns e raised to the specified power.
            //   pow       double/int               double/int, double/int       Returns a specified number raised to the specified power.
            //   log       double/int               double                       Returns the natural (base e) logarithm of a specified number.
            //   log10     double/int               double                       Returns the base 10 logarithm of a specified number.
            //   sqrt      double/int               double                       Returns the square root of a specified number.
            #region exp, pow, log, log10, sqrt
            else if (name == "exp")
            {
                if (parameters.Length == 1)
                {
                    if (parameters[0] is double || parameters[0] is int)
                        return Math.Exp(Convert.ToDouble(parameters[0]));
                    throw new HxsException("Invalid parameters");
                }
                else
                    throw new HxsInvalidParameterCountException(name, parameters.Length, 1);
            }
            else if (name == "pow")
            {
                if (parameters.Length == 2)
                {
                    if (parameters[0] is int && parameters[1] is int)
                        return IntPow(Convert.ToInt32(parameters[0]), Convert.ToInt32(parameters[1]));
                    else if ((parameters[0] is double || parameters[0] is int) && (parameters[1] is double || parameters[1] is int))
                        return Math.Pow(Convert.ToDouble(parameters[0]), Convert.ToDouble(parameters[1]));
                    throw new HxsException("Invalid parameters");
                }
                else
                    throw new HxsInvalidParameterCountException(name, parameters.Length, 2);
            }
            else if (name == "log")
            {
                if (parameters.Length == 1)
                {
                    if (parameters[0] is double || parameters[0] is int)
                        return Math.Log(Convert.ToDouble(parameters[0]));
                    throw new HxsException("Invalid parameters");
                }
                else
                    throw new HxsInvalidParameterCountException(name, parameters.Length, 1);
            }
            else if (name == "log10")
            {
                if (parameters.Length == 1)
                {
                    if (parameters[0] is double || parameters[0] is int)
                        return Math.Log10(Convert.ToDouble(parameters[0]));
                    throw new HxsException("Invalid parameters");
                }
                else
                    throw new HxsInvalidParameterCountException(name, parameters.Length, 1);
            }
            else if (name == "sqrt")
            {
                if (parameters.Length == 1)
                {
                    if (parameters[0] is double || parameters[0] is int)
                        return Math.Sqrt(Convert.ToDouble(parameters[0]));
                    throw new HxsException("Invalid parameters");
                }
                else
                    throw new HxsInvalidParameterCountException(name, parameters.Length, 1);
            }
            #endregion


            //   max       double/int, double/int   double/int                   Returns the larger of two decimal numbers.
            //   min       double/int, double/int   double/int                   Returns the larger of two decimal numbers.
            #region max, min
            else if (name == "max")
            {
                if (parameters.Length == 2)
                {
                    if (parameters[0] is int && parameters[1] is int)
                        return Math.Max(Convert.ToInt32(parameters[0]), Convert.ToInt32(parameters[1]));
                    else if ((parameters[0] is double || parameters[0] is int) && (parameters[1] is double || parameters[1] is int))
                        return Math.Max(Convert.ToDouble(parameters[0]), Convert.ToDouble(parameters[1]));
                    throw new HxsException("Invalid parameters");
                }
                else
                    throw new HxsInvalidParameterCountException(name, parameters.Length, 2);
            }
            else if (name == "min")
            {
                if (parameters.Length == 2)
                {
                    if (parameters[0] is int && parameters[1] is int)
                        return Math.Min(Convert.ToInt32(parameters[0]), Convert.ToInt32(parameters[1]));
                    else if ((parameters[0] is double || parameters[0] is int) && (parameters[1] is double || parameters[1] is int))
                        return Math.Min(Convert.ToDouble(parameters[0]), Convert.ToDouble(parameters[1]));
                    throw new HxsException("Invalid parameters");
                }
                else
                    throw new HxsInvalidParameterCountException(name, parameters.Length, 2);
            }
            #endregion

            //   cos       double/int               double                       Returns the cosine of the specified angle.
            //   cosh      double/int               double                       Returns the hyperbolic cosine of the specified angle.
            //   acos      double/int               double                       Returns the angle whose cosine is the specified number.
            //   sin       double/int               double                       Returns the sine of the specified angle.
            //   sinh      double/int               double                       Returns the hyperbolic sine of the specified angle.
            //   asin      double/int               double                       Returns the angle whose sine is the specified number.
            //   tan       double/int               double                       Returns the tangent of the specified angle.
            //   tanh      double/int               double                       Returns the hyperbolic tangent of the specified angle.
            //   atan      double/int               double                       Returns the angle whose tangent is the specified number.
            //   atan2     double/int,double/int    double                       Returns the angle whose tangent is the quotient of two specified numbers.
            #region cos, cosh, acos, sin, sinh, asin, tan, tanh, atan, atan2
            else if (name == "cos")
            {
                if (parameters.Length == 1)
                {
                    if (parameters[0] is double || parameters[0] is int)
                        return Math.Cos(Convert.ToDouble(parameters[0]));
                    throw new HxsException("Invalid parameters");
                }
                else
                    throw new HxsInvalidParameterCountException(name, parameters.Length, 1);
            }
            else if (name == "cosh")
            {
                if (parameters.Length == 1)
                {
                    if (parameters[0] is double || parameters[0] is int)
                        return Math.Cosh(Convert.ToDouble(parameters[0]));
                    throw new HxsException("Invalid parameters");
                }
                else
                    throw new HxsInvalidParameterCountException(name, parameters.Length, 1);
            }
            else if (name == "acos")
            {
                if (parameters.Length == 1)
                {
                    if (parameters[0] is double || parameters[0] is int)
                        return Math.Acos(Convert.ToDouble(parameters[0]));
                    throw new HxsException("Invalid parameters");
                }
                else
                    throw new HxsInvalidParameterCountException(name, parameters.Length, 1);
            }
            else if (name == "sin")
            {
                if (parameters.Length == 1)
                {
                    if (parameters[0] is double || parameters[0] is int)
                        return Math.Sin(Convert.ToDouble(parameters[0]));
                    throw new HxsException("Invalid parameters");
                }
                else
                    throw new HxsInvalidParameterCountException(name, parameters.Length, 1);
            }
            else if (name == "sinh")
            {
                if (parameters.Length == 1)
                {
                    if (parameters[0] is double || parameters[0] is int)
                        return Math.Sinh(Convert.ToDouble(parameters[0]));
                    throw new HxsException("Invalid parameters");
                }
                else
                    throw new HxsInvalidParameterCountException(name, parameters.Length, 1);
            }
            else if (name == "asin")
            {
                if (parameters.Length == 1)
                {
                    if (parameters[0] is double || parameters[0] is int)
                        return Math.Asin(Convert.ToDouble(parameters[0]));
                    throw new HxsException("Invalid parameters");
                }
                else
                    throw new HxsInvalidParameterCountException(name, parameters.Length, 1);
            }
            else if (name == "tan")
            {
                if (parameters.Length == 1)
                {
                    if (parameters[0] is double || parameters[0] is int)
                        return Math.Tan(Convert.ToDouble(parameters[0]));
                    throw new HxsException("Invalid parameters");
                }
                else
                    throw new HxsInvalidParameterCountException(name, parameters.Length, 1);
            }
            else if (name == "tanh")
            {
                if (parameters.Length == 1)
                {
                    if (parameters[0] is double || parameters[0] is int)
                        return Math.Tanh(Convert.ToDouble(parameters[0]));
                    throw new HxsException("Invalid parameters");
                }
                else
                    throw new HxsInvalidParameterCountException(name, parameters.Length, 1);
            }
            else if (name == "atan")
            {
                if (parameters.Length == 1)
                {
                    if (parameters[0] is double || parameters[0] is int)
                        return Math.Atan(Convert.ToDouble(parameters[0]));
                    throw new HxsException("Invalid parameters");
                }
                else
                    throw new HxsInvalidParameterCountException(name, parameters.Length, 1);
            }
            else if (name == "atan2")
            {
                if (parameters.Length == 2)
                {
                    if ((parameters[0] is double || parameters[0] is int) && (parameters[1] is double || parameters[1] is int))
                        return Math.Atan2(Convert.ToDouble(parameters[0]), Convert.ToDouble(parameters[1]));
                    throw new HxsException("Invalid parameters");
                }
                else
                    throw new HxsInvalidParameterCountException(name, parameters.Length, 1);
            }
            #endregion

            // --- END MATH FUNCTIONS ---

            // --- TYPE FUNCTION ---

            //   isnull
            //   isbool
            //   isint
            //   isdouble
            //   isstring
            #region isnull, isbool, isint, isdouble, isstring
            else if (name == "isnull")
            {
                if (parameters.Length == 1)
                    return parameters[0] == null;
                else
                    throw new HxsInvalidParameterCountException(name, parameters.Length, 1);
            }
            else if (name == "isbool")
            {
                if (parameters.Length == 1)
                    return parameters[0] != null && parameters[0] is bool;
                else
                    throw new HxsInvalidParameterCountException(name, parameters.Length, 1);
            }
            else if (name == "isint")
            {
                if (parameters.Length == 1)
                    return parameters[0] != null && parameters[0] is int;
                else
                    throw new HxsInvalidParameterCountException(name, parameters.Length, 1);
            }
            else if (name == "isdouble")
            {
                if (parameters.Length == 1)
                    return parameters[0] != null && parameters[0] is double;
                else
                    throw new HxsInvalidParameterCountException(name, parameters.Length, 1);
            }
            else if (name == "isstring")
            {
                if (parameters.Length == 1)
                    return parameters[0] != null && parameters[0] is string;
                else
                    throw new HxsInvalidParameterCountException(name, parameters.Length, 1);
            }
            #endregion

            //   canParseInt
            //   canParseDouble
            //   canParseBool
            #region canparseint, canparsedouble, canparsebool
            else if (name == "canparseint")
            {
                if (parameters.Length == 1)
                    return parameters[0] != null && (parameters[0] is int || parameters[0] is double);
                else
                    throw new HxsInvalidParameterCountException(name, parameters.Length, 1);
            }
            else if (name == "canparsedouble")
            {
                if (parameters.Length == 1)
                    return parameters[0] != null && (parameters[0] is int || parameters[0] is double);
                else
                    throw new HxsInvalidParameterCountException(name, parameters.Length, 1);
            }
            else if (name == "canparsebool")
            {
                if (parameters.Length == 1)
                {
                    if (parameters[0] == null)
                        return false;
                    else if (parameters[0] is bool)
                        return true;
                    else if (parameters[0] is double || parameters[0] is int || parameters[0] is string)
                    {
                        try
                        {
                            Convert.ToBoolean(parameters[0]);
                            return true;
                        }
                        catch { }
                    }
                    return false;
                }
                else
                    throw new HxsInvalidParameterCountException(name, parameters.Length, 1);
            }
            #endregion

            //   tobool
            //   toint
            //   todouble
            //   tostring
            #region tobool, toint, todouble, tostring
            else if (name == "tobool")
            {
                if (parameters.Length == 1)
                {
                    if (parameters[0] is bool)
                        return parameters[0];
                    else if (parameters[0] is double || parameters[0] is int || parameters[0] is string)
                        return Convert.ToBoolean(parameters[0]);
                    throw new HxsException("Invalid parameters");
                }
                else
                    throw new HxsInvalidParameterCountException(name, parameters.Length, 1);
            }
            else if (name == "toint")
            {
                if (parameters.Length == 1)
                {
                    if (parameters[0] is double || parameters[0] is int)
                        return Convert.ToInt32(parameters[0]);
                    throw new HxsException("Invalid parameters");
                }
                else
                    throw new HxsInvalidParameterCountException(name, parameters.Length, 1);
            }
            else if (name == "todouble")
            {
                if (parameters.Length == 1)
                {
                    if (parameters[0] is double || parameters[0] is int)
                        return Convert.ToDouble(parameters[0]);
                    throw new HxsException("Invalid parameters");
                }
                else
                    throw new HxsInvalidParameterCountException(name, parameters.Length, 1);
            }
            else if (name == "tostring")
            {
                if (parameters.Length == 1)
                {
                    if (parameters[0] is string)
                        return parameters[0];
                    else if (parameters[0] is double || parameters[0] is int || parameters[0] is bool)
                        return Convert.ToString(parameters[0]);
                    throw new HxsException("Invalid parameters");
                }
                else
                    throw new HxsInvalidParameterCountException(name, parameters.Length, 1);
            }
            #endregion

            // --- END TYPE FUNCTION ---

            // Function not found, return
            throw new HxsUndefinedFunctionException(name);
        }

        /// <summary>
        /// Get the function name and its arguments
        /// </summary>
        /// <author>Hielke Hielkema</author>
        /// <returns>function name and its arguments</returns>
        public override string ToExpString()
        {
            string parameters = "";
            foreach (HxsExpression par in Parameters)
                parameters += parameters == "" ? par.ToExpString() : ", " + par.ToExpString();
            return (_inverted ? "-" : "") + string.Format("{0}({1})", Name, parameters);
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
        /// Optimize the parameters
        /// </summary>
        /// <author>Hielke Hielkema</author>
        /// <param name="functionExecuter">the functiuon executer</param>
        public void Optimize(IHxsFunctionExecuter functionExecuter)
        {
            for (int i = 0; i < Parameters.Length; i++)
                Parameters[i].Optimize(functionExecuter);
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

        /// <summary>
        /// Rename a HXS object.
        /// For example component_1hb3.x to column1.x
        /// </summary>
        /// <author>Hielke Hielkema</author>
        /// <param name="oldName">the old object name</param>
        /// <param name="newName">the new object name</param>
        public override void RenameObject(string oldName, string newName)
        {
            if (Name.Split('.')[0].ToLower() == oldName)
                Name = newName + Name.Substring(oldName.Length);
        }

        #region IExternals

        /// <summary>
        /// Get the number of external variables and functions
        /// </summary>
        /// <author>Hielke Hielkema</author>
        /// <returns>number of external variables and functions</returns>
        int IExternals.GetExternalsCount()
        {
            return Parameters.Where(x => x is IExternals).Select(x => (x as IExternals).GetExternalsCount()).Aggregate(0, (a, b) => a + b) + (BuildInFunction ? 0 : 1);
        }

        /// <summary>
        /// Get the number of external variables
        /// </summary>
        /// <author>Hielke Hielkema</author>
        /// <returns>number of external variables</returns>
        int IExternals.GetExternalVariableCount()
        {
            return Parameters.Where(x => x is IExternals).Select(x => (x as IExternals).GetExternalVariableCount()).Aggregate(0, (a, b) => a + b);
        }

        /// <summary>
        /// Get the number of external functions
        /// </summary>
        /// <author>Hielke Hielkema</author>
        /// <returns>number of external functions</returns>
        int IExternals.GetExternalFunctionCount()
        {
            return Parameters.Where(x => x is IExternals).Select(x => (x as IExternals).GetExternalFunctionCount()).Aggregate(0, (a, b) => a + b) + (BuildInFunction ? 0 : 1);
        }

        /// <summary>
        /// Get the external variables
        /// </summary>
        /// <author>Hielke Hielkema</author>
        /// <returns>the external variables</returns>
        List<string> IExternals.GetExternalVariables()
        {
            return Parameters.Where(x => x is IExternals).Select(x => (x as IExternals).GetExternalVariables()).Aggregate(new List<string>(), (a, b) => a.Union(b).ToList());
        }

        /// <summary>
        /// Get the external functions
        /// </summary>
        /// <author>Hielke Hielkema</author>
        /// <returns>the external functions</returns>
        List<string> IExternals.GetExternalFunctions()
        {
            List<string> externals = Parameters.Where(x => x is IExternals).Select(x => (x as IExternals).GetExternalFunctions()).Aggregate(new List<string>(), (a, b) => a.Union(b).ToList());
            if (!BuildInFunction)
                externals.Add(Name);
            return externals;
        }

        #endregion
    }
}
