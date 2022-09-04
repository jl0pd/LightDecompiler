namespace jl0pd.Reflection.LightDecompiler.Tests;

public class TypeFormatTests
{
    const string TypeName = nameof(MethodHolder);
    const string TypeFullName = AssemblyName + BaseName + "." + nameof(MethodHolder);

    [Fact]
    public void TestSimpleType()
    {
        var result = ReferenceFormatter.FormatType(typeof(MethodHolder));
        Assert.Equal(TypeFullName, result);
    }
}
