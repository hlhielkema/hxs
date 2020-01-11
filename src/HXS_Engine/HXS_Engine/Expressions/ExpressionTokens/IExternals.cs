//  _    _ _      _ _          _    _ _      _ _                        
// | |  | (_)    | | |        | |  | (_)    | | |                       
// | |__| |_  ___| | | _____  | |__| |_  ___| | | _____ _ __ ___   __ _ 
// |  __  | |/ _ \ | |/ / _ \ |  __  | |/ _ \ | |/ / _ \ '_ ` _ \ / _` |
// | |  | | |  __/ |   <  __/ | |  | | |  __/ |   <  __/ | | | | | (_| |
// |_|  |_|_|\___|_|_|\_\___| |_|  |_|_|\___|_|_|\_\___|_| |_| |_|\__,_|
// _____________________________________________________________________
//
// Filename: IExternals.cs
// Project: HXS Engine
// Created: 16-01-2014 (DD-MM-YYYY)
// Changed: 16-07-2014  (DD-MM-YYYY)
//
// (C) Hielke Hielkema - 2014
//
// Author: Hielke Hielkema
// Contact: HielkeHielkema93@gmail.com
//

using System.Collections.Generic;

namespace HXS_Engine.Expressions.ExpressionTokens
{
    /// <summary>
    /// A interface for HxsExpressionTokens that use external data
    /// </summary>
    /// <author>Hielke Hielkema</author>
    internal interface IExternals
    {
        /// <summary>
        /// Get the number of external variables and functions
        /// </summary>
        /// <author>Hielke Hielkema</author>
        /// <returns>number of external variables and functions</returns>
        int GetExternalsCount();

        /// <summary>
        /// Get the number of external variables and functions
        /// </summary>
        /// <author>Hielke Hielkema</author>
        /// <returns>number of external variables and functions</returns>
        int GetExternalVariableCount();

        /// <summary>
        /// Get the number of external variables
        /// </summary>
        /// <author>Hielke Hielkema</author>
        /// <returns>number of external variables</returns>
        int GetExternalFunctionCount();

        /// <summary>
        /// Get the external variables
        /// </summary>
        /// <author>Hielke Hielkema</author>
        /// <returns>the external variables</returns>
        List<string> GetExternalVariables();

        /// <summary>
        /// Get the external functions
        /// </summary>
        /// <author>Hielke Hielkema</author>
        /// <returns>the external functions</returns>
        List<string> GetExternalFunctions();
    }
}
