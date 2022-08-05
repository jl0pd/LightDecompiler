namespace jl0pd.Reflection;

public interface IInstructionFormatter
{
    public void Format(IReadOnlyList<Instruction> instructions, TextWriter writer);
}
