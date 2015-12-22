using System;

namespace VLab.TSGen
{
    public class ResolveTypeInfoArgs : EventArgs
    {
        public ResolveTypeInfoArgs(Type type, TypeExportInfo info = null)
        {
            Type = type;
            TypeInfo = info;
        }

        public Type Type { get; private set; }
        public TypeExportInfo TypeInfo { get; set; }
    }
}