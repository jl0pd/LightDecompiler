namespace jl0pd.Reflection.LightDecompiler.Tests;

using static jl0pd.Reflection.LightDecompiler.Tests.Constants;

public class MethodFormatTests
{
    const string TypeName = nameof(MethodHolder);
    const string TypeFullName = AssemblyName + BaseName + "." + nameof(MethodHolder);

    [Fact]
    public void TestSimpleMethod()
    {
        var method = typeof(MethodHolder).GetMethod(nameof(MethodHolder.Simple))!;
        var result = MsilInstructionFormatter.FormatMethodOrCtor(method);

        Assert.Equal($"float32 {TypeFullName}::Simple(int32)", result);
    }
    
    [Fact]
    public void TestInstanceMethod()
    {
        var method = typeof(MethodHolder).GetMethod(nameof(MethodHolder.Instance))!;
        var result = MsilInstructionFormatter.FormatMethodOrCtor(method);

        Assert.Equal($"instance void {TypeFullName}::Instance()", result);
    }

    [Fact]
    public void TestGenericMethod()
    {
        var method = typeof(MethodHolder).GetMethod(nameof(MethodHolder.Generic))!.MakeGenericMethod(typeof(int));
        var result = MsilInstructionFormatter.FormatMethodOrCtor(method);

        Assert.Equal($"!!0 {TypeFullName}::Generic<int32>(!!0)", result);
    }
}
