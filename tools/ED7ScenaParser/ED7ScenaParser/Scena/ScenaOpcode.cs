
namespace ED7ScenaParser.Scena;

public class ScenaOpcodeCoder
{
   public static ScenaOpcodeCoder Instance { get; } = new();
   public ScenaReader? Reader { get; set; }


   private readonly Func<string>[] _opcodeFunc =
   [
      "ret".Op(),
      "call".Op([u16]),
      "NewScene".Op( [u16, i8, i8, i8]),
   ];

   private static Func<string?> i8 => () => Instance.Reader?.ReadByte().ToString();
   private static Func<string?> u16 => () => Instance.Reader?.ReadUInt16().ToString();
   private static Func<string?> u32 => () => Instance.Reader?.ReadUInt32().ToString();

   public string ReadFunc()
   {
      if(Reader is null)
         throw new NullReferenceException();
      while (true)
      {
         var opcode = Reader.ReadByte();
         
      }
   }
}