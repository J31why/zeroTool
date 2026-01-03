#region

using System.Text;

#endregion

namespace Extensions;

public static class BinaryReaderExtension
{
    public static string? ReadCString(this BinaryReader br, Encoding encoding)
    {
        if (br.BaseStream.Position >= br.BaseStream.Length)
            return null;
        var bytes = new List<byte>(0x200);
        while (br.BaseStream.Position < br.BaseStream.Length)
        {
            var b = br.ReadByte();
            if (b == 0)
                break;
            bytes.Add(b);
        }

        return encoding.GetString(bytes.ToArray());
    }
    public static string? ReadCStringWithOffset(this BinaryReader br,long offset ,Encoding encoding)
    {
        var pos =  br.BaseStream.Position;
        br.BaseStream.Seek(offset, SeekOrigin.Begin);
        var ret = br.ReadCString(encoding);
        br.BaseStream.Seek(pos, SeekOrigin.Begin);
        return ret;
    }
}