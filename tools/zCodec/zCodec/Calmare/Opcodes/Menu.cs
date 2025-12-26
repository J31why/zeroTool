using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.RegularExpressions;

namespace zCodec.Calmare.Opcodes;

public class Menu(string text) : Opcode(0x5E,text)
{
    public override byte[] Encode(Encoding encoding)
    {
        try
        {
            List<byte> bytes = new(0x200) { Code };
            var menu = NumReg.Match(Param[0]).Value;
            bytes.AddRange(BitConverter.GetBytes(Convert.ToUInt16(menu)));
            bytes.AddRange(BitConverter.GetBytes(Convert.ToInt16(Param[1])));
            bytes.AddRange(BitConverter.GetBytes(Convert.ToInt16(Param[2])));
            bytes.AddRange(Convert.ToByte(Param[3]));
            for (var i = 4; i < Param.Count; i++)
            {
                var text = Param[i];
                bytes.AddRange([..EncodeString(text, encoding), 1]);
            }

            bytes.Add(0);
            return bytes.ToArray();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}