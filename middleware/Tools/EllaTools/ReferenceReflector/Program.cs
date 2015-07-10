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
            Directory.SetCurrentDirectory(@"E:\Dev\skiline-movie-system\src\Recorder\bin\Debug");
            Reflector r=new Reflector();
            r.ReflectAssembly(Assembly.LoadFrom(@"E:\Dev\skiline-movie-system\src\Recorder\bin\Debug\Recorder.exe"));
            Console.Read();
        }
    }
}
