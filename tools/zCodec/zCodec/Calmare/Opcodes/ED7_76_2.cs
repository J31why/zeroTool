
using System.Text;
using Common;
using static zCodec.Calmare.CalmareCodec;

namespace zCodec.Calmare.Opcodes;

public class ED7_76_2(string text) : ScenaOpcode(text)
{
    protected override byte Code => 0x76;
    public override byte[] Compile(Encoding encoding)
    {
        List<byte> bytes = new(0x200) { Code };
        var id = Convert.ToByte(NumRegex().Match(Param[0]).Value);
        bytes.AddRange(id);
        bytes.AddRange(ClmStringToBytes(Param[1], ExtraEncoding.SJIS));
        bytes.AddRange([0,2]);
        bytes.AddRange(ClmStringToBytes(Param[2], ExtraEncoding.SJIS));
        bytes.Add(0);
        return bytes.ToArray();
    }
}

