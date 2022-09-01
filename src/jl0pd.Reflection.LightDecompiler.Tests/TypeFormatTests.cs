namespace jl0pd.Reflection.LightDecompiler.Tests;

using static jl0pd.Reflection.LightDecompiler.Tests.Constants;

public class TypeFormatTests
{
    const string TypeName = nameof(MethodHolder);
    const string TypeFullName = AssemblyName + BaseName + "." + nameof(MethodHolder);

    [Fact]
    public void TestSimpleType()
    {
        var result = MsilInstructionFormatter.FormatType(typeof(MethodHolder));
        Assert.Equal(TypeFullName, result);
    }
}
