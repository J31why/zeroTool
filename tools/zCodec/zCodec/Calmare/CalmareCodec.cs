#region

using System.Text;
using System.Text.RegularExpressions;
using Common;
using Extensions;
using zCodec.Calmare.Opcodes;

#endregion

namespace zCodec.Calmare;

public partial class CalmareCodec
{
    public static readonly Dictionary<string, string> RemapChars = new()
    {
        ["・"] = "丄",
        ["♪"] = "丅",
        ["⑪"] = "丆"
    };
    public static string Remap(string str) => RemapChars.Aggregate(str, (current, ch) => current.Replace(ch.Key, ch.Value));

    public void ParseFromFile(string file)
    {
        FileName = Path.GetFileName(file);
        var clmText = File.ReadAllText(file);
        Parse(clmText);
    }

    private void Parse(string clmText)
    {
        UsingText = clmText;
        var matches = FnRegex().Matches(clmText);
        Functions.AddRange(matches.Select(x => x.Value));
        matches = NpcNameStringRegex().Matches(clmText);
        NpcNameStrings.AddRange(matches.Select(x => x.Value));
        matches = LabelNameStringRegex().Matches(clmText);
        LabelNameStrings.AddRange(matches.Select(x => x.Value));
        BattleCount = BattleRegex().Count(clmText);
        ParseText();
    }

 
    private void ParseText()
    {
        for (var index = 0; index < Functions.Count; index++)
        {
            var func = Functions[index];
            var matches = FnTextRegex().Matches(func);
            if (matches.Count == 0)
                continue;
            FnTexts.Add((index,
                matches.Select(x =>
                        ScenaOpcode.TryParse(x.Value, out var opcode)
                            ? opcode
                            : throw new Exception($"Unknown func: {x.Value}"))
                    .ToList()));
        }
    }

    public bool CompileToFile(string outPath, string calmareFile, Encoding encoding)
    {
        var holderText = ExtraEncoding.DoubleByteCharRegex().Replace(UsingText ?? throw new InvalidOperationException(), x =>
        {
            var value = x.Value;
            if (RemapChars.TryGetValue(value, out var c))
                value = c;
            var count = encoding.GetByteCount(value);
            return count switch
            {
                2 => "果",
                _ => throw new Exception($"{Path.GetFileName(outPath)}非法字节：{x.Value}({count})")
            };
        });
        var holderCodec = new CalmareCodec();
        holderCodec.Parse(holderText);
        if (holderCodec.FnTexts.Count != FnTexts.Count ||
            holderCodec.NpcNameStrings.Count != NpcNameStrings.Count ||
            holderCodec.BattleCount != BattleCount || holderCodec.LabelNameStrings.Count != LabelNameStrings.Count)
            return false;
        File.WriteAllText(outPath, holderText.Replace("\r",""));
        var success = Utils.RunExe(calmareFile, $"\"{outPath}\"", 2);
        if (!success)
            return false;
        var binFile = Path.Combine(
            Path.GetDirectoryName(outPath) ?? throw new DirectoryNotFoundException(),
            Path.GetFileNameWithoutExtension(outPath) + ".bin");
        if (!File.Exists(binFile))
            return false;
        var binData = File.ReadAllBytes(binFile);
        using var br = new BinaryReader(new MemoryStream(binData));
        ReplaceFns(ref binData, br, holderCodec, encoding);
        ReplaceNames(ref binData, br, holderCodec, encoding);

        if(Path.GetFileNameWithoutExtension(outPath) == "t4030")
        {
            binData[0xb8] = 0xe5;
        }
        
        File.WriteAllBytes(binFile, binData);
        File.Delete(outPath);
        return true;
    }

    private void ReplaceNames(ref byte[] binData, BinaryReader br, CalmareCodec holderCodec, Encoding encoding)
    {
        br.BaseStream.Seek(0x34, SeekOrigin.Begin);
        var pString = br.ReadInt32();
        br.BaseStream.Seek(pString, SeekOrigin.Begin);
        var strings = new Queue<string>();
        while (br.BaseStream.Position < br.BaseStream.Length)
        {
            var str = br.ReadClmString(ExtraEncoding.SJIS) ?? throw new Exception("读取文本失败");
            strings.Enqueue(str);
        }

        strings.Dequeue();
        replace(ref binData, NpcNameStrings, holderCodec.NpcNameStrings);
        for (var i = 0; i < BattleCount; i++)
            strings.Dequeue();
        replace(ref binData, LabelNameStrings, holderCodec.LabelNameStrings);

        void replace(ref byte[] binData, List<string> list, List<string> holderList)
        {
            for (var index = 0; index < list.Count; index++)
            {
                var holderStr = holderList[index];
                var replaceStr = list[index];
                var binStr = strings.Dequeue();
                if (holderStr != binStr)
                    throw new Exception($"未找到NameString[{index}]: {replaceStr}");
                byte[] sjisBytes = [..ExtraEncoding.SJIS.GetBytes(holderStr), 0];
                byte[] replaceBytes = [..ClmStringToBytes(replaceStr,encoding), 0];
                var result = BitHelper.Replace(binData, sjisBytes, replaceBytes, pString, (int)br.BaseStream.Length, 1);
                if (!result.replaced)
                    throw new Exception($"未找到NameString[{index}]: {replaceStr}");
                binData = result.result;
            }
        }
    }

