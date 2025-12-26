using System.Diagnostics.CodeAnalysis;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Text;
using System.Text.RegularExpressions;

namespace zCodec.Calmare.Opcodes;

public partial class TextMessage(string text) : Opcode(0x55,text)
{
    public override byte[] Encode(Encoding encoding)
    {
        List<byte> bytes = new(0x200) { Code };
        bytes.AddRange(EncodeCid(Param[0]));
        for (var index = 1; index < Param.Count; index++)
        {
            if (index > 1)
                bytes.Add(3);
            var text = Param[index];
            bytes.AddRange(EncodeString(text, encoding));
        }

        bytes.Add(0);
        return bytes.ToArray();
    }
}