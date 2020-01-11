//  _    _ _      _ _          _    _ _      _ _                        
// | |  | (_)    | | |        | |  | (_)    | | |                       
// | |__| |_  ___| | | _____  | |__| |_  ___| | | _____ _ __ ___   __ _ 
// |  __  | |/ _ \ | |/ / _ \ |  __  | |/ _ \ | |/ / _ \ '_ ` _ \ / _` |
// | |  | | |  __/ |   <  __/ | |  | | |  __/ |   <  __/ | | | | | (_| |
// |_|  |_|_|\___|_|_|\_\___| |_|  |_|_|\___|_|_|\_\___|_| |_| |_|\__,_|
// _____________________________________________________________________
//
// Filename: HxsExpressionConditionalOperator.cs
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
using System.Linq;

namespace HXS_Engine.Expressions.ExpressionTokens
{
    /// <summary>
    /// HXS ?: Operator
    /// </summary>
    /// <author>Hielke Hielkema</author>
    public class HxsExpressionConditionalOperator : HxsExpressionToken, IExternals, IExecutableToken
    {
        /// <summary>
        /// Condition
        /// </summary>
        /// <author>Hielke Hielkema</author>
        public HxsExpression Condition { get; private set; }

        /// <summary>
        /// Value returned when the condition is true
        /// </summary>
        /// <author>Hielke Hielkema</author>
        public HxsExpression FirstOption { get; private set; }

        /// <summary>
        /// Value returned when the condition is false
        /// </summary>
        /// <author>Hielke Hielkema</author>
        public HxsExpression SecondOption { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <author>Hielke Hielkema</author>
        /// <param name="condition">select condition</param>
        /// <param name="firstOption">first option</param>
        /// <param name="secondOption">second option</param>
        public HxsExpressionConditionalOperator(HxsExpression condition, HxsExpression firstOption, HxsExpression secondOption)
        {
            Condition = condition;
            FirstOption = firstOption;
            SecondOption = secondOption;
        }

        /// <summary>
        /// Execute the ?: Operator
        /// 
        /// throws
        /// - HxsException
        /// 
        /// </summary>
        /// <author>Hielke Hielkema</author>
        /// <param name="variableProvider">variable provider</param>
        /// <param name="functionExecuter">function executer</param>
        /// <returns>execute result</returns>
        public object Execute(IHxsVariableProvider variableProvider, IHxsFunctionExecuter functionExecuter)
        {
            object ConditionResult = Condition.Execute(variableProvider, functionExecuter);
            if (ConditionResult != null && ConditionResult is bool)
                return ((bool)ConditionResult ? FirstOption : SecondOption).Execute(variableProvider, functionExecuter);
            else
                throw new HxsInvalidCondition(ToExpString());
        }

        /// <summary>
        /// Get a string representation of the token
        /// </summary>
        /// <author>Hielke Hielkema</author>
        /// <returns>string representation of the token</returns>
        public override string ToExpString()
        {
            return string.Format("{0} ? ({1}):({2})", Condition.ToExpString(), FirstOption.ToExpString(), SecondOption.ToExpString());
        }

        /// <summary>
        /// Rename a HXS object.
        /// For example component_1hb3.x to column1.x
        /// </summary>
        /// <author>Hielke Hielkema</author>
        /// <param name="oldName">the old object name</param>
        /// <param name="newName">the new object name</param>
        public override void RenameObject(string oldName, string newName)
        {
            Condition.RenameObject(oldName, newName);
            FirstOption.RenameObject(oldName, newName);
            SecondOption.RenameObject(oldName, newName);
        }

        #region IExternals

        /// <summary>
        /// Get the number of external variables and functions
        /// </summary>
        /// <author>Hielke Hielkema</author>
        /// <returns>number of external variables and functions</returns>
        int IExternals.GetExternalsCount()
        {
            return Condition.Tokens.Union(FirstOption.Tokens).Union(SecondOption.Tokens).Where(x => x is IExternals).Select(x => (x as IExternals).GetExternalsCount()).Aggregate(0, (a, b) => a + b);
        }

        /// <summary>
        /// Get the number of external variables
        /// </summary>
        /// <author>Hielke Hielkema</author>
        /// <returns>number of external variables</returns>
        int IExternals.GetExternalVariableCount()
        {
            return Condition.Tokens.Union(FirstOption.Tokens).Union(SecondOption.Tokens).Where(x => x is IExternals).Select(x => (x as IExternals).GetExternalVariableCount()).Aggregate(0, (a, b) => a + b);
        }

        /// <summary>
        /// Get the number of external functions
        /// </summary>
        /// <author>Hielke Hielkema</author>
        /// <returns>number of external functions</returns>
        int IExternals.GetExternalFunctionCount()
        {
            return Condition.Tokens.Union(FirstOption.Tokens).Union(SecondOption.Tokens).Where(x => x is IExternals).Select(x => (x as IExternals).GetExternalFunctionCount()).Aggregate(0, (a, b) => a + b);
        }

        /// <summary>
        /// Get the external variables
        /// </summary>
        /// <author>Hielke Hielkema</author>
        /// <returns>the external variables</returns>
        List<string> IExternals.GetExternalVariables()
        {
            return Condition.Tokens.Union(FirstOption.Tokens).Union(SecondOption.Tokens).Where(x => x is IExternals).Select(x => (x as IExternals).GetExternalVariables()).Aggregate(new List<string>(), (a, b) => a.Union(b).ToList());
        }

        /// <summary>
        /// Get the external functions
        /// </summary>
        /// <author>Hielke Hielkema</author>
        /// <returns>the external functions</returns>
        List<string> IExternals.GetExternalFunctions()
        {
            return Condition.Tokens.Union(FirstOption.Tokens).Union(SecondOption.Tokens).Where(x => x is IExternals).Select(x => (x as IExternals).GetExternalFunctions()).Aggregate(new List<string>(), (a, b) => a.Union(b).ToList());
        }

        #endregion
    }
}
