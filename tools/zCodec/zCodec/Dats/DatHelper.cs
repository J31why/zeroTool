using System.Text;

namespace zCodec.Dats;

public static class DatHelper
{
    public static void ToCsv(string file, string outDir,Encoding encoding)
    {
        var fileName = Path.GetFileNameWithoutExtension(file);
        switch (fileName)
        {
            case "t_name":
                NameDat.ToCsv(file, outDir, encoding);
                break;
        }
        
    }
    public static void ToDat(string file, string outDir,Encoding encoding)
    {
        var fileName = Path.GetFileNameWithoutExtension(file);
        switch (fileName)
        {
            case "t_name":
                NameDat.ToDat(file, outDir, encoding);
                break;
        }
        
    }
}