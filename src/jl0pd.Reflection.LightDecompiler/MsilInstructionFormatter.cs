namespace jl0pd.Reflection;

using System.Globalization;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

public sealed class MsilInstructionFormatter : IInstructionFormatter
{
    public static MsilInstructionFormatter Instance { get; } = new();

    public void Format(IReadOnlyList<Instruction> instructions, TextWriter writer)
    {
        int maxSize = instructions.Select(i => i.Offset.ToString(CultureInfo.InvariantCulture).Length).Max();

        var format3 = "{0:D" + maxSize.ToString(CultureInfo.InvariantCulture) + "}: {1} {2}";
        var format2 = "{0:D" + maxSize.ToString(CultureInfo.InvariantCulture) + "}: {1}";

        foreach (var instruction in instructions)
        {
            if (instruction.Operand is { })
            {
                writer.WriteLine(format3, instruction.Offset, instruction.OpCode, FormatOperand(instruction.Operand));
            }
            else
            {
                writer.WriteLine(format2, instruction.Offset, instruction.OpCode);
            }

            switch (instruction.OpCode.FlowControl)
            {
                case FlowControl.Branch:
                case FlowControl.Break:
                case FlowControl.Cond_Branch:
                case FlowControl.Meta:
                case FlowControl.Return:
                case FlowControl.Throw:
                    writer.WriteLine();
                    break;
            }
        }
    }

    private static string FormatOperand(object operand)
    {
        return operand switch
        {
            byte b => b.ToString(CultureInfo.InvariantCulture),
            short s => s.ToString(CultureInfo.InvariantCulture),
            int i => i.ToString(CultureInfo.InvariantCulture),
            long l => l.ToString(CultureInfo.InvariantCulture),
            float f => f.ToString(CultureInfo.InvariantCulture),
            double d => d.ToString(CultureInfo.InvariantCulture),
            string s => "\"" + s + "\"", // TODO: escape string
            byte[] bytes => FormatBytes(bytes),
            MethodBase methodOrCtor => FormatMethodOrCtor(methodOrCtor),
            Type type => FormatType(type),
            FieldInfo field => FormatField(field),
            _ => throw new NotImplementedException(),
        };
    }

    private static string FormatField(FieldInfo field)
    {
        throw new NotImplementedException();
    }

    private static string FormatType(Type type)
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

        var sb = new StringBuilder();
        sb.Append('[');
        sb.Append(type.Assembly.GetName().Name);
        sb.Append(']');
        sb.Append(type.FullName);
        return sb.ToString();
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

    private static string FormatMethodOrCtor(MethodBase methodOrCtor)
    {
        var sb = new StringBuilder();
        if (!methodOrCtor.IsStatic)
        {
            sb.Append("instance ");
        }

        if (methodOrCtor is MethodInfo mInfo)
        {
            sb.Append(FormatType(mInfo.ReturnParameter.ParameterType));
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
        foreach (var parameter in methodOrCtor.GetParameters())
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

    private static string FormatBytes(byte[] bytes)
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
