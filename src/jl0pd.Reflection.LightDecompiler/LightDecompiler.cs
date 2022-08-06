namespace jl0pd.Reflection;

using System.Buffers.Binary;
using System.Reflection;
using System.Reflection.Emit;

public static class LightDecompiler
{
    public static DecompiledMethod Decompile(MethodInfo methodInfo)
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

        var instructions = Decompile(il, methodInfo.Module);

        return new DecompiledMethod(instructions, methodInfo);
    }

    public static IReadOnlyList<Instruction> Decompile(ReadOnlySpan<byte> il, Module module)
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

        return instructions;
    }

    private static object? ReadOperand(ReadOnlySpan<byte> span, int offset, OperandType operandType, Module module, out int size)
    {
        size = OperandSize(operandType);

        return operandType switch
        {
            OperandType.InlineBrTarget => offset + size + BinaryPrimitives.ReadInt32LittleEndian(span),
            OperandType.InlineI or
            OperandType.InlineSwitch => BinaryPrimitives.ReadInt32LittleEndian(span),
            OperandType.InlineI8 => BinaryPrimitives.ReadInt64LittleEndian(span),
            OperandType.InlineMethod or
            OperandType.InlineTok or
            OperandType.InlineType or
            OperandType.InlineField => module.ResolveMember(BinaryPrimitives.ReadInt32LittleEndian(span)),
            OperandType.InlineSig => module.ResolveSignature(BinaryPrimitives.ReadInt32LittleEndian(span)),
            OperandType.InlineString => module.ResolveString(BinaryPrimitives.ReadInt32LittleEndian(span)),
            OperandType.InlineNone => null,
            OperandType.InlineR => BinaryPrimitives.ReadDoubleLittleEndian(span),
            OperandType.InlineVar => BinaryPrimitives.ReadInt16LittleEndian(span),
            OperandType.ShortInlineBrTarget => (byte)(span[0] + offset + size),
            OperandType.ShortInlineVar or
            OperandType.ShortInlineI => span[0],
            OperandType.ShortInlineR => BinaryPrimitives.ReadSingleLittleEndian(span),
            _ => throw new NotSupportedException(),
        };
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