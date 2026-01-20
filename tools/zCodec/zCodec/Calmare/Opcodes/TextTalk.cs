#region

using System.Text;
using static zCodec.Calmare.CalmareCodec;

#endregion

namespace zCodec.Calmare.Opcodes;

public class TextTalk(string text) : ScenaOpcode( text)
{
    protected override byte Code => 0x5C;
    public override byte[] Compile(Encoding encoding)
    {
        List<byte> bytes = new(0x200) { Code };
        bytes.AddRange(CompileCid(Param[0]));
        for (var index = 1; index < Param.Count; index++)
        {
            if (index > 1)
                bytes.Add(3);
            var text = Param[index];
            bytes.AddRange(ClmStringToBytes(text, encoding));
        }

        bytes.Add(0);
        return bytes.ToArray();
    }
}