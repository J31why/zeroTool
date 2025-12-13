using System.Diagnostics;
using System.Text;
using CalmareEncoder.Calmare;
using Common;

namespace CalmareEncoder;

internal static class Program
{
    public static void Main(string[] args)
    {
        string inputPath, calmare;
        bool isDecryptStr = false, isDecryptFile = false, isDecompress = false;
        Console.OutputEncoding = Encoding.UTF8;

        #region args

        try
        {
            inputPath = args[0];
            var currentDir = Environment.ProcessPath ?? throw new DirectoryNotFoundException();
            currentDir = Path.GetDirectoryName(currentDir) ?? throw new DirectoryNotFoundException();
            calmare = Path.Combine(currentDir, "calmare.exe");
            if (!File.Exists(inputPath) && !Directory.Exists(inputPath))
                throw new Exception();
            for (var i = 1; i < args.Length; i++)
                switch (args[i])
                {
                    case "-ds":
                        isDecryptStr = true;
                        break;
                    case "-decomp":
                        isDecompress = true;
                        break;
                    case "-df":
                        isDecryptFile = true;
                        break;
                    default:
                        throw new ArgumentException(args[i]);
                }
        }
        catch (Exception)
        {
            OutHelp();
            Console.ReadKey();
            return;
        }

        #endregion


        try
        {
            if (isDecryptFile)
            {
                var outPath = GetOutPath(inputPath, "decrypted");
                DecryptFile(inputPath,outPath);
                Console.WriteLine("已解密bin文件：{0}", outPath);
            }
            else if (isDecryptStr)
            {
                var outPath = GetOutPath(inputPath, "decrypted");
                DecryptStr(inputPath,outPath);
                Console.WriteLine("已解密云豹字符串：{0}", outPath);
            }
            else if (isDecompress)
            {
                if(!File.Exists(calmare))
                    throw new FileNotFoundException("未找到calmare.exe");
                DecompressBin(inputPath, calmare);
                Console.WriteLine("已反编译Bin文件");
            }
            else
            {
                if(!File.Exists(calmare))
                    throw new FileNotFoundException("未找到calmare.exe");
                var outPath = GetOutPath(inputPath, "compiled");
                Compile(inputPath, outPath,calmare);
                Console.WriteLine("已编译GBK编码CLM文件：{0}", outPath);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Console.ReadKey();
        }
    }

    private static void DecompressBin(string path, string calmare)
    {
        if (Directory.Exists(path))
        {
            var files = Directory.EnumerateFiles(path,"*.bin");
            foreach (var file in files)
            {
                Utils.RunExe(calmare, $"\"{file}\"",1);
            }
        }
        else if (File.Exists(path))
        {
            Utils.RunExe(calmare, path, 2);
        }
    }

    private static void Compile(string path, string outPath, string calmareFile)
    {
      
        if (Directory.Exists(path))
        {
            var files = Directory.EnumerateFiles(path,"*.clm");
            foreach (var file in files)
            {
                var outFile = Path.Combine(outPath, Path.GetFileName(file));
                De(file, outFile);
            }
        }
        else if (File.Exists(path))
        {
            var outFile = Path.Combine(outPath, Path.GetFileName(path));
            De(path, outFile);
        }

        return;

        void De(string file, string outfile)
        {
            try
            {
                var clmText = File.ReadAllText(file);
                var isSuccess = CalmareConverter.ConvertGBK(clmText, outfile,calmareFile);
                if(!isSuccess)
                    throw new InvalidDataException();
            }
            catch(Exception e)
            {
                Console.WriteLine("编译文件失败：{0}", file);
                Console.WriteLine(e.Message);
            }
        }
    }
    private static void DecryptStr(string path,string outPath)
    {
        if (Directory.Exists(path))
        {
            var files = Directory.EnumerateFiles(path,"*.clm");
            foreach (var file in files)
            {
                var outFile = Path.Combine(outPath, Path.GetFileName(file));
                De(file, outFile);
            }
        }
        else if (File.Exists(path))
        {
            var outFile = Path.Combine(outPath, Path.GetFileName(path));
            De(path, outFile);
        }
        void De(string file, string outfile)
        {
            try
            {
                var clmText = File.ReadAllText(file);
                clmText = CLEDecrypter.DecryptChar(clmText);
                File.WriteAllText(outfile, clmText);
            }
            catch (Exception e)
            {
                Console.WriteLine("解密字符串失败：{0}", file);
                Console.WriteLine(e.Message);
            }
        }
    }
    private static void DecryptFile(string path,string outPath)
    {
    
        if (Directory.Exists(path))
        {
            var files = Directory.EnumerateFiles(path);
            foreach (var file in files)
            {
                var outFile = Path.Combine(outPath, Path.GetFileName(file));
                De(file, outFile);
            }
        }
        else if (File.Exists(path))
        {
            var outFile = Path.Combine(outPath, Path.GetFileName(path));
            De(path, outFile);
        }

        return;

        void De(string file, string outfile)
        {
            try
            {
                var bytes = CLEDecrypter.DecryptFile(file);
                File.WriteAllBytes(outfile, bytes);
            }
            catch
            {
                Console.WriteLine("解密文件失败：{0}", file);
            }
        }
    }

    private static string GetOutPath(string path, string dir)
    {
        var outPath = "";
        if (File.Exists(path))
            outPath = Path.Combine(Path.GetDirectoryName(path) ?? throw new DirectoryNotFoundException(), dir);
        else if (Directory.Exists(path)) outPath = Path.Combine(path, dir);

        if (outPath == string.Empty || outPath == dir)
            throw new DirectoryNotFoundException();
        if (!Directory.Exists(outPath))
            Directory.CreateDirectory(outPath);
        return outPath;
    }

    private static void OutHelp()
    {
        Console.WriteLine(new string('-', 80));
        Console.WriteLine("CalmareEncoder使用方法(如果需要编译，请把calmare.exe放在目录下):");
        Console.WriteLine("1.编译GBK编码clm文件/目录: CalmareEncoder file/dir");
        Console.WriteLine("2.解密clm文件云豹加密字符串: CalmareEncoder file/dir -ds");
        Console.WriteLine("3.解密云豹加密文件: CalmareEncoder file/dir -df");
        Console.WriteLine("4.批量反编译bin文件: CalmareEncoder file/dir -decomp");
    }
}



/*
 * Microsoft Windows [版本 10.0.19045.3930]
(c) Microsoft Corporation。保留所有权利。

C:\Users\Jelly\RiderProjects\CalmareEncoder\CalmareEncoder\bin\Debug\net9.0>calmareencoder "E:\Games\The.Legend.of.Heroes.Zero.no.Kiseki.KAI\The Legend of Heroes Zero no Kiseki KAI\data_tc\scena\decrypted\decrypted"
编译文件失败：E:\Games\The.Legend.of.Heroes.Zero.no.Kiseki.KAI\The Legend of Heroes Zero no Kiseki KAI\data_tc\scena\decrypted\decrypted\c0010.clm
未找到Fn文本：  TextMessage null {
                {color 5}※※　安检官办公室　※※
                {} 　　　非工作人员
                　　 　 严禁入内。{wait}
        }
编译文件失败：E:\Games\The.Legend.of.Heroes.Zero.no.Kiseki.KAI\The Legend of Heroes Zero no Kiseki KAI\data_tc\scena\decrypted\decrypted\c0330.clm
未找到Fn文本：                  TextTalk char[2] {
                                #3800F『啊，这看起来很美味哦。
                                {}  苏菲亚，我吃一个啊～』{wait}
                        } {
                                （偷偷拿一个，嚼嚼嚼）……{wait}
                        }
编译文件失败：E:\Games\The.Legend.of.Heroes.Zero.no.Kiseki.KAI\The Legend of Heroes Zero no Kiseki KAI\data_tc\scena\decrypted\decrypted\c1010.clm
未找到Fn文本：          TextMessage null {
                        {0x06}　　　　　　　　　　动向　　　
                        ━━━━━━━━━━━━━━━━
                        {} 　亚里欧斯　：　玛因兹区域
                        {} 　 斯克特 　： 贝尔加德门区域
                        {} 　 温蔡尔 　： 贝尔加德门区域
                        {} 　　 林 　　： 唐古拉姆门区域
                        {} 　艾欧莉雅　： 唐古拉姆门区域
                        {} 　艾丝蒂尔　： 阿尔摩利卡区域
                        {} 　 约书亚 　： 阿尔摩利卡区域{wait}
                }
编译文件失败：E:\Games\The.Legend.of.Heroes.Zero.no.Kiseki.KAI\The Legend of Heroes Zero no Kiseki KAI\data_tc\scena\decrypted\decrypted\c101c.clm
未找到Fn文本：          TextMessage null {
                        {0x06}　　　　　　　　　　动向　　　
                        ━━━━━━━━━━━━━━━━
                        {} 　亚里欧斯　：　 雷米菲利亚公国
                        {} 　 斯克特 　： 　　『待命中』
                        {} 　 温蔡尔 　： 　　『待命中』
                        {} 　　 林 　　： ※休息（龙老饭店）
                        {} 　艾欧莉雅　： ※休息（面包咖啡馆）
                        {} 　艾丝蒂尔　： 　　　大教堂
                        {} 　 约书亚 　： 　　　大教堂{wait}
                }
编译文件失败：E:\Games\The.Legend.of.Heroes.Zero.no.Kiseki.KAI\The Legend of Heroes Zero no Kiseki KAI\data_tc\scena\decrypted\decrypted\c1020.clm
未找到Fn文本：                  TextTalk name[0] {
                                {} #0005F这、这么大的鱼缸！
                                （这里到底是
                                　什么地方呢……）{wait}
                        }
编译文件失败：E:\Games\The.Legend.of.Heroes.Zero.no.Kiseki.KAI\The Legend of Heroes Zero no Kiseki KAI\data_tc\scena\decrypted\decrypted\c110c.clm
未找到Fn文本：  TextMessage null {
                {color 5}　场所　：克洛斯贝尔市政厅
                　　　　　宴会大厅
                召开时间：纪念庆典第三日
                {} 主办者 ：亨利丄麦克道尔
                ※如欲旁听，需提前申请。{wait}
        }
编译文件失败：E:\Games\The.Legend.of.Heroes.Zero.no.Kiseki.KAI\The Legend of Heroes Zero no Kiseki KAI\data_tc\scena\decrypted\decrypted\c1150.clm
未找到Fn文本：                          TextTalk name[0] {
                                        #0006F（明明把爱慕自己的男人甩了，
                                        {} 却好像完全没有察觉啊……）{wait}
                                }
编译文件失败：E:\Games\The.Legend.of.Heroes.Zero.no.Kiseki.KAI\The Legend of Heroes Zero no Kiseki KAI\data_tc\scena\decrypted\decrypted\c1170.clm
未找到Fn文本：          Menu menu[0] 10 10 0
                        "★【 ３Ｆ 】" // 0
                        "　【 １Ｆ 】" // 1
                        "　【 离开 】" // 2
                var[2] = 0
        elif system[0] == 100:
                Menu menu[0] 10 10 0
                        "　【 ３Ｆ 】" // 0
                        "★【 １Ｆ 】" // 1
                        "　【 离开 】 " // 2
                var[2] = 1
        MenuWait
编译文件失败：E:\Games\The.Legend.of.Heroes.Zero.no.Kiseki.KAI\The Legend of Heroes Zero no Kiseki KAI\data_tc\scena\decrypted\decrypted\c1200.clm
未找到Fn文本：  TextMessage null {
                {color 5}前往『米修拉姆』的水上巴士丄时刻表
                ※米修拉姆引以为豪的主题乐园
                {} 『奇幻乐园』开园中！
                {} 请尽情享受欢乐时光！{wait}
        }
编译文件失败：E:\Games\The.Legend.of.Heroes.Zero.no.Kiseki.KAI\The Legend of Heroes Zero no Kiseki KAI\data_tc\scena\decrypted\decrypted\c1300.clm
未找到Fn文本：  TextMessage null {
                {color 5}Ｉ．Ｂ．Ｃ
                International Bank of Crossbell
                {} 需要与大楼内各公司联系的客人，
                {} 请到一楼大厅的服务台，
                {} 咨询接待人员。{wait}
        }
编译文件失败：E:\Games\The.Legend.of.Heroes.Zero.no.Kiseki.KAI\The Legend of Heroes Zero no Kiseki KAI\data_tc\scena\decrypted\decrypted\c1330.clm
未找到Fn文本：                  Menu menu[0] 10 10 0
                                "★【１６Ｆ】" // 0
                                "　【 １Ｆ 】" // 1
                                "　【 Ｂ５ 】" // 2
                                "　【 离开 】" // 3
                        var[2] = 0
                elif system[0] == 100:
                        Menu menu[0] 10 10 0
                                "　【１６Ｆ】" // 0
                                "★【 １Ｆ 】" // 1
                                "　【 Ｂ５ 】" // 2
                                "　【 离开 】" // 3
                        var[2] = 1
                elif system[0] == 102:
                        Menu menu[0] 10 10 0
                                "　【１６Ｆ】" // 0
                                "　【 １Ｆ 】" // 1
                                "★【 Ｂ５ 】" // 2
                                "　【 离开 】" // 3
                        var[2] = 2
                MenuWait
编译文件失败：E:\Games\The.Legend.of.Heroes.Zero.no.Kiseki.KAI\The Legend of Heroes Zero no Kiseki KAI\data_tc\scena\decrypted\decrypted\c133b.clm
未找到Fn文本：          Menu menu[0] 10 10 0
                        "★【１６Ｆ】" // 0
                        "　【 １Ｆ 】" // 1
                        "　【 Ｂ５ 】" // 2
                        "　【 离开 】" // 3
                var[2] = 0
        elif system[0] == 100:
                Menu menu[0] 10 10 0
                        "　【１６Ｆ】" // 0
                        "★【 １Ｆ 】" // 1
                        "　【 Ｂ５ 】" // 2
                        "　【 离开 】" // 3
                var[2] = 1
        elif system[0] == 102:
                Menu menu[0] 10 10 0
                        "　【１６Ｆ】" // 0
                        "　【 １Ｆ 】" // 1
                        "★【 Ｂ５ 】" // 2
                        "　【 离开 】" // 3
                var[2] = 2
        MenuWait
编译文件失败：E:\Games\The.Legend.of.Heroes.Zero.no.Kiseki.KAI\The Legend of Heroes Zero no Kiseki KAI\data_tc\scena\decrypted\decrypted\r0110.clm
Name not found : field_party[0]
编译文件失败：E:\Games\The.Legend.of.Heroes.Zero.no.Kiseki.KAI\The Legend of Heroes Zero no Kiseki KAI\data_tc\scena\decrypted\decrypted\r1500.clm
Name not found : field_party[0]
编译文件失败：E:\Games\The.Legend.of.Heroes.Zero.no.Kiseki.KAI\The Legend of Heroes Zero no Kiseki KAI\data_tc\scena\decrypted\decrypted\t0630.clm
Name not found : field_party[0]
编译文件失败：E:\Games\The.Legend.of.Heroes.Zero.no.Kiseki.KAI\The Legend of Heroes Zero no Kiseki KAI\data_tc\scena\decrypted\decrypted\t1650.clm
未找到Fn文本：  TextMessage null {
                {color 5}   药物学丄神经科研究室
                {} 　约亚西姆丄琼塔副教授{color 0}{wait}
        }
编译文件失败：E:\Games\The.Legend.of.Heroes.Zero.no.Kiseki.KAI\The Legend of Heroes Zero no Kiseki KAI\data_tc\scena\decrypted\decrypted\t4100.clm
未找到Fn文本：  TextMessage null {
                {color 5}{0x06} 　………………　……　　　　　
                {} 　……　……………………
                　　　　　……眠…于…
                ───────────────……
                　Ｓ１………　～　Ｓ１…８…　{color 0}{wait}
        }
已编译GBK编码CLM文件：E:\Games\The.Legend.of.Heroes.Zero.no.Kiseki.KAI\The Legend of Heroes Zero no Kiseki KAI\data_tc\scena\decrypted\decrypted\compiled

C:\Users\Jelly\RiderProjects\CalmareEncoder\CalmareEncoder\bin\Debug\net9.0>
 */