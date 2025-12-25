using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.RegularExpressions;

namespace zCodec.Calmare.Opcodes;

public partial class ED7MenuAdd : Opcode
{
    public override byte[] Encode(Encoding encoding)
    {
        var menu = NumReg.Match(Param[0]).Value;
        List<byte> bytes = new(0x200) { Code, 1, Convert.ToByte(menu) };
        bytes.AddRange(EncodeString(Param[1], encoding));
        bytes.Add(0);
        return bytes.ToArray();
    }


    public ED7MenuAdd(string text) : base(0xCE, text)
    {
        var m = OpReg.Match(text);
        Param.Add(m.Groups[1].Value);
        Param.Add(m.Groups[2].Value);
    }

    public static bool TryParse(string text, [MaybeNullWhen(false)] out Opcode result)
    {
        if (!OpReg.IsMatch(text) || !text.Contains($"\t{nameof(ED7MenuAdd)} "))
        {
            result = null;
            return false;
        }

        var op = new ED7MenuAdd(text);
        result = op;
        return true;
    }

    private static Regex OpReg { get; } = OpRegex();

    [GeneratedRegex("""
                    \tED7MenuAdd (.*?) "(.*?)"
                    """, RegexOptions.Compiled | RegexOptions.Multiline)]
    private static partial Regex OpRegex();
}