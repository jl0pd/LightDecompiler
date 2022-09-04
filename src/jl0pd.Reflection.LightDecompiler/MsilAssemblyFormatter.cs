namespace jl0pd.Reflection;

using System;
using System.Collections.Immutable;
using System.Globalization;
using System.Reflection;

public static partial class MsilAssemblyFormatter
{
    public static void FormatAssembly(Assembly assembly, TextWriter writer)
    {
        var textBuilder = new TextBuilder(writer);
        WriteHeader(assembly, textBuilder);
        WriteReferences(assembly, textBuilder);
        WriteModuleType(assembly, textBuilder);
        WriteTypes(assembly, textBuilder);
    }

    private static void WriteHeader(Assembly assembly, TextBuilder writer)
    {
        writer.Append(".assembly ");
        writer.AppendLine(assembly.GetName().Name ?? "_");
        writer.Append('{');
        using (writer.WithIndent())
        {
            foreach (var attr in assembly.GetCustomAttributesData())
            {
                WriteAttribute(attr, writer);
            }
        }
        writer.Append('}');
        writer.AppendLine();
    }

    private static void WriteAttribute(CustomAttributeData data, TextBuilder writer)
    {
    }

    private static void WriteReferences(Assembly assembly, TextBuilder writer)
    {
    }

    private static void WriteModuleType(Assembly assembly, TextBuilder writer)
    {
    }

    private static void WriteTypes(Assembly assembly, TextBuilder writer)
    {
        foreach (var type in assembly.GetTypes())
        {
            WriteType(type, writer);
        }
    }

    private static void WriteType(Type type, TextBuilder writer)
    {
        writer.Append(".class ");
        WriteTypeAttributes(type, writer);

        writer.Append(type.FullName ?? type.Name);
        if (type.IsGenericType)
        {
            WriteGenericArgs(type.GetGenericArguments(), writer);
        }

        writer.AppendLine();
        if (type.BaseType is { } baseType)
        {
            using (writer.WithIndent())
            {
                writer.Append("extends ");
                writer.AppendLine(ReferenceFormatter.FormatType(baseType));
            }
        }

        writer.AppendLine('{');
        using (writer.WithIndent())
        {
            WriteTypeMembers(type, writer);
        }
        writer.AppendLine('}');
        writer.AppendLine();
    }

    private static void WriteTypeMembers(Type type, TextBuilder writer)
    {
        var info = type.GetTypeInfo();
        foreach (var field in info.DeclaredFields)
        {
            WriteField(field, writer);
        }

        foreach (var @event in info.DeclaredEvents)
        {
            throw new NotImplementedException();
        }

        foreach (var property in info.DeclaredProperties)
        {
            throw new NotImplementedException();
        }

        foreach (var ctor in info.DeclaredConstructors)
        {
            throw new NotImplementedException();
        }

        foreach (var method in info.DeclaredMethods)
        {
            WriteMethod(method, writer);
        }

        foreach (var nestedType in info.DeclaredNestedTypes)
        {
            throw new NotImplementedException();
        }
    }

    private static void WriteMethod(MethodInfo method, TextBuilder writer)
    {
        writer.Append(".method ");
        WriteMethodAttributes(method, writer);

        if (!method.IsStatic)
        {
            writer.Append("instance ");
        }

        writer.Append(ReferenceFormatter.FormatType(method.ReturnType));
        writer.Append(' ');
        writer.Append(method.Name);
        if (method.IsGenericMethod)
        {
            WriteGenericArgs(method.GetGenericArguments(), writer);
        }

        WriteParameters(method, writer);

        WriteMethodImplFlags(method.MethodImplementationFlags, writer);
        writer.AppendLine();
        writer.AppendLine('{');
        using (writer.WithIndent())
        {
            var instructions = RuntimeDecompiler.Decompile(method);
            new MsilInstructionFormatter().Format(instructions, writer);
        }
        writer.AppendLine('}');
    }

    private static void WriteParameters(MethodInfo method, TextBuilder writer)
    {
        var parameters = method.GetParameters();
        
        if (parameters.Length > 1)
        {
            writer.AppendLine(" (");
        }
        else
        {
            writer.Append(" (");
        }

        using (writer.WithIndent())
        {
            WriteMethodParameters(parameters, writer);
        }
        
        writer.Append(") ");
    }

    private static void WriteMethodImplFlags(MethodImplAttributes flags, TextBuilder writer)
    {
        if (flags.HasFlag(MethodImplAttributes.Native))
        {
            writer.Append("native ");
        }
        else if (flags.HasFlag(MethodImplAttributes.Runtime))
        {
            writer.Append("runtime ");
        }
        else
        {
            writer.Append("cil ");
        }

        if (flags.HasFlag(MethodImplAttributes.Unmanaged))
        {
            writer.Append("unmanaged ");
        }
        else
        {
            writer.Append("managed ");
        }

        if (flags.HasFlag(MethodImplAttributes.ForwardRef))
        {
            writer.Append("forwardref ");
        }
        if (flags.HasFlag(MethodImplAttributes.InternalCall))
        {
            writer.Append("internalcall ");
        }
        if (flags.HasFlag(MethodImplAttributes.NoInlining))
        {
            writer.Append("noinlining ");
        }
        if (flags.HasFlag(MethodImplAttributes.NoOptimization))
        {
            writer.Append("nooptimization ");
        }
        if (flags.HasFlag(MethodImplAttributes.Synchronized))
        {
            writer.Append("synchronized ");
        }
    }

