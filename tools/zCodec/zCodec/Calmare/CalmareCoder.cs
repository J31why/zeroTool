#region

using System.Text;
using System.Text.RegularExpressions;
using Common;
using Extensions;
using zCodec.Calmare.Opcodes;

#endregion

namespace zCodec.Calmare;

public partial class CalmareCoder
{
    public static readonly Dictionary<string, string> Remaps = new()
    {
        ["・"] = "丄",
        ["♪"] = "丅"
    };

    public void ParseFile(string file)
    {
        FileName = Path.GetFileName(file);
        var clmText = File.ReadAllText(file);
        Parse(clmText);
    }

    private void Parse(string clmText)
    {
        clmText = Remaps.Aggregate(clmText, (current, ch) => current.Replace(ch.Key, ch.Value));
        ParsingText = clmText;
        var matches = FnReg.Matches(clmText);
        Functions.AddRange(matches.Select(x => x.Value));
        matches = NpcNameStringReg.Matches(clmText);
        NpcNameStrings.AddRange(matches.Select(x => x.Value));
        matches = LabelNameStringReg.Matches(clmText);
        LabelNameStrings.AddRange(matches.Select(x => x.Value));
        BattleCount = BattleReg.Count(clmText);
        ParseText();
    }

    private void ParseText()
    {
        for (var index = 0; index < Functions.Count; index++)
        {
            var func = Functions[index];
            var matches = FnTextReg.Matches(func);
            if (matches.Count == 0)
                continue;
            FnTexts.Add((index,
                matches.Select(x =>
                        Opcode.TryParse(x.Value, out var opcode)
                            ? opcode
                            : throw new Exception($"Unknown func: {x.Value}"))
                    .ToList()));
        }
    }

    public bool Encode2File(string outPath, string calmareFile, Encoding encoding)
    {
        var holderText = ExtraEncoding.DoubleByteCharReg.Replace(ParsingText, x =>
        {
            var value = x.Value;
            if (Remaps.TryGetValue(value, out var c))
                value = c;
            var count = encoding.GetByteCount(value);
            return count switch
            {
                2 => "果",
                _ => throw new Exception($"{Path.GetFileName(outPath)}非法字节：{x.Value}({count})")
            };
        });
        var holderEncoder = new CalmareCoder();
        holderEncoder.Parse(holderText);
        if (holderEncoder.FnTexts.Count != FnTexts.Count ||
            holderEncoder.NpcNameStrings.Count != NpcNameStrings.Count ||
            holderEncoder.BattleCount != BattleCount || holderEncoder.LabelNameStrings.Count != LabelNameStrings.Count)
            return false;
        File.WriteAllText(outPath, holderText);
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
        ReplaceFns(ref binData, br, holderEncoder, encoding);
        ReplaceNames(ref binData, br, holderEncoder, encoding);
        File.WriteAllBytes(binFile, binData);
        File.Delete(outPath);
        return true;
    }

    private void ReplaceNames(ref byte[] binData, BinaryReader br, CalmareCoder holderEncoder, Encoding encoding)
    {
        br.BaseStream.Seek(0x34, SeekOrigin.Begin);
        var pString = br.ReadInt32();
        br.BaseStream.Seek(pString, SeekOrigin.Begin);
        var strings = new Queue<string>();
        while (br.BaseStream.Position < br.BaseStream.Length)
        {
            var str = br.ReadCString(ExtraEncoding.SJIS) ?? throw new Exception("读取文本失败");
            strings.Enqueue(str);
        }

        strings.Dequeue();
        replace(ref binData, NpcNameStrings, holderEncoder.NpcNameStrings);
        for (var i = 0; i < BattleCount; i++)
            strings.Dequeue();
        replace(ref binData, LabelNameStrings, holderEncoder.LabelNameStrings);

        void replace(ref byte[] binData, List<string> list, List<string> holderList)
        {
            for (var index = 0; index < list.Count; index++)
            {
                var holderStr = holderList[index];
                var coderStr = list[index];
                var binStr = strings.Dequeue();
                if (holderStr != binStr)
                    throw new Exception($"未找到NameString[{index}]: {coderStr}");
                byte[] sjisBytes = [..ExtraEncoding.SJIS.GetBytes(holderStr), 0];
                byte[] coderBytes = [..encoding.GetBytes(coderStr), 0];
                var result = BitHelper.Replace(binData, sjisBytes, coderBytes, pString, (int)br.BaseStream.Length, 1);
                if (!result.replaced)
                    throw new Exception($"未找到NameString[{index}]: {coderStr}");
                binData = result.result;
            }
        }
    }

