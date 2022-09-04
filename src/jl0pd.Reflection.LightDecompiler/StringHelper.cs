namespace jl0pd.Reflection;

using System;

internal static class StringHelper
{
    public static string Repeat(this string str, int count)
    {
        return string.Create(str.Length * count, (str, count), (span, state) =>
        {
            for (int i = 0; i < state.count; i++)
            {
                state.str.AsSpan().CopyTo(span[i..]);
            }
        });
    }
}
