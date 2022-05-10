# LightDecompiler

~~Simple yet powerful~~ Just simple decompiler for methods in runtime.
I use it when I need to see resulting IL when working with `System.Reflection.Emit`
and unable to dump code into assembly or want to have quick feedback on code changes.

## Usage

Given `System.Reflection.MethodInfo` just pass it into `jl0pd.Reflection.LightDecompiler.PrintInstructions`

```csharp
using System.Reflection;

MethodInfo equals = typeof(object).GetMethod("Equals", BindingFlags.Public | BindingFlags.Static);
jl0pd.Reflection.LightDecompiler.PrintInstructions(equals);
```

Result:

```msil
00:  ldarg.0
01:  ldarg.1
02:  bne.un.s '6'
04:  ldc.i4.1
05:  ret
06:  ldarg.0
07:  brfalse.s '12'
09:  ldarg.1
10:  brtrue.s '14'
12:  ldc.i4.0
13:  ret
14:  ldarg.0
15:  ldarg.1
16:  callvirt 'Boolean Equals(System.Object)'
21:  ret
```
