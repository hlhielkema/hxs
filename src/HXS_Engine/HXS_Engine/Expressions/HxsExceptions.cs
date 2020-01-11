//  _    _ _      _ _          _    _ _      _ _                        
// | |  | (_)    | | |        | |  | (_)    | | |                       
// | |__| |_  ___| | | _____  | |__| |_  ___| | | _____ _ __ ___   __ _ 
// |  __  | |/ _ \ | |/ / _ \ |  __  | |/ _ \ | |/ / _ \ '_ ` _ \ / _` |
// | |  | | |  __/ |   <  __/ | |  | | |  __/ |   <  __/ | | | | | (_| |
// |_|  |_|_|\___|_|_|\_\___| |_|  |_|_|\___|_|_|\_\___|_| |_| |_|\__,_|
// _____________________________________________________________________
//
// Filename: HxsExceptions.cs
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

namespace HXS_Engine
{
    class HxsException : Exception
    {
        public HxsException(string message)
            : base(message)
        { }

        public HxsException(HxsException exception)
            : base(exception.Message)
        { }

        public HxsException(Exception exception)
            : base("Unexpected error in HXS expression parsing")
        { }
    }

    class HxsParseException : HxsException
    {
        public HxsParseException(string message)
            : base(message)
        { }
    }

    class HxsUnexpectedCharException : HxsParseException
    {
        public HxsUnexpectedCharException(char ch)
            : base(string.Format(@"Unexpected charachter '{0}' in expression.", ch))
        { }
    }

    class HxsUndefinedFunctionException : HxsException
    {
        public HxsUndefinedFunctionException(string name)
            : base(string.Format("Function \"{0}\" is not defined.", name))
        { }
    }

    class HxsInvalidParameterCountException : HxsException
    {
        public HxsInvalidParameterCountException(string function, int given, int expected)
            : base(string.Format("Function: '{0}' does not take {1} arguments but {2}.", function, given, expected))
        { }
    }

    class HxsUndefinedVariableException : HxsException
    {
        public HxsUndefinedVariableException(string name)
            : base(string.Format("Variable '{0}' is not defined", name))
        { }
    }

    class HxsInvalidCondition : HxsException
    {
        public HxsInvalidCondition(string condition)
            : base("Invalid condition for ?: operator:" + condition)
        { }
    }
}