    private void ReplaceFns(ref byte[] binData, BinaryReader br, CalmareCoder holderEncoder, Encoding encoding)
    {
        br.BaseStream.Seek(0x42, SeekOrigin.Begin);
        var pFunc = br.ReadUInt16();
        var nFunc = br.ReadUInt16() / 4;
        var pFunctions = new uint[nFunc];
        br.BaseStream.Seek(pFunc, SeekOrigin.Begin);
        for (var i = 0; i < nFunc; i++)
            pFunctions[i] = br.ReadUInt32();
        for (var i = 0; i < holderEncoder.FnTexts.Count; i++)
        {
            var holderFnText = holderEncoder.FnTexts[i];
            var fnText = FnTexts[i];
            var start = (int)pFunctions[holderFnText.index];
            var end = holderFnText.index + 1 <= pFunctions.Length - 1 ? (int)pFunctions[holderFnText.index + 1] : -1;

            for (var j = 0; j < holderFnText.func.Count; j++)
            {
                var sjisBytes = holderFnText.func[j].Encode(ExtraEncoding.SJIS);
                var coderBytes = fnText.func[j].Encode(encoding);
                if (sjisBytes.Length != coderBytes.Length)
                    throw new Exception("字节长度不一致");
                var result = BitHelper.Replace(binData, sjisBytes, coderBytes, start, end, 1);
                if (!result.replaced)
                    throw new Exception($"{FileName}未找到Fn文本：{fnText.func[j].RawText}");
                binData = result.result;
            }
        }
    }

    public string FileName;
    public string ParsingText;
    public readonly List<string> NpcNameStrings = new(100);
    public int BattleCount;
    public readonly List<string> LabelNameStrings = new(100);
    public readonly List<(int index, List<Opcode> func)> FnTexts = [];
    public readonly List<string> Functions = new(100);
    private static readonly Regex FnReg = FnRegex();
    private static readonly Regex FnTextReg = FnTextRegex();
    private static readonly Regex NpcNameStringReg = NpcNameStringRegex();
    private static readonly Regex LabelNameStringReg = LabelNameStringRegex();
    private static readonly Regex BattleReg = BattleRegex();

    [GeneratedRegex("""(?<=^npc.*?:\n\tname ").*?(?="$)""", RegexOptions.Multiline)]
    private static partial Regex NpcNameStringRegex();

    [GeneratedRegex("""(?<=^label.*?:\n\tname ").*?(?="$)""", RegexOptions.Multiline)]
    private static partial Regex LabelNameStringRegex();

    [GeneratedRegex("""^battle\[\d+\]:$""", RegexOptions.Multiline)]
    private static partial Regex BattleRegex();

    [GeneratedRegex("""fn\[\d+\]:$[\s\S]*?(?=\nfn|\z)""", RegexOptions.Multiline)]
    private static partial Regex FnRegex();

    //menu: c133b
    [GeneratedRegex("""
                    \t+TextSetName ".+?"$|\t+(?:TextMessage|TextTalk |TextTalkNamed).*?{$[\s\S]*?\n\t+}$|\t+Menu .*?$[\s\S]*?(?=\n(?!\t+"))|\t+ED7MenuAdd.*?$|\t+ScMenuSetTitle.*?$
                    """, RegexOptions.Compiled | RegexOptions.Multiline)]
    private static partial Regex FnTextRegex();
}