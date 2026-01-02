#region

using System.Text;
using System.Text.RegularExpressions;

#endregion

namespace Common;

public static partial class ExtraEncoding
{
    public static Encoding GBK { get; }
    public static Encoding SJIS { get; }
    public static Encoding UTF8NoBOM { get; } = new UTF8Encoding(false);
    public static Regex DoubleByteCharReg { get; } = DoubleByteCharRegex();

    static ExtraEncoding()
    {
        
        GBK = CodePagesEncodingProvider.Instance.GetEncoding("GBK") ?? throw new ArgumentException("error codepage");
        SJIS = CodePagesEncodingProvider.Instance.GetEncoding(932) ?? throw new ArgumentException("error codepage");
    }

    [GeneratedRegex("[\u00FF-\uffff]", RegexOptions.Compiled | RegexOptions.Multiline)]
    private static partial Regex DoubleByteCharRegex();
}