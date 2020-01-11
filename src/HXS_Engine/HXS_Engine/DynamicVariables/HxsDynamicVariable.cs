//  _    _ _      _ _          _    _ _      _ _                        
// | |  | (_)    | | |        | |  | (_)    | | |                       
// | |__| |_  ___| | | _____  | |__| |_  ___| | | _____ _ __ ___   __ _ 
// |  __  | |/ _ \ | |/ / _ \ |  __  | |/ _ \ | |/ / _ \ '_ ` _ \ / _` |
// | |  | | |  __/ |   <  __/ | |  | | |  __/ |   <  __/ | | | | | (_| |
// |_|  |_|_|\___|_|_|\_\___| |_|  |_|_|\___|_|_|\_\___|_| |_| |_|\__,_|
// _____________________________________________________________________
//
// Filename: HxsDynamicVariable.cs
// Project: HXS Engine
// Created: 16-06-2014 (DD-MM-YYYY)
// Changed: 16-07-2014  (DD-MM-YYYY)
//
// (C) Hielke Hielkema - 2014
//
// Author: Hielke Hielkema
// Contact: HielkeHielkema93@gmail.com
//

using HXS_Engine.Expressions.ExpressionTokens;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HXS_Engine.DynamicVariables
{
    /// <summary>
    /// A variable that depends on other values
    /// It is stored as an HXS expression and a current value
    /// </summary>
    /// <author>Hielke Hielkema</author>
    /// <typeparam name="T">
    ///     Variable type, supported types are:
    ///     - System.Int32 
    ///     - System.Double
    ///     - System.Boolean
    ///     - System.String
    /// </typeparam>
    public sealed class HxsDynamicVariable<T> : IInfiniteLoopDetection, IHxsVariable where T : IComparable
    {
        /// <summary>
        /// Gets/Sets the value
        /// Holds the last expression evaluation result
        /// </summary>
        /// <author>Hielke Hielkema</author>
        public T Value
        {
            get
            {
                return _value;
            }
            private set
            {
                if (_value.CompareTo(value) != 0)
                {
                    _value = value;
                    OnExpressionValueChanged();
                }
            }
        }

        /// <summary>
        /// Contains the last exception
        /// </summary>
        internal HxsException LastError { get; private set; }

        /// <summary>
        /// Gets/Sets the expression used to calculate the value of the variable.
        /// A value of null means that the value is constant.
        /// Setting the value will invoke the ExpressionChanged event.
        /// </summary>
        /// <author>Hielke Hielkema</author>
        public HxsExpression Expression
        {
            get
            {
                return _expression;
            }
            private set
            {
                if (_expression != value)
                {
                    _expression = value;
                    OnExpressionChanged();
                }
            }
        }

        /// <summary>
        /// Function executer
        /// </summary>
        /// <author>Hielke Hielkema</author>
        public IHxsFunctionExecuter FunctionExecuter { get; set; }

        /// <summary>
        /// Variable provider
        /// </summary>
        /// <author>Hielke Hielkema</author>
        public IHxsVariableProvider VariableProvider { get; set; }

        /// <summary>
        /// Occurs when the value changes
        /// </summary>
        /// <author>Hielke Hielkema</author>
        public event EventHandler ExpressionValueChanged;

        /// <summary>
        /// Occurs when the expression changes
        /// </summary>
        /// <author>Hielke Hielkema</author>
        public event EventHandler ExpressionChanged;

        // Private fields
        private string _name;
        private T _value;
        private HxsExpression _expression;

        #region Constructor(s)

        /// <summary>
        /// Constructor
        /// </summary>
        /// <author>Hielke Hielkema</author>
        /// <param name="name">name of the variable</param>
        /// <param name="value">begin value of the variable</param>
        /// <param name="expression">variable expression(can be null)</param>
        public HxsDynamicVariable(string name, T value, HxsExpression expression)
        {
            // Argument checking
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("name");

            // Check the 
            string typeName = typeof(T).FullName;
            if (typeName != "System.Int32" && typeName != "System.Double" && typeName != "System.Boolean" && typeName != "System.String")
                throw new ArgumentException("Type not supported:" + typeName);

            _name = name;
            _value = value;
            _expression = expression;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <author>Hielke Hielkema</author>
        /// <param name="name">name of the variable</param>
        /// <param name="value">begin value of the variable</param>
        public HxsDynamicVariable(string name, T value)
            : this(name, value, null)
        { }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputString"></param>
        public void SetValue(string inputString)
        {
            try
            {
                HxsExpression expression = new HxsExpression(inputString, true, null);
                object result = expression.Execute(VariableProvider, FunctionExecuter);
                if (typeof(T).FullName == "System.Int32" && result is double)
                    result = (int)Math.Round((double)result);
                else if (typeof(T).FullName == "System.Double" && result is int)
                    result = (double)(int)result;
                if (result is T)
                {
                    Expression = ((expression as IExternals).GetExternalsCount() > 0) ? expression:null; // dynamic variables with no externals don't need to hold an expression
                    Value = (T)result;
                    LastError = null;
                }
                else
                    throw new HxsException("Types do not match");
            }
            catch (Exception ex)
            {
                LastError = (ex is HxsException) ? (ex as HxsException):new HxsException(ex);
                if (_expression != null)
                {
                    _expression = null;
                    OnExpressionChanged();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void RemoveExpression()
        {
            Expression = null;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Update()
        {
            try
            {
                if (_expression != null)
                {
                    // Execute the expression
                    object result = _expression.Execute(VariableProvider, FunctionExecuter);
                    if (typeof(T).FullName == "System.Int32" && result is double)
                        result = (int)Math.Round((double)result);
                    else if (typeof(T).FullName == "System.Double" && result is int)
                        result = (double)(int)result;
                    if (result is T)
                        Value = (T)result;
                    else
                        throw new HxsException("Types do not match");
                }
            }
            catch
            {
                Expression = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string value = _value.ToString();
            if (_value is string)
            {
                if (value.Length > 100)
                    value = value.Substring(0, 97) + "...";
                value = string.Format("\"{0}\"", value);
            }

            if (_expression == null)
                return string.Format("{0}: {1}", _name, value);
            else
                return string.Format("{0}: {1} [{2}]", _name, value, _expression.ToExpString());
        }

        #region IHxsDynamicVariable

        public string GetName()
        {
            return _name;
        }

        public void SetName(string name)
        {
            _name = name;
        }

        public object GetValue()
        {
            return _value;
        }

        public void SetParentPool(HxsDynamicVariablePool pool)
        {
            if (pool == null)
            {
                FunctionExecuter = null;
                VariableProvider = null;
            }
            else
            {
                FunctionExecuter = pool;
                VariableProvider = pool;
            }
        }

        bool IHxsVariable.HasExternals()
        {
            return _expression != null;
        }

        string IHxsVariable.Serialize()
        {
            return _expression == null ? "#null":_expression.ToExpString();
        }

        bool IHxsVariable.CanBeSerialized()
        {
            return true;
        }

        string IHxsVariable.GetTypeName()
        {
            return typeof(T).FullName;
        }

        string IHxsVariable.GetAccessString()
        {
            return string.Format("<RW, {0}>", typeof(T).FullName.Split('.').Last());
        }

        void IHxsVariable.RenameObject(string oldName, string newName)
        {
            if (_name.Split('.')[0].ToLower() == oldName.ToLower())
                _name = newName + _name.Substring(oldName.Length);

            if (_expression != null)
                _expression.RenameObject(oldName, newName);
        }

        #endregion

        #region IInfiniteLoopDetection

        bool IInfiniteLoopDetection.DetectLoops(HxsDynamicVariablePool pool)
        {
            return (this as IInfiniteLoopDetection).DetectLoops(new List<string>(), pool);
        }

        bool IInfiniteLoopDetection.DetectLoops(List<string> found, HxsDynamicVariablePool pool)
        {
            // Check if the variable was found before
            if (found.Contains(_name.ToLower()))
                return true; // loop detected

            if (_expression != null)
            {
                // Add the variable
                found.Add(_name.ToLower());
                // Check for dependency loops
                List<string> externalVariables = (_expression as IExternals).GetExternalVariables();
                foreach (string externalVariable in externalVariables)
                {
                    // Check the childs (with a copy of the list)
                    IHxsVariable child = pool.Get(externalVariable);
                    if (child != null && child is IInfiniteLoopDetection && (child as IInfiniteLoopDetection).DetectLoops(found.ToList(), pool))
                        return true; // loop detected
                }
            }
            return false;
        }

        bool IHxsVariable.HasExpression()
        {
            return _expression != null;
        }

        #endregion

        #region Event raising

        /// <summary>
        /// Raises the ExpressionValueChanged event
        /// </summary>
        private void OnExpressionValueChanged()
        {
            if (ExpressionValueChanged != null)
                ExpressionValueChanged(this, new EventArgs());
        }

        /// <summary>
        /// Raises the ExpressionChanged event
        /// </summary>
        private void OnExpressionChanged()
        {
            if (ExpressionChanged != null)
                ExpressionChanged(this, new EventArgs());
        }

        #endregion

    }
}
