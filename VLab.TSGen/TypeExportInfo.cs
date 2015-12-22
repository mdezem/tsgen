using System;

namespace VLab.TSGen
{
    public class TypeExportInfo
    {
        public TypeExportInfo(Type type)
        {
            Type = type;

            Name = type.IsEnum ? type.Name : String.Format("I{0}", type.Name);
        }

        public Type Type { get; set; }
        public string Name { get; set; }
        public string Module { get; set; }
        internal bool Exported { get; set; }

        public string FullName
        {
            get { return String.IsNullOrWhiteSpace(Module) ? Name : String.Format("{0}.{1}", Module, Name); }
        }
    }
}