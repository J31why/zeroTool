using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.RegularExpressions;

namespace CalmareEncoder.Calmare.Base;

public partial class ED7MenuAdd : CalmareContentFunc
{
    private string? Cid { get; set; }
    protected override byte OpCode => 0xCE;
    protected override string Pattern =>
        """
        ED7MenuAdd menu\[(\d+)\] "(.*?)"
        """;
    public new static bool TryParse(string text, [MaybeNullWhen(false)] out CalmareFunc result)
    {
        var op = new ED7MenuAdd();
        var match = op.Match(text);
        if (!op.IsMatch(text) || !match.Value.StartsWith(nameof(ED7MenuAdd)))
        {
            result = null;
            return false;
        }

        op.RawText = text;
        op.IndentLevel = GetIndentLevel(text);
        op.Cid = match.Groups[1].Value;
        op.Content.Add(match.Groups[2].Value);
        result = op;
        return true;
    }
    
    public override byte[] Encode(Encoding encoding)
    {
        if (string.IsNullOrEmpty(Cid) || Content[0] == null)
            throw new ArgumentNullException($"ED7MenuAdd encode error : \n{RawText}");
        List<byte> bytes = new(0x100) { OpCode, 1, byte.Parse(Cid) };
        bytes.AddRange(ToBytes(Content[0], encoding));
        bytes.Add(0);
        return bytes.ToArray();
    }
    
}