    private void ReplaceFns(ref byte[] binData, BinaryReader br, CalmareCodec holderCodec, Encoding encoding)
    {
        br.BaseStream.Seek(0x42, SeekOrigin.Begin);
        var pFunc = br.ReadUInt16();
        var nFunc = br.ReadUInt16() / 4;
        var pFunctions = new uint[nFunc];
        br.BaseStream.Seek(pFunc, SeekOrigin.Begin);
        for (var i = 0; i < nFunc; i++)
            pFunctions[i] = br.ReadUInt32();
        for (var i = 0; i < holderCodec.FnTexts.Count; i++)
        {
            var holderFnText = holderCodec.FnTexts[i];
            var fnText = FnTexts[i];
            var start = (int)pFunctions[holderFnText.index];
            var end = holderFnText.index + 1 <= pFunctions.Length - 1 ? (int)pFunctions[holderFnText.index + 1] : -1;

            for (var j = 0; j < holderFnText.func.Count; j++)
            {
                var sjisBytes = holderFnText.func[j].Compile(ExtraEncoding.SJIS);
                var replaceBytes = fnText.func[j].Compile(encoding);
                if (sjisBytes.Length != replaceBytes.Length)
                    throw new Exception("字节长度不一致");
                var result = BitHelper.Replace(binData, sjisBytes, replaceBytes, start, end, 1);
                if (!result.replaced)
                    throw new Exception($"{FileName}未找到Fn文本：{fnText.func[j].RawText}");
                binData = result.result;
            }
        }
    }
    public static byte[] ClmStringToBytes(string text, Encoding encoding)
    {
        if (encoding.CodePage == 936)
            text = Remap(text);
        var texts = ContentSplitRegex().Matches(text.Replace("\r", "")).Select(x => x.Value);
        var bytes = new List<byte>(0x200);
        foreach (var str in texts)
            bytes.AddRange(str switch
            {
                "\n" => [1],
                "{}" => [],
                "{wait}" => [2],
                _ when str.StartsWith("{color") => [07, Convert.ToByte(str[7..^1])],
                _ when str.StartsWith("{item[") =>
                    [0x1F, ..BitConverter.GetBytes(Convert.ToUInt16(str[6..^2]))],
                _ when str.StartsWith("{0x") =>
                    [Convert.ToByte(str[3..^1], 16)],
                _ => encoding.GetBytes(str)
            });
        return bytes.ToArray();
    }

    public static string BytesToClmString(byte[] bytes, Encoding encoding)
    {
        var sb = new  StringBuilder();
        var temp = new List<byte>(0x100);
        for (var index = 0; index < bytes.Length; index++)
        {
            var b = bytes[index];
            if (b > 0x1f)
            {
                temp.Add(b);
                continue;
            }
            if (temp.Count > 0)
            {
                sb.Append(encoding.GetString(temp.ToArray()));
                temp.Clear();
            }
            switch (b)
            {
                case 0:
                    throw new ArgumentException();
                case 1:
                    sb.Append('\n');
                    break;
                case 2:
                    sb.Append("{wait}");
                    break;
                case 7:
                    b = bytes[++index];
                    sb.Append($"{{color {b}}}");
                    break;
                case 0x1f:
                    var id = BitConverter.ToUInt16([bytes[++index], bytes[++index]]);
                    sb.Append($"{{item[{id}]}}");
                    break;
                default:
                    sb.Append($"{{0x{b:X2}}}");
                    break;
            }
        }

        if (temp.Count > 0)
        {
            sb.Append(encoding.GetString(temp.ToArray()));
            temp.Clear();
        }
     
        return sb.ToString();
    }
    
    public string? FileName;
    public string? UsingText;
    public readonly List<string> NpcNameStrings = new(100);
    public int BattleCount;
    public readonly List<string> LabelNameStrings = new(100);
    public readonly List<(int index, List<ScenaOpcode> func)> FnTexts = [];
    public readonly List<string> Functions = new(100);


    
    [GeneratedRegex(@"\d+", RegexOptions.Compiled | RegexOptions.Multiline)]
    public static partial Regex NumRegex();
    [GeneratedRegex("""\n|\{[\s\S]*?\}|[\s\S]+?(?=$|\n|{)""", RegexOptions.Compiled | RegexOptions.Multiline)]
    public static partial Regex ContentSplitRegex();
    [GeneratedRegex("""(?<=^npc.*?:\n\tname ").*?(?="$)""", RegexOptions.Multiline)]
    public static partial Regex NpcNameStringRegex();

    [GeneratedRegex("""(?<=^label.*?:\n\tname ").*?(?="$)""", RegexOptions.Multiline)]
    public static partial Regex LabelNameStringRegex();

    [GeneratedRegex("""^battle\[\d+\]:$""", RegexOptions.Multiline)]
    public static partial Regex BattleRegex();

    [GeneratedRegex("""fn\[\d+\]:$[\s\S]*?(?=\nfn|\z)""", RegexOptions.Multiline)]
    public static partial Regex FnRegex();

    //menu: c133b
    [GeneratedRegex("""
                    \t+TextSetName ".+?"$|\t+(?:TextMessage|TextTalk |TextTalkNamed).*?{$[\s\S]*?\n\t+}$|\t+Menu .*?$[\s\S]*?(?=\n(?!\t+"))|\t+ED7MenuAdd.*?$|\t+ScMenuSetTitle.*?$|\t+CharSetName.*?$
                    """, RegexOptions.Compiled | RegexOptions.Multiline)]
    public static partial Regex FnTextRegex();
}