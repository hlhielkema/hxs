//  _    _ _      _ _          _    _ _      _ _                        
// | |  | (_)    | | |        | |  | (_)    | | |                       
// | |__| |_  ___| | | _____  | |__| |_  ___| | | _____ _ __ ___   __ _ 
// |  __  | |/ _ \ | |/ / _ \ |  __  | |/ _ \ | |/ / _ \ '_ ` _ \ / _` |
// | |  | | |  __/ |   <  __/ | |  | | |  __/ |   <  __/ | | | | | (_| |
// |_|  |_|_|\___|_|_|\_\___| |_|  |_|_|\___|_|_|\_\___|_| |_| |_|\__,_|
// _____________________________________________________________________
//
// Filename: IInfiniteLoopDetection.cs
// Project: HXS Engine
// Created: 15-07-2014 (DD-MM-YYYY)
// Changed: 16-07-2014  (DD-MM-YYYY)
//
// (C) Hielke Hielkema - 2014
//
// Author: Hielke Hielkema
// Contact: HielkeHielkema93@gmail.com
//

using System.Collections.Generic;

namespace HXS_Engine.DynamicVariables
{
    /// <summary>
    /// 
    /// </summary>
    /// <author>Hielke Hielkema</author>
    public interface IInfiniteLoopDetection
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pool"></param>
        /// <returns></returns>
        bool DetectLoops(HxsDynamicVariablePool pool);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="found"></param>
        /// <param name="pool"></param>
        /// <returns></returns>
        bool DetectLoops(List<string> found, HxsDynamicVariablePool pool);
    }
}
