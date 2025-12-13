using System.Text;

namespace Extensions;

public static class BinaryReaderExtension
{
    public static string? ReadCString(this BinaryReader br, Encoding encoding)
    {
        if (br.BaseStream.Position >= br.BaseStream.Length)
            return null;
        var bytes = new List<byte>(100);
        while (br.BaseStream.Position < br.BaseStream.Length)
        {
            var b = br.ReadByte();
            if (b == 0)
                break;
            bytes.Add(b);
        }

        return encoding.GetString(bytes.ToArray());
    }
}