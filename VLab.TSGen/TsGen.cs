using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace VLab.TSGen
{
    public class TsGen
    {
        private readonly Dictionary<Type, TypeExportInfo> _typeInfo = new Dictionary<Type, TypeExportInfo>();

        public event ResolveTypeInfoDelegate ResolveTypeInfo;

        public string GetTypeDeclaration<T>()
        {
            return GetTypeDeclaration(typeof(T));
        }

        public string GetTypeDeclaration(Type type)
        {
            if (type == null) throw new ArgumentNullException("type");

            if (type.IsEnum)
                return GetEnumDeclaration(type);
            if (type.IsAbstract) return String.Empty;
            return (type.IsGenericTypeDefinition 
                ? String.Empty 
                : GetInterfaceDeclaration(type));
        }

        private static string GetEnumDeclaration(Type type)
        {
            var result = new StringBuilder(128);
            result.AppendFormat("export enum {0} {{\n", type.Name);

            var names = Enum.GetNames(type);
            var values = names
                .Select(enumItem => Enum.Parse(type, enumItem))
                .Select(Convert.ToInt32)
                .ToArray();

            for (var i = 0; i < names.Length; i++)
            {
                result.AppendFormat("    {0} = {1}", names[i], values[i]);
                if (i < names.Length - 1)
                    result.AppendLine(",");
                else result.AppendLine();
            }
            result.AppendLine("}");

            return result.ToString();
        }

        private string GetInterfaceDeclaration(Type type)
        {
            var result = new StringBuilder(512);
            result.AppendFormat("export interface I{0} {{\n", type.Name);

            var properties = type.GetProperties();
            foreach (var property in properties)
            {
                result.AppendFormat("    {0}: {1};\n",
                    ResolveMemberDeclarationName(property),
                    ResolveTypeTsName(property.PropertyType));
            }
            result.AppendLine("} ");
            return result.ToString();
        }

        private bool IsNullable(Type type)
        {
            return (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>));
        }

        private Type GetMemberType(MemberInfo member)
        {
            var prop = member as PropertyInfo;
            if (prop != null)
                return prop.PropertyType;
            return null;
        }

        private string ResolveTypeTsName(Type type)
        {
            if (IsNullable(type))
                type = Nullable.GetUnderlyingType(type);

            if (type.IsEnum)
            {
                var typeInfo = ResolveTypeInfoInternal(type);
                return typeInfo.Name;
            }

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean:
                    return "boolean";
                case TypeCode.Char:
                case TypeCode.String:
                    return "string";

                case TypeCode.Byte:
                case TypeCode.Decimal:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.SByte:
                case TypeCode.Single:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    return "number";
                case TypeCode.DateTime:
                    return "Date";
                case TypeCode.Object:
                    if (type == typeof(Object))
                        return "any";

                    if (type.IsArray)
                    {
                        var elemName = ResolveTypeTsName(type.GetElementType());
                        return String.Format("Array<{0}>", elemName);
                    }

                    var enumType = type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>) 
                        ? type
                        : type.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));

                    if (enumType != null)
                    {
                        var elemName = ResolveTypeTsName(enumType.GetGenericArguments().Single());
                        return String.Format("Array<{0}>", elemName);
                    }

                    var typeInfo = ResolveTypeInfoInternal(type);
                    return typeInfo.Name;

                default:
                    return String.Format("ErrorForType{0}", type.Name);
            }
        }

        private TypeExportInfo ResolveTypeInfoInternal(Type type)
        {
            TypeExportInfo info;
            if (_typeInfo.TryGetValue(type, out info))
                return info;

            if (ResolveTypeInfo != null)
            {
                var args = new ResolveTypeInfoArgs(type, new TypeExportInfo(type));
                ResolveTypeInfo(args);
                info = args.TypeInfo;
            }
            else
            {
                info = new TypeExportInfo(type);
            }
            _typeInfo.Add(type, info);
            return info;
        }

        private static string ResolveMemberName(MemberInfo member)
        {
            return Regex.Replace(member.Name, @"([A-Z])([A-Z][a-z])|([a-z0-9])([A-Z])", "$1$3_$2$4").ToLower();
        }

        private string ResolveMemberDeclarationName(MemberInfo member)
        {
            var name = ResolveMemberName(member);
            var type = GetMemberType(member);
            if (IsNullable(type))
                name = String.Format("{0}?", name);
            return name;
        }
    }
}
