using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace CalmareEncoder.Calmare.Common;

public class TextTalkNamed : CalmareContentFunc
{
    private string? Cid { get; set; }
    private string? Name { get; set; }
    protected override byte OpCode => 0x5D;

    protected override string Pattern =>
        """
        TextTalkNamed (.*?) \"(.*?)\" |{$\n([\s\S]*?)\n\t+}
        """;

    public new static bool TryParse(string text, [MaybeNullWhen(false)] out CalmareFunc result)
    {
        var op = new TextTalkNamed();
        var matches = op.Matches(text);
        if (!op.IsMatch(text) || !matches[0].Value.StartsWith(nameof(TextTalkNamed)))
        {
            result = null;
            return false;
        }

        op.RawText = text;
        op.IndentLevel = GetIndentLevel(text);
        op.Cid = matches[0].Groups[1].Value;
        op.Name = matches[0].Groups[2].Value;
        for (var i = 1; i < matches.Count; i++)
        {
            var value = matches[i].Groups[3].Value;
            value = ContentTrim(value, op.IndentLevel);
            op.Content.Add(value);
        }

        result = op;
        return true;
    }

    public override byte[] Encode(Encoding encoding)
    {
        ArgumentException.ThrowIfNullOrEmpty(Cid);
        if (Name == null)
            throw new InvalidOperationException("Name is not set");
        List<byte> bytes = new(0x100) { OpCode };
        bytes.AddRange(CidToBytes(Cid));
        bytes.AddRange(encoding.GetBytes(Name));
        bytes.Add(0);
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