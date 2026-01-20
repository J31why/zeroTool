#region

using System.Text;
using static zCodec.Calmare.CalmareCodec;

#endregion

namespace zCodec.Calmare.Opcodes;

public class ED7MenuAdd(string text) : ScenaOpcode(text)
{
    protected override byte Code => (byte)(IsAo ? 0xD0 : 0xCE);
    public override byte[] Compile(Encoding encoding)
    {
        var menu =  NumRegex().Match(Param[0]).Value;
        List<byte> bytes = new(0x200) { Code, 1, Convert.ToByte(menu) };
        bytes.AddRange(ClmStringToBytes(Param[1], encoding));
        bytes.Add(0);
        return bytes.ToArray();
    }
}