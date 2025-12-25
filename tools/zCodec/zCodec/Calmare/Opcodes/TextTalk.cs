using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.RegularExpressions;

namespace zCodec.Calmare.Opcodes;

public partial class TextTalk : Opcode
{
    public override byte[] Encode(Encoding encoding)
    {
        List<byte> bytes = new(0x200) { Code };
        bytes.AddRange(EncodeCid(Param[0]));
        for (var index = 1; index < Param.Count; index++)
        {
            if (index > 1)
                bytes.Add(3);
            var text = Param[index];
            bytes.AddRange(EncodeString(text, encoding));
        }

        bytes.Add(0);
        return bytes.ToArray();
    }

    public TextTalk(string text) : base(0x5c, text)
    {
        var matches = OpReg.Matches(text);
        Param.Add(matches[0].Groups[1].Value);
        Param.AddRange(matches.Skip(1).Select(x => TrimContent(x.Value)));
    }

    public static bool TryParse(string text, [MaybeNullWhen(false)] out Opcode result)
    {
        if (!OpReg.IsMatch(text) || !text.Contains($"\t{nameof(TextTalk)} "))
        {
            result = null;
            return false;
        }

        var op = new TextTalk(text);
        result = op;
        return true;
    }

    private static Regex OpReg { get; } = OpRegex();

    [GeneratedRegex("""\tTextTalk (.*?) |(?<={\n)[\s\S]+?(?=\n\t+})""", RegexOptions.Compiled | RegexOptions.Multiline)]
    private static partial Regex OpRegex();
}