using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.RegularExpressions;

namespace zCodec.Calmare.Opcodes;

public partial class TextSetName : Opcode
{
    public override byte[] Encode(Encoding encoding)
    {
        return [Code, ..encoding.GetBytes(Param[0]), 0];
    }

    public TextSetName(string text) : base(0x61, text)
    {
        var m = OpReg.Match(text);
        Param.Add(m.Groups[1].Value);
    }

    public static bool TryParse(string text, [MaybeNullWhen(false)] out Opcode result)
    {
        if (!OpReg.IsMatch(text) || !text.Contains($"\t{nameof(TextSetName)} "))
        {
            result = null;
            return false;
        }

        var op = new TextSetName(text);
        result = op;
        return true;
    }

    private static Regex OpReg { get; } = OpRegex();

    [GeneratedRegex("""
                    \tTextSetName "(.*?)"
                    """, RegexOptions.Compiled | RegexOptions.Multiline)]
    private static partial Regex OpRegex();
}