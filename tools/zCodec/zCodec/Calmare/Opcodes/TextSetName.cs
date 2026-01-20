#region

using System.Text;

#endregion

namespace zCodec.Calmare.Opcodes;

public class TextSetName(string text) : ScenaOpcode(text)
{
    protected override byte Code => 0x61;
    public override byte[] Compile(Encoding encoding)
    {
        return [Code, ..CalmareCodec.ClmStringToBytes(Param[0],encoding), 0];
    }
}