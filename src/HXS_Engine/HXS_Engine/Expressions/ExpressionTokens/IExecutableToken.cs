namespace HXS_Engine.Expressions.ExpressionTokens
{
    interface IExecutableToken
    {
        /// <summary>
        /// Get the result from the HXS token
        /// </summary>
        /// <author>Hielke Hielkema</author>
        /// <param name="variableProvider">variable provider</param>
        /// <param name="functionExecuter">function executer</param>
        /// <returns>execute result</returns>
        object Execute(IHxsVariableProvider variableProvider, IHxsFunctionExecuter functionExecuter);
    }
}
