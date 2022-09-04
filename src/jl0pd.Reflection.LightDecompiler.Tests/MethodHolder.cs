namespace jl0pd.Reflection.LightDecompiler.Tests;

// these types should be kept in sync and have same amout of tests

internal class MethodHolder
{
    public static T Generic<T>(T value) => value;

    public static float Simple(int x) => x;

    public void Instance() { }

    public object? Box<T>(T value) => value;
}

internal class MethodHolder<TClass>
{
    public static TMethod Generic<TMethod>(TMethod value, TClass @class) => value;

    public static float Simple(int x) => x;

    public void Instance() { }
 
    public object? Box<T>(T value) => value;
}
