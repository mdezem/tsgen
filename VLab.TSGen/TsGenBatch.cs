using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;

namespace VLab.TSGen
{
    public class TsGenBatch
    {
        public TsGenBatch()
        {
            Assemblies = new List<Assembly>();
            Namespaces = new List<string>();
            Prefixes = new List<string>();
            Suffixes = new List<string>();
            Classes = new List<string>();
        }

        public List<Assembly> Assemblies { get; set; }
        public List<String> Namespaces { get; set; }
        public List<String> Prefixes { get; set; }
        public List<String> Suffixes { get; set; }
        public List<String> Classes { get; set; }
        public string ModuleName { get; set; }

        public IEnumerable<Type> GetTypes()
        {
            IEnumerable<Type> allTypes = null;
            foreach (var asm in Assemblies)
            {
                var asmTypes = asm.GetExportedTypes();
                var  types = asmTypes.AsEnumerable();
                if (Namespaces.Count > 0)
                    types = types.Where(type => Namespaces.Any(ns =>
                       ns.EndsWith("*") ? type.Namespace.StartsWith(ns.Substring(0, ns.Length - 1)) : type.Namespace == ns));
                if (Suffixes.Count > 0)
                    types = types.Where(type => Suffixes.Any(suffix => type.Name.EndsWith(suffix)));
                if (Prefixes.Count > 0)
                    types = types.Where(type => Prefixes.Any(suffix => type.Name.StartsWith(suffix)));
                if (Classes.Count > 0)
                    types = types.Concat(asmTypes.Where(type => Classes.Any(cls => type.FullName == cls)));

                allTypes = allTypes == null ? types : allTypes.Concat(types);
            }
            return allTypes.OrderBy(type => type.Name).Distinct();
        }

        public string GetDeclarations()
        {
            var tsgen = new TsGen();
            var types = GetTypes();
            var result = new StringBuilder(128);
            var ident = new String(' ', 4);
            var currIdent = "";

            if (!String.IsNullOrWhiteSpace(ModuleName))
            {
                result.AppendFormat("module {0} {{\n", ModuleName);
                currIdent = ident;
            }

            foreach (var type in types)
            {
                var typeDecl = tsgen.GetTypeDeclaration(type)
                    .Split('\n')
                    .Select(line => String.Format("{0}{1}\n", currIdent, line))
                    .Aggregate("", (aggr, item) => aggr + item);

                result.Append(typeDecl);
            }

            if (!String.IsNullOrWhiteSpace(ModuleName))
            {
                result.AppendLine("}");
            }

            return result.ToString();
        }

    }
}
