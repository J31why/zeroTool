using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.RegularExpressions;

namespace zCodec.Calmare.Opcodes;

public partial class Menu : Opcode
{
    public override byte[] Encode(Encoding encoding)
    {
        try
        {
            List<byte> bytes = new(0x200) { Code };
            var menu = NumReg.Match(Param[0]).Value;
            bytes.AddRange(BitConverter.GetBytes(Convert.ToUInt16(menu)));
            bytes.AddRange(BitConverter.GetBytes(Convert.ToInt16(Param[1])));
            bytes.AddRange(BitConverter.GetBytes(Convert.ToInt16(Param[2])));
            bytes.AddRange(Convert.ToByte(Param[3]));
            for (var i = 4; i < Param.Count; i++)
            {
                var text = Param[i];
                bytes.AddRange([..EncodeString(text, encoding), 1]);
            }

            bytes.Add(0);
            return bytes.ToArray();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public Menu(string text) : base(0x5E, text)
    {
        var matches = OpReg.Matches(text);
        Param.Add(matches[0].Groups[1].Value);
        Param.Add(matches[0].Groups[2].Value);
        Param.Add(matches[0].Groups[3].Value);
        Param.Add(matches[0].Groups[4].Value);
        Param.AddRange(matches.Skip(1).Select(x => x.Value));
    }

    public static bool TryParse(string text, [MaybeNullWhen(false)] out Opcode result)
    {
        if (!OpReg.IsMatch(text) || !text.Contains($"\t{nameof(Menu)} "))
        {
            result = null;
            return false;
        }

        var op = new Menu(text);
        result = op;
        return true;
    }

    private static Regex OpReg { get; } = OpRegex();

    [GeneratedRegex("""\tMenu ([\w\[\]]+) ([\d-]+) ([\d-]+) ([\d-]+)|(?<=").*?(?=")""",
        RegexOptions.Compiled | RegexOptions.Multiline)]
    private static partial Regex OpRegex();
}