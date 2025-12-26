using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.RegularExpressions;

namespace zCodec.Calmare.Opcodes;

public class TextSetName(string text) : Opcode(0x61,text)
{
    public override byte[] Encode(Encoding encoding)
    {
        return [Code, ..encoding.GetBytes(Param[0]), 0];
    }
}