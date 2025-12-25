using System.Text;
using System.Text.RegularExpressions;

namespace zCodec.Calmare.Opcodes;

public abstract partial class Opcode
{
    public byte Code { get; }
    public List<string> Param { get; } = new(20);
    public string RawText { get; }
    public int IndentLevel { get; }

    public abstract byte[] Encode(Encoding encoding);

    public Opcode(byte code, string text)
    {
        Code = code;
        RawText = text;
        IndentLevel = text.TakeWhile(x => x == '\t').Count();
    }

    public static byte[] EncodeCid(string text)
    {
        switch (text)
        {
            case "null":
                return [0xff, 0];
            case "self":
                return [0xfe, 0];
        }

        if (text.StartsWith("name"))
        {
            var id = Convert.ToUInt16(NumReg.Match(text).Value) + 0x101;
            return BitConverter.GetBytes((ushort)id);
        }

        if (text.StartsWith("char"))
        {
            var id = Convert.ToUInt16(NumReg.Match(text).Value) + 0x8;
            return BitConverter.GetBytes((ushort)id);
        }

        if (text.StartsWith("field_party"))
        {
            var id = Convert.ToUInt16(NumReg.Match(text).Value);
            return BitConverter.GetBytes(id);
        }

        throw new ArgumentException($"error cid : {text}");
    }

    public static byte[] EncodeString(string text, Encoding encoding)
    {
        var texts = ContentSplitReg.Matches(text).Select(x => x.Value);
        var bytes = new List<byte>(0x200);
        foreach (var str in texts)
            if (str == "\n")
            {
                bytes.Add(1);
            }
            else if (str == "{}")
            {
            }
            else if (str == "{wait}")
            {
                bytes.Add(2);
            }
            else if (str.StartsWith("{color"))
            {
                bytes.AddRange([07, Convert.ToByte(NumReg.Match(str[6..]).Value)]);
            }
            else if (str.StartsWith("{item["))
            {
                bytes.AddRange([0x1F, ..BitConverter.GetBytes(Convert.ToUInt16(NumReg.Match(str[6..]).Value))]);
            }
            else if (str.StartsWith("{0x"))
            {
                bytes.Add(Convert.ToByte(NumReg.Match(str[3..]).Value, 16));
            }
            else
            {
                bytes.AddRange(encoding.GetBytes(str));
            }

        return bytes.ToArray();
    }

    protected string TrimContent(string content)
    {
        if (string.IsNullOrEmpty(content))
            return string.Empty;
        var indentLevel = IndentLevel + 1;
        var sb = new StringBuilder();
        var tabs = new string('\t', indentLevel);
        using var reader = new StringReader(content);
        while (reader.ReadLine() is { } line)
            if (line.StartsWith(tabs))
                sb.AppendLine(line[indentLevel..]);
            else if (line == "")
                sb.AppendLine("");
            else
                throw new Exception("unexpected line in calmare");
        if (sb.Length > 0 && sb[^1] == '\n')
            sb.Length--;
        return sb.ToString().Replace("\r", "");
    }


    private static Regex ContentSplitReg { get; } = ContentSplitRegex();
    protected static Regex NumReg { get; } = NumRegex();

    [GeneratedRegex("""\n|\{[\s\S]*?\}|[\s\S]+?(?=$|\n|{)""", RegexOptions.Compiled | RegexOptions.Multiline)]
    private static partial Regex ContentSplitRegex();

    [GeneratedRegex("""\d+""", RegexOptions.Compiled | RegexOptions.Multiline)]
    private static partial Regex NumRegex();
}