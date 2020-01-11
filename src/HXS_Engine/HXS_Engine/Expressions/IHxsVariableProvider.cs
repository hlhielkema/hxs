//  _    _ _      _ _          _    _ _      _ _                        
// | |  | (_)    | | |        | |  | (_)    | | |                       
// | |__| |_  ___| | | _____  | |__| |_  ___| | | _____ _ __ ___   __ _ 
// |  __  | |/ _ \ | |/ / _ \ |  __  | |/ _ \ | |/ / _ \ '_ ` _ \ / _` |
// | |  | | |  __/ |   <  __/ | |  | | |  __/ |   <  __/ | | | | | (_| |
// |_|  |_|_|\___|_|_|\_\___| |_|  |_|_|\___|_|_|\_\___|_| |_| |_|\__,_|
// _____________________________________________________________________
//
// Filename: IHxsVariableProvider.cs
// Project: HXS Engine
// Created: 16-01-2014 (DD-MM-YYYY)
// Changed: 16-07-2014  (DD-MM-YYYY)
//
// (C) Hielke Hielkema - 2014
//
// Author: Hielke Hielkema
// Contact: HielkeHielkema93@gmail.com
//

namespace HXS_Engine
{
    /// <summary>
    /// An interface that can be implemented to provide the execution method 
    /// of a HxsExpression with the values of external variables.
    /// </summary>
    /// <author>Hielke Hielkema</author>
    public interface IHxsVariableProvider
    {
        /// <summary>
        /// Get the value of a variable
        /// throw HXS_Engine.HxsUndefinedVariableException if the variable is undefined.
        /// </summary>
        /// <author>Hielke Hielkema</author>
        /// <param name="name">variable name</param>
        /// <returns>variable value</returns>
        object GetValue(string name);
    }
}
