using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace ConsoleApp1;

internal partial class Program
{
    [GeneratedRegex("""(?<= {\n)[\s\S]+?(?=\n\t+})""", RegexOptions.Multiline)]
    private static partial Regex dialogRegex();
    [GeneratedRegex("""#\d+[VIMF]""", RegexOptions.Multiline)]
    private static partial Regex tagRegex();

    private static Regex dialogReg = dialogRegex();
    private static Regex tagReg = tagRegex();
    public static void a(byte[] data)
    {
        using var ms = new MemoryStream(data);
        using var br = new BinaryReader(ms);
        using var outMs = new MemoryStream();
        using var bw = new BinaryWriter(outMs);
        var chunkSize = 8;
        var len = data.Length / chunkSize;
        var maxlen = 0;
        var last = 0;
        for (int i = 0; i < chunkSize; i++)
        {
            var buffer = br.ReadBytes(len);
            Compress(buffer, bw, 8);
            var nowLen = bw.BaseStream.Length - last;
            maxlen = Math.Max(maxlen, (int)nowLen);
            last = (int)bw.BaseStream.Length;
        }
        File.WriteAllBytes("chrimg00.itp.lzss1", outMs.ToArray());
    }
    static void Compress(byte[] input, BinaryWriter bw, int mode = 8)
    {
        int startPos = (int)bw.BaseStream.Position;
        // 占位符：csize, usize, mode
        bw.Write(0);
        bw.Write(input.Length);
        bw.Write(mode);

        int cursor = 0;
        int maxOp = (1 << mode) - 1;       // op 的最大值
        int maxLookback = (1 << (16 - mode)) - 1; // num 的最大值

        while (cursor < input.Length)
        {
            int bestMatchLen = 0;
            int bestMatchDist = 0;

            // 在滑动窗口内寻找最长匹配 (LZ77 逻辑)
            // 注意：根据解压逻辑，op > 0 时最后会多读一个 byte，所以匹配长度限制在 maxOp
            int searchStart = Math.Max(0, cursor - maxLookback - 1);
            for (int j = searchStart; j < cursor; j++)
            {
                int matchLen = 0;
                while (matchLen < maxOp &&
                       cursor + matchLen < input.Length - 1 && // 留一个字节给随后的 ReadByte
                       input[j + matchLen] == input[cursor + matchLen])
                {
                    matchLen++;
                }

                if (matchLen >= bestMatchLen)
                {
                    bestMatchLen = matchLen;
                    bestMatchDist = cursor - j - 1;
                }
            }

            if (bestMatchLen > 0)
            {
                // 写入 字典引用 模式 (op > 0)
                ushort control = (ushort)((bestMatchDist << mode) | (bestMatchLen & maxOp));
                bw.Write(control);
                cursor += bestMatchLen;

                // 写入随后的那一个字节 (outData.Add(br.ReadByte()))
                bw.Write(input[cursor]);
                cursor++;
            }
            else
            {
                // 写入 原始数据 模式 (op == 0)
                // 这里简单处理，每次只写1个字节的原始数据
                ushort control = (ushort)(1 << mode); // num = 1, op = 0
                bw.Write(control);
                bw.Write(input[cursor]);
                cursor++;
            }
        }

        // 回填 csize (总长度 - 4)
        int endPos = (int)bw.BaseStream.Position;
        int csize = endPos - startPos;
        bw.BaseStream.Seek(startPos, SeekOrigin.Begin);
        bw.Write(csize);
        bw.BaseStream.Seek(endPos, SeekOrigin.Begin);
    }
    static void Decompress(BinaryReader br, List<byte> outData)
    {
        int csize = br.ReadInt32();
        int usize = br.ReadInt32();
        int mode = br.ReadInt32(); // 这里通常是 4, 5 或 6, 但现在是8

        int startPos = outData.Count;

        if (mode == 0)
        {
            outData.AddRange(br.ReadBytes(csize - 4));
        }
        else
        {
            int endPos = startPos + usize;
            while (outData.Count < endPos)
            {
                ushort x = br.ReadUInt16();
                int op = x & ((1 << mode) - 1); 
                int num = x >> mode;

                if (op == 0)
                {
                    outData.AddRange(br.ReadBytes(num));
                }
                else
                {
                    for (int i = 0; i < op; i++)
                    {
                        // 字典回溯
                        outData.Add(outData[outData.Count - num - 1]);
                    }
                    outData.Add(br.ReadByte());
                }
            }
        }
    }
    static void SaveDds(string path, byte[] data, int w, int h)
    {
        using (BinaryWriter bw = new BinaryWriter(File.Create(path)))
        {
            bw.Write(0x20534444); // Magic
            bw.Write(124); bw.Write(0x1 | 0x2 | 0x4 | 0x1000 | 0x20000);
            bw.Write(h); bw.Write(w); bw.Write(0); bw.Write(0); bw.Write(1);
            for (int i = 0; i < 11; i++) bw.Write(0);
            bw.Write(32); bw.Write(0x4); bw.Write(0x30315844); // "DX10"
            for (int i = 0; i < 5; i++) bw.Write(0);
            bw.Write(0x1000); bw.Write(0); bw.Write(0); bw.Write(0); bw.Write(0);

            // DX10 扩展头
            bw.Write(98); // DXGI_FORMAT_BC7_UNORM
            bw.Write(3); bw.Write(0); bw.Write(1); bw.Write(0);

            bw.Write(data);
        }
    }
    static void Main(string[] args)
    {
        MatchMorePortrait_Scena_ao();
        return;
        #region MyRegion
        string inputPath = "chrimg00.itp";
        string outputPath = "chrimg00.dds";
        if (!File.Exists(inputPath)) return;

        using (BinaryReader br = new BinaryReader(File.OpenRead(inputPath)))
        {
            // 1. 验证魔数
            if (br.ReadUInt32() != 0xFF505449) throw new Exception("不是有效的 ITP 文件");

            // 2. 寻找 IDAT 块 (简单定位，实际应遍历 Chunk)
            br.BaseStream.Seek(0x68, SeekOrigin.Begin);
            if (new string(br.ReadChars(4)) != "IDAT") throw new Exception("未找到 IDAT 块");

            int idatChunkSize = br.ReadInt32();
            Console.WriteLine(br.ReadInt32()); // 8
            Console.WriteLine(br.ReadInt16()); // 0
            Console.WriteLine(br.ReadInt16()); // mip index

            // 3. 这里的 0x80000001 是 Minor 10 的标志
            if (br.ReadUInt32() != 0x80000001) throw new Exception("不支持的压缩模式");

            int nChunks = br.ReadInt32();
            int totalCSize = br.ReadInt32();
            int largestCSize = br.ReadInt32();
            int totalUSize = br.ReadInt32(); // 这个应该是 (W*H)

            List<byte> decompressedData = new List<byte>();

            for (int i = 0; i < nChunks; i++)
            {
                //lzss.Decompress(decompressedData);
                Decompress(br, decompressedData);
                //0x27e5-0x8c
            }
            //0x8c 0x7d011
            // 5. 保存为 DDS
            //File.WriteAllBytes(outputPath, decompressedData.ToArray());
            a(decompressedData.ToArray());

            //SaveDds(outputPath, decompressedData.ToArray(), 1024, 512);
            Console.WriteLine("提取完成: " + outputPath);
        }
        #endregion

    }
    private static void Match_scena()
    {
        var replaceReg = new Regex(
            """
            (name ").*?(?=")|(?<=ED7MenuAdd menu\[\d\] ").*?(?=")|(?<=\t").*(?=" \/\/ \d)|(?<= {\n)[\s\S]+?(?=\n\t+})|(?<=TextSetName ").*?(?=")|(?<=ScMenuSetTitle \d+ \d+ \d+ ").*?(?=")|(?<=TextTalkNamed .*?").*?(?=")|(?<=CharSetName .*? ").*?(?=")
            """);

        var cnDir = @"C:\Users\Jelly\RiderProjects\zDiffer\zDiffer\bin\Debug\net9.0\out";
        var jpDir = @"F:\源码\C#\ed7zeroTool\trans\ao\scena\jp";
        var count = 0;
        var cnFiles = Directory.GetFiles(cnDir);
        var kanaReg = new Regex("[[\u3040-\u309F\u30A0-\u30FF]]");
        foreach (var cnFile in cnFiles)
        {
            var fileName = Path.GetFileName(cnFile);
            //var jpFile = Path.Combine(jpDir, fileName);
            var cnFileContent = File.ReadAllText(cnFile);
            if (kanaReg.IsMatch(cnFileContent))
            {
                Console.WriteLine(fileName);
            }
            //var jpFileContent = File.ReadAllText(jpFile);
            //var cnContent = replaceReg.Replace(cnFileContent.Replace("\r",""), "");
            //var jpContent = replaceReg.Replace(jpFileContent, "");
        
            //if(cnContent.GetHashCode()!= jpContent.GetHashCode())
            //{
            //    Console.WriteLine(fileName);
            //    count++;
            //}
        }
        Console.WriteLine(count);

    }
    private static void TagMatch_Dat()
    {
        var cnDir = @"F:\源码\C#\ed7zeroTool\trans\ao\text\cn";
        var jpDir = @"F:\源码\C#\ed7zeroTool\trans\ao\text\jp";
        var cnFiles = Directory.GetFiles(cnDir);
        foreach (var cnFile in cnFiles)
        {
            var fileName = Path.GetFileName(cnFile);
            var jpFile = Path.Combine(jpDir, fileName);
            if (!File.Exists(jpFile))
            {
                Console.WriteLine($"JP file not found: {fileName}");
                continue;
            }
            var cnContent = File.ReadAllText(cnFile);
            var jpContent = File.ReadAllText(jpFile);
            var cnMatches = tagReg.Matches(cnContent);
            var jpMatches = tagReg.Matches(jpContent);
            if (cnMatches.Count != jpMatches.Count)
            {
                Console.WriteLine($"Mismatch in number of tags for file: {fileName}");
                continue;
            }
        }
    }

