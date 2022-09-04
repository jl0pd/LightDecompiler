namespace jl0pd.Reflection;

using System;
using System.Diagnostics;

[DebuggerDisplay("{_writer,nq}")]
internal sealed class TextBuilder
{
    private readonly string _newLine;
    private readonly TextWriter _writer;

    private int _indent = 0;
    private bool _doIndent = false;

    private readonly Dictionary<int, string> _indents = new();

    public TextBuilder(TextWriter sb, string newLine)
    {
        _writer = sb;
        _newLine = newLine;
    }

    public TextBuilder(TextWriter sb)
        : this(sb, Environment.NewLine)
    {
    }

    private string GetIndentString()
    {
        if (!_indents.TryGetValue(_indent, out string? value))
        {
            value = new string(' ', _indent * 4);
            _indents[_indent] = value;
        }

        return value;
    }

    private void WriteIndent()
    {
        if (_doIndent)
        {
            _writer.Write(GetIndentString());
        }
    }

    public void Append(char ch)
    {
        WriteIndent();
        _writer.Write(ch);
        _doIndent = false;
    }

    public void Append(string s)
    {
        WriteIndent();
        _writer.Write(s);
        _doIndent = false;
    }

    public void AppendLine(string s)
    {
        WriteIndent();
        _doIndent = true;
        _writer.Write(s);
        _writer.Write(_newLine);
    }

    public void AppendLine(char ch)
    {
        WriteIndent();
        _doIndent = true;
        _writer.Write(ch);
        _writer.Write(_newLine);
    }

    public void AppendLine()
    {
        WriteIndent();
        _doIndent = true;
        _writer.Write(_newLine);
    }

    public Indenter WithIndent() => new(this);

    public readonly struct Indenter : IDisposable
    {
        private readonly TextBuilder _builder;

        public Indenter(TextBuilder builder)
        {
            _builder = builder;
            _builder._indent++;
        }

        public void Dispose()
        {
            _builder._indent--;
        }
    }
}
