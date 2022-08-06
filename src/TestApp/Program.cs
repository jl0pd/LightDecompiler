using jl0pd.Reflection;
using System.Reflection;


var curMeth = MethodBase.GetCurrentMethod()!;
var ty = curMeth.DeclaringType!.GetTypeInfo();
var meth = ty.DeclaredMethods.First(m => m.Name.Contains(nameof(PrintHw)));

var instr = jl0pd.Reflection.LightDecompiler.Decompile(meth);
new MsilInstructionFormatter().Format(instr, Console.Out);

static void PrintHw()
{
    Console.WriteLine(ToString((object)"qwe"));
}

static string ToString<T>(T value)
{
    return value.ToString();
}