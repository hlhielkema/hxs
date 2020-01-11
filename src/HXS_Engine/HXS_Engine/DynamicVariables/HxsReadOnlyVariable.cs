//  _    _ _      _ _          _    _ _      _ _                        
// | |  | (_)    | | |        | |  | (_)    | | |                       
// | |__| |_  ___| | | _____  | |__| |_  ___| | | _____ _ __ ___   __ _ 
// |  __  | |/ _ \ | |/ / _ \ |  __  | |/ _ \ | |/ / _ \ '_ ` _ \ / _` |
// | |  | | |  __/ |   <  __/ | |  | | |  __/ |   <  __/ | | | | | (_| |
// |_|  |_|_|\___|_|_|\_\___| |_|  |_|_|\___|_|_|\_\___|_| |_| |_|\__,_|
// _____________________________________________________________________
//
// Filename: HxsReadOnlyVariable.cs
// Project: HXS Engine
// Created: 16-06-2014 (DD-MM-YYYY)
// Changed: 16-07-2014  (DD-MM-YYYY)
//
// (C) Hielke Hielkema - 2014
//
// Author: Hielke Hielkema
// Contact: HielkeHielkema93@gmail.com
//
using System.Linq;

namespace HXS_Engine.DynamicVariables
{
    public class HxsReadOnlyVariable<T> : IHxsVariable
    {
        // private fields
        private string _name;
        public GetFunctionDelegate _getFunction;

        public delegate T GetFunctionDelegate();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name"></param>
        /// <param name="getFunction"></param>
        public HxsReadOnlyVariable(string name, GetFunctionDelegate getFunction)
        {
            _name = name;
            _getFunction = getFunction;
            (this as IHxsVariable).Update();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            try
            {
                object result = (this as IHxsVariable).GetValue();
                string value = result.ToString();
                if (result is string)
                {
                    if (value.Length > 100)
                        value = value.Substring(0, 97) + "...";
                    value = string.Format("\"{0}\"", value);
                }

                return string.Format("{0}: {1}", _name, value);
            }
            catch
            {
                return string.Format("{0}: \"#error\"", _name);
            }
        }

        #region IHxsVariable

        void IHxsVariable.Update() { }

        string IHxsVariable.GetName()
        {
            return _name;
        }

        void IHxsVariable.SetName(string name)
        {
            _name = name;
        }

        object IHxsVariable.GetValue()
        {
            return _getFunction();
        }

        void IHxsVariable.SetValue(string inputString) { }

        void IHxsVariable.RemoveExpression() { }        

        bool IHxsVariable.HasExternals()
        {
            return true;
        }

        void IHxsVariable.SetParentPool(HxsDynamicVariablePool pool) { }

        string IHxsVariable.Serialize()
        {
            return null;
        }

        bool IHxsVariable.CanBeSerialized()
        {
            return false;
        }

        string IHxsVariable.GetTypeName()
        {
            return typeof(T).FullName;
        }

        string IHxsVariable.GetAccessString()
        {
            return string.Format("<R_, {0}>", typeof(T).FullName.Split('.').Last());
        }

        void IHxsVariable.RenameObject(string oldName, string newName)
        {
            if (_name.Split('.')[0].ToLower() == oldName.ToLower())
                _name = newName + _name.Substring(oldName.Length);
        }

        bool IHxsVariable.HasExpression()
        {
            return false;
        }

        #endregion
    }
}