    private static string currentFile = "";
    private static void MatchMorePortrait_Scena_ao()
    {
        var jpDir = @"F:\源码\C#\ed7zeroTool\trans\ao\scena\MorePortrait_jp";
        var cnDir = @"F:\源码\C#\ed7zeroTool\trans\ao\scena\cn_2_校对原版";
        var outDir = @"F:\源码\C#\ed7zeroTool\trans\ao\scena\cn_3_校对更多肖像";
        var jpFiles = Directory.GetFiles(jpDir);
       
        foreach (var jpFile in jpFiles)
        {
            var fileName = Path.GetFileName(jpFile);
            currentFile = fileName;
            var cnFile = Path.Combine(cnDir, fileName);
            if (!File.Exists(cnFile))
            {
                continue;
            }
            var jp_Content = File.ReadAllText(jpFile).Replace("\r", "");
            var cn_Content = File.ReadAllText(cnFile).Replace("\r", "");
            var jp_texts = dialogReg.Matches(jp_Content).Select(x => x.Value).ToList();
            var cn_texts = dialogReg.Matches(cn_Content).Select(x => x.Value).ToList();
            if(fileName == "c0210.clm")
            {
                cn_texts.Insert(0x218, "\t\t\t啊，對喔，\n\t\t\t確實有這麼一回事。{wait}");
                cn_texts.Insert(0x219, "\t\t\t我想想喔，說到本店的麵包，\n\t\t\t每一種都值得推薦，不過……{wait}");
            }
            else if (fileName == "c1120.clm")
            {
                cn_texts.Insert(0xf5, "\t\t\t這是選美大會！\n\t\t\t男人不能參加啦！！{wait}");
                cn_texts.Insert(0xf6, "\t\t\t就這樣，\n\t\t\t快點選別人出來！！{wait}");
                cn_texts.Insert(0x13b, "\t\t\t那麼，終於到了最後一位！\n\t\t\t讓我們歡迎七號參賽者！{wait}");
                cn_texts.Insert(0x13e, "\t\t\t那麼，終於到了最後一位！\n\t\t\t讓我們歡迎七號參賽者！{wait}");
            }
            else if (fileName == "t2520.clm")
            {
                cn_texts.Insert(0x154, "\t\t\t#11P雖然我也想這麼做……\n\t\t\t不過就像我剛才說的，\n\t\t\t警備隊目前是高度戒備狀態。{wait}");
                cn_texts.Insert(0x155, "\t\t\t#11P被認為是克洛斯貝爾市襲擊犯的\n\t\t\t《紅色星座》也消失無蹤，\n\t\t\t連一點線索都沒有。{wait}");
                cn_texts.Insert(0x156, "\t\t\t#11P再加上，帝國跟共和國\n\t\t\t打算開始進行大規模演習的\n\t\t\t這個狀況……{wait}");
            }

            if (jp_texts.Count != cn_texts.Count)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Mismatch in number of matches for file: {fileName}");
                Console.ForegroundColor = ConsoleColor.Gray;
                continue;
            }
            for (int i = 0; i < jp_texts.Count; i++)
            {
                var jp_text = jp_texts[i];
                var cn_text = cn_texts[i];
                var fixedText =fixText(jp_text, cn_text);
                jp_Content = ReplaceFirst(jp_Content, jp_text, fixedText);
            }
            jp_Content = replaceOther_ao(cn_Content, jp_Content);
            File.WriteAllText(Path.Combine(outDir,fileName),jp_Content.Replace("\r",""));
          
        }
    }



    private static string replaceOther_ao(string cn_Content, string jp_Content)
    {
        //name 
        var reg = NameRegex();
        var cn_texts = reg.Matches(cn_Content).Select(x => x.Value).ToList();
        var jp_texts = reg.Matches(jp_Content).Select(x => x.Value).ToList();
        if (cn_texts.Count != jp_texts.Count)
            throw new Exception();
        for (int i = 0; i < cn_texts.Count; i++)
            jp_Content = ReplaceFirst(jp_Content, jp_texts[i], cn_texts[i]);
        //menu
        reg = MenuRegex();
        cn_texts = reg.Matches(cn_Content).Select(x => x.Value).ToList();
        jp_texts = reg.Matches(jp_Content).Select(x => x.Value).ToList();
        if (cn_texts.Count != jp_texts.Count)
            throw new Exception();
        for (int i = 0; i < cn_texts.Count; i++)
            jp_Content = ReplaceFirst(jp_Content, jp_texts[i], cn_texts[i]);
        //TextSetName
        reg = TextSetNameRegex();
        cn_texts = reg.Matches(cn_Content).Select(x => x.Value).ToList();
        jp_texts = reg.Matches(jp_Content).Select(x => x.Value).ToList();
        switch (currentFile)
        {
            case "c0140.clm":
                cn_texts.Add("TextSetName \"溫蒂\"");
                break;
            case "c0400.clm":
                cn_texts.Add("TextSetName \"凱特巡警\"");
                break;
            case "c1010.clm":
                cn_texts.Insert(0, "TextSetName \"遊擊士林\"");
                cn_texts.Insert(1, "TextSetName \"遊擊士艾歐莉雅\"");
                cn_texts.Insert(2, "TextSetName \"遊擊士斯克特\"");
                cn_texts.Insert(3, "TextSetName \"遊擊士溫澤爾\"");
                break;
            case "c1030.clm":
                cn_texts.Add("TextSetName \"珊珊\"");
                break;
            case "c1440.clm":
                cn_texts.Add("TextSetName \"艾胥莉\"");
                break;
            case "t1650.clm":
                cn_texts.Add("TextSetName \"女醫生\"");
                break;
        }
        if (cn_texts.Count != jp_texts.Count)
            throw new Exception();
        for (int i = 0; i < cn_texts.Count; i++)
            jp_Content = ReplaceFirst(jp_Content, jp_texts[i], cn_texts[i]);

        //ED7MenuAdd
        reg = ED7MenuAddRegex();
        cn_texts = reg.Matches(cn_Content).Select(x => x.Value).ToList();
        jp_texts = reg.Matches(jp_Content).Select(x => x.Value).ToList();
        if (cn_texts.Count != jp_texts.Count)
            throw new Exception();
        for (int i = 0; i < cn_texts.Count; i++)
            jp_Content = ReplaceFirst(jp_Content, jp_texts[i], cn_texts[i]);
        //ScMenuSetTitle
        reg = ScMenuSetTitleRegex();
        cn_texts = reg.Matches(cn_Content).Select(x => x.Value).ToList();
        jp_texts = reg.Matches(jp_Content).Select(x => x.Value).ToList();
        if (cn_texts.Count != jp_texts.Count)
            throw new Exception();
        for (int i = 0; i < cn_texts.Count; i++)
            jp_Content = ReplaceFirst(jp_Content, jp_texts[i], cn_texts[i]);
        //CharSetName
        reg = CharSetNameRegex();
        cn_texts = reg.Matches(cn_Content).Select(x => x.Value).ToList();
        jp_texts = reg.Matches(jp_Content).Select(x => x.Value).ToList();
        if (cn_texts.Count != jp_texts.Count)
            throw new Exception();
        for (int i = 0; i < cn_texts.Count; i++)
            jp_Content = ReplaceFirst(jp_Content, jp_texts[i], cn_texts[i]);

        //TextTalkNamed
        reg = TextTalkNamedRegex();
        cn_texts = reg.Matches(cn_Content).Select(x => x.Value).ToList();
        jp_texts = reg.Matches(jp_Content).Select(x => x.Value).ToList();

        switch (currentFile)
        {
            case "m1060.clm":
            case "m1140.clm":
                cn_texts.RemoveAt(cn_texts.Count - 1);
                break;
            case "t1650.clm":
                cn_texts.RemoveAt(cn_texts.Count - 1);
                cn_texts.RemoveAt(cn_texts.Count - 1);
                break;
        }

        if (cn_texts.Count != jp_texts.Count)
            throw new Exception();
        for (int i = 0; i < cn_texts.Count; i++)
            jp_Content = ReplaceFirst(jp_Content, jp_texts[i], cn_texts[i]);

        return jp_Content;
    }

    static string fixText(string jp, string cn)
    {
        var cnTag = GetFaceTag(cn);
        var jpTag = GetFaceTag(jp);
        var cn_indentCount = GetIndentCount(cn);
        var jp_indentCount = GetIndentCount(jp);
        if (cn_indentCount == jp_indentCount && cnTag.Equals(jpTag))
            return cn;
     
        var indent = new string('\t', jp_indentCount);
        var strs = cn.Split('\n').Select(x => x[cn_indentCount..]).ToList();
        var firstLine = "";
        if (cnTag != "" && !jpTag.Equals(cnTag))
        {
            firstLine = $"{indent}{strs[0].Replace(cnTag, jpTag)}\n";
        }
        else
        {
            firstLine = $"{indent}{jpTag}{strs[0]}\n";
        }
        var sb = new StringBuilder(firstLine);
        for (int i = 1; i < strs.Count; i++)
        {
            sb.Append($"{indent}{strs[i]}\n");
        }
        sb.Length--;
        return sb.ToString();
    }
    static int GetIndentCount(string text)
    {
        for (int i = 0; i < text.Length; i++)
        {
            if (text[i] != '\t')
                return i;
        }
        return 0;
    }
    static string ReplaceFirst(string source, string find, string replace)
    {
        int place = source.IndexOf(find);
        if (place == -1)
            throw new Exception();
        return source.Remove(place, find.Length).Insert(place, replace);
    }
    static Regex faceTagReg = new Regex(@"#\d+F");
    private static string GetFaceTag(string str)
    {
        return faceTagReg.IsMatch(str)? faceTagReg.Match(str).Value : "";
    }

    private static void TagMatch_Scena()
    {
        var cnDir = @"F:\源码\C#\ed7zeroTool\trans\ao\scena\jp";
        var jpDir = @"F:\源码\C#\ed7zeroTool\trans\ao\scena\MorePortrait_jp";
        var cnFiles = Directory.GetFiles(cnDir);
        foreach (var cnFile in cnFiles)
        {
            var fileName = Path.GetFileName(cnFile);
            var jpFile = Path.Combine(jpDir, fileName);
            if (!File.Exists(jpFile))
            {
                Console.WriteLine($"JP file not found: {fileName}");
                continue;
            }
            var cnContent = File.ReadAllText(cnFile).Replace("\r","");
            var jpContent = File.ReadAllText(jpFile);
            var cnMatches = dialogReg.Matches(cnContent);
            var jpMatches = dialogReg.Matches(jpContent);
            if (cnMatches.Count != jpMatches.Count)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Mismatch in number of matches for file: {fileName}");
                Console.ForegroundColor = ConsoleColor.Gray;
                var list = new List<string>(jpMatches.Select(x => x.Value));
                foreach (Match cnMatch in cnMatches)
                {
                    var values = cnMatch.Value.Split("\n").Select(x=>x.Trim('\t'));
                    var item= list.FirstOrDefault(x => 
                    {
                        foreach (var v in values)
                        {
                            if (!x.Contains(v))
                                return false;
                        }
                        return true;
                    });
                    if(item != null)
                    {
                        list.Remove(item);
                    }
                }
                continue;
            }
            for (int i = 0; i < cnMatches.Count; i++)
            {
                var cnText = cnMatches[i].Value;
                var jpText = jpMatches[i].Value;
                var cnTags = tagReg.Matches(cnText);
                var jpTags = tagReg.Matches(jpText);
                if (cnTags.Count != jpTags.Count)
                {
                    Console.WriteLine(new string('-', 60));
                    Console.WriteLine($"Mismatch in number of tags for file: {fileName}, dialog index: {i}");
                    Console.WriteLine($"cn dialog:\n{cnText}");
                    Console.WriteLine($"jp dialog:\n{jpText}");
                    Console.WriteLine(new string('-', 60));
                    //Console.ReadKey();
                }
            }
        }
    }

    [GeneratedRegex("""
        (?<=\t)name ".+?"
        """)]
    private static partial Regex NameRegex();
    [GeneratedRegex("""
        (?<=\t)TextSetName ".+?"
        """)]
    private static partial Regex TextSetNameRegex();
    [GeneratedRegex("""
        (?<=\tED7MenuAdd .*? )".+?"
        """)]
    private static partial Regex ED7MenuAddRegex();
    [GeneratedRegex("""
        (?<=\tScMenuSetTitle .*? )".+?"
        """)]
    private static partial Regex ScMenuSetTitleRegex();
    [GeneratedRegex("""
        (?<=\tCharSetName .*? )".+?"
        """)]
    private static partial Regex CharSetNameRegex();
    [GeneratedRegex("""(?<=\t)".+?" \/\/""")]
    private static partial Regex MenuRegex();
    [GeneratedRegex("""
        (?<=\tTextTalkNamed .*? )".+?"
        """)]
    private static partial Regex TextTalkNamedRegex();
}
