using System.Diagnostics;
using System.Text;
using Common;
using Extensions;

namespace CalmareEncoder.Calmare;

public static partial class CalmareConverter
{
    private static readonly Dictionary<string, string> InvalidChars = new()
    {
        //https://www.toolhelper.cn/Encoding/GBK
        ["・"] = "丄",
        ["♪"] = "丅"
    };

    public static bool ConvertGBK(string clmText, string outPath, string calmareFile)
    {
        CalmareEncoder gbkEncoder = new(), holderEncoder = new();
        var holderText = ExtraEncoding.DoubleByteCharReg.Replace(clmText, x =>
        {
            var value = x.Value;
            if (InvalidChars.TryGetValue(value, out var c))
                value = c;

            var count = ExtraEncoding.GBK.GetByteCount(value);
            return count switch
            {
                2 => "果",
                _ => throw new Exception($"非法字节：{x.Value}({count})")
            };
        });
        gbkEncoder.Parse(ReplaceInvalidChar(clmText));
        holderEncoder.Parse(holderText);
        if (holderEncoder.FnTexts.Count != gbkEncoder.FnTexts.Count)
            return false;
        if (holderEncoder.NpcNames.Count != gbkEncoder.NpcNames.Count)
            return false;
        File.WriteAllText(outPath, holderText);
        var success = Utils.RunExe(calmareFile, $"\"{outPath}\"",2);
        if (!success)
            return false;
        var binFile = Path.Combine(
            Path.GetDirectoryName(outPath) ?? throw new DirectoryNotFoundException(),
            Path.GetFileNameWithoutExtension(outPath) + ".bin");
        if (!File.Exists(binFile))
            return false;
        var binBytes = File.ReadAllBytes(binFile);
        using var br = new BinaryReader(new MemoryStream(binBytes));
        ReplaceFn(ref binBytes, br, gbkEncoder, holderEncoder);
        ReplaceNpcName(ref binBytes, br, gbkEncoder, holderEncoder);
        File.WriteAllBytes(binFile, binBytes);
        File.Delete(outPath);
        return true;
    }

    public static string ReplaceInvalidChar(string text)
    {
        return InvalidChars.Aggregate(text, (current, ch) => current.Replace(ch.Key, ch.Value));
    }

    private static void ReplaceNpcName(ref byte[] binBytes, BinaryReader br, CalmareEncoder gbkEncoder,
        CalmareEncoder holderEncoder)
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

        for (var index = 0; index < holderEncoder.NpcNames.Count; index++)
        {
            var holderStr = holderEncoder.NpcNames[index];
            var gbkStr = gbkEncoder.NpcNames[index];
            var binStr = strings.Dequeue();
            if (holderStr != binStr)
                throw new Exception("npc name cannot be found.");
            byte[] sjisBytes = [..ExtraEncoding.SJIS.GetBytes(holderStr), 0];
            byte[] gbkBytes = [..ExtraEncoding.GBK.GetBytes(gbkStr), 0];
            var result = BitHelper.Replace(binBytes, sjisBytes, gbkBytes, pString, (int)br.BaseStream.Length, 1);
            if (!result.replaced)
                throw new Exception($"未找到NpcNames: {gbkStr}");
            binBytes = result.result;
        }
    }

    private static void ReplaceFn(ref byte[] binBytes, BinaryReader br, CalmareEncoder gbkEncoder,
        CalmareEncoder holderEncoder)
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
            var gbkFnText = gbkEncoder.FnTexts[i];
            var start = (int)pFunctions[holderFnText.index];
            var end = holderFnText.index + 1 <= pFunctions.Length - 1 ? (int)pFunctions[holderFnText.index + 1] : -1;

            for (var j = 0; j < holderFnText.func.Count; j++)
            {
                var sjisBytes = holderFnText.func[j].Encode(ExtraEncoding.SJIS);

                var gbkBytes = gbkFnText.func[j].Encode(ExtraEncoding.GBK);
                if (sjisBytes.Length != gbkBytes.Length)
                    throw new Exception("字节长度不一致");
                var result = BitHelper.Replace(binBytes, sjisBytes, gbkBytes, start, end, 1);
                if (!result.replaced)
                    throw new Exception($"未找到Fn文本：{gbkFnText.func[j].RawText}");
                binBytes = result.result;
            }
        }
    }


}