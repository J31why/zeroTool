#region

using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using OpenCCNET;

#endregion

// ReSharper disable MemberCanBePrivate.Global

namespace Common;

public static partial class CLEDecrypter
{
    private static byte[]? _table;
    private static readonly Regex TextReg = TextRegex();

    static CLEDecrypter()
    {
        var currentDir = Environment.ProcessPath ?? throw new DirectoryNotFoundException();
        currentDir = Path.GetDirectoryName(currentDir) ?? throw new DirectoryNotFoundException();
        ZhConverter.Initialize(Path.Combine(currentDir, "Dictionary"),
            Path.Combine(currentDir, "JiebaResource"),
            true);
        ZhConverter.ZhSegment.Jieba.AddWord("明瞭");
    }

    public static byte[] DecryptFile(string file)
    {
        if (!File.Exists(file))
            throw new FileNotFoundException(file);
        var encrypted = File.ReadAllBytes(file);
        var decrypted = DecryptFile(encrypted);
        return decrypted.Length == 0 ? throw new InvalidDataException() : decrypted;
    }

    public static byte[] DecryptFile(byte[] encryptedData)
    {
        using var aes = Aes.Create();
        aes.Key =
        [
            0x3E, 0x93, 0xBF, 0x99, 0x95, 0xDD, 0x9A, 0x86,
            0xC2, 0xD4, 0x8D, 0x85, 0xCC, 0x06, 0xCA, 0x1F
        ];
        aes.IV =
        [
            0xF2, 0xD9, 0x61, 0xF2, 0x22, 0xB5, 0x22, 0x68,
            0xBA, 0x3A, 0x84, 0xBA, 0x48, 0x8C, 0x8B, 0x27
        ];
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        using var ms = new MemoryStream();
        using var decryptor = aes.CreateDecryptor();
        using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Write);
        cs.Write(encryptedData, 0, encryptedData.Length);
        cs.FlushFinalBlock();

        var decryptedWithHeader = ms.ToArray();

        if (decryptedWithHeader.Length < 4)
            throw new InvalidDataException("解密数据太小，无法包含大小头");

        var originalSize = BitConverter.ToInt32(decryptedWithHeader, 0);
        if (originalSize > decryptedWithHeader.Length - 4)
            throw new InvalidDataException($"大小头指示的大小无效: {originalSize} > {decryptedWithHeader.Length - 4}");

        var result = new byte[originalSize];
        Array.Copy(decryptedWithHeader, 4, result, 0, originalSize);

        return result;
    }

    public static string Tw2s(string input)
    {
        input = TextReg.Replace(input, x => ZhConverter.TWToHans(x.Value, true));
        return input;
    }

    public static string DecryptChar(string input, out List<string> warnList)
    {
        if (_table == null)
        {
            var currentDir = Environment.ProcessPath ?? throw new DirectoryNotFoundException();
            currentDir = Path.GetDirectoryName(currentDir) ?? throw new DirectoryNotFoundException();
            _table = File.ReadAllBytes(Path.Combine(currentDir, "utf8.table"));
        }

        var warn = new List<string>(200);
        input = ExtraEncoding.DoubleByteCharReg.Replace(input, x =>
        {
            var sjisBytes = ExtraEncoding.SJIS.GetBytes(x.Value);
            var index = (sjisBytes[0] << 8) - 0x8900 + sjisBytes[1];
            if (index < 0)
            {
                if (ExtraEncoding.GBK.GetByteCount(x.Value) == 1)
                    warn.Add(x.Value);
                return x.Value;
            }

            index *= 3;
            index += 4;
            byte[] utf8Bytes = [_table[index], _table[index + 1], _table[index + 2]];
            utf8Bytes = BitHelper.TrimEnd(utf8Bytes, 0);
            return Encoding.UTF8.GetString(utf8Bytes);
        });
        warnList = warn;
        return input;
    }

    [GeneratedRegex("""[\u00FF-\uFFFF]+""",
        RegexOptions.Compiled | RegexOptions.Multiline)]
    private static partial Regex TextRegex();
}