using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using VLab.TSGen;

namespace TsGenCli
{
    class Program
    {
        static void Main(string[] args)
        {
            var cmdArgs = CommandLineArgs.ParseStr(args);
            var batch = new TsGenBatch();

            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            foreach (var asmName in cmdArgs.Assemblies)
            {
                var asm = Assembly.LoadFrom(asmName);
                batch.Assemblies.Add(asm);
            }

            batch.Namespaces.AddRange(cmdArgs.NamespaceFilter);
            batch.Prefixes.AddRange(cmdArgs.PrefixFilter);
            batch.Suffixes.AddRange(cmdArgs.SuffixFilter);
            batch.Classes.AddRange(cmdArgs.Classes);
            batch.ModuleName = cmdArgs.ModuleName;

            batch.IncludeExternalReferences = cmdArgs.IncludeExternalReferences;

            var types = batch.GetTypes().ToList();
            Console.WriteLine("{0} types found: ", types.Count);
            types.ForEach(type => Console.WriteLine(type.FullName));
            

            var result = batch.GetDeclarations();

            if (!String.IsNullOrWhiteSpace(cmdArgs.OutputFile))
                File.WriteAllText(cmdArgs.OutputFile, result);

            Console.Write(result);
        }

        static HashSet<String> AsmNames = new HashSet<string>();
        static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            if (!AsmNames.Contains(args.Name))
            {
                AsmNames.Add(args.Name);
                Console.WriteLine("Resolving dependency {0}", args.Name);
            }
            var requestingAsmPath = Path.GetDirectoryName(args.RequestingAssembly.Location) ?? "";
            var asmName = Path.Combine(requestingAsmPath, args.Name);
            if (File.Exists(asmName))
                return Assembly.LoadFrom(asmName);
            return null;
        }


    }
}
