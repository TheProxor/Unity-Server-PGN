using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.CodeDom.Compiler;
using System.Reflection;

using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Scripting;

namespace PGN.Runtime
{
    public static class ScriptsCompiler
    {
        private static string rawCode = string.Empty;

        private static List<ScriptBehaviour> scripts = new List<ScriptBehaviour>();
        private static List<TypeBehaviour> types = new List<TypeBehaviour>();

        public static void ReadScripts(string path)
        {
            rawCode = File.ReadAllText(path);
        }
 

        public static void TestCompile()
        {
            var scriptOptions = ScriptOptions.Default;
            scriptOptions.AddImports("System");
            scriptOptions.AddImports("System.Collections");
            scriptOptions.AddImports("System.Collections.Generic");
            scriptOptions.AddImports("System.Text");
            scriptOptions.AddImports("System");

            var result = CSharpScript.EvaluateAsync("Console.WriteLine(\"Hello world!\")", scriptOptions).Result;
        }
    }
}
