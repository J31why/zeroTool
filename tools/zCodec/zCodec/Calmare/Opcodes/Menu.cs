#region

using System.Text;
using static zCodec.Calmare.CalmareCodec;

#endregion

namespace zCodec.Calmare.Opcodes;

public class Menu(string text) : ScenaOpcode(text)
{
    protected override byte Code => 0x5E;
    public override byte[] Compile(Encoding encoding)
    {
        try
        {
            List<byte> bytes = new(0x200) { Code };
            var menu = NumRegex().Match(Param[0]).Value;
            bytes.AddRange(BitConverter.GetBytes(Convert.ToUInt16(menu)));
            bytes.AddRange(BitConverter.GetBytes(Convert.ToInt16(Param[1])));
            bytes.AddRange(BitConverter.GetBytes(Convert.ToInt16(Param[2])));
            bytes.AddRange(Convert.ToByte(Param[3]));
            for (var i = 4; i < Param.Count; i++)
            {
                var text = Param[i];
                bytes.AddRange([..ClmStringToBytes(text, encoding), 1]);
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