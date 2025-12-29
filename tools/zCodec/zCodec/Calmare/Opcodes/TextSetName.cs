#region

using System.Text;

#endregion

namespace zCodec.Calmare.Opcodes;

public class TextSetName(string text) : Opcode(0x61, text)
{
    public override byte[] Encode(Encoding encoding)
    {
        return [Code, ..encoding.GetBytes(Param[0]), 0];
    }
}