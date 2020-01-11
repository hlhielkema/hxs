//  _    _ _      _ _          _    _ _      _ _                        
// | |  | (_)    | | |        | |  | (_)    | | |                       
// | |__| |_  ___| | | _____  | |__| |_  ___| | | _____ _ __ ___   __ _ 
// |  __  | |/ _ \ | |/ / _ \ |  __  | |/ _ \ | |/ / _ \ '_ ` _ \ / _` |
// | |  | | |  __/ |   <  __/ | |  | | |  __/ |   <  __/ | | | | | (_| |
// |_|  |_|_|\___|_|_|\_\___| |_|  |_|_|\___|_|_|\_\___|_| |_| |_|\__,_|
// _____________________________________________________________________
//
// Filename: IHxsFunctionExecuter.cs
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
    /// An interface that can be implemented to provide the execution method.
    /// of a HxsExpression with information about external functions.
    /// </summary>
    /// <author>Hielke Hielkema</author>
    public interface IHxsFunctionExecuter
    {
        /// <summary>
        /// Get if an external function is defined
        /// </summary>
        /// <author>Hielke Hielkema</author>
        /// <param name="name">function name</param>
        /// <returns>
        ///     true = function exists
        ///     false = function does not exist
        /// </returns>
        bool FunctionExists(string name);

        /// <summary>
        /// Gets if a function always returns the same result for a given input.
        /// 
        /// example static funtions:
        /// - abs
        /// - sqrt
        /// example not static functions:
        /// - getValueOfVariable
        /// - getNameForElement
        /// 
        /// </summary>
        /// <author>Hielke Hielkema</author>
        /// <param name="name">function name</param>
        /// <returns>
        ///     true  = function result does not depend on external factors and the result for a given input is always the same
        ///     false = the function result for a given input can differ from time to time
        /// </returns>
        bool FunctionIsStatic(string name);

        /// <summary>
        /// Execute an external function
        /// 
        /// throw HXS_Engine.HxsUndefinedFunctionException if the function is not defined.
        /// this will never be the case if IHxsFunctionExecuter.FunctionExists is implemented correctly.
        /// </summary>
        /// <author>Hielke Hielkema</author>
        /// <param name="name">function name</param>
        /// <param name="parameters">parameters</param>
        /// <returns>execution result</returns>
        object ExecuteFunction(string name, object[] parameters);
    }
}
