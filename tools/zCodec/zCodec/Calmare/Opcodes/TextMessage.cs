using System.Diagnostics.CodeAnalysis;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Text;
using System.Text.RegularExpressions;

namespace zCodec.Calmare.Opcodes;

public partial class TextMessage : Opcode
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

    public TextMessage(string text) : base(0x55, text)
    {
        var matches = OpReg.Matches(text);
        Param.Add(matches[0].Groups[1].Value);
        Param.AddRange(matches.Skip(1).Select(x => TrimContent(x.Value)));
    }

    public static bool TryParse(string text, [MaybeNullWhen(false)] out Opcode result)
    {
        if (!OpReg.IsMatch(text) || !text.Contains($"\t{nameof(TextMessage)} "))
        {
            result = null;
            return false;
        }

        var op = new TextMessage(text);
        result = op;
        return true;
    }

    private static Regex OpReg { get; } = OpRegex();

    [GeneratedRegex("""\tTextMessage (.*?) |(?<={\n)[\s\S]+?(?=\n\t+})""",
        RegexOptions.Compiled | RegexOptions.Multiline)]
    private static partial Regex OpRegex();
}