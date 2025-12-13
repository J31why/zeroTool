using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.RegularExpressions;

namespace CalmareEncoder.Calmare.Common;

public partial class Menu : CalmareContentFunc
{
    private string[] Param { get; } = new string[4];
    protected override byte OpCode => 0x5E;
    private static readonly Regex MenuReg = MenuRegex();

    protected override string Pattern =>
        """
        Menu (.*?) (.*?) (.*?) (.*?)$\n|"(.*?)"
        """;

    public new static bool TryParse(string text, [MaybeNullWhen(false)] out CalmareFunc result)
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
            op.Param[i] = matches[0].Groups[i + 1].Value;
        for (var i = 1; i < matches.Count; i++)
            op.Content.Add(matches[i].Groups[5].Value);
        result = op;
        return true;
    }

    public override byte[] Encode(Encoding encoding)
    {
        List<byte> bytes = new(0x100) { OpCode };
        var menuId = MenuReg.Match(Param[0]).Groups[1].Value;
        var num = BitConverter.GetBytes(Convert.ToUInt16(menuId));
        bytes.AddRange(num);
        num = BitConverter.GetBytes(Convert.ToInt16(Param[1]));
        bytes.AddRange(num);
        num = BitConverter.GetBytes(Convert.ToInt16(Param[2]));
        bytes.AddRange(num);
        bytes.AddRange(Convert.ToByte(Param[3]));
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