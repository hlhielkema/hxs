//  _    _ _      _ _          _    _ _      _ _                        
// | |  | (_)    | | |        | |  | (_)    | | |                       
// | |__| |_  ___| | | _____  | |__| |_  ___| | | _____ _ __ ___   __ _ 
// |  __  | |/ _ \ | |/ / _ \ |  __  | |/ _ \ | |/ / _ \ '_ ` _ \ / _` |
// | |  | | |  __/ |   <  __/ | |  | | |  __/ |   <  __/ | | | | | (_| |
// |_|  |_|_|\___|_|_|\_\___| |_|  |_|_|\___|_|_|\_\___|_| |_| |_|\__,_|
// _____________________________________________________________________
//
// Filename: HxsDynamicFunction.cs
// Project: HXS Engine
// Created: 16-06-2014 (DD-MM-YYYY)
// Changed: 16-07-2014  (DD-MM-YYYY)
//
// (C) Hielke Hielkema - 2014
//
// Author: Hielke Hielkema
// Contact: HielkeHielkema93@gmail.com
//

using System;

namespace HXS_Engine.DynamicVariables
{
    /// <summary>
    /// A shell for a function that can be called from HXS code
    /// </summary>
    /// <author>Hielke Hielkema</author>
    public sealed class HxsDynamicFunction
    {
        /// <summary>
        /// Gets/Sets the name of the function
        /// </summary>
        /// <author>Hielke Hielkema</author>
        public string Name { get; private set; }

        /// <summary>
        /// Gets/Sets if the function is static
        /// Static means that there are no external values needed
        /// This is important for optimizing and analysing.
        /// </summary>
        /// <author>Hielke Hielkema</author>
        public bool Static { get; private set; }

        /// <summary>
        /// The function description (for debug)
        /// </summary>
        /// <author>Hielke Hielkema</author>
        public string Description { get; private set; }

        /// <summary>
        /// A reference to the function code
        /// </summary>
        /// <author>Hielke Hielkema</author>
        public FunctionDelegate Function { get; private set; }
        
        /// <summary>
        /// The function delegate
        /// </summary>
        /// <author>Hielke Hielkema</author>
        /// <param name="parameters">a object array of parameters</param>
        /// <returns>return the result boxed as a object</returns>
        public delegate object FunctionDelegate(object[] parameters);

        /// <summary>
        /// Constructor
        /// The description will be "-"
        /// </summary>
        /// <author>Hielke Hielkema</author>
        /// <param name="name">name of the function</param>
        /// <param name="isStatic">if the function is static(see property description)</param>
        /// <param name="function">the function</param>
        public HxsDynamicFunction(string name, bool isStatic, FunctionDelegate function)
            : this(name, isStatic, function, "-")
        { }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <author>Hielke Hielkema</author>
        /// <param name="name">name of the function</param>
        /// <param name="isStatic">if the function is static(see property description)</param>
        /// <param name="function">the function</param>
        /// <param name="description">funtion description</param>
        public HxsDynamicFunction(string name, bool isStatic, FunctionDelegate function, string description)
        {
            // Argument checking
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("name");
            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException("description");

            // Set values
            Name = name;
            Static = isStatic;
            Function = function;
            Description = description ?? "-";
        }

        /// <summary>
        /// Get a string representation of the object
        /// </summary>
        /// <author>Hielke Hielkema</author>
        /// <returns>a string representation of the object</returns>
        public override string ToString()
        {
            return string.Format("{0}{1}(..) // {2}", Static ? "static ":"", Name, Description);
        }
    }
}