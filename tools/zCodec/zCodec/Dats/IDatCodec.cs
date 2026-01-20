using System.Collections;
using System.Text;
using zCodec.Calmare;

namespace zCodec.Dats;

public enum DatSaveFormat
{
    Csv,
    Json
}

public interface IDatCodec
{
    bool IsAo { get; }
    bool CanDecompile(string file);
    DatSaveFormat DatSaveFormat { get; }
    IDatCodec Load(string file);
    object Decompile(string file, Encoding encoding);
    byte[] Compile(Encoding encoding);
}