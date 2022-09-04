namespace jl0pd.Reflection;

using System.Globalization;
using System.Reflection;
using System.Reflection.Emit;

public sealed class MsilInstructionFormatter
{
    public MsilInstructionFormatterOptions Options { get; set; } = MsilInstructionFormatterOptions.Default;

    public void Format(IReadOnlyList<Instruction> instructions, TextWriter writer)
    {
        Format(instructions, new TextBuilder(writer));
    }

    internal void Format(IReadOnlyList<Instruction> instructions, TextBuilder writer)
    {
        int maxSize = instructions.Select(i => i.Offset.ToString(CultureInfo.InvariantCulture).Length).Max();

        var format3 = "{0:D" + maxSize.ToString(CultureInfo.InvariantCulture) + "}: {1} {2}";
        var format2 = "{0:D" + maxSize.ToString(CultureInfo.InvariantCulture) + "}: {1}";

        foreach (var instruction in instructions)
        {
            if (instruction.Operand is { })
            {
                writer.AppendLine(string.Format(format3, instruction.Offset, instruction.OpCode, FormatOperand(instruction.Operand)));
            }
            else
            {
                writer.AppendLine(string.Format(format2, instruction.Offset, instruction.OpCode));
            }

            if (Options.SeparateJumps)
            {
                switch (instruction.OpCode.FlowControl)
                {
                    case FlowControl.Branch:
                    case FlowControl.Break:
                    case FlowControl.Cond_Branch:
                    case FlowControl.Meta:
                    case FlowControl.Return:
                    case FlowControl.Throw:
                        writer.AppendLine();
                        break;
                }
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
            byte[] bytes => ReferenceFormatter.FormatBytes(bytes),
            MethodBase methodOrCtor => ReferenceFormatter.FormatMethodOrCtor(methodOrCtor),
            Type type => ReferenceFormatter.FormatType(type),
            FieldInfo field => ReferenceFormatter.FormatField(field),
            _ => throw new NotImplementedException(),
        };
    }
}
