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

    static ExtraEncoding()
    {
        
        GBK = CodePagesEncodingProvider.Instance.GetEncoding("GBK") ?? throw new ArgumentException("error codepage");
        SJIS = CodePagesEncodingProvider.Instance.GetEncoding(932) ?? throw new ArgumentException("error codepage");
    }

    [GeneratedRegex("[\u00FF-\uffff]", RegexOptions.Compiled | RegexOptions.Multiline)]
    public static partial Regex DoubleByteCharRegex();
}