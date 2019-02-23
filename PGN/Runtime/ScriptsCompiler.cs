using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.CodeDom.Compiler;
using System.Reflection;
using Microsoft.CSharp;

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
            CSharpCodeProvider provider = new CSharpCodeProvider();
            CompilerParameters parameters = new CompilerParameters();

            string code = @"
                            using System;

                            namespace First
                            {
                                public class Program
                                {
                                    public static void Main()
                                    {
                                    " +
                                        "Console.WriteLine(\"THIS INVOKE FROM SCRIPT!\");"
                                        + @"
                                    }
                                }
                            }
                        ";

            // True - memory generation, false - external file generation
            parameters.GenerateInMemory = true;
            // True - exe file generation, false - dll file generation
            parameters.GenerateExecutable = true;

            CompilerResults results = provider.CompileAssemblyFromSource(parameters, code);

            if (results.Errors.HasErrors)
            {
                StringBuilder sb = new StringBuilder();

                foreach (CompilerError error in results.Errors)
                {
                    sb.AppendLine(String.Format("Error ({0}): {1}", error.ErrorNumber, error.ErrorText));
                }

                throw new InvalidOperationException(sb.ToString());
            }

            Assembly assembly = results.CompiledAssembly;
            Type program = assembly.GetType("First.Program");
            MethodInfo main = program.GetMethod("Main");

            main.Invoke(null, null);
        }
    }
}
