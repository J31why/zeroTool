#region

using System.Text;

#endregion

namespace zCodec.Calmare.Opcodes;

public class ScMenuSetTitle(string text) : Opcode(0x58, text)
{
    public override byte[] Encode(Encoding encoding)
    {
        List<byte> bytes = new(0x200) { Code };
        bytes.AddRange(BitConverter.GetBytes(Convert.ToUInt16(Param[0])));
        bytes.AddRange(BitConverter.GetBytes(Convert.ToInt16(Param[1])));
        bytes.AddRange(BitConverter.GetBytes(Convert.ToInt16(Param[2])));
        bytes.AddRange(EncodeString(Param[3], encoding));
        bytes.Add(0);
        return bytes.ToArray();
    }
}