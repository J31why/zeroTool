#region

using System.Text;
using static zCodec.Calmare.CalmareCodec;

#endregion

namespace zCodec.Calmare.Opcodes;

public class ScMenuSetTitle(string text) : ScenaOpcode( text)
{
    protected override byte Code => 0x58;
    public override byte[] Compile(Encoding encoding)
    {
        List<byte> bytes = new(0x200) { Code };
        bytes.AddRange(BitConverter.GetBytes(Convert.ToUInt16(Param[0])));
        bytes.AddRange(BitConverter.GetBytes(Convert.ToInt16(Param[1])));
        bytes.AddRange(BitConverter.GetBytes(Convert.ToInt16(Param[2])));
        bytes.AddRange(ClmStringToBytes(Param[3], encoding));
        bytes.Add(0);
        return bytes.ToArray();
    }
}