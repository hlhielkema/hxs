using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HXS_Engine.DynamicVariables;

namespace HXS_Engine
{
    class Program
    {
        static void Main(string[] args)
        {
            // Create the variables pool
            HxsDynamicVariablePool pool = new HxsDynamicVariablePool();
          
            // Example initial variables
            pool.Register(new HxsDynamicVariable<int>("x", 0));
            pool.Register(new HxsDynamicVariable<int>("y", 100));
            pool.Register(new HxsDynamicVariable<string>("z", "abc"));

            // Example dynamic function
            pool.RegisterFunction("add44", true, p => 44 + (int)p[0], "Example function");            
            
            while (true)
            {
                Console.Write("hxs\\> ");
                Console.WriteLine(pool.Execute(Console.ReadLine()));
            }
        }
    }
}
