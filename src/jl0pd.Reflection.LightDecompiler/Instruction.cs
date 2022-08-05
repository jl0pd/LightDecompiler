namespace jl0pd.Reflection;
using System.Reflection.Emit;

public sealed record Instruction(int Offset, OpCode OpCode, object? Operand);
