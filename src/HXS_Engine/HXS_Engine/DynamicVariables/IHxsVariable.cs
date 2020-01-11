//  _    _ _      _ _          _    _ _      _ _                        
// | |  | (_)    | | |        | |  | (_)    | | |                       
// | |__| |_  ___| | | _____  | |__| |_  ___| | | _____ _ __ ___   __ _ 
// |  __  | |/ _ \ | |/ / _ \ |  __  | |/ _ \ | |/ / _ \ '_ ` _ \ / _` |
// | |  | | |  __/ |   <  __/ | |  | | |  __/ |   <  __/ | | | | | (_| |
// |_|  |_|_|\___|_|_|\_\___| |_|  |_|_|\___|_|_|\_\___|_| |_| |_|\__,_|
// _____________________________________________________________________
//
// Filename: IHxsVariable.cs
// Project: HXS Engine
// Created: 16-06-2014 (DD-MM-YYYY)
// Changed: 16-07-2014  (DD-MM-YYYY)
//
// (C) Hielke Hielkema - 2014
//
// Author: Hielke Hielkema
// Contact: HielkeHielkema93@gmail.com
//

namespace HXS_Engine.DynamicVariables
{
    public interface IHxsVariable
    {
        void Update();
        string GetName();
        void SetName(string name);
        object GetValue();
        void SetValue(string inputString);
        void RemoveExpression();
        bool HasExternals();
        void SetParentPool(HxsDynamicVariablePool pool);
        bool CanBeSerialized();
        string Serialize();
        string GetTypeName();
        string GetAccessString();
        void RenameObject(string oldName, string newName);
        bool HasExpression();
    }
}
