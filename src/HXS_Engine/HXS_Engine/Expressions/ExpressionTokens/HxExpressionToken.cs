//  _    _ _      _ _          _    _ _      _ _                        
// | |  | (_)    | | |        | |  | (_)    | | |                       
// | |__| |_  ___| | | _____  | |__| |_  ___| | | _____ _ __ ___   __ _ 
// |  __  | |/ _ \ | |/ / _ \ |  __  | |/ _ \ | |/ / _ \ '_ ` _ \ / _` |
// | |  | | |  __/ |   <  __/ | |  | | |  __/ |   <  __/ | | | | | (_| |
// |_|  |_|_|\___|_|_|\_\___| |_|  |_|_|\___|_|_|\_\___|_| |_| |_|\__,_|
// _____________________________________________________________________
//
// Filename: HxsExpressionToken.cs
// Project: HXS Engine
// Created: 16-01-2014 (DD-MM-YYYY)
// Changed: 16-07-2014  (DD-MM-YYYY)
//
// (C) Hielke Hielkema - 2014
//
// Author: Hielke Hielkema
// Contact: HielkeHielkema93@gmail.com
//

namespace HXS_Engine.Expressions.ExpressionTokens
{
    /// <summary>
    /// HXS Expression Token
    /// </summary>
    public abstract class HxsExpressionToken
    {
        // base class for:
        //   [TYPE]             [EXAMPLE]
        // - Expression         "img1.x + 58 - abs(img1.w * 2)"
        // - Operator           +, -, *, /
        // - Constant           1, 4, -5, 8.0, .33, "hello", True
        // - Variable           pages.p11.controls.img1.x
        // - Function           math.pow(5, 1)
        // - ?:                 (i == 5) ? 6:1

        /// <summary>
        /// Get a string representation of the token
        /// </summary>
        /// <author>Hielke Hielkema</author>
        /// <returns>string representation of the token</returns>
        public abstract string ToExpString();

        /// <summary>
        /// Inverse the value of the token
        /// </summary>
        /// <author>Hielke Hielkema</author>
        public virtual void Invert()
        {
            throw new HxsParseException("Token can not be inversed");
        }

        /// <summary>
        /// Get if the value of the token is invertable
        /// </summary>
        /// <author>Hielke Hielkema</author>
        /// <returns>if the value of the token is invertable</returns>
        public virtual bool IsInvertable()
        {
            return false; // not supported by default
        }

        /// <summary>
        /// Rename a HXS object.
        /// For example component_1hb3.x to column1.x
        /// </summary>
        /// <author>Hielke Hielkema</author>
        /// <param name="oldName">the old object name</param>
        /// <param name="newName">the new object name</param>
        public virtual void RenameObject(string oldName, string newName) { }
    }
}