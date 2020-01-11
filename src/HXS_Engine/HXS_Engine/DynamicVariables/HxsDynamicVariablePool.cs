//  _    _ _      _ _          _    _ _      _ _                        
// | |  | (_)    | | |        | |  | (_)    | | |                       
// | |__| |_  ___| | | _____  | |__| |_  ___| | | _____ _ __ ___   __ _ 
// |  __  | |/ _ \ | |/ / _ \ |  __  | |/ _ \ | |/ / _ \ '_ ` _ \ / _` |
// | |  | | |  __/ |   <  __/ | |  | | |  __/ |   <  __/ | | | | | (_| |
// |_|  |_|_|\___|_|_|\_\___| |_|  |_|_|\___|_|_|\_\___|_| |_| |_|\__,_|
// _____________________________________________________________________
//
// Filename: HxsDynamicVariablePool.cs
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
using HXS_Engine.Hashing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace HXS_Engine.DynamicVariables
{
    [Serializable]
    public sealed class HxsDynamicVariablePool : IHxsVariableProvider, IHxsFunctionExecuter, ISerializable
    {
        // Private fields
        private List<IHxsVariable> _variables;
        private List<HxsDynamicFunction> _functions;
        private Dictionary<string, object> _constants;
        private bool _debug = false;

        // Private constant
        private const bool DEBUG_MODE_ENABLED = true;
        private const string DEBUG_MODE_KEY_HASH = "$2a$10$IE2BdcSpF/8S4dj0gJlx9eypfrbVWbHbwPvGTQXmXsdOUqmGcinxu";
        private const int SERIALIZE_FORMAT_VERSION = 1;        

        /// <summary>
        /// Constructor
        /// </summary>
        public HxsDynamicVariablePool()
        {
            _variables = new List<IHxsVariable>();
            _functions = new List<HxsDynamicFunction>();
            _constants = new Dictionary<string, object>();
            Init();            
        }

        /// <summary>
        /// Initialize the pool
        /// </summary>
        private void Init()
        {
            if (DEBUG_MODE_ENABLED)
            {
                RegisterFunction("debug.enable", true, p =>
                {
                    if (!_debug)
                    {
                        if (p.Length > 0 && p[0] is string && BCrypt.CheckPassword((string)p[0], DEBUG_MODE_KEY_HASH))
                        {
                            // Register debug functions
                            RegisterFunction("debug.exit", true, _ => { Environment.Exit(0); return ""; }, "Close the application.");
                            RegisterFunction("debug.pool", true, _ => ToString(), "Get the dynamic variable pool information.");
                            RegisterFunction("debug.vars", true, _ => VariablesToString(), "Get the variables.");
                            RegisterFunction("debug.objects", true, par => ObjectsToString(par.Length > 0 && par[0].ToString().ToLower() == "true"), "Get the objects.");
                            RegisterFunction("debug.funcs", true, _ => FunctionsToString(), "Get the functions.");

                            _debug = true;
                            return "Debug mode active.";
                        }
                        else
                            return "Access denied.";
                    }
                    else if (p.Length > 0 && p[0] is bool && (bool)p[0] == false)
                    {

                        return "Debug no longer active.";
                    }
                    else
                        return "Debug mode is already active.";
                }, "Enable the debug mode");
            }
        }        

        /// <summary>
        /// Update all variable values
        /// </summary>
        public void UpdateAll()
        {            
            _variables.ForEach(x => x.Update());
        }
        
        /// <summary>
        /// Execute a expression
        /// </summary>
        /// <param name="command">command to execute</param>
        /// <returns>command result</returns>
        public string Execute(string command)
        {
            try
            {
                HxsDynamicVariable<string> var = new HxsDynamicVariable<string>(Guid.NewGuid().ToString(), "");
                var.SetParentPool(this);
                var.SetValue(string.Format("tostring({0})", command));
                string result = (string)var.GetValue();
                Unregister(var);
               
                if (var.LastError != null)
                    return string.Format("Error: {0}", var.LastError.Message);

                return result;
            }
            catch (HxsException ex)
            {
                return "HxsException:" + ex.Message;
            }
        }

        /// <summary>
        /// Reset the pool
        /// </summary>
        public void Clear()
        {
            _variables.Clear();
            _constants.Clear();
            _functions.Clear();
            _debug = false;
            Init();
        }

        /// <summary>
        /// Set a variables
        /// </summary>
        /// <param name="name">name of the variable(no case sensitive)</param>
        /// <param name="value">new value</param>
        public void Set(string name, string value)
        {
            var vars = _variables.Where(x => x.GetName().ToLower() == name.ToLower());
            foreach (var var in vars)
            {
                var.SetValue(value);
                if (var is IInfiniteLoopDetection)
                {
                    if ((var as IInfiniteLoopDetection).DetectLoops(this))
                        var.RemoveExpression();
                }
            }
        }

        /// <summary>
        /// Get a variable with a given name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IHxsVariable Get(string name)
        {
            return _variables.FirstOrDefault(x => x.GetName().ToLower() == name.ToLower());
        }

        #region Registration

        /// <summary>
        /// 
        /// </summary>
        /// <param name="variable"></param>
        public void Register(IHxsVariable variable)
        {
            _variables.Add(variable);
            variable.SetParentPool(this);
            variable.Update();
        }

        public void Unregister(string name)
        {
            _variables.RemoveAll(x => x.GetName() == name);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="variable"></param>
        public void Unregister(IHxsVariable variable)
        {
            if (_variables.Contains(variable))
            {
                variable.SetParentPool(null);
                _variables.Remove(variable);
                UpdateAll();
            }
        }

        #endregion

        #region Object management

        public void UnregisterObject(string name)
        {
            for (int i = 0; i < _variables.Count; i++)
            {
                if (_variables[i].GetName().Split('.')[0] == name)
                    _variables.RemoveAt(i--);
            }
        }

        public void RenameObject(string oldName, string newName)
        {
            _variables.ForEach(v => v.RenameObject(oldName, newName));
        }

        public void SetObjectType(string objectName, string type)
        {
            string typeVarName = string.Format("{0}.__type", objectName);
            var matches = _variables.Where(x => x.GetName() == typeVarName);
            if (matches.Any())
            {
                foreach (var match in matches)
                    match.SetValue(type);
            }
            else
                _variables.Add(new HxsDynamicVariable<string>(typeVarName, type));
        }

        public bool ContainsObject(string name)
        {
            for (int i = 0; i < _variables.Count; i++)
            {
                if (_variables[i].GetName().Split('.')[0] == name)
                    return true;
            }
            return false;
        }

        public List<IHxsVariable> GetVariablesForObject(string name)
        {
            return _variables.Where(v => v.GetName().Split('.')[0] == name).ToList();
        }

        public List<IHxsVariable> GetDynamicVariablesForObject(string name)
        {
            return _variables.Where(v => v.HasExpression() && v.GetName().Split('.')[0] == name).ToList();
        }

        #endregion

        #region ISerializable

        /// <summary>
        /// Deserializer constructor
        /// </summary>
        /// <param name="info">serialize info</param>
        /// <param name="context">streaming context</param>
        public HxsDynamicVariablePool(SerializationInfo info, StreamingContext context)
        {
            // Create new
            _variables = new List<IHxsVariable>();
            _functions = new List<HxsDynamicFunction>();
            _constants = new Dictionary<string, object>();
            Init();

            try
            {
                int serializeFormatVersion = info.GetInt32("__FORMAT_VERSION");
                if (SERIALIZE_FORMAT_VERSION == serializeFormatVersion)
                {
                    string[] variableNames = (string[])info.GetValue("variables", typeof(string[]));
                    foreach (string variableName in variableNames)
                    {
                        string type = info.GetString(string.Format("variables.{0}.type", variableName));
                        string value = info.GetString(string.Format("variables.{0}.value", variableName));
                        string code = info.GetString(string.Format("variables.{0}.code", variableName));
                        IHxsVariable variable = null;
                        if (type == "System.Int32")
                        {
                            variable = new HxsDynamicVariable<int>(variableName,
                                                                   int.Parse(value),
                                                                   code == "#null" ? null : new HxsExpression(code, false, this));
                        }
                        else if (type == "System.Double")
                        {
                            variable = new HxsDynamicVariable<double>(variableName,
                                                                      double.Parse(value),
                                                                      code == "#null" ? null : new HxsExpression(code, false, this));
                        }
                        else if (type == "System.Boolean")
                        {
                            variable = new HxsDynamicVariable<bool>(variableName,
                                                                    bool.Parse(value),
                                                                    code == "#null" ? null : new HxsExpression(code, false, this));
                        }
                        else if (type == "System.String")
                        {
                            variable = new HxsDynamicVariable<string>(variableName,
                                                                      value,
                                                                      code == "#null" ? null : new HxsExpression(code, false, this));
                        }
                        else
                            throw new Exception("Invalid value type");
                        variable.SetParentPool(this);
                        _variables.Add(variable);
                    }

                    _constants = (Dictionary<string, object>)info.GetValue("constants", typeof(Dictionary<string, object>));
                }
                else
                    throw new FormatException();
            }
            catch
            {
                
            }
        }

        /// <summary>
        /// Serialize object data
        /// </summary>
        /// <param name="info">serialize data</param>
        /// <param name="context">streaming context</param>
        //[SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter =true)]
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("__FORMAT_VERSION", SERIALIZE_FORMAT_VERSION);
            string[] functionNames = _variables.Where(x => x.CanBeSerialized()).Select(x => x.GetName()).ToArray<string>();
            info.AddValue("variables", functionNames);
            foreach (IHxsVariable variable in _variables)
            {
                if (variable.CanBeSerialized())
                {
                    info.AddValue(string.Format("variables.{0}.type", variable.GetName()), variable.GetTypeName());
                    info.AddValue(string.Format("variables.{0}.value", variable.GetName()), variable.GetValue());
                    info.AddValue(string.Format("variables.{0}.code", variable.GetName()), variable.Serialize());
                }
            }
            info.AddValue("constants", _constants);
        }

        #endregion

        #region IHxsVariableProvider

        object IHxsVariableProvider.GetValue(string name)
        {
            if (_variables.Any(x => x.GetName() == name))
            {
                var variable = _variables.Where(x => x.GetName() == name).First();
                if (variable.HasExternals())
                    variable.Update();
                return variable.GetValue();
            }
            else if (_constants.ContainsKey(name))
                return _constants[name];
            else
                throw new HxsUndefinedVariableException(name);
        }

        public void RegisterConstant(string name, object value)
        {
            if (_constants.ContainsKey(name))
                _constants[name] = value;
            else
                _constants.Add(name, value);
        }

        public bool ContainsVariable(string name)
        {
            name = name.ToLower();
            return _variables.Any(x => x.GetName().ToLower() == name);
        }

        #endregion

        #region Functions & IHxsFunctionExecuter

        public void RegisterFunction(HxsDynamicFunction function)
        {
            _functions.Add(function);
        }

        public void RegisterFunction(string name, bool isStatic, HXS_Engine.DynamicVariables.HxsDynamicFunction.FunctionDelegate function)
        {
            if (_functions.Any(x => x.Name.ToLower() == name.ToLower()))
                UnregisterFunction(name);            
            _functions.Add(new HxsDynamicFunction(name, isStatic, function));
        }

        public void RegisterFunction(string name, bool isStatic, HXS_Engine.DynamicVariables.HxsDynamicFunction.FunctionDelegate function, string description)
        {
            if (_functions.Any(x => x.Name.ToLower() == name.ToLower()))
                UnregisterFunction(name);
            _functions.Add(new HxsDynamicFunction(name, isStatic, function, description));
        }

        public void UnregisterFunction(string name)
        {
            HxsDynamicFunction func = _functions.FirstOrDefault(x => x.Name.ToLower() == name.ToLower());
            if (func != null && _functions.Contains(func))
                _functions.Remove(func);
        }

        bool IHxsFunctionExecuter.FunctionExists(string name)
        {
            return _functions.Any(x => x.Name.ToLower() == name.ToLower());
        }

        bool IHxsFunctionExecuter.FunctionIsStatic(string name)
        {
            return _functions.First(x => x.Name.ToLower() == name.ToLower()).Static;
        }

        object IHxsFunctionExecuter.ExecuteFunction(string name, object[] parameters)
        {
            return _functions.First(x => x.Name.ToLower() == name.ToLower()).Function(parameters);
        }

        #endregion

        #region ToString functions

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string VariablesToString()
        {
            string str = "";
            _variables.ForEach(x => str += x.ToString() + ", ");
            return str.Substring(0, str.Length - 2);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="compact"></param>
        /// <returns></returns>
        public string ObjectsToString(bool compact)
        {
            // Group and sort the variables
            var res = (from o in _variables
                       group o by o.GetName().Split('.')[0] into g
                       select g);

            // Build the string
            string str = "\n";
            if (compact)
            {
                foreach (var obj in res)
                {
                    string typeValueName = string.Format("{0}.__type", obj.Key);
                    string type = " : <Object>";
                    foreach (IHxsVariable variable in obj)
                    {
                        if (variable.GetName() == typeValueName)
                            type = string.Format(" : <{0}>", variable.GetValue());
                    }
                    str += string.Format("{0}{1}\n", obj.Key, type);
                }
            }
            else
            {
                foreach (var obj in res)
                {
                    if (obj.Count() > 1)
                    {
                        string typeValueName = string.Format("{0}.__type", obj.Key);
                        string type = " : <Object>";
                        foreach (IHxsVariable variable in obj)
                        {
                            if (variable.GetName() == typeValueName)
                                type = string.Format(" : <{0}>", variable.GetValue());
                        }

                        str += string.Format("{0}{1}\n{{\n", obj.Key, type);
                        foreach (IHxsVariable variable in obj)
                        {
                            if (variable.GetName() != typeValueName)
                            {
                                int pointPos = variable.GetName().IndexOf('.');
                                if (pointPos == -1)
                                    str += string.Format("   {0} {1}\n", variable.GetAccessString(), variable.ToString());
                                else
                                    str += string.Format("   {0} {1}\n", variable.GetAccessString(), variable.ToString().Substring(pointPos + 1));
                            }
                        }
                        str += "}\n";
                    }
                    else
                    {
                        if (obj.First().GetName() == string.Format("{0}.__type", obj.Key))
                            str += string.Format("{0} : <{1}>\n{{\n   #null\n}}\n", obj.Key, obj.First().GetValue());
                        else
                            str += string.Format("{0}\n", obj.First().ToString());
                    }
                }
            }
            return str;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string FunctionsToString()
        {
            string str = "";
            _functions.ForEach(x => str += x.ToString() + '\n');
            return "\n" + str.Substring(0, str.Length - 1) + "\n";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return (_variables.Count > 0) ? string.Format("<HxsDynamicVariablePool>[{0}]", VariablesToString()) : "<HxsDynamicVariablePool>[null]";
        }

        #endregion

    }
}
