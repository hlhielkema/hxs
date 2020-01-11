//  _    _ _      _ _          _    _ _      _ _                        
// | |  | (_)    | | |        | |  | (_)    | | |                       
// | |__| |_  ___| | | _____  | |__| |_  ___| | | _____ _ __ ___   __ _ 
// |  __  | |/ _ \ | |/ / _ \ |  __  | |/ _ \ | |/ / _ \ '_ ` _ \ / _` |
// | |  | | |  __/ |   <  __/ | |  | | |  __/ |   <  __/ | | | | | (_| |
// |_|  |_|_|\___|_|_|\_\___| |_|  |_|_|\___|_|_|\_\___|_| |_| |_|\__,_|
// _____________________________________________________________________
//
// Filename: HxsExpressionConstant.cs
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
using System.Globalization;

namespace HXS_Engine.Expressions.ExpressionTokens
{
    /// <summary>
    /// HXS Constant
    /// </summary>
    /// <author>Hielke Hielkema</author>
    public class HxsExpressionConstant : HxsExpressionToken, IExecutableToken
    {
        /// types:
        ///  [TYPE]       [EXAMPLE]
        /// - INT32       1, 5, 4, -2
        /// - DOUBLE64    1.4, -66.3, 32.0
        /// - BOOL        true, false, True, FALSE
        /// - STRING      "HELLO WORLD!"

        /// <summary>
        /// Supported constant types
        /// </summary>
        /// <author>Hielke Hielkema</author>
        public enum ConstantTypes
        {
            Integer,
            Double,
            Bool,
            String
        }

        /// <summary>
        /// Value of the constant
        /// </summary>
        /// <author>Hielke Hielkema</author>
        public object Value { get; private set; }

        /// <summary>
        /// Type of the constant
        /// </summary>
        /// <author>Hielke Hielkema</author>
        public ConstantTypes ConstantType { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <author>Hielke Hielkema</author>
        /// <param name="value">value</param>
        public HxsExpressionConstant(string value)
        {
            try
            {
                if (value[0] == '"')
                {
                    Value = value.Substring(1, value.Length - 2);
                    ConstantType = ConstantTypes.String;
                }
                else
                {
                    value = value.ToLower();
                    if (value == "true")
                    {
                        // bool = true
                        Value = true;
                        ConstantType = ConstantTypes.Bool;
                    }
                    else if (value == "false")
                    {
                        // bool = false
                        Value = false;
                        ConstantType = ConstantTypes.Bool;
                    }
                    else if (value.Contains(".") || (value.Contains(",")))
                    {
                        // double
                        Value = ParseDouble(value.Replace(",", "."));
                        ConstantType = ConstantTypes.Double;
                    }
                    else
                    {
                        // int
                        Value = int.Parse(value);
                        ConstantType = ConstantTypes.Integer;
                    }
                }
            }
            catch
            {
                throw new Exception("Not supported type");
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <author>Hielke Hielkema</author>
        /// <param name="value">value</param>
        public HxsExpressionConstant(object value)
        {
            Value = value;
            if (value is double)
                ConstantType = ConstantTypes.Double;
            else if (value is bool)
                ConstantType = ConstantTypes.Bool;
            else if (value is string)
                ConstantType = ConstantTypes.String;
            else if (value is int)
                ConstantType = ConstantTypes.Integer;
            else
                throw new Exception("Not supported type");
        }

        /// <summary>
        /// Convert the value of the contant to a string
        /// </summary>
        /// <author>Hielke Hielkema</author>
        /// <returns>string representation of the value</returns>
        public override string ToExpString()
        {
            if (Value is string)
                return string.Format("\"{0}\"", Value);
            return Value + "";
        }

        /// <summary>
        /// Invert the value
        /// </summary>
        /// <author>Hielke Hielkema</author>
        public override void Invert()
        {
            if (ConstantType == ConstantTypes.Integer)
                Value = -(int)Value;
            else if (ConstantType == ConstantTypes.Double)
                Value = -(double)Value;
            else
                throw new HxsParseException("Token can not be inversed");
        }

        /// <summary>
        /// Get if the value of the token is invertable
        /// </summary>
        /// <author>Hielke Hielkema</author>
        /// <returns>if the value of the token is invertable</returns>
        public override bool IsInvertable()
        {
            return ConstantType == ConstantTypes.Integer || ConstantType == ConstantTypes.Double;
        }

        /// <summary>
        /// Parse a double without localization error's.
        /// example: 5.5 and 5,5 are both parsed correctly.
        /// </summary>
        /// <author>Hielke Hielkema</author>
        /// <param name="value">input string</param>
        /// <returns>float</returns>
        private double ParseDouble(string value)
        {
            CultureInfo ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();
            ci.NumberFormat.CurrencyDecimalSeparator = ".";
            return double.Parse(value.Replace(',', '.'), NumberStyles.Any, ci);
        }

        /// <summary>
        /// Get the result from the HXS token
        /// </summary>
        /// <author>Hielke Hielkema</author>
        /// <param name="variableProvider">variable provider</param>
        /// <param name="functionExecuter">function executer</param>
        /// <returns>execute result</returns>
        object IExecutableToken.Execute(IHxsVariableProvider variableProvider, IHxsFunctionExecuter functionExecuter)
        {
            return Value;
        }
    }
}