    private static void WriteMethodParameters(ParameterInfo[] parameters, TextBuilder writer)
    {
        for (int i = 0; i < parameters.Length; i++)
        {
            if (i != 0)
            {
                writer.Append(", ");
            }

            Type type = parameters[i].ParameterType;
            if (type.IsGenericMethodParameter)
            {
                writer.Append("!!");
                writer.Append(type.GenericParameterPosition.ToString(CultureInfo.InvariantCulture));
                writer.Append(type.Name);
            }
            else if (type.IsGenericTypeParameter)
            {
                writer.Append("!");
                writer.Append(type.GenericParameterPosition.ToString(CultureInfo.InvariantCulture));
                writer.Append(type.Name);
            }
            else
            {
                writer.Append(ReferenceFormatter.FormatType(type));
            }

            if (parameters[i].Name is { } name)
            {
                writer.Append(' ');
                writer.Append(name);
            }
        }
    }

    private static void WriteMethodAttributes(MethodInfo method, TextBuilder writer)
    {
        writer.Append((method.Attributes & MethodAttributes.MemberAccessMask) switch
        {
            MethodAttributes.Private => "private ",
            MethodAttributes.FamANDAssem => "famandassem ",
            MethodAttributes.Assembly => "assembly ",
            MethodAttributes.FamORAssem => "famorassem ",
            MethodAttributes.Public => "public ",
            MethodAttributes.PrivateScope => "compilercontrolled ",
            _ => throw ThrowHelper.Unreachable,
        });

        if (method.IsHideBySig)
        {
            writer.Append("hidebysig ");
        }

        if (method.IsFinal)
        {
            writer.Append("final ");
        }

        if (method.Attributes.HasFlag(MethodAttributes.NewSlot))
        {
            writer.Append("newslit ");
        }

        if (method.Attributes.HasFlag(MethodAttributes.PinvokeImpl))
        {
            throw new NotImplementedException();
        }

        if (method.Attributes.HasFlag(MethodAttributes.SpecialName))
        {
            writer.Append("specialname ");
        }

        if (method.Attributes.HasFlag(MethodAttributes.RTSpecialName))
        {
            writer.Append("rtspecialname ");
        }

        if (method.IsStatic)
        {
            writer.Append("static ");
        }

        if (method.Attributes.HasFlag(MethodAttributes.CheckAccessOnOverride))
        {
            writer.Append("strict ");
        }

        if (method.IsVirtual)
        {
            writer.Append("virtual ");
        }
    }

    private static void WriteField(FieldInfo field, TextBuilder writer)
    {
        throw new NotImplementedException();
    }

    private static void WriteGenericArgs(Type[] genArgs, TextBuilder writer)
    {
        writer.Append('<');
        for (int i = 0; i < genArgs.Length; i++)
        {
            if (i != 0)
            {
                writer.Append(", ");
            }

            Type? t = genArgs[i];
            if (t.GenericParameterAttributes.HasFlag(GenericParameterAttributes.ReferenceTypeConstraint))
            {
                writer.Append("valuetype ");
            }
            if (t.GenericParameterAttributes.HasFlag(GenericParameterAttributes.ReferenceTypeConstraint))
            {
                writer.Append("class ");
            }
            if (t.GenericParameterAttributes.HasFlag(GenericParameterAttributes.DefaultConstructorConstraint))
            {
                writer.Append(".ctor ");
            }

            if (t.GetGenericParameterConstraints() is { Length: > 0 } constraints)
            {
                writer.Append('(');
                for (int j = 0; j < constraints.Length; j++)
                {
                    if (j != 0)
                    {
                        writer.Append(", ");
                    }
                    writer.Append(ReferenceFormatter.FormatType(constraints[j]));
                }
                writer.Append(") ");
            }

            // it doens't make sense to have both these attributes, but it may be possible
            if (t.GenericParameterAttributes.HasFlag(GenericParameterAttributes.Covariant))
            {
                writer.Append('+');
            }
            if (t.GenericParameterAttributes.HasFlag(GenericParameterAttributes.Contravariant))
            {
                writer.Append('-');
            }

            writer.Append(t.Name);
        }
        writer.Append('>');
    }

    private static void WriteTypeAttributes(Type type, TextBuilder writer)
    {
        if (type.IsInterface)
        {
            writer.Append("interface ");
        }

        if (type.IsAbstract)
        {
            writer.Append("abstract ");
        }

        writer.Append((type.Attributes & TypeAttributes.VisibilityMask) switch
        {
            TypeAttributes.NestedAssembly => "nested assembly ",
            TypeAttributes.NestedFamANDAssem => "nested famandassem ",
            TypeAttributes.NestedFamily => "nested family ",
            TypeAttributes.NestedFamORAssem => "nested famorassem ",
            TypeAttributes.NestedPrivate => "nested private ",
            TypeAttributes.NestedPublic => "nested public ",
            TypeAttributes.Public => "public ",
            TypeAttributes.NotPublic => "private ",
            _ => throw ThrowHelper.Unreachable,
        });

        if (type.IsAutoLayout)
        {
            writer.Append("auto ");
        }
        else if (type.IsLayoutSequential)
        {
            writer.Append("sequential ");
        }
        else if (type.IsExplicitLayout)
        {
            writer.Append("explicit ");
        }
        else
        {
            throw ThrowHelper.Unreachable;
        }

        if (type.IsAnsiClass)
        {
            writer.Append("ansi ");
        }

        if (type.IsSealed)
        {
            writer.Append("sealed ");
        }

        if (type.IsSerializable)
        {
            writer.Append("serializable ");
        }

        if (type.Attributes.HasFlag(TypeAttributes.BeforeFieldInit))
        {
            writer.Append("beforefieldinit ");
        }

        if (type.IsSpecialName)
        {
            writer.Append("specialname");
        }

        if (type.Attributes.HasFlag(TypeAttributes.RTSpecialName))
        {
            writer.Append("rtspecialname");
        }
    }
}
