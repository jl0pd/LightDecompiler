namespace jl0pd.Reflection;

using System.Buffers.Binary;
using System.Globalization;
using System.Reflection;
using System.Reflection.Emit;

internal sealed record Instruction(int Offset, OpCode OpCode, object? Operand);

public static class LightDecompiler
{
    public static void PrintInstructions(MethodInfo methodInfo)
        => WriteInstructions(methodInfo, Console.Out);

    public static void PrintInstructions(ReadOnlySpan<byte> il, Module module)
        => WriteInstructions(il, module, Console.Out);

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

        WriteInstructions(il, methodInfo.Module, writer);
    }

    public static void WriteInstructions(ReadOnlySpan<byte> il, Module module, TextWriter writer)
    {
        var instructions = new List<Instruction>();
        for (int i = 0; i < il.Length;)
        {
            int offset = i;

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
            var operand = ReadOperand(il[i..], i, opCode.OperandType, module, out int size);
            i += size;

            instructions.Add(new Instruction(offset, opCode, operand));
        }

        FormatInstructions(instructions, writer);
    }

    private static void FormatInstructions(List<Instruction> instructions, TextWriter writer)
    {
        int maxSize = instructions.Select(i => i.Offset.ToString(CultureInfo.InvariantCulture).Length).Max();

        var format3 = "{0:D" + maxSize.ToString(CultureInfo.InvariantCulture) + "}:  {1} '{2}'";
        var format2 = "{0:D" + maxSize.ToString(CultureInfo.InvariantCulture) + "}:  {1}";

        foreach (var instruction in instructions)
        {
            if (instruction.Operand is { })
            {
                writer.WriteLine(format3, instruction.Offset, instruction.OpCode, instruction.Operand);
            }
            else
            {
                writer.WriteLine(format2, instruction.Offset, instruction.OpCode);
            }
        }
    }

    private static object? ReadOperand(ReadOnlySpan<byte> span, int offset, OperandType operandType, Module module, out int size)
    {
        size = OperandSize(operandType);

        switch (operandType)
        {
            case OperandType.InlineBrTarget:
                return offset + size + BinaryPrimitives.ReadInt32BigEndian(span);
            case OperandType.InlineI:
            case OperandType.InlineSwitch:
                return BinaryPrimitives.ReadInt32BigEndian(span);

            case OperandType.InlineI8:
                return BinaryPrimitives.ReadInt64BigEndian(span);

            case OperandType.InlineMethod:
            case OperandType.InlineTok:
            case OperandType.InlineType:
            case OperandType.InlineField:
                return module.ResolveMember(BinaryPrimitives.ReadInt32LittleEndian(span));

            case OperandType.InlineSig:
                return module.ResolveSignature(BinaryPrimitives.ReadInt32LittleEndian(span));

            case OperandType.InlineString:
                return module.ResolveString(BinaryPrimitives.ReadInt32BigEndian(span));

            case OperandType.InlineNone:
                return null;

            case OperandType.InlineR:
                return BinaryPrimitives.ReadDoubleBigEndian(span);

            case OperandType.InlineVar:
                return BinaryPrimitives.ReadInt16LittleEndian(span);

            case OperandType.ShortInlineBrTarget:
                return span[0] + offset + size;

            case OperandType.ShortInlineVar:
            case OperandType.ShortInlineI:
                return span[0];

            case OperandType.ShortInlineR:
                return BinaryPrimitives.ReadSingleBigEndian(span);

            default:
                throw new NotSupportedException();
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