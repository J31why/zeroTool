using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.RegularExpressions;

namespace zCodec.Calmare.Opcodes;

public partial class TextTalkNamed : Opcode
{
    public override byte[] Encode(Encoding encoding)
    {
        List<byte> bytes = new(0x200) { Code };
        bytes.AddRange(EncodeCid(Param[0]));
        bytes.AddRange([..encoding.GetBytes(Param[1]), 0]);
        for (var index = 2; index < Param.Count; index++)
        {
            if (index > 2)
                bytes.Add(3);
            var text = Param[index];
            bytes.AddRange(EncodeString(text, encoding));
        }

        bytes.Add(0);
        return bytes.ToArray();
    }

    public TextTalkNamed(string text) : base(0x5d, text)
    {
        var matches = OpReg.Matches(text);
        Param.Add(matches[0].Groups[1].Value);
        Param.Add(matches[0].Groups[2].Value);
        Param.AddRange(matches.Skip(1).Select(x => TrimContent(x.Value)));
    }

    public static bool TryParse(string text, [MaybeNullWhen(false)] out Opcode result)
    {
        if (!OpReg.IsMatch(text) || !text.Contains($"\t{nameof(TextTalkNamed)} "))
        {
            result = null;
            return false;
        }

        var op = new TextTalkNamed(text);
        result = op;
        return true;
    }

    private static Regex OpReg { get; } = OpRegex();

    [GeneratedRegex("""\tTextTalkNamed (.*?) "(.*?)"|(?<={\n)[\s\S]+?(?=\n\t+})""",
        RegexOptions.Compiled | RegexOptions.Multiline)]
    private static partial Regex OpRegex();
}