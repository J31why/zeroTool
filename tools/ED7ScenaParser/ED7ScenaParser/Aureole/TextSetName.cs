using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace ED7ScenaParser.Aureole;

public class TextSetName : AureoleFunc
{
    private string? Name { get; set; }
    protected override byte OpCode => 0x61;

    protected override string Pattern =>
        """
        TextSetName "(.*?)"
        """;

    public new static bool TryParse(string text, [MaybeNullWhen(false)] out AureoleFunc result)
    {
        var op = new TextSetName();
        var matches = op.Matches(text);
        if (!op.IsMatch(text) || !matches[0].Value.StartsWith(nameof(TextSetName)))
        {
            result = null;
            return false;
        }

        op.RawText = text;
        op.IndentLevel = GetIndentLevel(text);
        op.Name = matches[0].Groups[1].Value;
        result = op;
        return true;
    }

    public override byte[] Encode(Encoding encoding)
    {
        ArgumentException.ThrowIfNullOrEmpty(Name);
        return [OpCode, ..ToBytes(Name, encoding)];
    }
}