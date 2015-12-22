using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace VLab.TSGen
{
    public class TsGenBatch
    {
        private readonly Dictionary<Type, TypeExportInfo> _typeInfo = new Dictionary<Type, TypeExportInfo>();
        private List<Type> _types;

        public TsGenBatch()
        {
            Assemblies = new List<Assembly>();
            Namespaces = new List<string>();
            Prefixes = new List<string>();
            Suffixes = new List<string>();
            Classes = new List<string>();
            ExportReferencedTypes = true;
        }

        public List<Assembly> Assemblies { get; set; }
        public List<String> Namespaces { get; set; }
        public List<String> Prefixes { get; set; }
        public List<String> Suffixes { get; set; }
        public List<String> Classes { get; set; }
        public string ModuleName { get; set; }
        public bool ExportReferencedTypes { get; set; }
        public bool IncludeExternalReferences { get; set; }

        public IEnumerable<Type> GetTypes()
        {
            IEnumerable<Type> allTypes = null;
            foreach (var asm in Assemblies)
            {
                var asmTypes = asm.GetExportedTypes();
                var types = asmTypes.AsEnumerable();
                if (Namespaces.Count > 0)
                {
                    types = types.Where(type =>
                    {
                        if (String.IsNullOrWhiteSpace(type.Namespace))
                            return false;

                        return Namespaces.Any(ns => ns.EndsWith("*")
                            ? type.Namespace.StartsWith(ns.Substring(0, ns.Length - 1))
                            : type.Namespace == ns);
                    });
                }

                if (Suffixes.Count > 0)
                    types = types.Where(type => Suffixes.Any(suffix => type.Name.EndsWith(suffix)));

                if (Prefixes.Count > 0)
                    types = types.Where(type => Prefixes.Any(suffix => type.Name.StartsWith(suffix)));

                if (Classes.Count > 0)
                    types = types.Concat(asmTypes.Where(type => Classes.Any(cls => type.FullName == cls)));

                allTypes = allTypes == null ? types : allTypes.Concat(types);
            }
            return (allTypes == null
                ? Type.EmptyTypes
                : allTypes.OrderBy(type => type.Name).Distinct());
        }

        public string GetDeclarations()
        {
            var tsgen = new TsGen();
            var result = new StringBuilder(128);
            var ident = new String(' ', 4);
            var currIdent = "";

            tsgen.ResolveTypeInfo += TsGenResolveTypeInfo;
            _types = GetTypes()
                .OrderBy(type => type.FullName)
                .ToList();

            if (!String.IsNullOrWhiteSpace(ModuleName))
            {
                result.AppendFormat("module {0} {{\n", ModuleName);
                currIdent = ident;
            }

            for (var i = 0; i < _types.Count; i++)
            {
                var type = _types[i];
                var typeInfo = ResolveTypeInfo(type);
                if (typeInfo.Exported)
                    continue;

                Console.WriteLine("Exporting {0}", typeInfo.FullName);

                var typeDecl = tsgen.GetTypeDeclaration(type);
                typeInfo.Exported = true;

                if (String.IsNullOrWhiteSpace(typeDecl))
                    continue;

                typeDecl = typeDecl
                    .Split('\n')
                    .Select(line => String.Format("{0}{1}\n", currIdent, line))
                    .Aggregate("", (aggr, item) => aggr + item);

                result.Append(typeDecl);
            }

            if (!String.IsNullOrWhiteSpace(ModuleName))
            {
                result.AppendLine("}");
            }

            tsgen.ResolveTypeInfo -= TsGenResolveTypeInfo;

            return result.ToString();
        }

        private void TsGenResolveTypeInfo(ResolveTypeInfoArgs args)
        {
            args.TypeInfo = ResolveTypeInfo(args.Type);
        }

        private TypeExportInfo ResolveTypeInfo(Type type)
        {
            TypeExportInfo info;
            if (_typeInfo.TryGetValue(type, out info))
                return info;

            if (!_types.Contains(type) && IncludeExternalReferences)
                _types.Add(type);

            info = new TypeExportInfo(type)
            {
                Module = ModuleName
            };

            _typeInfo.Add(type, info);

            return info;
        }
    }
}