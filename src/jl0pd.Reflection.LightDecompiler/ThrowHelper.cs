namespace jl0pd.Reflection;

using System;
using System.Diagnostics;

internal static class ThrowHelper
{
    public static Exception Unreachable
    {
        get
        {
            Debug.Assert(false);
            return new Exception("Execution is reached location that thought to be unreachable");
        }
    }
}
