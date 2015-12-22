using System;
using System.Collections.Generic;
using System.Linq;

namespace TsGenCli
{
    public class CommandLineArgs
    {
        private const string IncludeExternalReferencesArgName = "-externalrefs:";
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
            Classes = new List<string>();
        }

        public string OutputFile { get; set; }
        public string ModuleName { get; set; }
        public List<string> Classes { get; set; }
        public List<string> Assemblies { get; set; }
        public List<string> PrefixFilter { get; set; }
        public List<string> NamespaceFilter { get; set; }
        public List<string> SuffixFilter { get; set; }

        internal static CommandLineArgs ParseStr(string[] args)
        {
            var result = new CommandLineArgs();
            
            foreach (string arg in args)
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
                if (TryCommand(args, IncludeExternalReferencesArgName, out cmdArg))
                {
                    var val = false;
                    Boolean.TryParse(cmdArg, out val);
                    result.IncludeExternalReferences = val;
                }
            }
            return result;
        }

        public bool IncludeExternalReferences { get; set; }

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