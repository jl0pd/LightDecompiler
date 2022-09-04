using System.Reflection.Emit;

namespace jl0pd.Reflection.LightDecompiler.Tests;

public class DecompilerTests
{
    [Fact]
    public void TestDecompilationOfMethodWithGenericTypeUsedInInstruction()
    {
        var method = typeof(MethodHolder).GetMethod(nameof(MethodHolder.Box))!;
        var instructions = RuntimeDecompiler.Decompile(method);

        Assert.Equal(3, instructions.Count);

        Assert.Equal(OpCodes.Ldarg_1, instructions[0].OpCode);

        Assert.Equal(OpCodes.Box, instructions[1].OpCode);
        Assert.Equal(method.GetGenericArguments()[0], instructions[1].Operand);

        Assert.Equal(OpCodes.Ret, instructions[2].OpCode);
    }
}
