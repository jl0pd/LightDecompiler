using System.Reflection;

MethodInfo equals = typeof(object).GetMethod("Equals", BindingFlags.Public | BindingFlags.Static)!;
jl0pd.Reflection.LightDecompiler.PrintInstructions(equals);
