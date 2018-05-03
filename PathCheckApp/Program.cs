using System;
using System.IO;
using System.Reflection;

namespace PathCheckApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)), "Recipes.json");
            Console.ReadKey();
        }
    }
}
