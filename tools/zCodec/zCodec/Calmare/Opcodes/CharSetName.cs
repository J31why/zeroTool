using System.Text;

namespace zCodec.Calmare.Opcodes;

public class CharSetName(string text) : ScenaOpcode(text)
{
    protected override byte Code => 0x8e;
    public override byte[] Compile(Encoding encoding)
    {
        List<byte> bytes = new(0x200) { Code };
        bytes.AddRange(CompileCid(Param[0]));
        bytes.AddRange(CalmareCodec.ClmStringToBytes(Param[1], encoding));
        bytes.Add(0);
        return bytes.ToArray();
    }
}