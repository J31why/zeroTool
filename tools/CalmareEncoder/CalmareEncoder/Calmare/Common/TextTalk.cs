using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace CalmareEncoder.Calmare.Common;

public class TextTalk : CalmareContentFunc
{
    private string? Cid { get; set; }
    protected override byte OpCode => 0x5C;

    protected override string Pattern =>
        """
        TextTalk (.*?) |{$\n([\s\S]*?)\n\t+}
        """;

    public new static bool TryParse(string text, [MaybeNullWhen(false)] out CalmareFunc result)
    {
        var op = new TextTalk();
        var matches = op.Matches(text);
        if (!op.IsMatch(text) || !matches[0].Value.StartsWith(nameof(TextTalk)))
        {
            result = null;
            return false;
        }

        op.RawText = text;
        op.IndentLevel = GetIndentLevel(text);
        op.Cid = matches[0].Groups[1].Value;
        for (var i = 1; i < matches.Count; i++)
        {
            var value = matches[i].Groups[2].Value;
            value = ContentTrim(value, op.IndentLevel);
            op.Content.Add(value);
        }

        result = op;
        return true;
    }

    public override byte[] Encode(Encoding encoding)
    {
        if (Cid == null)
            throw new InvalidOperationException("Name is not set");
        List<byte> bytes = new(0x100) { OpCode };
        bytes.AddRange(CidToBytes(Cid));

        for (var index = 0; index < Content.Count; index++)
        {
            if (index > 0)
                bytes.Add(3);
            var text = Content[index];
            bytes.AddRange(ToBytes(text, encoding));
        }

        bytes.Add(0);
        return bytes.ToArray();
    }
}