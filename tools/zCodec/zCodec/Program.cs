
using System.Text;
using System.Text.RegularExpressions;
using Common;
using JiebaNet.Segmenter.Common;
using zCodec.Calmare;

namespace zCodec;

internal static class Program
{
   
    private static string _inputPath = "", _calmare = "";
    private static void Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        
        bool isCompile = false, isDecompile = false, isTw2s = false,
            isDecryptString = false, isDecryptFile = false;
        
        try
        {
            _inputPath = args[0];
            var currentDir = Environment.ProcessPath ?? throw new DirectoryNotFoundException();
            currentDir = Path.GetDirectoryName(currentDir) ?? throw new DirectoryNotFoundException();
            _calmare = Path.Combine(currentDir, "calmare.exe");
            if (!File.Exists(_inputPath) && !Directory.Exists(_inputPath))
                throw new Exception();
            switch (args[1])
            {
                case "-c":
                    isCompile = true;
                    break;
                case "-de":
                    isDecompile = true;
                    break;
                case "-ds":
                    isDecryptString = true;
                    break;
                case "-df":
                    isDecryptFile = true;
                    break;
                case "-tw2s":
                    isTw2s = true;
                    break;
                default:
                    throw new ArgumentException();
            }
        }
        catch (Exception)
        {
            OutHelp();
            return;
        }
        Console.WriteLine();
        try
        {
            if (isCompile)
            {
                if (!File.Exists(_calmare))
                    throw new FileNotFoundException("未找到calmare.exe");
                var outPath = GetOutPath(_inputPath, "compiled");
                Console.WriteLine($"正在编译{ExtraEncoding.GBK.BodyName}编码clm文件");
                Compile(_inputPath,outPath);
                Console.WriteLine($"已编译{ExtraEncoding.GBK.EncodingName}编码clm文件");
            }
            else if (isDecompile)
            {
                if (!File.Exists(_calmare))
                    throw new FileNotFoundException("未找到calmare.exe");
                Console.WriteLine("正在反编译Bin文件");
                DecompileBin(_inputPath, _calmare);
                Console.WriteLine("已反编译Bin文件");
            }
            else if (isDecryptString)
            {
                var outPath = GetOutPath(_inputPath, "decrypted");
                Console.WriteLine("正在解密云豹字符串");
                DecryptStr(_inputPath, outPath);
                Console.WriteLine("已解密云豹字符串：{0}", outPath);
            }
            else if (isDecryptFile)
            {
                Console.WriteLine("正在解密云豹文件");
                var outPath = GetOutPath(_inputPath, "decrypted");
                DecryptFile(_inputPath, outPath);
                Console.WriteLine("已解密云豹文件：{0}", outPath);
            }
            else if (isTw2s)
            {
                Console.WriteLine("正在转换为大陆简体");
                var outPath = GetOutPath(_inputPath, "converted");
                ClmTw2s(_inputPath, outPath);
                Console.WriteLine("已转换为大陆简体：{0}", outPath);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        Console.ReadKey();
    }

    private static void Compile(string path, string outPath)
    {
        void compile(string file, string outfile)
        {
            try
            {
                var coder = new CalmareCoder();
                coder.ParseFile(file);
                coder.Encode2File(outfile, _calmare, ExtraEncoding.GBK);
            }
            catch (Exception e)
            {
                Console.WriteLine($"[ERROR]:{file}");
                Console.WriteLine(e.Message);
            }
        }
        
        if (Directory.Exists(path))
        {
            var files = Directory.EnumerateFiles(path, "*.clm");
            foreach (var file in files)
            {
                var outFile = Path.Combine(outPath, Path.GetFileName(file));
                compile(file, outFile);
            }
        }
        else if (File.Exists(path))
        {
            var outFile = Path.Combine(outPath, Path.GetFileName(path));
            compile(path, outFile);
        }
  
    }
    private static void ClmTw2s(string path, string outPath)
    {
        void decrypt(string file, string outfile)
        {
            try
            {
                var clm = CLEDecrypter.Tw2s(File.ReadAllText(file));
                File.WriteAllText(outfile,clm);
            }
            catch (Exception e)
            {
                Console.WriteLine("转大陆简体失败: {0}",file);
            }
        }
        
        if (Directory.Exists(path))
        {
            var files = Directory.EnumerateFiles(path, "*.clm");
            foreach (var file in files)
            {
                var outFile = Path.Combine(outPath, Path.GetFileName(file));
                decrypt(file, outFile);
            }
        }
        else if (File.Exists(path))
        {
            var outFile = Path.Combine(outPath, Path.GetFileName(path));
            decrypt(path, outFile);
        }
    }
    private static void DecompileBin(string path, string calmare)
    {
        if (Directory.Exists(path))
        {
            var files = Directory.EnumerateFiles(path, "*.bin");
            foreach (var file in files) Utils.RunExe(calmare, $"\"{file}\"", 1);
        }
        else if (File.Exists(path))
        {
            Utils.RunExe(calmare, path, 2);
        }
    }
    
    private static void DecryptStr(string path, string outPath)
    {
        void decrypt(string file, string outfile)
        {
            try
            {
                var clmText = File.ReadAllText(file);
                clmText = CLEDecrypter.DecryptChar(clmText,out var warnList);
                if (warnList.Count > 0)
                {
                    Console.WriteLine("\n非法文字: {0}", string.Join(' ', warnList));
                    Console.WriteLine("文件有非法文字: {0}\n", file);
                }
                File.WriteAllText(outfile, clmText);
            }
            catch (Exception e)
            {
                Console.WriteLine("解密字符串失败：{0}", file);
                Console.WriteLine(e.Message);
            }
        }
        if (Directory.Exists(path))
        {
            var files = Directory.EnumerateFiles(path, "*.clm");
            foreach (var file in files)
            {
                var outFile = Path.Combine(outPath, Path.GetFileName(file));
                decrypt(file, outFile);
            }
        }
        else if (File.Exists(path))
        {
            var outFile = Path.Combine(outPath, Path.GetFileName(path));
            decrypt(path, outFile);
        }
    }
    
    private static void DecryptFile(string path, string outPath)
    {
        void decrypt(string file, string outfile)
        {
            try
            {
                var bytes = CLEDecrypter.DecryptFile(file);
                File.WriteAllBytes(outfile, bytes);
            }
            catch (Exception e)
            {
                Console.WriteLine("解密文件失败：{0}", file);
                Console.WriteLine(e.Message);
            }
        }
        
        if (Directory.Exists(path))
        {
            var files = Directory.EnumerateFiles(path);
            foreach (var file in files)
            {
                var outFile = Path.Combine(outPath, Path.GetFileName(file));
                decrypt(file, outFile);
            }
        }
        else if (File.Exists(path))
        {
            var outFile = Path.Combine(outPath, Path.GetFileName(path));
            decrypt(path, outFile);
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
        Console.WriteLine("zCodec使用方法(如果需要编译，请把calmare.exe放在目录下):");
        Console.WriteLine("1. 使用calmare编译GBK编码clm文件: zCodec file/dir -c");
        Console.WriteLine("2. 使用calmare批量反编译bin文件: zCodec file/dir -de");
        Console.WriteLine("3. 解密云豹clm文件加密字符串: zCodec file/dir -ds"); 
        Console.WriteLine("4. 解密云豹加密文件: zCodec file/dir -df");
        Console.WriteLine("5. 把clm文件中的台湾文本转为大陆简体文本: zCodec file/dir -tw2s");
        Console.WriteLine(new string('-', 80));
    }
    
    static void SearchTWPhrases(string dir)
    {
        //查看有多少个台湾IT用语
        var dic = new HashSet<string>(300);
        //opencc 原版TWPhrasesIT.txt
        var txt =  File.ReadAllText("Dictionary\\TWPhrasesIT.txt");
        var matches = Regex.Matches(txt, """(?<=\s)(?!^)\S+""", RegexOptions.Multiline);
        var pattern = string.Join('|', matches.Select(x => x.Value).ToList());
        var reg = new Regex(pattern,RegexOptions.Multiline);
        var files = Directory.GetFiles(dir, "*.clm");
        foreach (var file in files)
        {
            matches =  reg.Matches(File.ReadAllText(file));
            foreach (Match match in matches)
                dic.Add(match.Value);
        }
        
        var sb = new StringBuilder();
        foreach (var s in dic)
        {
            var p = @"^.*?\s" + s + @"(?: .*?$|$)";
            var r = new  Regex(p,RegexOptions.Multiline);
            if (r.IsMatch(txt))
            {            
                var match = r.Match(txt);
                if (!sb.ToString().Contains(match.Value))
                {
                    sb.AppendLine(match.Value);
                }
                else
                {
                    Console.WriteLine(match.Value);
                }
            }
            else
            {
                Console.WriteLine("error: " + s);
            }
        }
        File.WriteAllText("TWPhrasesIT.txt",sb.ToString());
        Console.WriteLine("dic.Count: " + dic.Count);
    }
    
}