//  _    _ _      _ _          _    _ _      _ _                        
// | |  | (_)    | | |        | |  | (_)    | | |                       
// | |__| |_  ___| | | _____  | |__| |_  ___| | | _____ _ __ ___   __ _ 
// |  __  | |/ _ \ | |/ / _ \ |  __  | |/ _ \ | |/ / _ \ '_ ` _ \ / _` |
// | |  | | |  __/ |   <  __/ | |  | | |  __/ |   <  __/ | | | | | (_| |
// |_|  |_|_|\___|_|_|\_\___| |_|  |_|_|\___|_|_|\_\___|_| |_| |_|\__,_|
// _____________________________________________________________________
//
// Filename: HxsExpressionVariable.cs
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
    /// HXS Variable
    /// </summary>
    /// <author>Hielke Hielkema</author>
    public class HxsExpressionVariable : HxsExpressionToken, IExternals, IExecutableToken
    {
        // Predefined variables:
        // - NULL
        // - PI
        // - E
        // - HXS
        // - HXS.VERSION
        // - HXS.AUTHOR

        // Predefined variable constant
        private static string[] PREDEF_VARIABLES = new string[] { "NULL", "PI", "E", "HXS", "HXS.VERSION", "HXS.AUTHOR" };
        private const string HXS_VERSION_STRING = "HXS Engine x1.01 by Hielke Hielkema";
        private const string HXS_AUTHOR = "Hielke Hielkema";
        private const int HXS_VERSION_NUM = 1;

        /// <summary>
        /// Variable name
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets if the value is build in
        /// </summary>
        public bool BuildInVariable { get; private set; }

        // Private fields
        private bool _inverted = false;

        /// <summary>
        /// Get the value of the variable
        /// </summary>
        /// <author>Hielke Hielkema</author>
        /// <param name="variableProvider">variable value provider</param>
        /// <returns>value of the variable</returns>
        public object GetValue(IHxsVariableProvider variableProvider)
        {
            object value = null;
            if (BuildInVariable)
            {
                switch (Name.ToUpper())
                {
                    case "NULL": value = null; break;
                    case "PI": value = Math.PI; break;
                    case "E": value = Math.E; break;
                    case "HXS": value = HXS_VERSION_STRING; break;
                    case "HXS.VERSION": value = HXS_VERSION_NUM; break;
                    case "HXS.AUTHOR": value = HXS_AUTHOR; break;
                }
            }
            else if (variableProvider != null)
                value = variableProvider.GetValue(Name);
            else
                throw new HxsUndefinedVariableException(Name);

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
        /// Constructor
        /// </summary>
        /// <author>Hielke Hielkema</author>
        /// <param name="name">variable name</param>
        public HxsExpressionVariable(string name)
        {
            BuildInVariable = PREDEF_VARIABLES.Contains(name.ToUpper());
            Name = name;
        }

        /// <summary>
        /// Get the variable name
        /// </summary>
        /// <author>Hielke Hielkema</author>
        /// <returns>the variable name</returns>
        public override string ToExpString()
        {
            return (_inverted ? "-" : "") + Name;
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
            return !BuildInVariable || (Name.ToUpper() == "PI" || Name.ToUpper() == "E" || Name.ToUpper() == "HXS.VERSION");
        }

        /// <summary>
        /// Gets if the variable can be converted to a HXS constant
        /// </summary>
        /// <author>Hielke Hielkema</author>
        /// <returns>if the variable can be converted to a HXS constant</returns>
        public bool CanBeConvertedToAHxsConstant()
        {
            return BuildInVariable && (Name.ToUpper() == "PI" || Name.ToUpper() == "E" || Name.ToUpper() == "HXS.VERSION");
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
            return BuildInVariable ? 0 : 1;
        }

        /// <summary>
        /// Get the number of external variables
        /// </summary>
        /// <author>Hielke Hielkema</author>
        /// <returns>number of external variables</returns>
        int IExternals.GetExternalVariableCount()
        {
            return BuildInVariable ? 0 : 1;
        }

        /// <summary>
        /// Get the number of external functions
        /// </summary>
        /// <author>Hielke Hielkema</author>
        /// <returns>number of external functions</returns>
        int IExternals.GetExternalFunctionCount()
        {
            return 0;
        }

        /// <summary>
        /// Get the external variables
        /// </summary>
        /// <author>Hielke Hielkema</author>
        /// <returns>the external variables</returns>
        List<string> IExternals.GetExternalVariables()
        {
            List<string> variables = new List<string>();
            if (!BuildInVariable)
                variables.Add(Name);
            return variables;
        }

        /// <summary>
        /// Get the external functions
        /// </summary>
        /// <author>Hielke Hielkema</author>
        /// <returns>the external functions</returns>
        List<string> IExternals.GetExternalFunctions()
        {
            return new List<string>();
        }

        #endregion

        /// <summary>
        /// Get the result from the HXS token
        /// </summary>
        /// <author>Hielke Hielkema</author>
        /// <param name="variableProvider">variable provider</param>
        /// <param name="functionExecuter">function executer</param>
        /// <returns>execute result</returns>
        object IExecutableToken.Execute(IHxsVariableProvider variableProvider, IHxsFunctionExecuter functionExecuter)
        {
            return GetValue(variableProvider);
        }
    }
}
