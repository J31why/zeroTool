#region

using System.Text;

#endregion

namespace zCodec.Calmare.Opcodes;

public class ED7MenuAdd(string text) : Opcode(0xCE, text)
{
    public override byte[] Compile(Encoding encoding)
    {
        var menu = NumReg.Match(Param[0]).Value;
        List<byte> bytes = new(0x200) { Code, 1, Convert.ToByte(menu) };
        bytes.AddRange(CompileString(Param[1], encoding));
        bytes.Add(0);
        return bytes.ToArray();
    }
}