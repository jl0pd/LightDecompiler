namespace jl0pd.Reflection;

public sealed record MsilInstructionFormatterOptions()
{
    public bool SeparateJumps { get; init; } = true;

    public static MsilInstructionFormatterOptions Default { get; } = new();
}
