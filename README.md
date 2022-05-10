# LightDecompiler

Simple decompiler for methods in runtime

## Usage

```csharp
MethodInfo equals = typeof(System.Object).GetMethod("Equals");
jl0pd.Reflection.LightDecompiler.PrintInstructions(equals);
```
