using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ReferenceReflector
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("digraph G {\n");
            foreach (string s in args)
            {
                if (!File.Exists(s))
                {
                    continue;
                }
                Directory.SetCurrentDirectory(Path.GetDirectoryName(s));
                Reflector r = new Reflector();
                try
                {
                    r.ReflectAssembly(Assembly.LoadFrom(s));
                }
                catch (Exception e)
                {

                }
            }
            Console.WriteLine("\n}");
            Console.Read();
        }
    }
}
