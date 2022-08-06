namespace jl0pd.Reflection;

using System.Reflection;

public sealed class DecompiledMethod
{
    public DecompiledMethod(IReadOnlyList<Instruction> instructions, MethodInfo method)
    {
        Instructions = instructions;
        Method = method;
    }

    public IReadOnlyList<Instruction> Instructions { get; }
    public MethodInfo Method { get; }
}
