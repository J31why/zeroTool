using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.RegularExpressions;

namespace ED7ScenaParser.Aureole;

public partial class Menu : AureoleContentFunc
{
    private string[] param { get; set; } = new string[4];
    protected override byte OpCode => 0x5E;

    protected override string Pattern =>
        """
        Menu (.*?) (.*?) (.*?) (.*?)$\n|"(.*?)"
        """;

    public new static bool TryParse(string text, [MaybeNullWhen(false)] out AureoleFunc result)
    {
        var op = new Menu();
        var matches = op.Matches(text);
        if (!op.IsMatch(text) || !matches[0].Value.StartsWith(nameof(Menu)))
        {
            result = null;
            return false;
        }

        op.RawText = text;
        op.IndentLevel = GetIndentLevel(text);
        for (var i = 0; i < 4; i++)
            op.param[i] = matches[0].Groups[i + 1].Value;
        for (var i = 1; i < matches.Count; i++)
            op.Content.Add(matches[i].Groups[5].Value);
        result = op;
        return true;
    }

    public override byte[] Encode(Encoding encoding)
    {
        List<byte> bytes = new(0x100) { OpCode };
        var menuId = MenuRegex().Match(param[0]).Groups[1].Value;
        var num = BitConverter.GetBytes(Convert.ToUInt16(menuId));
        bytes.AddRange(num);
        num = BitConverter.GetBytes(Convert.ToInt16(param[1]));
        bytes.AddRange(num);
        num = BitConverter.GetBytes(Convert.ToInt16(param[2]));
        bytes.AddRange(num);
        bytes.AddRange(Convert.ToByte(param[3]));
        for (var index = 0; index < Content.Count; index++)
        {
            var text = Content[index];
            bytes.AddRange([..ToBytes(text, encoding), 1]);
        }

        bytes.Add(0);
        return bytes.ToArray();
    }

    [GeneratedRegex("menu\\[(\\d+)\\]")]
    private static partial Regex MenuRegex();
}