using System.Diagnostics;
using System.Text;
using CalmareEncoder.Calmare;
using Common;
using OpenCCNET;

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

