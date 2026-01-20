#region

using System.Text;
using zCodec.Calmare;

#endregion

namespace Extensions;

public static class BinaryReaderExtension
{
    public static string ReadCString(this BinaryReader br, Encoding encoding)
    {
        if (br.BaseStream.Position >= br.BaseStream.Length)
            throw new OutOfMemoryException();
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
    public static string ReadCStringWithOffset(this BinaryReader br,long offset ,Encoding encoding)
    {
        var pos =  br.BaseStream.Position;
        br.BaseStream.Seek(offset, SeekOrigin.Begin);
        var ret = br.ReadCString(encoding);
        br.BaseStream.Seek(pos, SeekOrigin.Begin);
        return ret;
    }

    public static string ReadClmString(this BinaryReader br, Encoding encoding)
    {
        if (br.BaseStream.Position >= br.BaseStream.Length)
            throw new OutOfMemoryException();
        var bytes = new List<byte>(0x200);
        while (br.BaseStream.Position < br.BaseStream.Length)
        {
            var b = br.ReadByte();
            if (b == 0)
                break;
            bytes.Add(b);
            switch (b)
            {
                case 7:
                    bytes.Add(br.ReadByte());
                    break;
                case 0x1f:
                    bytes.Add(br.ReadByte());
                    bytes.Add(br.ReadByte());
                    break;
            }
        }
        return CalmareCodec.BytesToClmString(bytes.ToArray(), encoding);
    }
    public static string ReadClmStringWithOffset(this BinaryReader br,long offset ,Encoding encoding)
    {
        var pos =  br.BaseStream.Position;
        br.BaseStream.Seek(offset, SeekOrigin.Begin);
        var ret = br.ReadClmString(encoding);
        br.BaseStream.Seek(pos, SeekOrigin.Begin);
        return ret;
    }
}