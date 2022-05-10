namespace jl0pd.Reflection;

using System.Buffers.Binary;
using System.Reflection;
using System.Reflection.Emit;

public static class LightDecompiler
{
    public static void PrintInstructions(MethodInfo methodInfo)
        => WriteInstructions(methodInfo, Console.Out);

    public static void PrintInstructions(ReadOnlySpan<byte> il)
        => WriteInstructions(il, Console.Out);

    public static void WriteInstructions(MethodInfo methodInfo, TextWriter writer)
    {
        if (!methodInfo.MethodImplementationFlags.HasFlag(MethodImplAttributes.IL))
        {
            throw new ArgumentException("Method doesn't implemented with IL", nameof(methodInfo));
        }

        var body = methodInfo.GetMethodBody();
        if (body is null)
        {
            throw new ArgumentException("Cannot get method body", nameof(methodInfo));
        }

        var il = body.GetILAsByteArray();
        if (il is null)
        {
            throw new ArgumentException("Cannot get IL byte array", nameof(methodInfo));
        }

        WriteInstructions(il, writer);
    }

    public static void WriteInstructions(ReadOnlySpan<byte> il, TextWriter writer)
    {
        for (int i = 0; i < il.Length;)
        {
            short id = il[i];
            if (IsPrefix(id))
            {
                id = BinaryPrimitives.ReadInt16BigEndian(il[i..]);
                i += 2;
            }
            else
            {
                i++;
            }

            var opCode = s_instructions[id];
            i += OperandSize(opCode.OperandType);

            writer.WriteLine(opCode);
        }
    }

    private static int OperandSize(OperandType operandType)
    {
        return operandType switch
        {
            OperandType.InlineBrTarget => 4,
            OperandType.InlineField => 4,
            OperandType.InlineI => 4,
            OperandType.InlineI8 => 8,
            OperandType.InlineMethod => 4,
            OperandType.InlineNone => 0,
            OperandType.InlineR => 8,
            OperandType.InlineSig => 4,
            OperandType.InlineString => 4,
            OperandType.InlineSwitch => 4,
            OperandType.InlineTok => 4,
            OperandType.InlineType => 4,
            OperandType.InlineVar => 2,
            OperandType.ShortInlineBrTarget => 1,
            OperandType.ShortInlineI => 1,
            OperandType.ShortInlineR => 4,
            OperandType.ShortInlineVar => 1,
            //OperandType.InlinePhi => throw new NotImplementedException(), // reserved in spec
            _ => throw new NotSupportedException(),
        };
    }

    private static bool IsPrefix(short id)
    {
        return s_instructions[id].Name?.StartsWith("prefix") ?? false;
    }

    static LightDecompiler()
    {
        var fields = typeof(OpCodes).GetFields();
        foreach (var opCodeField in fields)
        {
            var opCode = (OpCode)opCodeField.GetValue(null)!;
            s_instructions[opCode.Value] = opCode;
        }
    }

    private static Dictionary<short, OpCode> s_instructions = new();
}