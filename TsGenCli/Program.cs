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

    public class CommandLineArgs
    {
        private const string NamespaceFilterArgName = "-ns:";
        private const string PrefixFilterArgName = "-prefix:";
        private const string SuffixFilterArgName = "-suffix:";
        private const string AssemblyArgName = "-asm:";
        private const string ClassArgName = "-class:";
        private const string ModuleArgName = "-module:";
        private const string OutFileArgName = "-out:";

        public CommandLineArgs()
        {
            NamespaceFilter = new List<string>();
            SuffixFilter = new List<string>();
            PrefixFilter = new List<string>();
            Assemblies = new List<string>();
            Classes = new List<String>();
        }

        public string OutputFile { get; set; }
        public string ModuleName { get; set; }
        public List<string> Classes { get; set; }
        public List<string> Assemblies { get; set; }
        public List<string> PrefixFilter { get; set; }
        public List<String> NamespaceFilter { get; set; }
        public List<String> SuffixFilter { get; set; }

        internal static CommandLineArgs ParseStr(string[] args)
        {
            var result = new CommandLineArgs();
            for (var i = 0; i < args.Length; i++)
            {
                var cmdArg = "";
                if (TryCommand(args, NamespaceFilterArgName, out cmdArg))
                {
                    result.NamespaceFilter.AddRange(cmdArg.Split(new []{';'}, StringSplitOptions.RemoveEmptyEntries));
                }
                if (TryCommand(args, SuffixFilterArgName, out cmdArg))
                {
                    result.SuffixFilter.AddRange(cmdArg.Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries));
                }
                if (TryCommand(args, PrefixFilterArgName, out cmdArg))
                {
                    result.PrefixFilter.AddRange(cmdArg.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries));
                }
                if (TryCommand(args, AssemblyArgName, out cmdArg))
                {
                    result.Assemblies.AddRange(cmdArg.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries));
                }
                if (TryCommand(args, ClassArgName, out cmdArg))
                {
                    result.Classes.AddRange(cmdArg.Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries));
                }
                if (TryCommand(args, ModuleArgName, out cmdArg))
                {
                    result.ModuleName = cmdArg;
                }
                if (TryCommand(args, OutFileArgName, out cmdArg))
                {
                    result.OutputFile = cmdArg;
                }


            }
            return result;
        }

        private static bool TryCommand(IEnumerable<string> args, string argName, out string argValue)
        {
            var argHasParam = argName.EndsWith(":");
            var argsList = new List<string>(args.Select(arg => arg.Trim()));
            var itemIndex = argsList.FindIndex(arg => arg.Trim().StartsWith(argName));

            if (itemIndex == -1) { 
                argValue = null;
                return false;
            }
            argValue = argsList[itemIndex].Substring(argName.Length);

            if (argHasParam && String.IsNullOrWhiteSpace(argValue))
            {
                if (itemIndex == argsList.Count - 1)
                    throw new InvalidOperationException("Parameter missing for command");
                argValue = argsList[itemIndex + 1];
            }
            return !String.IsNullOrWhiteSpace(argValue);
        }
    }
}
