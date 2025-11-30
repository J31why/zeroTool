using System.Security.Cryptography;


var p = "C:\\Users\\Jelly\\Desktop\\m0000.bin";
var encrypted = File.ReadAllBytes(p);
var decrypted = Decrypt(encrypted);

if(decrypted.Length ==0)
{
    Console.WriteLine("解密错误");
    return;
}
// table[(high << 8 - 0x8900 + low)*3]


var dir = Path.Combine(Path.GetDirectoryName(p)??"","decrypted");
if(!Directory.Exists(dir))
    Directory.CreateDirectory(dir);
File.WriteAllBytes(Path.Combine(dir, Path.GetFileName(p)), decrypted);
Console.WriteLine($"解密完成: {decrypted.Length}字节大小");


byte[] Decrypt(byte[] encryptedData)
{
    using var aes = Aes.Create();
    aes.Key =  
    [
        0x3E, 0x93, 0xBF, 0x99, 0x95, 0xDD, 0x9A, 0x86,
        0xC2, 0xD4, 0x8D, 0x85, 0xCC, 0x06, 0xCA, 0x1F
    ];
    ;
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