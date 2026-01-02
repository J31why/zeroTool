#region

using System.Text;

#endregion

namespace zCodec.Calmare.Opcodes;

public class TextTalkNamed(string text) : Opcode(0x5D, text)
{
    public override byte[] Compile(Encoding encoding)
    {
        List<byte> bytes = new(0x200) { Code };
        bytes.AddRange(CompileCid(Param[0]));
        bytes.AddRange([..encoding.GetBytes(Param[1]), 0]);
        for (var index = 2; index < Param.Count; index++)
        {
            if (index > 2)
                bytes.Add(3);
            var text = Param[index];
            bytes.AddRange(CompileString(text, encoding));
        }

        bytes.Add(0);
        return bytes.ToArray();
    }
}