﻿namespace jl0pd.Reflection;

using System.Globalization;
using System.Reflection;
using System.Text;

internal static class ReferenceFormatter
{
    internal static string FormatField(FieldInfo field)
    {
        throw new NotImplementedException();
    }

    internal static string FormatAssembly(Assembly assembly)
    {
        return "[" + assembly.GetName().Name + "]";
    }

    internal static string FormatType(Type type)
    {
        if (type.IsPointer)
        {
            return FormatType(type.GetElementType()!) + "*";
        }

        if (type.IsByRef)
        {
            return FormatType(type.GetElementType()!) + "&";
        }

        if (GetSpecialTypeString(type) is { } ts)
        {
            return ts;
        }

        if (type.IsGenericTypeParameter)
        {
            return "!" + type.GenericParameterPosition.ToString(CultureInfo.InvariantCulture);
        }

        if (type.IsGenericMethodParameter)
        {
            return "!!" + type.GenericParameterPosition.ToString(CultureInfo.InvariantCulture);
        }

        return FormatAssembly(type.Assembly) + (type.FullName ?? type.Name); // type lacks FullName if it's global, i.e. declared in file without namespace
    }

    private static string? GetSpecialTypeString(Type type)
    {
        return type.FullName switch
        {
            "System.Object" => "object",
            "System.Void" => "void",

            "System.Int8" => "int8",
            "System.Int16" => "int16",
            "System.Int32" => "int32",
            "System.Int64" => "int64",

            "System.UInt8" => "unsigned int8",
            "System.UInt16" => "unsigned int16",
            "System.UInt32" => "unsigned int32",
            "System.UInt64" => "unsigned int64",

            "System.IntPtr" => "native int",
            "System.UIntPtr" => "native unsigned int",

            "System.Single" => "float32",
            "System.Double" => "float64",

            "System.String" => "string",
            "System.Boolean" => "bool",
            _ => null,
        };
    }

    internal static string FormatMethodOrCtor(MethodBase methodOrCtor)
    {
        var sb = new StringBuilder();
        if (!methodOrCtor.IsStatic)
        {
            sb.Append("instance ");
        }

        var genMethodDef = methodOrCtor.IsGenericMethod ? ((MethodInfo)methodOrCtor).GetGenericMethodDefinition() : null;

        if (methodOrCtor is MethodInfo mInfo)
        {
            sb.Append(FormatType((genMethodDef ?? mInfo).ReturnParameter.ParameterType));
            sb.Append(' ');
        }
        else
        {
            sb.Append("void ");
        }

        sb.Append(FormatType(methodOrCtor.DeclaringType!));
        sb.Append("::");
        sb.Append(methodOrCtor.Name); // todo: escape

        if (methodOrCtor.IsGenericMethod)
        {
            sb.Append('<');

            foreach (var type in methodOrCtor.GetGenericArguments())
            {
                sb.Append(FormatType(type));
            }

            sb.Append('>');
        }

        sb.Append('(');
        int i = 0;
        foreach (var parameter in (genMethodDef ?? methodOrCtor).GetParameters())
        {
            if (i != 0)
            {
                sb.Append(", ");
            }
            sb.Append(FormatType(parameter.ParameterType));
            i++;
        }
        sb.Append(')');

        return sb.ToString();
    }

    internal static string FormatBytes(byte[] bytes)
    {
        var sb = new StringBuilder(bytes.Length * 3 + 2); // 2 chars per byte + 1 for space. 2 additional bytes for open and close paren
        sb.Append('(');
        for (int i = 0; i < bytes.Length; i++)
        {
            if (i != 0)
            {
                sb.Append(' ');
            }
            sb.Append(bytes[i].ToString("X2"));
        }
        sb.Append(')');

        return sb.ToString();
    }
}
